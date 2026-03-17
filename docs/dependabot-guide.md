# 📦 Dependabot Guide — Practical Dependency Management

This guide covers everything you need to use Dependabot effectively — from initial setup to monorepo strategies and PR volume control.

---

## What Dependabot Does

Dependabot is GitHub's built-in **dependency management tool**. It:

1. **Monitors** your dependency manifests (package.json, requirements.txt, pom.xml, etc.)
2. **Alerts** you when a dependency has a known vulnerability (CVE)
3. **Opens PRs** to update vulnerable or outdated dependencies automatically

Dependabot integrates directly into your GitHub workflow — no external services, no tokens to manage, no CI pipelines to configure.

---

## Three Types of Dependabot

Dependabot provides three distinct capabilities. It's important to understand the difference:

| Capability | What It Does | Requires Config File? | Enabled Where? |
|-----------|-------------|----------------------|----------------|
| 🔔 **Dependabot Alerts** | Notifies you when a dependency has a known vulnerability (CVE) | No | Settings → Code security |
| 🔒 **Security Updates** | Automatically opens PRs to fix *vulnerable* dependencies | No | Settings → Code security |
| 📦 **Version Updates** | Automatically opens PRs to keep dependencies *current* (regardless of vulnerabilities) | **Yes** — `dependabot.yml` | `.github/dependabot.yml` |

### How They Work Together

```
Vulnerability found in lodash@4.17.20
        │
        ▼
   Dependabot Alert created ─────── 🔔 Alert (notification)
        │
        ▼
   Security Update PR opened ────── 🔒 "Bump lodash from 4.17.20 to 4.17.21"
                                        (fixes the CVE)

Meanwhile, on a weekly schedule:
   Version Update PR opened ─────── 📦 "Bump lodash from 4.17.21 to 4.18.0"
                                        (latest version, regardless of CVEs)
```

> 💡 **Recommendation:** Enable all three. Alerts + Security Updates require no config. Version Updates require a `dependabot.yml` but are well worth the effort.

---

## Setting Up dependabot.yml

Create `.github/dependabot.yml` in your repository. Here's an annotated example:

```yaml
# .github/dependabot.yml
# This file configures Dependabot version updates.
# Alerts and security updates are configured in Settings → Code security.

version: 2  # Required — always "2"

updates:
  # ┌──────────────────────────────────────────────────────────────────┐
  # │ Each entry configures updates for one package ecosystem         │
  # │ in one directory                                                │
  # └──────────────────────────────────────────────────────────────────┘

  - package-ecosystem: "npm"        # What package manager to use
    directory: "/"                  # Where to find the manifest file
    schedule:
      interval: "weekly"            # How often to check (daily/weekly/monthly)
      day: "monday"                 # Which day (for weekly)
      time: "09:00"                 # What time (24h format)
      timezone: "America/New_York"  # What timezone
    open-pull-requests-limit: 10    # Max open PRs at once (default: 5)
    reviewers:                      # Who to assign for review
      - "my-team"
    labels:                         # Labels to add to PRs
      - "dependencies"
      - "automated"
    commit-message:                 # How to format commit messages
      prefix: "deps"               # e.g., "deps: Bump lodash from 4.17.20 to 4.17.21"
    groups:                         # Group related updates into single PRs
      production:
        dependency-type: "production"
        update-types:
          - "minor"
          - "patch"
      dev-dependencies:
        dependency-type: "development"
        update-types:
          - "minor"
          - "patch"

  - package-ecosystem: "github-actions"  # Keep your Actions up to date too!
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      actions:
        patterns:
          - "*"
```

---

## Supported Ecosystems

Dependabot supports **26 package ecosystems**:

