# 🔍 CodeQL Guide — Practical Code Scanning for Teams

This guide covers everything you need to use CodeQL effectively — from initial setup to custom queries and performance tuning.

---

## What CodeQL Does

CodeQL is GitHub's **semantic code analysis engine**. Unlike linters that check syntax, CodeQL builds a database that models your code's structure, data flow, and control flow — then runs queries against it to find real security vulnerabilities.

Think of it this way:
- **Linters** check *how* your code looks
- **CodeQL** understands *what* your code does and finds paths where untrusted data reaches dangerous operations

CodeQL supports: **JavaScript/TypeScript, Python, Java/Kotlin, C#, Go, C/C++, Ruby, Rust, Swift**, and **GitHub Actions workflows**.

---

## Default vs Advanced Setup

### Decision Tree

```
         Need Code Scanning?
                │
                ▼
     ┌──────────────────────┐
     │ Is your repo using   │
     │ only interpreted     │──── Yes ──→  Start with Default Setup
     │ languages (JS, Py,   │
     │ Ruby, Go)?           │
     └──────────┬───────────┘
                │ No (or mixed)
                ▼
     ┌──────────────────────┐
     │ Do you need custom   │
     │ queries, specific    │──── No ───→  Try Default Setup first
     │ build steps, or      │              (switch later if needed)
     │ path filtering?      │
     └──────────┬───────────┘
                │ Yes
                ▼
        Use Advanced Setup
```

### Comparison

| Feature | Default Setup | Advanced Setup |
|---------|--------------|----------------|
| **Configuration effort** | None — click a button | Create & maintain a workflow YAML |
| **Language detection** | Automatic | You specify languages |
| **Query selection** | GitHub-managed default suite | Full control (default, security-extended, custom packs) |
| **Custom queries** | ❌ | ✅ |
| **Build configuration** | Automatic | You define build steps |
| **Trigger control** | Push + PR (managed) | Full control (push, PR, schedule, paths) |
| **Monorepo support** | Limited | Full (per-component categories) |
| **Performance tuning** | Automatic | Timeout, caching, path exclusions |
| **Copilot Autofix** | ✅ | ✅ |
| **Best for** | Most repos, getting started | Complex builds, compliance, monorepos |

> 💡 **Recommendation:** Start with Default Setup. You can switch to Advanced at any time without losing alert history.

---

## Setting Up CodeQL

### Option A: Default Setup (2 Minutes)

1. Go to **Settings** → **Code security**
2. Under **Code scanning**, click **Set up** → **Default**
3. Review the detected languages
4. Click **Enable CodeQL**

<!-- Screenshot placeholder: Default setup dialog showing language checkboxes and "Enable CodeQL" button -->

GitHub will now automatically:
- Run CodeQL on every push to your default branch
- Run CodeQL on every pull request targeting the default branch
- Report results in the Security tab and on PRs

### Option B: Advanced Setup (10 Minutes)

1. Go to **Settings** → **Code security**
2. Under **Code scanning**, click **Set up** → **Advanced**
3. GitHub generates a starter workflow — customize it:

```yaml
# .github/workflows/codeql.yml
name: "CodeQL Analysis"

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    # Run weekly to catch new vulnerabilities in unchanged code
    - cron: "30 1 * * 1"

permissions:
  security-events: write
  contents: read
  actions: read

jobs:
  analyze:
    name: Analyze (${{ matrix.language }})
    runs-on: ${{ matrix.language == 'swift' && 'macos-latest' || 'ubuntu-latest' }}
    strategy:
      fail-fast: false
      matrix:
        include:
          - language: javascript-typescript
            build-mode: none
          - language: python
            build-mode: none
          # For compiled languages, choose a build-mode:
          #   none    — no build required (less accurate for some languages)
          #   autobuild — CodeQL attempts to detect and run the build
          #   manual  — you provide explicit build steps
          # - language: java-kotlin
          #   build-mode: autobuild
          # - language: csharp
          #   build-mode: manual

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Initialize CodeQL
        uses: github/codeql-action/init@v3
        with:
          languages: ${{ matrix.language }}
          build-mode: ${{ matrix.build-mode }}
          # Use security-extended for more coverage:
          # queries: security-extended
          # Or use a custom config file:
          # config-file: .github/codeql/codeql-config.yml

      # Manual build steps (only for build-mode: manual)
      # - name: Build
      #   run: |
      #     dotnet build MySolution.sln

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v3
        with:
          category: "/language:${{ matrix.language }}"
```

