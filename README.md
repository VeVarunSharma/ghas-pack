# 🛡️ GHAS-Pack

### GitHub Advanced Security Starter Kit

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-passing-brightgreen?logo=github)](../../actions)
[![CodeQL](https://img.shields.io/badge/CodeQL-enabled-blue?logo=github)](../../security/code-scanning)

**GHAS-Pack** is a comprehensive, ready-to-deploy starter kit for teams adopting [GitHub Advanced Security (GHAS)](https://docs.github.com/en/get-started/learning-about-github/about-github-advanced-security). It bundles live security configurations, vulnerable and secure code examples across multiple languages, reusable configuration templates, and step-by-step documentation — everything a security champion or engineering team needs to evaluate, pilot, and roll out GHAS across an organization.

---

## 🔐 What is GHAS?

GitHub Advanced Security is a suite of application security tools built directly into the GitHub developer workflow. It enables teams to find and fix vulnerabilities before they reach production, without leaving their existing development environment.

GHAS is built on three pillars:

| Pillar | Description |
|---|---|
| 🔍 **Code Scanning (CodeQL)** | Static analysis engine that finds security vulnerabilities and coding errors in your source code using semantic code analysis. |
| 📦 **Supply Chain Security (Dependabot)** | Automated dependency updates and vulnerability alerts that keep your open-source dependencies secure and up to date. |
| 🔐 **Secret Scanning** | Detects secrets (API keys, tokens, credentials) accidentally committed to your repository and prevents them from being pushed. |

---

## ⚡ Quick Start

```text
1. 🍴 Fork    →  Fork this repo to your account or organization
2. 🔓 Enable  →  Turn on GHAS in Settings → Code security and analysis
3. 🔍 Explore →  Check the Security tab — alerts will appear automatically
```

> 📘 For detailed setup instructions, see the **[Getting Started Guide](docs/getting-started.md)**.

---

## 📦 What's Included

| Category | Description | Location | Status |
|---|---|---|---|
| **Live Configurations** | Dependabot, CodeQL, and Secret Scanning configs | [`.github/`](.github/) | ✅ Ready |
| **GitHub Actions Workflows** | CodeQL analysis, Dependency Review, OpenSSF Scorecard | [`.github/workflows/`](.github/workflows/) | ✅ Ready |
| **Vulnerable Code Examples** | JS, Python, Java, C# with known vulnerabilities | [`examples/vulnerable/`](examples/vulnerable/) | ⚠️ Educational |
| **Secure Code Examples** | Fixed counterparts showing proper remediation | [`examples/secure/`](examples/secure/) | ✅ Best Practices |
| **Configuration Templates** | Copy-paste configs for Dependabot, CodeQL, Secret Scanning | [`examples/configs/`](examples/configs/) | 📋 Templates |
| **Custom CodeQL Queries** | Example organization-specific security queries | [`examples/custom-queries/`](examples/custom-queries/) | 🔍 Examples |
| **Documentation & Guides** | Getting started, practical guides, FAQ | [`docs/`](docs/) | 📖 Guides |
| **Reference Material** | Detailed Copilot skills for CodeQL, Dependabot, Secret Scanning | [`.github/skills/`](.github/skills/) | 📚 Reference |

---

## 📖 Documentation

| Guide | Description |
|---|---|
| 🚀 [Getting Started](docs/getting-started.md) | First-time setup, enabling GHAS, and initial configuration |
| 🔍 [CodeQL Guide](docs/codeql-guide.md) | Code scanning setup, custom queries, and CI integration |
| 📦 [Dependabot Guide](docs/dependabot-guide.md) | Dependency updates, security alerts, and configuration strategies |
| 🔐 [Secret Scanning Guide](docs/secret-scanning-guide.md) | Secret detection, push protection, and custom patterns |
| 🏢 [Advanced Scenarios](docs/advanced-scenarios.md) | Monorepos, enterprise rollout, GHAS at scale |
| ✅ [Rollout Checklist](docs/rollout-checklist.md) | Step-by-step checklist for organization-wide GHAS adoption |
| ❓ [FAQ](docs/faq.md) | Frequently asked questions and troubleshooting |

---

## 🗂️ Repository Structure

```text
ghas-pack/
├── .github/
│   ├── dependabot.yml              # Live Dependabot configuration
│   ├── secret_scanning.yml         # Live Secret Scanning configuration
│   ├── codeql/
│   │   └── codeql-config.yml       # CodeQL analysis configuration
│   ├── workflows/
│   │   ├── codeql-analysis.yml     # CodeQL scanning workflow
│   │   ├── dependency-review.yml   # Dependency review on PRs
│   │   └── scorecard.yml           # OpenSSF Scorecard
│   └── skills/                     # Copilot reference skills
│       ├── codeql/
│       ├── dependabot/
│       └── secret-scanning/
├── docs/
│   ├── getting-started.md
│   ├── codeql-guide.md
│   ├── dependabot-guide.md
│   ├── secret-scanning-guide.md
│   ├── advanced-scenarios.md
│   ├── rollout-checklist.md
│   └── faq.md
├── examples/
│   ├── configs/                    # Configuration templates
│   │   ├── codeql/
│   │   ├── dependabot/
│   │   ├── secret-scanning/
│   │   └── workflows/
│   ├── custom-queries/             # Custom CodeQL queries
│   │   ├── javascript/
│   │   └── python/
│   ├── vulnerable/                 # ⚠️ Deliberately vulnerable code
│   │   ├── csharp/
│   │   ├── java/
│   │   ├── javascript/
│   │   └── python/
│   └── secure/                     # ✅ Secure counterparts
├── LICENSE
├── README.md
└── SECURITY.md
```

---

## 🏁 For Teams Getting Started

Here's the recommended reading order to get the most out of GHAS-Pack:

1. **📘 Read the [Getting Started Guide](docs/getting-started.md)** — understand what GHAS is and how to enable it on your repositories.
2. **⚙️ Review the live configs in [`.github/`](.github/)** — see how Dependabot, CodeQL, and Secret Scanning are configured in this repo.
3. **🔍 Explore [vulnerable](examples/vulnerable/) vs. [secure](examples/secure/) examples** — compare vulnerable code with its fixed counterpart to learn what GHAS detects and how to remediate.
4. **📋 Copy [configuration templates](examples/configs/) for your repos** — grab ready-made configs and adapt them to your team's stack.
5. **✅ Follow the [Rollout Checklist](docs/rollout-checklist.md)** — use the step-by-step checklist to drive org-wide GHAS adoption.

---

## 🤝 Contributing

Contributions are welcome! Whether it's a new vulnerable/secure example, an additional config template, a custom CodeQL query, or a documentation improvement — we'd love your help.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-contribution`)
3. Commit your changes (`git commit -m 'Add new example'`)
4. Push to the branch (`git push origin feature/my-contribution`)
5. Open a Pull Request

Please ensure any new vulnerable code examples are clearly marked and isolated in the `examples/vulnerable/` directory.

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

## 🔗 Additional Resources

- [GitHub Advanced Security Documentation](https://docs.github.com/en/get-started/learning-about-github/about-github-advanced-security)
- [CodeQL Documentation](https://codeql.github.com/docs/)
- [Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)
- [GitHub Security Blog](https://github.blog/security/)
- [OpenSSF Scorecard](https://securityscorecards.dev/)
