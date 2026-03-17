# 🔑 Secret Scanning Guide — Protecting Credentials in Your Code

This guide covers everything you need to detect and prevent credential leaks — from enabling secret scanning to custom patterns and remediation workflows.

---

## What Secret Scanning Does

Secret scanning **detects credentials, tokens, API keys, and other secrets** that have been accidentally committed to your repository. It scans:

- 📂 **Git history** — Every commit, including old ones
- 📝 **Issues, PRs, and discussions** — Including comments
- 📖 **Wikis** — All wiki pages
- 🗂️ **GitHub Actions logs** — Workflow run output

### How Detection Works

GitHub's secret scanning uses three detection methods:

| Method | What It Detects | How |
|--------|----------------|-----|
| **Partner patterns** | Tokens from 200+ service providers (AWS, Azure, Stripe, etc.) | GitHub partners with providers who define their token formats |
| **Custom patterns** | Your organization's internal tokens and API keys | You define regex patterns |
| **Generic detection (AI)** | Unstructured secrets (passwords, connection strings) | Copilot-powered AI detection |

When a **partner pattern** is detected, GitHub can automatically notify the service provider, who may revoke the token — often before you even see the alert.

---

## Enabling Secret Scanning

### For Public Repositories

Secret scanning is **enabled by default** for all public repositories on GitHub.com. No action needed.

### For Private/Internal Repositories

Requires GitHub Advanced Security (GHAS) license.

1. Go to **Settings** → **Code security**
2. Under **Secret scanning**, click **Enable**
3. Enable additional features:

| Feature | What to Enable | Why |
|---------|---------------|-----|
| **Secret scanning** | ✅ On | Core detection of 200+ secret types |
| **Push protection** | ✅ On | Block secrets *before* they're pushed |
| **Validity checks** | ✅ On | Check if detected secrets are still active |
| **Non-provider patterns** | ✅ On | Detect generic secrets (passwords, connection strings) |
| **Scan for generic secrets using Copilot** | ✅ On | AI-powered detection of unstructured secrets |

<!-- Screenshot placeholder: Secret scanning settings panel in Code Security, showing all feature toggles in the enabled state -->

### Organization-Wide Enablement

Organization owners can enable secret scanning for all repositories at once:

1. Go to **Organization** → **Settings** → **Code security**
2. Enable secret scanning for all repositories (new and existing)
3. Configure push protection at the org level

---

## Push Protection

Push protection is a **preventative control** that blocks git pushes containing secrets *before* they reach the remote repository.

### How It Works

```
Developer runs: git push origin main
         │
         ▼
┌─────────────────────────────────────┐
│  GitHub scans the push for secrets  │
└──────────────┬──────────────────────┘
               │
        ┌──────┴──────┐
        │Secret found?│
        └──────┬──────┘
         │           │
        Yes          No
         │           │
         ▼           ▼
┌─────────────┐  ┌────────────────┐
│ Push blocked │  │ Push succeeds  │
│ Error shown  │  │ (no action)    │
│ with details │  └────────────────┘
└──────┬──────┘
       │
       ▼
┌──────────────────────────────────────────┐
│          Developer has 3 options:         │
├──────────────────────────────────────────┤
│                                          │
│  Option A: Remove the secret             │
│  ├─ Remove from code                     │
│  ├─ git commit --amend (or rebase)       │
│  └─ git push (succeeds)                 │
│                                          │
│  Option B: Bypass (if allowed)           │
│  ├─ Provide a reason                     │
│  ├─ Push succeeds with bypass            │
│  └─ Alert created for review             │
│                                          │
│  Option C: Request bypass (delegated)    │
│  ├─ Submit bypass request                │
│  ├─ Reviewer approves or denies          │
│  └─ Push succeeds if approved            │
│     (request expires after 7 days)       │
│                                          │
└──────────────────────────────────────────┘
```

### What the Developer Sees

When a push is blocked, the developer sees an error like:

```
remote: error: GH013: Repository rule violations found for refs/heads/main.
remote:
remote: - GITHUB PUSH PROTECTION
remote:   —————————————————————————————————————————
remote:     Resolve the following violations before pushing again
remote:
remote:     — Push cannot contain secrets —
remote:
remote:
remote:      (?) To push, remove secret from commit(s) or follow this URL to allow the secret.
remote:
remote:      https://github.com/my-org/my-repo/security/secret-scanning/unblock-secret/...
remote:
remote:      — locations —
remote:        - commit: abc1234
remote:          path: src/config.js:3
remote:          secret type: GitHub Personal Access Token
```