4. Commit the workflow file
5. CodeQL analysis will run on the next push

### CodeQL Config File (Optional)

For fine-grained control, create a configuration file:

```yaml
# .github/codeql/codeql-config.yml
name: "Custom CodeQL Config"

# Use the security-extended query suite for broader coverage
queries:
  - uses: security-extended

# Add custom query packs
packs:
  javascript:
    - my-org/custom-js-queries@~1.0.0
  python:
    - codeql/python-queries:experimental/Security/CWE-090

# Exclude paths from analysis
paths-ignore:
  - "**/test/**"
  - "**/tests/**"
  - "**/*_test.go"
  - "**/vendor/**"
  - "**/node_modules/**"
  - "docs/**"
  - "scripts/**"

# Only scan specific paths (use instead of paths-ignore)
# paths:
#   - src
#   - lib
```

---

## Understanding Alerts

### Severity Levels

CodeQL alerts have two severity dimensions:

**Standard Severity** (code quality):

| Level | Meaning | Action |
|-------|---------|--------|
| **Error** | Likely a bug or vulnerability | Fix promptly |
| **Warning** | Potential issue or code smell | Review and fix |
| **Note** | Informational finding | Evaluate during code review |

**Security Severity** (derived from CVSS score):

| Level | CVSS Score | Meaning | Action |
|-------|-----------|---------|--------|
| 🔴 **Critical** | 9.0–10.0 | Easily exploitable, severe impact | Fix immediately |
| 🟠 **High** | 7.0–8.9 | Significant risk | Fix within days |
| 🟡 **Medium** | 4.0–6.9 | Moderate risk, requires some conditions | Fix within sprints |
| 🔵 **Low** | 0.1–3.9 | Minor risk, hard to exploit | Fix when convenient |

### Reading Alert Details

Each alert contains:

1. **Title** — What was found (e.g., "SQL query built from user-controlled sources")
2. **Location** — Exact file and line number
3. **Data flow path** — Shows how untrusted data flows from source to sink (for dataflow queries)
4. **CWE reference** — Links to the Common Weakness Enumeration
5. **Recommendation** — How to fix the issue
6. **Copilot Autofix** — AI-generated fix suggestion (when available)

---

## Common Alert Types

Here are the most frequently encountered CodeQL findings:

### Top 10 CodeQL Findings

| # | Finding | CWE | Languages | What Happens |
|---|---------|-----|-----------|-------------|
| 1 | **SQL Injection** | CWE-089 | All | User input inserted directly into SQL queries |
| 2 | **Cross-Site Scripting (XSS)** | CWE-079 | JS/TS, Java, C# | User input rendered in HTML without escaping |
| 3 | **Path Traversal** | CWE-022 | All | User input used to construct file paths (`../../etc/passwd`) |
| 4 | **Command Injection** | CWE-078 | All | User input passed to shell commands |
| 5 | **Insecure Deserialization** | CWE-502 | Java, C#, Python | Untrusted data deserialized without validation |
| 6 | **Server-Side Request Forgery (SSRF)** | CWE-918 | All | User-controlled URLs used in server-side HTTP requests |
| 7 | **Log Injection** | CWE-117 | All | User input written to logs without sanitization |
| 8 | **Hardcoded Credentials** | CWE-798 | All | Passwords or API keys embedded in source code |
| 9 | **Missing Authentication** | CWE-306 | Java, C# | Sensitive operations without authentication checks |
| 10 | **Prototype Pollution** | CWE-1321 | JS/TS | User input modifies JavaScript object prototypes |

### Example: SQL Injection Alert

```javascript
// ❌ VULNERABLE — user input goes directly into the query
app.get("/users", (req, res) => {
  const query = "SELECT * FROM users WHERE name = '" + req.query.name + "'";
  db.execute(query);
});

// ✅ FIXED — parameterized query
app.get("/users", (req, res) => {
  const query = "SELECT * FROM users WHERE name = ?";
  db.execute(query, [req.query.name]);
});
```

---

## Using Copilot Autofix