| Ecosystem | Manifest Files | Notes |
|-----------|---------------|-------|
| `bundler` | `Gemfile`, `Gemfile.lock` | Ruby gems |
| `cargo` | `Cargo.toml`, `Cargo.lock` | Rust crates |
| `composer` | `composer.json`, `composer.lock` | PHP packages |
| `devcontainers` | `devcontainer.json` | Dev container features |
| `docker` | `Dockerfile`, `docker-compose.yml` | Container images |
| `elm` | `elm.json` | Elm packages |
| `github-actions` | `.github/workflows/*.yml` | Action versions |
| `gitsubmodule` | `.gitmodules` | Git submodules |
| `gomod` | `go.mod`, `go.sum` | Go modules |
| `gradle` | `build.gradle`, `build.gradle.kts` | Java/Kotlin (Gradle) |
| `hex` | `mix.exs`, `mix.lock` | Elixir packages |
| `maven` | `pom.xml` | Java/Kotlin (Maven) |
| `npm` | `package.json`, `package-lock.json` | JavaScript/TypeScript |
| `nuget` | `.csproj`, `.fsproj`, `packages.config` | .NET packages |
| `pip` | `requirements.txt`, `setup.py`, `Pipfile` | Python packages |
| `pnpm` | `package.json`, `pnpm-lock.yaml` | JavaScript (pnpm) |
| `pub` | `pubspec.yaml` | Dart/Flutter packages |
| `swift` | `Package.swift` | Swift packages |
| `terraform` | `*.tf` | Terraform providers/modules |
| `uv` | `pyproject.toml`, `uv.lock` | Python (uv) |
| `yarn` | `package.json`, `yarn.lock` | JavaScript (Yarn) |

> 💡 **Tip:** Use `github-actions` ecosystem to keep your CI workflows using the latest (and most secure) versions of actions.

---

## Managing PR Volume

The biggest complaint about Dependabot? **Too many PRs.** Here's how to keep things manageable.

### Strategy 1: Group Related Updates

Group updates so multiple dependency bumps come in a single PR:

```yaml
updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      # All minor and patch updates in one PR
      minor-and-patch:
        update-types:
          - "minor"
          - "patch"

      # Major updates come individually (they may have breaking changes)
      # (no group needed — they're individual by default)

      # Group by pattern
      aws-sdk:
        patterns:
          - "@aws-sdk/*"
          - "aws-*"

      # Group by dependency type
      dev-deps:
        dependency-type: "development"
```

### Strategy 2: Optimize Schedule

Don't check for updates daily if weekly is fine:

```yaml
schedule:
  interval: "weekly"      # Check once a week
  day: "monday"           # Monday morning
  time: "09:00"           # Before the team starts work
  timezone: "US/Eastern"
```

For less critical ecosystems, use monthly:

```yaml
schedule:
  interval: "monthly"     # Good for stable dependencies
```

### Strategy 3: Limit Open PRs

```yaml
open-pull-requests-limit: 5    # Default is 5; reduce if needed
# Set to 0 to temporarily pause version updates
# (security updates are NOT affected by this limit)
```

### Strategy 4: Allow/Ignore Rules

Only update what matters:

```yaml
# Only allow production dependencies
allow:
  - dependency-type: "production"

# Ignore specific dependencies or version ranges
ignore:
  - dependency-name: "lodash"
    versions: [">=5.0.0"]        # Don't bump to v5 yet
  - dependency-name: "aws-sdk"
    update-types: ["version-update:semver-major"]  # No major bumps
```

### Strategy 5: Use Cooldown Periods

Avoid updating to releases that were *just* published (and might be reverted):

```yaml
updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
      cooldown:
        default: 3      # Wait 3 days after a new release
        semver-major: 7  # Wait 7 days for major versions
```

### Putting It All Together

A well-tuned config that minimizes PR noise:

```yaml
version: 2
updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
      time: "09:00"
      timezone: "US/Eastern"
    open-pull-requests-limit: 10
    groups:
      prod-minor-patch:
        dependency-type: "production"
        update-types: ["minor", "patch"]
      dev-minor-patch:
        dependency-type: "development"
        update-types: ["minor", "patch"]
    ignore:
      - dependency-name: "*"
        update-types: ["version-update:semver-major"]
    labels: ["dependencies"]
    commit-message:
      prefix: "deps"
```

> This config: groups minor/patch updates, ignores major bumps (handle manually), runs weekly on Monday, and limits to 10 open PRs.

---

## Private Registries

If your dependencies live in private registries, configure authentication in `dependabot.yml`:

### npm (Private Registry)

```yaml
version: 2
registries:
  npm-private:
    type: npm-registry
    url: https://npm.pkg.github.com    # or your private registry
    token: ${{ secrets.NPM_TOKEN }}     # Stored in repo/org secrets

updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
    registries:
      - npm-private    # Reference the registry defined above
```

