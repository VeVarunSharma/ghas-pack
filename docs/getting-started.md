# 🚀 Getting Started with GitHub Advanced Security (GHAS)

Welcome! This guide walks your team through enabling GitHub Advanced Security from zero to fully protected in under 30 minutes.

---

## 🔐 What is GHAS?

GitHub Advanced Security (GHAS) is a suite of security tools built directly into GitHub. It rests on **three pillars**:

| Pillar | What It Does | Key Feature |
|--------|-------------|-------------|
| 🔍 **Code Scanning (CodeQL)** | Finds vulnerabilities in your source code using semantic analysis | Catches SQL injection, XSS, and hundreds of other vulnerability types |
| 📦 **Dependabot (Supply Chain Security)** | Monitors your dependencies for known vulnerabilities and keeps them updated | Automated PRs to fix vulnerable or outdated packages |
| 🔑 **Secret Scanning** | Detects credentials, tokens, and API keys accidentally committed to your repo | Push protection blocks secrets *before* they reach the remote |

Together, these tools provide **shift-left security** — catching issues early in your development workflow rather than in production.

---

## ✅ Prerequisites

Before you begin, make sure you have:

- [ ] **GitHub Enterprise Cloud or a public repository** — GHAS features are included for public repos; Enterprise Cloud (with a GHAS license) is required for private/internal repos
- [ ] **Repository admin access** — You need admin permissions to enable security features in Settings
- [ ] **A repository with code** — At least one supported language for Code Scanning (JavaScript, Python, Java, C#, Go, C/C++, Ruby, Rust, Swift, Kotlin)
- [ ] **Basic familiarity with GitHub Actions** — Code Scanning runs via Actions workflows (though default setup requires zero configuration)

> 💡 **Tip:** If you're on GitHub Enterprise Server, GHAS is available from version 3.0+, but some features may require newer versions.

---

## Step 1: Enable Secret Scanning 🔑

Secret scanning is the fastest win — it immediately starts detecting leaked credentials across your repository's entire history.

### Where to Find It

1. Navigate to your repository on GitHub
2. Click **Settings** → **Code security** (in the left sidebar under "Security")

<!-- Screenshot placeholder: Repository Settings page showing the "Code security" section in the left sidebar with Secret Scanning toggle visible -->

### What to Enable

| Setting | Recommended | Why |
|---------|-------------|-----|
| **Secret scanning** | ✅ On | Scans your repo history, issues, PRs, and discussions for over 200 secret types |
| **Push protection** | ✅ On | Blocks pushes that contain secrets *before* they reach the remote |
| **Validity checks** | ✅ On | Checks with the secret provider whether detected secrets are still active |
| **Non-provider patterns** | ✅ On | Detects generic secrets like passwords and database connection strings |
| **Scan for generic secrets using Copilot** | ✅ On | AI-powered detection of unstructured secrets |

### Quick Steps

1. In **Code security**, find the **Secret scanning** section
2. Click **Enable** to turn on secret scanning
3. Check the box for **Push protection**
4. Enable additional detection features (validity checks, non-provider patterns)
5. Click **Save changes**

<!-- Screenshot placeholder: Secret scanning settings panel with all toggles enabled, showing push protection checkbox -->

> 🎉 **Done!** Secret scanning is now active. Any existing secrets in your repo will appear under **Security** → **Secret scanning alerts** within a few minutes.

📖 **Deep dive:** See the [Secret Scanning Guide](secret-scanning-guide.md) for custom patterns, push protection workflows, and remediation steps.

---

## Step 2: Enable Dependabot 📦

Dependabot protects your software supply chain by alerting you to vulnerable dependencies and automatically creating PRs to fix them.

### Enable Dependabot Alerts

1. In **Settings** → **Code security**, find the **Dependabot** section
2. Enable **Dependabot alerts** — GitHub will scan your dependency manifests and notify you of known vulnerabilities
3. Enable **Dependabot security updates** — Dependabot will automatically open PRs to fix vulnerable dependencies

<!-- Screenshot placeholder: Dependabot settings showing toggles for alerts, security updates, and version updates -->

### Enable Dependabot Version Updates

Version updates keep your dependencies current (not just secure). This requires a configuration file.

1. Enable **Dependabot version updates** in Settings
2. Create `.github/dependabot.yml` in your repository:

```yaml
# .github/dependabot.yml
version: 2
updates:
  # Keep GitHub Actions up to date
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"

  # Example: Keep npm dependencies up to date
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      # Group minor/patch updates to reduce PR noise
      production-dependencies:
        dependency-type: "production"
        update-types:
          - "minor"
          - "patch"
      dev-dependencies:
        dependency-type: "development"
        update-types:
          - "minor"
          - "patch"
    open-pull-requests-limit: 10
```

> 💡 **Tip:** Start with `weekly` updates and adjust once you see the volume of PRs. Use `groups` to batch related updates into single PRs.

📖 **Deep dive:** See the [Dependabot Guide](dependabot-guide.md) for monorepo configs, private registries, and PR management strategies.

---

## Step 3: Enable Code Scanning 🔍

Code Scanning uses CodeQL (GitHub's semantic analysis engine) to find security vulnerabilities in your source code.

### Default Setup vs Advanced Setup

Choose the right setup for your needs:

```
                    Do you need Code Scanning?
                              │
                              ▼
              ┌───────────────────────────────┐
              │  Do you need custom queries,  │
              │  specific triggers, or manual │
              │  build steps?                 │
              └───────────┬───────────────────┘
                    │              │
                   No             Yes
                    │              │
                    ▼              ▼
           ┌──────────────┐  ┌──────────────┐
           │ Default Setup│  │Advanced Setup│
           │  (start here)│  │              │
           └──────────────┘  └──────────────┘
```

| | Default Setup | Advanced Setup |
|---|---|---|
| **Configuration** | Zero — GitHub manages everything | You manage a workflow YAML file |
| **Language detection** | Automatic | You specify languages |
| **Custom queries** | ❌ Not supported | ✅ Full control |
| **Build steps** | Automatic | You define them |
| **Best for** | Most repositories, getting started | Monorepos, compiled languages with custom builds, custom security rules |

### Enable Default Setup (Recommended Start)

1. Go to **Settings** → **Code security**
2. Find **Code scanning** and click **Set up** → **Default**
3. Review the detected languages and click **Enable CodeQL**

<!-- Screenshot placeholder: Code scanning default setup dialog showing detected languages (e.g., JavaScript, Python) with "Enable CodeQL" button -->

That's it! GitHub will automatically:
- Detect your languages
- Run analysis on every push and pull request
- Report findings in the **Security** tab

### When to Switch to Advanced Setup

Consider switching to advanced setup when you need:
- Custom CodeQL queries or query packs
- Specific workflow triggers or schedules
- Manual build steps for compiled languages (Java, C#, C++)
- Per-component analysis in a monorepo
- Path filtering to scan specific directories

📖 **Deep dive:** See the [CodeQL Guide](codeql-guide.md) for advanced workflows, custom queries, monorepo configs, and performance tuning.

---

## Step 4: Review Your Security Dashboard 📊

Now that everything is enabled, let's see what GitHub found.

### Where to Find Alerts

1. Navigate to your repository
2. Click the **Security** tab
3. You'll see three alert categories in the sidebar:

<!-- Screenshot placeholder: Security tab overview showing sidebar with "Code scanning", "Dependabot", and "Secret scanning" alert categories, each with a count badge -->

| Section | What You'll See |
|---------|----------------|
| **Code scanning alerts** | Vulnerabilities found in your source code by CodeQL |
| **Dependabot alerts** | Known vulnerabilities in your dependencies (CVEs) |
| **Secret scanning alerts** | Credentials and tokens found in your repo |

### How to Triage Alerts

Follow this priority order:

1. **🔴 Secret scanning alerts** — Rotate any active credentials *immediately*
2. **🟠 Critical/High Dependabot alerts** — Update vulnerable dependencies, especially those with known exploits
3. **🟡 Critical/High Code scanning alerts** — Fix security vulnerabilities in your code
4. **🔵 Medium/Low alerts** — Address during regular development cycles

> 💡 **Tip:** Use the **Security overview** at the organization level (**Organization** → **Security** → **Overview**) to see alerts across all repositories.

### Organization Security Overview

If you have organization admin access:

1. Go to your **Organization** page
2. Click **Security** → **Overview**
3. View aggregated risk across all repositories
4. Filter by severity, tool, or team

<!-- Screenshot placeholder: Organization security overview dashboard showing a grid of repositories with risk indicators (red/yellow/green) -->

---

## 📁 What's in This Repo

This repository contains everything you need to learn, configure, and customize GHAS:

| Path | Description |
|------|-------------|
| 📖 [`docs/`](.) | You are here! Practical guides for each GHAS feature |
| ├── [`getting-started.md`](getting-started.md) | This quick-start guide |
| ├── [`codeql-guide.md`](codeql-guide.md) | CodeQL deep dive — setup, alerts, custom queries, performance |
| ├── [`dependabot-guide.md`](dependabot-guide.md) | Dependabot deep dive — config, ecosystems, PR management |
| └── [`secret-scanning-guide.md`](secret-scanning-guide.md) | Secret scanning deep dive — push protection, custom patterns |
| 🎓 [`.github/skills/`](../.github/skills/) | Detailed reference material and procedural skill guides |
| ├── [`codeql/`](../.github/skills/codeql/) | CodeQL workflow config, CLI commands, SARIF output, troubleshooting |
| ├── [`dependabot/`](../.github/skills/dependabot/) | `dependabot.yml` reference, example configs, PR commands |
| └── [`secret-scanning/`](../.github/skills/secret-scanning/) | Alerts & remediation, custom patterns, push protection |
| 📝 [`examples/`](../examples/) | Example code and configurations |
| ├── [`configs/`](../examples/configs/) | Sample CodeQL, Dependabot, and workflow configs |
| ├── [`custom-queries/`](../examples/custom-queries/) | Custom CodeQL queries for JavaScript and Python |
| ├── [`secure/`](../examples/secure/) | Secure code examples (C#, Java, JavaScript, Python) |
| └── [`vulnerable/`](../examples/vulnerable/) | Vulnerable code examples for testing detection |
| 🔒 [`SECURITY.md`](../SECURITY.md) | Security policy and vulnerability reporting process |

---

## 🔗 Next Steps

Now that GHAS is enabled, deepen your knowledge with these guides:

1. **[CodeQL Guide](codeql-guide.md)** — Learn about alert types, custom queries, monorepo configuration, and performance optimization
2. **[Dependabot Guide](dependabot-guide.md)** — Master dependency management with grouped updates, private registries, and PR volume control
3. **[Secret Scanning Guide](secret-scanning-guide.md)** — Set up custom patterns, configure push protection bypass workflows, and establish remediation processes

### Additional Resources

- 📚 [GitHub Advanced Security Documentation](https://docs.github.com/en/get-started/learning-about-github/about-github-advanced-security)
- 🎓 [GitHub Skills](https://skills.github.com/) — Interactive courses for GHAS features
- 💬 [GitHub Community Discussions](https://github.com/orgs/community/discussions) — Ask questions and share experiences
- 📝 [GitHub Changelog](https://github.blog/changelog/) — Stay up to date on new GHAS features

---

> 📣 **Need help?** Open an issue in this repository or reach out to your GitHub account team for enterprise support.