### Resolving a Blocked Push

**Option A: Remove the Secret (Recommended)**

```bash
# 1. Remove the secret from your code
# 2. If the secret is in the latest commit:
git add .
git commit --amend --no-edit
git push

# If the secret is in an earlier commit:
git rebase -i HEAD~3   # Go back to the commit with the secret
# Mark the commit as "edit", remove the secret, continue rebase
git push --force-with-lease
```

**Option B: Bypass Push Protection**

If you determine the detection is a false positive or the secret is safe (e.g., a test fixture):

1. Click the bypass URL shown in the error message
2. Select a reason: *"It's used in tests"*, *"It's a false positive"*, or *"I'll fix it later"*
3. Push again — it will succeed

> ⚠️ **Note:** Bypassed secrets create an alert that security teams can review. The bypass window is 3 hours.

---

## Alert Types

### Provider Alerts (Partner Program)

- Detected using patterns from 200+ service providers
- GitHub may automatically notify the provider
- The provider may revoke the token
- Examples: AWS keys, GitHub tokens, Stripe API keys, Azure connection strings

### Custom Pattern Alerts

- Detected using regex patterns you define
- Scoped to repository, organization, or enterprise
- Not shared with any external service

### Generic Alerts

- AI-powered detection of unstructured secrets
- Detects passwords, connection strings, and other non-standard formats
- Higher false positive rate than provider patterns
- Limited to 5,000 generic alerts per repository

### Validity Checks

For supported providers, GitHub checks whether detected secrets are still active:

| Status | Meaning | Action |
|--------|---------|--------|
| **Active** 🔴 | The secret is valid and could be used | Rotate immediately |
| **Inactive** ⚪ | The secret has been revoked or expired | Verify and dismiss |
| **Unknown** 🟡 | Couldn't determine validity | Investigate and rotate if unsure |

---

## Custom Patterns

### When You Need Them

Custom patterns are valuable when your organization uses:

- Internal API keys with a specific format (e.g., `MYCO_sk_live_[a-zA-Z0-9]{32}`)
- Database connection strings with custom prefixes
- Internal service tokens not covered by GitHub's partner program
- Legacy credential formats unique to your systems

### Step-by-Step: Create a Custom Pattern

#### 1. Define Your Pattern

Identify the format of the secret. For example, an internal API key that looks like: `myco_api_k3y_abc123def456abc123def456abc123de`

#### 2. Write the Regex

| Component | Purpose | Example |
|-----------|---------|---------|
| **Secret format** (required) | The main pattern to match | `myco_api_[a-z0-9]{3}_[a-f0-9]{32}` |
| **Before secret** (optional) | Context that appears before the secret | `(?:api[_-]?key|token)\s*[=:]\s*` |
| **After secret** (optional) | Context that appears after the secret | `\s*(?:[;,\n"']|$)` |

#### 3. Create the Pattern in GitHub

**Repository-level:**
1. Go to **Settings** → **Code security** → **Secret scanning**
2. Click **New pattern**
3. Fill in the pattern details:

```
Pattern name:        Internal API Key
Secret format:       myco_api_[a-z0-9]{3}_[a-f0-9]{32}
Before secret:       (?i)(?:api[_-]?key|token|secret)\s*[=:]\s*["']?
After secret:        ["']?\s*[;,\n})]?
Sample test string:  api_key = "myco_api_k3y_abc123def456abc123def456abc123de"
```

**Organization-level:**
1. Go to **Organization** → **Settings** → **Code security** → **Secret scanning**
2. Click **New pattern**
3. Patterns defined here apply to all repos in the org

#### 4. Dry Run Testing

Before publishing, **always run a dry run**:

1. After defining the pattern, click **Save and dry run**
2. GitHub scans up to 1,000 results across the selected scope
3. Review the results for false positives and missed detections
4. Adjust the regex as needed
5. Once satisfied, click **Publish pattern**

### Regex Tips for Common Patterns