### Maven (Private Repository)

```yaml
registries:
  maven-private:
    type: maven-repository
    url: https://maven.example.com/releases
    username: ${{ secrets.MAVEN_USER }}
    password: ${{ secrets.MAVEN_PASS }}

updates:
  - package-ecosystem: "maven"
    directory: "/"
    schedule:
      interval: "weekly"
    registries:
      - maven-private
```

### Python (Private Index)

```yaml
registries:
  pypi-private:
    type: python-index
    url: https://pypi.example.com/simple
    username: ${{ secrets.PYPI_USER }}
    password: ${{ secrets.PYPI_PASS }}

updates:
  - package-ecosystem: "pip"
    directory: "/"
    schedule:
      interval: "weekly"
    registries:
      - pypi-private
```

### NuGet (Private Feed)

```yaml
registries:
  nuget-private:
    type: nuget-feed
    url: https://pkgs.dev.azure.com/my-org/_packaging/my-feed/nuget/v3/index.json
    username: ${{ secrets.NUGET_USER }}
    password: ${{ secrets.NUGET_PAT }}

updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    registries:
      - nuget-private
```

> 🔐 **Important:** Always use GitHub repository or organization secrets (`${{ secrets.* }}`) for credentials. Never hardcode tokens in `dependabot.yml`.

---

## Monorepo Configuration

### Multiple Directories

```yaml
version: 2
updates:
  # Frontend app
  - package-ecosystem: "npm"
    directory: "/apps/frontend"
    schedule:
      interval: "weekly"

  # Backend API
  - package-ecosystem: "pip"
    directory: "/apps/api"
    schedule:
      interval: "weekly"

  # Shared library
  - package-ecosystem: "npm"
    directory: "/packages/shared"
    schedule:
      interval: "weekly"
```

### Glob Patterns

Instead of listing every directory, use globs:

```yaml
version: 2
updates:
  # All npm workspaces
  - package-ecosystem: "npm"
    directories:
      - "/apps/*"
      - "/packages/*"
    schedule:
      interval: "weekly"
    groups:
      all-minor-patch:
        update-types: ["minor", "patch"]
```

### Cross-Directory Grouping

Group updates across multiple directories into single PRs:

```yaml
version: 2
updates:
  - package-ecosystem: "npm"
    directories:
      - "/apps/*"
      - "/packages/*"
    schedule:
      interval: "weekly"
    groups:
      # Single PR for all React-related updates across all workspaces
      react-updates:
        patterns:
          - "react"
          - "react-dom"
          - "@types/react*"
```

---

## Reviewing Dependabot PRs

### What to Check

When a Dependabot PR arrives, look for:

1. **Compatibility score** — Dependabot shows how many other repos updated this dependency without issues
2. **Changelog** — Link to the dependency's changelog (what changed?)
3. **Commit diff** — What actually changed in the dependency
4. **CI results** — Did your tests pass with the new version?
5. **Release notes** — Any breaking changes or deprecations?

### Auto-Merge Low-Risk Updates

Combine Dependabot with GitHub auto-merge for low-risk updates:

```yaml
# .github/workflows/dependabot-auto-merge.yml
name: Auto-merge Dependabot PRs

on: pull_request

permissions:
  contents: write
  pull-requests: write

jobs:
  auto-merge:
    runs-on: ubuntu-latest
    if: github.actor == 'dependabot[bot]'
    steps:
      - name: Fetch Dependabot metadata
        id: metadata
        uses: dependabot/fetch-metadata@v2
        with:
          github-token: "${{ secrets.GITHUB_TOKEN }}"

      - name: Auto-merge minor and patch updates
        if: >
          steps.metadata.outputs.update-type == 'version-update:semver-minor' ||
          steps.metadata.outputs.update-type == 'version-update:semver-patch'
        run: gh pr merge --auto --squash "$PR_URL"
        env:
          PR_URL: ${{ github.event.pull_request.html_url }}
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

> ⚠️ **Note:** Require passing CI checks as a branch protection rule before enabling auto-merge. This ensures auto-merged updates don't break your code.

---

## Common Patterns

### Frontend Application (React/Next.js)

```yaml
version: 2
updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
    groups:
      react:
        patterns: ["react", "react-dom", "next", "@next/*"]
      types:
        patterns: ["@types/*"]
        dependency-type: "development"
      tooling:
        patterns: ["eslint*", "prettier*", "typescript"]
        dependency-type: "development"
      prod-deps:
        dependency-type: "production"
        exclude-patterns: ["react", "react-dom", "next", "@next/*"]
        update-types: ["minor", "patch"]
    labels: ["dependencies", "frontend"]
    commit-message:
      prefix: "deps(frontend)"
