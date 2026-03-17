# 📚 GHAS-Pack Examples

This directory contains hands-on examples to help you understand, demonstrate, and adopt GitHub Advanced Security (GHAS) features. Whether you're evaluating GHAS for your organization or training your development teams, these examples provide real-world scenarios across multiple languages and tools.

---

## 🔴 Vulnerable Code Examples

**[`vulnerable/`](vulnerable/README.md)**

Deliberately vulnerable code samples designed for demonstration and educational purposes. Each file contains one or more known security vulnerabilities that GHAS features (CodeQL, Dependabot, Secret Scanning) can detect.

> ⚠️ **Warning:** These examples contain intentional security flaws. Do **not** use this code in production.

---

## 🟢 Secure Code Examples

**[`secure/`](secure/README.md)**

Fixed counterparts of the vulnerable examples, showing how to properly remediate each vulnerability. Compare these side-by-side with the vulnerable versions to understand the exact changes needed to write secure code.

---

## 📋 Configuration Templates

**[`configs/`](configs/README.md)**

Ready-to-use configuration templates for GHAS tools, including:

- **Dependabot** — dependency update strategies (basic, enterprise, monorepo, grouped updates)
- **CodeQL** — analysis workflows and configuration files (basic, extended, multi-language)
- **Secret Scanning** — custom pattern definitions
- **Workflows** — dependency review and OpenSSF Scorecard actions

Copy these templates into your own repositories and customize them for your needs.

---

## 🔍 Custom CodeQL Queries

**[`custom-queries/`](custom-queries/README.md)**

Example custom CodeQL queries that go beyond the default query suites. Use these as a starting point for writing organization-specific security rules, such as:

- Detecting dangerous function calls (`eval`, `exec`)
- Enforcing internal coding standards
- Finding project-specific vulnerability patterns

---

## 🌐 Languages Covered

| Language | Vulnerable Examples | Secure Examples | Config Templates |
|---|---|---|---|
| **JavaScript** | ✅ SQL Injection, XSS, Prototype Pollution | 🟢 Available | ✅ CodeQL, Dependabot |
| **Python** | ✅ SQL Injection, Command Injection, Path Traversal | 🟢 Available | ✅ CodeQL, Dependabot |
| **Java** | ✅ SQL Injection, XXE, Insecure Deserialization | 🟢 Available | ✅ CodeQL, Dependabot |
| **C# (.NET)** | ✅ SQL Injection, LDAP Injection, Path Traversal | 🟢 Available | ✅ CodeQL |

---

## 🚀 How to Use These Examples

1. **Fork this repository** to your own GitHub account or organization.

2. **Enable GitHub Advanced Security** on your fork:
   - Go to **Settings → Code security and analysis**
   - Enable **Dependency graph**, **Dependabot alerts**, **Dependabot security updates**
   - Enable **Code scanning** (CodeQL) and **Secret scanning**

3. **Watch the alerts appear** — within minutes, GHAS will begin analyzing the vulnerable examples and surfacing security findings in the **Security** tab.

4. **Compare vulnerable vs. secure** — review the alerts, then look at the corresponding secure examples to understand the recommended fix for each vulnerability.

5. **Copy configuration templates** — browse the [`configs/`](configs/README.md) directory and copy the templates that match your team's needs into your own repositories.

---

## 📖 Further Reading

- [Getting Started Guide](../docs/getting-started.md)
- [CodeQL Guide](../docs/codeql-guide.md)
- [Dependabot Guide](../docs/dependabot-guide.md)
- [Secret Scanning Guide](../docs/secret-scanning-guide.md)
- [Root README](../README.md)
