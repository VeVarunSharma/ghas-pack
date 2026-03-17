# Advanced GHAS Scenarios

This guide covers advanced enterprise scenarios for GitHub Advanced Security (GHAS). Each section builds on the foundational setup described in the skill guides under [`.github/skills/`](../.github/skills/).

---

## Table of Contents

1. [Monorepo Code Scanning](#1-monorepo-code-scanning)
2. [Custom CodeQL Queries for Org-Specific Rules](#2-custom-codeql-queries-for-org-specific-rules)
3. [Third-Party Scanner Integration (SARIF Upload)](#3-third-party-scanner-integration-sarif-upload)
4. [Organization-Wide Dependabot Config](#4-organization-wide-dependabot-config)
5. [Dependency Review in CI/CD](#5-dependency-review-in-cicd)
6. [GHAS API and Automation](#6-ghas-api-and-automation)
7. [GitHub Security Overview](#7-github-security-overview)
8. [Integration with Ticketing Systems](#8-integration-with-ticketing-systems)

---

## 1. Monorepo Code Scanning

Large monorepos often contain multiple languages, build systems, and service directories. A single CodeQL workflow quickly becomes insufficient. Use a **matrix strategy** to analyze each language (and optionally each directory) in parallel.

### Key Concepts

| Challenge | Solution |
|-----------|----------|
| Multiple languages | Matrix strategy with per-language build steps |
| Long analysis times | Split into parallel jobs; use `paths` filters |
| Separate alert ownership | Use `category` to partition results by service |
| Different build systems | Conditional build steps per matrix entry |

### Workflow Example

```yaml
# .github/workflows/codeql-monorepo.yml
name: "CodeQL – Monorepo"

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  schedule:
    - cron: "25 4 * * 1" # Weekly Monday 4:25 AM UTC

concurrency:
  group: codeql-${{ github.ref }}-${{ matrix.language }}-${{ matrix.service }}
  cancel-in-progress: true

jobs:
  analyze:
    name: Analyze (${{ matrix.language }} / ${{ matrix.service }})
    runs-on: ${{ matrix.language == 'swift' && 'macos-latest' || 'ubuntu-latest' }}
    permissions:
      security-events: write
      contents: read

    strategy:
      fail-fast: false
      matrix:
        include:
          # --- Backend (Java) ---
          - language: java-kotlin
            build-mode: manual
            service: backend
            build-cmd: cd backend && mvn package -DskipTests -q
          # --- Frontend (JavaScript/TypeScript) ---
          - language: javascript-typescript
            build-mode: none
            service: frontend
            build-cmd: ""
          # --- Infrastructure (Python) ---
          - language: python
            build-mode: none
            service: infra
            build-cmd: ""
          # --- Shared libraries (C#) ---
          - language: csharp
            build-mode: manual
            service: shared-libs
            build-cmd: cd shared-libs && dotnet build -q

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Initialize CodeQL
        uses: github/codeql-action/init@v3
        with:
          languages: ${{ matrix.language }}
          build-mode: ${{ matrix.build-mode }}
          # Scope the database to the service directory
          # to reduce noise and speed up analysis
          config: |
            paths:
              - ${{ matrix.service }}

      - name: Manual build
        if: matrix.build-mode == 'manual'
        shell: bash
        run: ${{ matrix.build-cmd }}

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v3
        with:
          # Category separates results per service in the Security tab
          category: "/language:${{ matrix.language }}/service:${{ matrix.service }}"
```

### Tips

- **Use `paths` in `config`** to limit the CodeQL database to the relevant service directory. This dramatically reduces analysis time.
- **Set `category`** so that each service's alerts appear separately in the Security tab and can be triaged independently.
- **Use `fail-fast: false`** so a build failure in one service doesn't cancel analysis of the others.
- **Trigger only on changed paths** if you want even faster PR checks — add `paths` filters to the `on.pull_request` trigger.

> **See also:** [Workflow configuration reference](../.github/skills/codeql/references/workflow-configuration.md) for full trigger, runner, and query suite options.

---

## 2. Custom CodeQL Queries for Org-Specific Rules

CodeQL's default query suites catch common vulnerabilities, but every organization has coding patterns, internal APIs, or compliance rules that need custom detection. CodeQL query packs let you author, test, and distribute custom rules.

### Creating a Custom Query Pack

#### 1. Initialize the pack

```bash
# Install the CodeQL CLI (see .github/skills/codeql/references/cli-commands.md)
codeql pack init my-org/security-queries
```

This creates a directory with a `qlpack.yml`:

```yaml
# qlpack.yml
name: my-org/security-queries
version: 1.0.0
library: false
dependencies:
  codeql/javascript-all: "*"   # Add per target language
  codeql/python-all: "*"
```

#### 2. Write a query

```ql
// queries/js/NoHardcodedPasswords.ql
/**
 * @name Hardcoded password in source
 * @description Finds string literals assigned to variables named "password".
 * @kind problem
 * @problem.severity error
 * @precision high
 * @id my-org/js/no-hardcoded-passwords
 * @tags security
 *       custom
 */

import javascript

from AssignExpr assign, StringLiteral value
where
  assign.getLhs().(VarAccess).getName().regexpMatch("(?i).*(password|passwd|pwd|secret).*") and
  assign.getRhs() = value
select assign, "Hardcoded credential assigned to " + assign.getLhs().(VarAccess).getName()
```

#### 3. Test the query

Create a test directory with sample code and expected results:

```
queries/js/NoHardcodedPasswords/
  NoHardcodedPasswords.ql        # Symlink or copy of the query
  NoHardcodedPasswords.expected  # Expected results (one line per finding)
  test.js                        # Sample vulnerable code
```

```bash
# Run tests
codeql test run queries/js/NoHardcodedPasswords/
```

#### 4. Publish to GitHub Container Registry

```bash
# Authenticate (use a PAT with write:packages scope)
echo $GITHUB_TOKEN | codeql pack publish --github-auth-stdin my-org/security-queries
```

The pack is now available at `ghcr.io/my-org/security-queries`.

### Using Query Packs in Workflows

```yaml
- name: Initialize CodeQL
  uses: github/codeql-action/init@v3
  with:
    languages: javascript-typescript
    packs: |
      my-org/security-queries@~1.0.0
      codeql/javascript-queries:cwe-089   # Mix with standard packs
```

### Best Practices

| Practice | Why |
|----------|-----|
| Pin pack versions with `@~1.0.0` | Prevents unexpected query changes from breaking builds |
| Use `@id` prefixed with your org name | Avoids ID collisions with standard queries |
| Set `@precision high` only when confident | Reduces false positives in PR checks |
| Run `codeql test run` in CI for your pack | Catches regressions before publishing |
| Keep queries in a dedicated repo | Centralizes review and versioning |

---

## 3. Third-Party Scanner Integration (SARIF Upload)

GitHub's code scanning accepts results from **any** tool that produces [SARIF v2.1.0](https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html) output. This means you can upload findings from Semgrep, Snyk, Trivy, Checkmarx, SonarQube, or any other SARIF-producing scanner and view them alongside CodeQL results in the Security tab.

> **See also:** [SARIF output reference](../.github/skills/codeql/references/sarif-output.md) for the expected SARIF structure.

### Generic Upload Pattern

```yaml
# After any tool writes results.sarif:
- name: Upload SARIF
  uses: github/codeql-action/upload-sarif@v3
  with:
    sarif_file: results.sarif
    category: tool-name   # Keeps results separate per tool
```

### Workflow Example — Multi-Tool Pipeline

```yaml
# .github/workflows/third-party-scanners.yml
name: Third-Party Security Scanners

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  # ── Semgrep ──────────────────────────────────────────
  semgrep:
    name: Semgrep SAST
    runs-on: ubuntu-latest
    permissions:
      security-events: write
      contents: read
    container:
      image: semgrep/semgrep
    steps:
      - uses: actions/checkout@v4
      - name: Run Semgrep
        run: semgrep scan --config auto --sarif --output semgrep.sarif
        env:
          SEMGREP_APP_TOKEN: ${{ secrets.SEMGREP_APP_TOKEN }}
      - name: Upload Semgrep SARIF
        uses: github/codeql-action/upload-sarif@v3
        if: always()
        with:
          sarif_file: semgrep.sarif
          category: semgrep

  # ── Trivy (Container / IaC) ─────────────────────────
  trivy:
    name: Trivy Scan
    runs-on: ubuntu-latest
    permissions:
      security-events: write
      contents: read
    steps:
      - uses: actions/checkout@v4
      - name: Run Trivy (filesystem mode)
        uses: aquasecurity/trivy-action@master
        with:
          scan-type: fs
          scan-ref: .
          format: sarif
          output: trivy.sarif
          severity: CRITICAL,HIGH,MEDIUM
      - name: Upload Trivy SARIF
        uses: github/codeql-action/upload-sarif@v3
        if: always()
        with:
          sarif_file: trivy.sarif
          category: trivy

  # ── Snyk (SCA + Code) ───────────────────────────────
  snyk:
    name: Snyk Security
    runs-on: ubuntu-latest
    permissions:
      security-events: write
      contents: read
    steps:
      - uses: actions/checkout@v4
      - name: Run Snyk
        uses: snyk/actions/node@master
        continue-on-error: true
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
        with:
          args: --sarif-file-output=snyk.sarif
      - name: Upload Snyk SARIF
        uses: github/codeql-action/upload-sarif@v3
        if: always()
        with:
          sarif_file: snyk.sarif
          category: snyk
```

### Important Notes

- **Always use `if: always()`** on the upload step so results are uploaded even if the scanner returns a non-zero exit code (which most do when findings exist).
- **Set `category`** per tool to avoid results from one tool overwriting another.
- **SARIF file size limit** is 10 MB per upload. For large results, filter by severity before uploading.
- **`continue-on-error: true`** on the scan step prevents the workflow from failing before the upload step runs.

---

## 4. Organization-Wide Dependabot Config

Managing `dependabot.yml` across hundreds of repositories is impractical with manual file creation. GitHub provides several mechanisms to manage Dependabot at scale.

### Approach 1: Organization-Level Dependabot Defaults

GitHub supports **organization-level default setup** for Dependabot, which automatically enables alerts and security updates on all repositories in your org without any per-repo configuration.

**Enable via org settings:**
1. Go to **Organization → Settings → Code security and analysis**
2. Enable **Dependabot alerts** for all repos (new and existing)
3. Enable **Dependabot security updates** for all repos
4. Optionally enable **Dependabot version updates** on new repos

### Approach 2: Repository Rulesets for Enforcement

Use **repository rulesets** to enforce that all repositories run Dependabot and that PRs cannot be merged if they introduce known vulnerabilities:

1. Go to **Organization → Settings → Rulesets**
2. Create a new ruleset targeting the `main` branch across selected (or all) repos
3. Under **Branch rules → Require status checks**, add the dependency review check
4. Enable **Require Dependabot security updates** where available

### Approach 3: Templated `dependabot.yml` via Repository Templates

For version updates with specific schedules and grouping, create a **repository template** that includes your standard `dependabot.yml`:

```yaml
# Template repo: .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
    groups:
      production:
        dependency-type: "production"
      development:
        dependency-type: "development"
        update-types: ["minor", "patch"]
    labels: ["dependencies"]
    open-pull-requests-limit: 10

  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
    labels: ["ci"]
```

### Approach 4: Programmatic Config Distribution

Use the GitHub API or a script to push a standard `dependabot.yml` to all repos:

```bash
#!/bin/bash
# Push dependabot.yml to all repos in an org
ORG="my-org"
REPOS=$(gh repo list "$ORG" --limit 500 --json name -q '.[].name')

for REPO in $REPOS; do
  echo "Configuring $ORG/$REPO..."
  gh api "repos/$ORG/$REPO/contents/.github/dependabot.yml" \
    --method PUT \
    -f message="chore: add standard Dependabot config" \
    -f content="$(base64 -w0 dependabot-template.yml)" \
    2>/dev/null || echo "  ⚠ Already exists or failed"
done
```

### Auto-Triage Rules

Reduce noise by configuring auto-dismiss rules at the org level. Dependabot can automatically dismiss alerts that meet certain criteria:

```yaml
# In each repo's dependabot.yml or via org-level config
version: 2
updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
    # Auto-dismiss low-impact alerts
    ignore:
      - dependency-name: "*"
        update-types: ["version-update:semver-major"]
```

> **See also:** [Dependabot YAML reference](../.github/skills/dependabot/references/dependabot-yml-reference.md) and [example configs](../.github/skills/dependabot/references/example-configs.md) for detailed configuration options.

---

## 5. Dependency Review in CI/CD

The [dependency-review-action](https://github.com/actions/dependency-review-action) inspects the dependency diff in a pull request and can **block merges** that introduce known vulnerabilities or disallowed licenses.

### Basic Setup

```yaml
# .github/workflows/dependency-review.yml
name: Dependency Review

on: pull_request

permissions:
  contents: read
  pull-requests: write

jobs:
  dependency-review:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Dependency Review
        uses: actions/dependency-review-action@v4
        with:
          fail-on-severity: high
          comment-summary-in-pr: always
```

### License Compliance

Block PRs that introduce dependencies with disallowed licenses:

```yaml
- name: Dependency Review
  uses: actions/dependency-review-action@v4
  with:
    fail-on-severity: moderate
    # Deny specific licenses
    deny-licenses: GPL-3.0, AGPL-3.0, SSPL-1.0
    comment-summary-in-pr: always
```

Or use an **allow-list** approach to only permit pre-approved licenses:

```yaml
- name: Dependency Review
  uses: actions/dependency-review-action@v4
  with:
    allow-licenses: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, 0BSD
    comment-summary-in-pr: always
```

### Vulnerability Allow/Deny Lists

```yaml
- name: Dependency Review
  uses: actions/dependency-review-action@v4
  with:
    fail-on-severity: high
    # Skip specific advisories that have been reviewed and accepted
    allow-ghsas: GHSA-xxxx-yyyy-zzzz, GHSA-aaaa-bbbb-cccc
    # Always block specific dependencies regardless of severity
    deny-packages: pkg:npm/event-stream, pkg:pypi/insecure-package
```

### External Configuration File

For complex policies, use a configuration file:

```yaml
# .github/dependency-review-config.yml
fail-on-severity: "moderate"
allow-licenses:
  - "MIT"
  - "Apache-2.0"
  - "BSD-2-Clause"
  - "BSD-3-Clause"
  - "ISC"
deny-packages:
  - "pkg:npm/colors@>=1.4.1"
allow-ghsas:
  - "GHSA-xxxx-yyyy-zzzz"
comment-summary-in-pr: "always"
```

```yaml
# In the workflow:
- name: Dependency Review
  uses: actions/dependency-review-action@v4
  with:
    config-file: .github/dependency-review-config.yml
```

### Enforcing with Branch Protection

To make dependency review a **required** status check:

1. Go to **Settings → Branches → Branch protection rules** for `main`
2. Enable **Require status checks to pass before merging**
3. Add `dependency-review` to the list of required checks

---

## 6. GHAS API and Automation

GitHub provides REST APIs for all GHAS features, enabling programmatic alert management, data export, and triage automation.

### List Code Scanning Alerts

```bash
# List open code scanning alerts for a repo
gh api "/repos/{owner}/{repo}/code-scanning/alerts?state=open" \
  --jq '.[] | {number, rule: .rule.id, severity: .rule.security_severity_level, file: .most_recent_instance.location.path}'

# List critical/high alerts only
gh api "/repos/{owner}/{repo}/code-scanning/alerts?state=open&severity=critical,high" \
  --jq '.[] | [.number, .rule.id, .most_recent_instance.location.path] | @tsv'
```

### List Dependabot Alerts

```bash
# List open Dependabot alerts
gh api "/repos/{owner}/{repo}/dependabot/alerts?state=open" \
  --jq '.[] | {number, package: .security_vulnerability.package.name, severity: .security_advisory.severity, summary: .security_advisory.summary}'

# Count alerts by severity across an org
ORG="my-org"
for REPO in $(gh repo list "$ORG" --limit 100 --json name -q '.[].name'); do
  echo "=== $REPO ==="
  gh api "/repos/$ORG/$REPO/dependabot/alerts?state=open&per_page=100" \
    --jq 'group_by(.security_advisory.severity) | .[] | {severity: .[0].security_advisory.severity, count: length}' \
    2>/dev/null
done
```

### List Secret Scanning Alerts

```bash
# List open secret scanning alerts
gh api "/repos/{owner}/{repo}/secret-scanning/alerts?state=open" \
  --jq '.[] | {number, secret_type: .secret_type_display_name, created: .created_at}'

# Org-wide secret scanning alerts
gh api "/orgs/{org}/secret-scanning/alerts?state=open&per_page=100" \
  --jq '.[] | [.repository.full_name, .secret_type_display_name, .created_at] | @tsv'
```

### Dismiss / Update Alerts

```bash
# Dismiss a code scanning alert
gh api "/repos/{owner}/{repo}/code-scanning/alerts/{alert_number}" \
  --method PATCH \
  -f state=dismissed \
  -f dismissed_reason="won't fix" \
  -f dismissed_comment="Reviewed: not exploitable in this context"

# Dismiss a Dependabot alert
gh api "/repos/{owner}/{repo}/dependabot/alerts/{alert_number}" \
  --method PATCH \
  -f state=dismissed \
  -f dismissed_reason="tolerable_risk" \
  -f dismissed_comment="Risk accepted per security review #1234"
```

### Export Security Data for Dashboards

```bash
# Export all code scanning alerts as JSON for a dashboard
gh api --paginate "/repos/{owner}/{repo}/code-scanning/alerts?state=open&per_page=100" \
  > code-scanning-alerts.json

# Export as CSV (using jq)
gh api --paginate "/repos/{owner}/{repo}/code-scanning/alerts?per_page=100" \
  --jq '.[] | [.number, .state, .rule.id, .rule.security_severity_level, .most_recent_instance.location.path, .created_at, .dismissed_at] | @csv' \
  > code-scanning-export.csv
```

### Automate Alert Triage with GitHub Actions

```yaml
# .github/workflows/alert-triage.yml
name: Auto-Triage Security Alerts
on:
  code_scanning_alert:
    types: [appeared_in_branch]
  schedule:
    - cron: "0 9 * * 1" # Weekly digest

jobs:
  triage:
    runs-on: ubuntu-latest
    permissions:
      security-events: write
      issues: write
    steps:
      - name: Auto-dismiss test-file alerts
        env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          # Dismiss alerts found only in test files
          gh api --paginate "/repos/${{ github.repository }}/code-scanning/alerts?state=open&per_page=100" \
            --jq '.[] | select(.most_recent_instance.location.path | test("(test|spec|__tests__)")) | .number' \
          | while read -r NUM; do
            gh api "/repos/${{ github.repository }}/code-scanning/alerts/$NUM" \
              --method PATCH \
              -f state=dismissed \
              -f dismissed_reason="won't fix" \
              -f dismissed_comment="Auto-dismissed: finding in test code"
          done

      - name: Create issue for critical alerts
        env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          CRITICAL=$(gh api "/repos/${{ github.repository }}/code-scanning/alerts?state=open&severity=critical" --jq 'length')
          if [ "$CRITICAL" -gt 0 ]; then
            gh issue create \
              --title "🚨 $CRITICAL critical code scanning alert(s) require attention" \
              --body "Review critical alerts: https://github.com/${{ github.repository }}/security/code-scanning?query=is:open+severity:critical" \
              --label "security,critical"
          fi
```

---

## 7. GitHub Security Overview

The **Security Overview** dashboard (available at the organization and enterprise level) provides a centralized view of GHAS adoption and alert trends across all repositories.

### Accessing Security Overview

- **Organization level:** `https://github.com/orgs/{org}/security`
- **Enterprise level:** `https://github.com/enterprises/{enterprise}/security`

### Key Dashboard Views

| View | What It Shows |
|------|---------------|
| **Risk** | Repositories ranked by number of open alerts (code scanning, Dependabot, secret scanning) |
| **Coverage** | Which repos have each GHAS feature enabled — quickly find gaps in your rollout |
| **CodeQL pull request alerts** | Alert trends for code scanning findings on PRs |
| **Dependabot** | Dependency alert trends, auto-fix rate, and mean time to remediate |
| **Secret scanning** | Secret leak trends and push protection bypass rates |

### Using Security Overview Effectively

1. **Track rollout progress:** Use the **Coverage** view to see which repositories still need GHAS features enabled. Filter by team, topic, or visibility.

2. **Prioritize remediation:** The **Risk** view highlights repos with the most open alerts. Sort by severity to focus on critical findings first.

3. **Monitor trends:** Use date-range filters to see if your alert backlog is growing or shrinking over time.

4. **Export data:** Use the CSV export button or the API (see [Section 6](#6-ghas-api-and-automation)) to pull data into external dashboards like Grafana, Power BI, or Datadog.

5. **Set up alerts:** Combine Security Overview with the `code_scanning_alert`, `dependabot_alert`, and `secret_scanning_alert` webhook events to trigger notifications when new critical alerts appear.

### Filter Syntax

Security Overview supports a filter bar with queries like:

```
# Repos with critical code scanning alerts
is:public severity:critical tool:codeql

# Repos without Dependabot enabled
dependabot:disabled

# Repos owned by a specific team
team:my-org/backend-team is:private
```

---

## 8. Integration with Ticketing Systems

Many organizations need security findings to flow into their existing ticketing systems (Jira, Azure DevOps, ServiceNow, etc.) for tracking and compliance.

### Pattern 1: Webhook-Driven Ticket Creation

GitHub sends webhook events when GHAS alerts are created or updated. Configure a webhook handler to create tickets automatically.

**Supported webhook events:**
- `code_scanning_alert` — fired when CodeQL finds or updates a finding
- `dependabot_alert` — fired when a new dependency vulnerability is detected
- `secret_scanning_alert` — fired when a secret is detected

**Webhook setup:**
1. Go to **Organization → Settings → Webhooks → Add webhook**
2. Set the payload URL to your integration endpoint
3. Select events: `Code scanning alerts`, `Dependabot alerts`, `Secret scanning alerts`
4. Set content type to `application/json`

### Pattern 2: GitHub Actions–Based Integration

Use a GitHub Actions workflow to create tickets when alerts appear:

```yaml
# .github/workflows/jira-sync.yml
name: Sync GHAS Alerts to Jira
on:
  code_scanning_alert:
    types: [appeared_in_branch]

jobs:
  create-ticket:
    if: >
      github.event.alert.rule.security_severity_level == 'critical' ||
      github.event.alert.rule.security_severity_level == 'high'
    runs-on: ubuntu-latest
    steps:
      - name: Create Jira ticket
        uses: atlassian/gajira-create@v3
        with:
          project: SEC
          issuetype: Bug
          summary: |
            [GHAS] ${{ github.event.alert.rule.description }}
          description: |
            **Repository:** ${{ github.repository }}
            **Alert:** #${{ github.event.alert.number }}
            **Rule:** ${{ github.event.alert.rule.id }}
            **Severity:** ${{ github.event.alert.rule.security_severity_level }}
            **File:** ${{ github.event.alert.most_recent_instance.location.path }}
            **Link:** ${{ github.event.alert.html_url }}
          fields: |
            {
              "priority": { "name": "${{ github.event.alert.rule.security_severity_level == 'critical' && 'Highest' || 'High' }}" },
              "labels": ["ghas", "security", "${{ github.event.alert.tool.name }}"]
            }
        env:
          JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
          JIRA_USER_EMAIL: ${{ secrets.JIRA_USER_EMAIL }}
          JIRA_API_TOKEN: ${{ secrets.JIRA_API_TOKEN }}
```

### Pattern 3: Azure DevOps Work Item Creation

```yaml
# .github/workflows/ado-sync.yml
name: Sync GHAS Alerts to Azure DevOps
on:
  code_scanning_alert:
    types: [appeared_in_branch]

jobs:
  create-work-item:
    if: github.event.alert.rule.security_severity_level == 'critical'
    runs-on: ubuntu-latest
    steps:
      - name: Create Azure DevOps work item
        run: |
          curl -s -X POST \
            "https://dev.azure.com/$ADO_ORG/$ADO_PROJECT/_apis/wit/workitems/\$Bug?api-version=7.0" \
            -H "Authorization: Basic $(echo -n ":$ADO_PAT" | base64)" \
            -H "Content-Type: application/json-patch+json" \
            -d '[
              {"op": "add", "path": "/fields/System.Title", "value": "[GHAS] ${{ github.event.alert.rule.description }}"},
              {"op": "add", "path": "/fields/System.Description", "value": "Alert #${{ github.event.alert.number }} in ${{ github.repository }}. Severity: ${{ github.event.alert.rule.security_severity_level }}. <a href=\"${{ github.event.alert.html_url }}\">View alert</a>"},
              {"op": "add", "path": "/fields/Microsoft.VSTS.Common.Priority", "value": 1},
              {"op": "add", "path": "/fields/System.Tags", "value": "ghas;security"}
            ]'
        env:
          ADO_ORG: ${{ secrets.ADO_ORG }}
          ADO_PROJECT: ${{ secrets.ADO_PROJECT }}
          ADO_PAT: ${{ secrets.ADO_PAT }}
```

### Pattern 4: Scheduled Batch Sync

For organizations that prefer batch syncing over real-time, use a scheduled workflow:

```yaml
name: Weekly Security Alert Sync
on:
  schedule:
    - cron: "0 9 * * 1" # Every Monday at 9 AM UTC

jobs:
  sync:
    runs-on: ubuntu-latest
    steps:
      - name: Fetch and sync open alerts
        env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          # Fetch critical/high alerts opened in the last 7 days
          SINCE=$(date -d '7 days ago' -Iseconds)

          gh api --paginate "/repos/${{ github.repository }}/code-scanning/alerts?state=open&per_page=100" \
            --jq ".[] | select(.created_at >= \"$SINCE\") | select(.rule.security_severity_level == \"critical\" or .rule.security_severity_level == \"high\")" \
            > new-alerts.json

          # Process each alert (create tickets, send notifications, etc.)
          jq -c '.[]' new-alerts.json | while read -r alert; do
            echo "Processing alert: $(echo "$alert" | jq -r '.number')"
            # Add your ticketing API call here
          done
```

### Best Practices for Ticketing Integration

| Practice | Why |
|----------|-----|
| Only create tickets for high/critical severity | Avoids ticket fatigue |
| Include the alert URL in the ticket | Enables one-click navigation to the finding |
| Add a deduplication check | Prevents creating duplicate tickets for the same alert |
| Sync alert state back to GitHub | Close alerts when tickets are resolved |
| Use labels/tags consistently | Enables filtering and reporting in your ticketing system |

---

## Next Steps

- Start with the [rollout checklist](rollout-checklist.md) to plan your GHAS adoption
- Review the [FAQ](faq.md) for common questions
- Explore the skill guides in [`.github/skills/`](../.github/skills/) for step-by-step setup instructions