```

### Python Service

```yaml
version: 2
updates:
  - package-ecosystem: "pip"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      aws:
        patterns: ["boto3", "botocore", "awscli"]
      testing:
        patterns: ["pytest*", "coverage", "mypy", "ruff"]
        dependency-type: "development"
      minor-patch:
        update-types: ["minor", "patch"]
        exclude-patterns: ["boto3", "botocore", "awscli", "django", "flask"]
    ignore:
      - dependency-name: "django"
        update-types: ["version-update:semver-major"]
    labels: ["dependencies", "python"]
    commit-message:
      prefix: "deps(python)"

  - package-ecosystem: "docker"
    directory: "/"
    schedule:
      interval: "weekly"
    labels: ["dependencies", "docker"]
```

### Java Microservice

```yaml
version: 2
updates:
  - package-ecosystem: "maven"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      spring:
        patterns: ["org.springframework*"]
      testing:
        patterns:
          - "org.junit*"
          - "org.mockito*"
          - "org.assertj*"
      jackson:
        patterns: ["com.fasterxml.jackson*"]
      minor-patch:
        update-types: ["minor", "patch"]
        exclude-patterns:
          - "org.springframework*"
          - "com.fasterxml.jackson*"
    labels: ["dependencies", "java"]
    commit-message:
      prefix: "deps(java)"

  - package-ecosystem: "docker"
    directory: "/"
    schedule:
      interval: "monthly"

  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      actions:
        patterns: ["*"]
```

---

## Troubleshooting

### Issue 1: Dependabot PRs Fail CI

**Cause:** The updated dependency introduces breaking changes or is incompatible.

**Fix:**
- Check the changelog and release notes for migration instructions
- Pin the current version using `ignore` rules until you can update
- If it's a major version bump, handle it manually with proper testing

```yaml
ignore:
  - dependency-name: "problematic-package"
    versions: [">=3.0.0"]
```

### Issue 2: Dependabot Doesn't Detect My Manifest

**Cause:** The manifest file is in a non-standard location or format.

**Fix:**
- Ensure the `directory` in `dependabot.yml` points to the folder *containing* the manifest
- Check that the manifest filename is standard (e.g., `package.json`, not `deps.json`)
- For monorepos, use `directories` with glob patterns

### Issue 3: Too Many PRs

**Cause:** Default configuration opens individual PRs for every dependency.

**Fix:** See [Managing PR Volume](#managing-pr-volume) above. Key strategies:
- Use `groups` to batch updates
- Set `open-pull-requests-limit`
- Use `ignore` for non-critical major bumps
- Schedule less frequently (`weekly` or `monthly`)

### Issue 4: Private Registry Authentication Fails

**Cause:** Secrets are misconfigured or the registry URL is incorrect.

**Fix:**
- Verify the secret exists in **Settings** → **Secrets and variables** → **Dependabot**
- Note: Dependabot uses its own secret store, separate from Actions secrets
- Check the registry URL is correct (include `/simple` for PyPI, `/v3/index.json` for NuGet)
- Test the credentials manually first

### Issue 5: Security Updates Are Disabled

**Cause:** Security updates may be disabled at the org level or the feature isn't enabled.

**Fix:**
- Check **Settings** → **Code security** → **Dependabot security updates** is enabled
- Organization admins can override repo-level settings
- `open-pull-requests-limit: 0` does *not* affect security updates (only version updates)
- Ensure **Dependabot alerts** are also enabled (security updates depend on them)

---

## 📖 Additional Resources

- **[Dependabot Skill Reference](../.github/skills/dependabot/)** — Complete YAML reference, example configs, PR commands
- **[Getting Started](getting-started.md)** — Enable all GHAS features
- **[GitHub Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)**
- **[dependabot.yml Reference](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)**
