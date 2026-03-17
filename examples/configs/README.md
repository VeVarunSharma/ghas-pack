# GHAS Configuration Templates

Copy-paste-ready configuration templates for GitHub Advanced Security features.
Pick the template that matches your use case, copy it into your repository, and customize as needed.

## How to Use

1. Browse the templates below and find the one closest to your needs.
2. Copy the file into your repository at the correct path:
   - **Dependabot**: `.github/dependabot.yml`
   - **CodeQL workflows**: `.github/workflows/<name>.yml`
   - **CodeQL configs**: `.github/codeql/<name>.yml`
   - **Secret scanning**: `.github/secret_scanning.yml`
   - **Other workflows**: `.github/workflows/<name>.yml`
3. Replace placeholder values (marked with `# TODO` comments).
4. Commit and push.

---

## Table of Contents

### Dependabot

| Template | Description |
|----------|-------------|
| [dependabot/basic.yml](dependabot/basic.yml) | Single-ecosystem (npm) with weekly updates — the simplest starting point. |
| [dependabot/monorepo.yml](dependabot/monorepo.yml) | Multiple directories and ecosystems for monorepo setups. |
| [dependabot/grouped-updates.yml](dependabot/grouped-updates.yml) | Group dependency PRs by type, pattern, or security criticality. |
| [dependabot/enterprise.yml](dependabot/enterprise.yml) | Full enterprise config with private registries, reviewers, and multiple ecosystems. |

### CodeQL

| Template | Description |
|----------|-------------|
| [codeql/basic-workflow.yml](codeql/basic-workflow.yml) | Minimal single-language (JavaScript) CodeQL workflow. |
| [codeql/multi-language-workflow.yml](codeql/multi-language-workflow.yml) | Matrix strategy for JS, Python, Java, and C# with custom builds. |
| [codeql/codeql-config-basic.yml](codeql/codeql-config-basic.yml) | Basic CodeQL config using `security-extended` queries. |
| [codeql/codeql-config-extended.yml](codeql/codeql-config-extended.yml) | Extended config with custom query packs, path filters, and severity tuning. |
| [codeql/third-party-sarif.yml](codeql/third-party-sarif.yml) | Upload SARIF results from third-party scanners (Semgrep, Snyk, Trivy). |

### Secret Scanning

| Template | Description |
|----------|-------------|
| [secret-scanning/secret_scanning.yml](secret-scanning/secret_scanning.yml) | Path exclusion config for secret scanning. |
| [secret-scanning/custom-patterns.md](secret-scanning/custom-patterns.md) | Guide and examples for defining custom secret patterns. |

### Workflows

| Template | Description |
|----------|-------------|
| [workflows/dependency-review.yml](workflows/dependency-review.yml) | PR-time dependency review with license checks and severity gating. |
| [workflows/scorecard.yml](workflows/scorecard.yml) | OSSF Scorecard supply-chain security analysis. |

---

## Tips

- **Start simple.** Use `basic` templates first, then layer on complexity.
- **Keep secrets in GitHub Secrets.** Never hard-code tokens in config files.
- **Test in a branch.** Push config changes to a feature branch first to validate.
- **Check the docs.** GitHub's official docs are the source of truth:
  - [Dependabot configuration](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)
  - [CodeQL analysis](https://docs.github.com/en/code-security/code-scanning/creating-an-advanced-setup-for-code-scanning/customizing-your-advanced-setup-for-code-scanning)
  - [Secret scanning](https://docs.github.com/en/code-security/secret-scanning)
