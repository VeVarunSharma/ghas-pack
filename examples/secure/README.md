# ✅ Secure Code Examples

These are the **fixed** versions of the deliberately vulnerable examples in
[`../vulnerable/`](../vulnerable/). Each file mirrors the structure of its
vulnerable counterpart so you can compare them side by side.

## How to use

1. Open a vulnerable file and its secure counterpart in a diff view.
2. Read the header comments — they explain the security principle applied.
3. Look for `// GOOD:` or `# GOOD:` comments marking the fixed code.

## File mapping

| Vulnerable File | Secure File | Vulnerability Fixed | Security Principle |
|---|---|---|---|
| `javascript/sql-injection.js` | `javascript/sql-injection-fixed.js` | SQL Injection (CWE-89) | Parameterized queries — separate code from data |
| `javascript/xss-example.js` | `javascript/xss-example-fixed.js` | Reflected XSS (CWE-79) | Output encoding — escape untrusted data before rendering |
| `javascript/prototype-pollution.js` | *(no dedicated fix file)* | Prototype Pollution (CWE-1321) | Allowlist keys — reject `__proto__`, `constructor`, `prototype` |
| `javascript/package.json` | `javascript/package.json` | Known CVEs in dependencies | Keep dependencies up to date |
| `python/sql_injection.py` | `python/sql_injection_fixed.py` | SQL Injection (CWE-89) | Parameterized queries — separate code from data |
| `python/command_injection.py` | `python/command_injection_fixed.py` | OS Command Injection (CWE-78) | Avoid shells — use `subprocess` with list args + input validation |
| `python/path_traversal.py` | `python/path_traversal_fixed.py` | Path Traversal (CWE-22) | Canonicalize paths and verify they stay inside the allowed directory |
| `python/requirements.txt` | `python/requirements.txt` | Known CVEs in dependencies | Keep dependencies up to date |

## General principles

- **Never trust user input.** Validate, sanitize, and encode at every boundary.
- **Use parameterized queries** for all database access — never concatenate strings.
- **Use subprocess with a list** instead of shell commands.
- **Canonicalize and confine paths** before file operations.
- **Keep dependencies patched** — run Dependabot / `npm audit` / `pip-audit` regularly.
