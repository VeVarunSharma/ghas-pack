# ⚠️ WARNING: Deliberately Vulnerable Code ⚠️

> **DO NOT use any code in this directory in production applications.**
> **DO NOT deploy these examples to any environment accessible from the internet.**

## Purpose

This directory contains **intentionally vulnerable code examples** designed for educational purposes and GitHub Advanced Security (GHAS) demonstrations. Each file showcases a specific vulnerability pattern that GHAS tools — **CodeQL**, **Dependabot**, and **Secret Scanning** — are designed to detect.

These examples help teams:

- Understand how common vulnerabilities appear in real code
- See GHAS detection capabilities in action
- Learn to recognize and avoid insecure coding patterns
- Practice triaging and remediating security alerts

## Vulnerability Catalog

| File | Vulnerability | CWE | GHAS Tool |
|------|--------------|-----|-----------|
| [`javascript/sql-injection.js`](javascript/sql-injection.js) | SQL Injection | [CWE-89](https://cwe.mitre.org/data/definitions/89.html) | CodeQL |
| [`javascript/xss-example.js`](javascript/xss-example.js) | Reflected Cross-Site Scripting (XSS) | [CWE-79](https://cwe.mitre.org/data/definitions/79.html) | CodeQL |
| [`javascript/prototype-pollution.js`](javascript/prototype-pollution.js) | Prototype Pollution | [CWE-1321](https://cwe.mitre.org/data/definitions/1321.html) | CodeQL |
| [`javascript/package.json`](javascript/package.json) | Known Vulnerable Dependencies | Multiple CVEs | Dependabot |
| [`python/sql_injection.py`](python/sql_injection.py) | SQL Injection | [CWE-89](https://cwe.mitre.org/data/definitions/89.html) | CodeQL |
| [`python/command_injection.py`](python/command_injection.py) | OS Command Injection | [CWE-78](https://cwe.mitre.org/data/definitions/78.html) | CodeQL |
| [`python/path_traversal.py`](python/path_traversal.py) | Path Traversal | [CWE-22](https://cwe.mitre.org/data/definitions/22.html) | CodeQL |
| [`python/requirements.txt`](python/requirements.txt) | Known Vulnerable Dependencies | Multiple CVEs | Dependabot |

## Secure Counterparts

Each vulnerable example has a **fixed counterpart** in [`../secure/`](../secure/) that demonstrates the recommended remediation. Use them side-by-side to understand both the problem and the solution.

## How to Use

1. **Enable GHAS** on your repository (Code scanning, Dependabot, Secret scanning)
2. **Push these files** to a branch and observe the security alerts generated
3. **Review each alert** to understand the detection rationale
4. **Compare with `../secure/`** to see the recommended fix
5. **Resolve the alerts** by applying the secure patterns

---

*These examples are maintained as part of the [ghas-pack](../../README.md) starter kit.*