```regex
# AWS-style key (20 uppercase alphanumeric characters)
[A-Z0-9]{20}

# Hex-encoded token (64 characters)
[a-f0-9]{64}

# Base64-encoded token (at least 20 characters)
[A-Za-z0-9+/]{20,}={0,2}

# JWT token
eyJ[A-Za-z0-9_-]+\.eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+

# Connection string with server and password
Server=[^;]+;.*Password=[^;]+

# API key with prefix
(?:sk|pk)_(?:live|test)_[A-Za-z0-9]{24,}
```

### Enable Push Protection for Custom Patterns

After publishing a custom pattern:

1. Go to **Settings** → **Code security** → **Secret scanning**
2. Find your custom pattern in the list
3. Click **Enable push protection** for that pattern
4. New pushes containing matches will be blocked

---

## Delegated Bypass

Delegated bypass gives security teams control over who can bypass push protection and adds an approval workflow.

### How It Works

```
Developer push blocked
        │
        ▼
Developer requests bypass
(provides justification)
        │
        ▼
┌──────────────────────────┐
│ Request sent to reviewers │
│ (bypass list members,     │
│  security managers)       │
└───────────┬──────────────┘
            │
     ┌──────┴──────┐
     │             │
  Approved       Denied
     │             │
     ▼             ▼
Push succeeds   Developer must
with alert      remove secret
created         before pushing
     │
     │
  Request expires
  after 7 days
  if not acted on
```

### Setting Up Delegated Bypass

1. Go to **Settings** → **Code security** → **Push protection**
2. Under **Who can bypass push protection**, select **Specific roles or teams**
3. Add the teams or roles that are allowed to bypass (or approve bypasses)
4. Enable **Require reviewer approval** to add an approval step

### Who Can Always Bypass

These roles can always bypass push protection (regardless of delegated bypass settings):

- Organization owners
- Security managers
- Users in the bypass list

---

## Remediation Workflow

When a secret scanning alert fires, follow this workflow:

### Step 1: Rotate the Credential 🔄

**This is the most important step.** Do this *immediately*, before anything else.

| Secret Type | How to Rotate |
|------------|---------------|
| GitHub PAT | **Settings** → **Developer settings** → **Personal access tokens** → Generate new token |
| AWS Key | AWS Console → IAM → Security credentials → Create new access key |
| Azure Secret | Azure Portal → App registrations → Certificates & secrets → New client secret |
| Database Password | Change via your database management tool |
| API Key (third-party) | Visit the provider's dashboard → Generate new key |

### Step 2: Revoke the Old Credential ❌

After deploying the new credential:

1. Delete or deactivate the old credential at the provider
2. Verify your services still work with the new credential
3. Check logs for any unauthorized use of the old credential

### Step 3: Update Your Application 🔧

Replace the old credential in your application:

```bash
# Use environment variables (recommended)
export DATABASE_URL="new-connection-string"

# Or update your secrets manager
gh secret set DATABASE_URL --body "new-connection-string"
```

### Step 4: Dismiss the Alert ✅

1. Go to **Security** → **Secret scanning alerts**
2. Find the alert
3. Click **Close as** → Select reason:
   - **Revoked** — You've rotated and revoked the credential
   - **False positive** — The detection was incorrect
   - **Used in tests** — The secret is intentionally in test fixtures