Copilot Autofix is an AI-powered feature that generates fix suggestions for CodeQL alerts directly in pull requests.

### How It Works

1. CodeQL finds a vulnerability in your PR
2. Copilot Autofix analyzes the alert and surrounding code
3. A fix suggestion appears as a comment on the PR with a diff preview
4. You can **apply the fix** with one click or modify it before applying

### Tips for Copilot Autofix

- Autofix works best for well-defined vulnerability patterns (SQL injection, XSS, path traversal)
- Always **review the suggestion** before applying — AI-generated fixes may not account for all business logic
- Autofix is available for JavaScript/TypeScript, Python, Java, C#, Go, Ruby, and C/C++
- No additional subscription is required — it's included with Code Scanning

<!-- Screenshot placeholder: PR conversation showing a CodeQL alert with a Copilot Autofix suggestion, including a diff preview and "Apply fix" button -->

---

## Custom Queries

### When to Write Custom Queries

Custom CodeQL queries are valuable when you need to:

- Enforce **organization-specific security rules** (e.g., "all database calls must go through our DAL")
- Detect **custom vulnerability patterns** unique to your frameworks
- Ban **specific dangerous APIs** in your codebase
- Enforce **architectural constraints** (e.g., "UI layer cannot call database directly")

### Basic Query Structure

CodeQL queries use a SQL-like language called QL. Here's the anatomy of a query:

```ql
/**
 * @name Use of insecure random number generator
 * @description Math.random() is not cryptographically secure.
 *              Use crypto.getRandomValues() instead.
 * @kind problem
 * @problem.severity warning
 * @security-severity 5.0
 * @precision high
 * @id js/insecure-random
 * @tags security
 *       external/cwe/cwe-338
 */

import javascript

from CallExpr call
where
  call.getCalleeName() = "random" and
  call.getReceiver().(VarAccess).getName() = "Math"
select call, "Math.random() is not cryptographically secure. Use crypto.getRandomValues() instead."
```

### Using Custom Query Packs

Reference custom queries in your workflow:

```yaml
# In your CodeQL workflow
- name: Initialize CodeQL
  uses: github/codeql-action/init@v3
  with:
    languages: javascript-typescript
    packs: |
      my-org/custom-security-queries@~1.0.0
    # Or reference a config file
    config-file: .github/codeql/codeql-config.yml
```

> 📖 See the [`examples/custom-queries/`](../examples/custom-queries/) directory for sample custom queries in JavaScript and Python.

---

## Monorepo Configuration

For monorepos, you need CodeQL to analyze different components separately.

### Strategy: Per-Component Analysis

```yaml
# .github/workflows/codeql.yml
name: "CodeQL Monorepo"

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

permissions:
  security-events: write
  contents: read
  actions: read

jobs:
  analyze-frontend:
    name: Analyze Frontend
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: github/codeql-action/init@v3
        with:
          languages: javascript-typescript
          build-mode: none
      - uses: github/codeql-action/analyze@v3
        with:
          category: "frontend"

  analyze-backend-api:
    name: Analyze Backend API
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: github/codeql-action/init@v3
        with:
          languages: java-kotlin
          build-mode: autobuild
      - uses: github/codeql-action/analyze@v3
        with:
          category: "backend-api"

  analyze-services:
    name: Analyze Python Services
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: github/codeql-action/init@v3
        with:
          languages: python
          build-mode: none
          config-file: .github/codeql/codeql-config.yml
      - uses: github/codeql-action/analyze@v3
        with:
          category: "services"
```

### Path Filtering for Monorepos

Run analysis only when relevant code changes:

```yaml
on:
  push:
    branches: [main]
    paths:
      - "frontend/**"
      - "!frontend/**/*.test.js"
  pull_request:
    branches: [main]
    paths:
      - "frontend/**"
```

> ⚠️ **Important:** Always include a `schedule` trigger even with path filtering. Scheduled runs catch new vulnerabilities in unchanged code when the CodeQL query database is updated.

---

## Performance Optimization

Large codebases can hit analysis timeouts or consume excessive resources. Here's how to keep things fast.

### Tips for Large Codebases

#### 1. Exclude Non-Production Code