> ⚠️ **Important:** Never dismiss an alert without rotating the credential first (unless it's a confirmed false positive). Once a secret is in Git history, it should be considered compromised.

---

## Best Practices

### 1. Use Environment Variables and Secrets Managers

Never hardcode secrets in source code. Instead:

```javascript
// ❌ NEVER DO THIS
const apiKey = "sk_live_abc123def456";

// ✅ DO THIS — use environment variables
const apiKey = process.env.API_KEY;
```

Store secrets in:
- **GitHub Actions secrets** — For CI/CD workflows (`${{ secrets.API_KEY }}`)
- **Azure Key Vault**, **AWS Secrets Manager**, **HashiCorp Vault** — For production applications
- **`.env` files** — For local development only (always in `.gitignore`)

### 2. Configure .gitignore for Secret Files

Prevent common secret-containing files from being committed:

```gitignore
# Environment files
.env
.env.local
.env.*.local
.env.production

# Key files
*.pem
*.key
*.p12
*.pfx
*.jks

# Configuration files that may contain secrets
local.settings.json
appsettings.Development.json
secrets.yml
secrets.yaml
credentials.json
service-account.json

# IDE configuration
.idea/
.vscode/settings.json

# Terraform state (may contain secrets)
*.tfstate
*.tfstate.backup
```

### 3. Pre-Commit Hooks as Additional Protection

Add a local safety net with pre-commit hooks:

```yaml
# .pre-commit-config.yaml
repos:
  - repo: https://github.com/gitleaks/gitleaks
    rev: v8.18.0
    hooks:
      - id: gitleaks
```

Install and use:

```bash
# Install pre-commit
pip install pre-commit

# Install hooks in your repo
pre-commit install

# Now gitleaks runs on every commit
git commit -m "my changes"
# → gitleaks scans staged files before the commit is created
```

> 💡 Pre-commit hooks are a **defense-in-depth** measure. They complement (not replace) GitHub's push protection because they catch secrets before they even enter local Git history.

### 4. Exclude Known Test Fixtures

If you have test files with intentional fake secrets, configure exclusions:

```yaml
# .github/secret_scanning.yml
paths-ignore:
  - "test/**"
  - "tests/**"
  - "**/*_test.go"
  - "**/*.test.js"
  - "**/*.test.ts"
  - "docs/examples/**"
  - "fixtures/**"
```

> ⚠️ **Caution:** Use path exclusions sparingly. Excluding too many paths could let real secrets slip through.

### 5. Regular Audits

- Review open secret scanning alerts weekly
- Check validity status of detected secrets
- Review and update custom patterns quarterly
- Audit bypass activity for push protection

---

## Troubleshooting

### Issue 1: False Positive — Test Credential Detected

**Cause:** A fake or intentionally included secret in test files triggers an alert.

**Fix:**
- Add the test directory to `.github/secret_scanning.yml` `paths-ignore`
- Dismiss the alert as **"Used in tests"**
- Consider using obviously fake values in tests (e.g., `FAKE_KEY_DO_NOT_USE_000000`)

### Issue 2: Push Blocked but the Secret Is Intentional

**Cause:** Push protection flagged a secret that you intentionally want to commit (e.g., a public example).

**Fix:**
- Click the bypass URL in the error message
- Select an appropriate reason
- If delegated bypass is enabled, submit a bypass request
- Consider moving the example to use a placeholder instead

### Issue 3: Secret Scanning Not Finding Known Secrets

**Cause:** The secret format isn't covered by built-in patterns or custom patterns.

**Fix:**
- Check if the secret type is in GitHub's [supported secret list](https://docs.github.com/en/code-security/secret-scanning/introduction/supported-secret-scanning-patterns)
- Create a custom pattern for your specific secret format
- Enable **non-provider patterns** and **Copilot generic detection** in Settings
- Verify the file isn't in a `paths-ignore` exclusion

### Issue 4: Too Many Generic Secret Alerts

**Cause:** AI-powered generic detection can have a higher false positive rate.

**Fix:**
- Review and dismiss false positives with appropriate reasons (this improves future detection)
- Use `paths-ignore` to exclude files that commonly trigger false positives (config examples, documentation)
- Generic alerts are capped at 5,000 per repository

### Issue 5: Partner Alert but Token Already Revoked

**Cause:** GitHub detected a token in Git history that was previously rotated.

**Fix:**
- Dismiss the alert as **"Revoked"** after confirming the token is truly inactive
- Check the validity status — if it shows "Inactive", you're safe
- Consider cleaning Git history with `git filter-repo` to remove the secret from all commits (optional but recommended for compliance)

```bash
# Remove a secret from Git history (use with caution!)
pip install git-filter-repo
git filter-repo --replace-text replacements.txt
# Where replacements.txt contains: OLD_SECRET==>***REMOVED***
git push --force
```

> ⚠️ **Warning:** Force-pushing rewrites history. Coordinate with your team before doing this on shared branches.

---

## 📖 Additional Resources

- **[Secret Scanning Skill Reference](../.github/skills/secret-scanning/)** — Detailed reference for alerts, remediation, custom patterns, and push protection
- **[Getting Started](getting-started.md)** — Enable all GHAS features
- **[GitHub Secret Scanning Documentation](https://docs.github.com/en/code-security/secret-scanning)**
- **[Supported Secret Types](https://docs.github.com/en/code-security/secret-scanning/introduction/supported-secret-scanning-patterns)** — Full list of detected patterns