```yaml
# .github/codeql/codeql-config.yml
paths-ignore:
  - "**/test/**"
  - "**/tests/**"
  - "**/__tests__/**"
  - "**/*_test.go"
  - "**/*.test.js"
  - "**/*.spec.ts"
  - "**/vendor/**"
  - "**/node_modules/**"
  - "**/third_party/**"
  - "docs/**"
  - "scripts/**"
  - "tools/**"
```

#### 2. Increase Timeout for Large Projects

```yaml
- name: Perform CodeQL Analysis
  uses: github/codeql-action/analyze@v3
  timeout-minutes: 120  # Default is 360 (6 hours)
```

#### 3. Use Dependency Caching

```yaml
- name: Cache CodeQL dependencies
  uses: actions/cache@v4
  with:
    path: ~/.codeql
    key: codeql-${{ runner.os }}-${{ hashFiles('**/pom.xml', '**/package-lock.json') }}
```

#### 4. Choose the Right Runner

| Codebase Size | Recommended Runner | RAM | CPUs |
|--------------|-------------------|-----|------|
| Small (< 100K lines) | `ubuntu-latest` | 8 GB | 2 |
| Medium (100K–500K lines) | `ubuntu-latest-4-cores` | 16 GB | 4 |
| Large (500K+ lines) | `ubuntu-latest-16-cores` | 64 GB | 8+ |

```yaml
jobs:
  analyze:
    runs-on: ubuntu-latest-16-cores  # For large codebases
```

#### 5. Use `none` Build Mode When Possible

For interpreted languages and some compiled languages, `none` build mode is significantly faster than `autobuild` or `manual`:

```yaml
matrix:
  include:
    - language: java-kotlin
      build-mode: none    # Faster but may miss some findings
    # - language: java-kotlin
    #   build-mode: autobuild  # More thorough but slower
```

> ⚠️ Using `none` for compiled languages may result in fewer findings because call graph and type information may be incomplete.

---

## Troubleshooting

### Issue 1: "No source code was seen during the build"

**Cause:** CodeQL couldn't find or compile your code.

**Fix:**
- For interpreted languages, ensure source files are in the repository root or are not excluded by `paths-ignore`
- For compiled languages, switch from `autobuild` to `manual` build mode and provide explicit build commands
- Check that your `.gitignore` isn't excluding source files needed for analysis

### Issue 2: Analysis Timeout

**Cause:** The codebase is too large or the build is too slow.

**Fix:**
- Increase `timeout-minutes` (up to 360)
- Exclude test and vendor directories in `codeql-config.yml`
- Use a larger runner (see Performance Optimization above)
- Split analysis across multiple jobs (per-component)

### Issue 3: "Resource not accessible by integration" (403 Error)

**Cause:** The workflow doesn't have the right permissions.

**Fix:** Ensure your workflow has the correct permissions block:

```yaml
permissions:
  security-events: write
  contents: read
  actions: read
```

### Issue 4: Two Workflows Running Code Scanning

**Cause:** Both Default Setup and an Advanced Setup workflow are enabled.

**Fix:**
- If using Advanced Setup, disable Default Setup in **Settings** → **Code security** → **Code scanning** → **Default setup** → **Disable**
- You cannot have both active simultaneously for the same language

### Issue 5: Too Many Alerts / Noisy Results

**Cause:** The default query suite may flag lower-severity or style issues.

**Fix:**
- Use `security-and-quality` for comprehensive results or `security-extended` for security-focused results
- Add `paths-ignore` to exclude generated code, vendored dependencies, and test fixtures
- Dismiss false positives with appropriate reasons to improve future results
- Use alert labels (Generated, Test, Library, Documentation) to filter noise

---

## 📖 Additional Resources

- **[CodeQL Skill Reference](../.github/skills/codeql/)** — Detailed reference docs for workflows, CLI, SARIF, and troubleshooting
- **[Custom Query Examples](../examples/custom-queries/)** — Sample CodeQL queries for JavaScript and Python
- **[Getting Started](getting-started.md)** — Enable all GHAS features
- **[GitHub CodeQL Documentation](https://docs.github.com/en/code-security/code-scanning/introduction-to-code-scanning/about-code-scanning-with-codeql)**
- **[CodeQL Query Help](https://codeql.github.com/docs/)** — Official CodeQL language reference and tutorials
