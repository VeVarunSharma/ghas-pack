# Custom CodeQL Queries

## What Are Custom CodeQL Queries?

[CodeQL](https://codeql.github.com/) is GitHub's semantic code analysis engine. It treats code as data — you write queries in a SQL-like language called **QL** to find patterns, bugs, and security vulnerabilities in your codebase.

While CodeQL ships with thousands of built-in queries, **custom queries** let you:

- Enforce organization-specific coding standards
- Detect project-specific anti-patterns (e.g., banned API usage)
- Extend coverage for frameworks or libraries not fully covered by default queries
- Codify lessons learned from past security incidents

## Included Queries

| Query | Language | Description |
|-------|----------|-------------|
| [`javascript/detect-eval-usage.ql`](javascript/detect-eval-usage.ql) | JavaScript | Detects usage of `eval()`, which can lead to code injection vulnerabilities |
| [`python/detect-exec-usage.ql`](python/detect-exec-usage.ql) | Python | Detects usage of `exec()` and `eval()`, which can lead to command injection |

## Query Structure

Every CodeQL query follows a **from → where → select** pattern, similar to SQL:

```ql
/**
 * @name Name of the query
 * @description What the query detects
 * @kind problem
 * @problem.severity warning
 * @id my-org/language/query-id
 * @tags security
 */

import <language>           // Import the CodeQL language library

from <Type> <variable>      // Declare variables with their types
where <condition>           // Filter to the pattern you care about
select <element>, <message> // Report results with a human-readable message
```

### Metadata Annotations

The comment block at the top of each query contains metadata that CodeQL uses:

| Annotation | Purpose |
|------------|---------|
| `@name` | Human-readable name shown in results |
| `@description` | Longer explanation of what the query finds |
| `@kind` | Query type — `problem` for simple alerts, `path-problem` for data-flow |
| `@problem.severity` | One of `error`, `warning`, or `recommendation` |
| `@id` | Unique identifier (use `org/language/name` convention) |
| `@tags` | Categories like `security`, `correctness`, `maintainability` |

## How to Create a QLPack

A **qlpack** groups related queries and declares their dependencies. Create a `qlpack.yml` file:

```yaml
name: my-org/custom-queries
version: 0.0.1
dependencies:
  codeql/javascript-all: "*"
  codeql/python-all: "*"
```

Key fields:
- **`name`** — unique pack identifier (typically `org/pack-name`)
- **`version`** — semantic version of your query pack
- **`dependencies`** — CodeQL standard libraries your queries depend on (with version constraints)

After creating the pack, install dependencies:

```bash
codeql pack install examples/custom-queries
```

## How to Use These Queries

### Option 1: Reference in `codeql-config.yml`

Add custom queries to your Code Scanning workflow configuration:

```yaml
# .github/codeql/codeql-config.yml
name: "Custom CodeQL Config"
queries:
  - uses: ./examples/custom-queries
```

Then reference the config in your GitHub Actions workflow:

```yaml
- name: Initialize CodeQL
  uses: github/codeql-action/init@v4
  with:
    config-file: .github/codeql/codeql-config.yml
```

### Option 2: Run with the CodeQL CLI

Analyze a database directly:

```bash
# Create a CodeQL database
codeql database create my-db --language=javascript --source-root=.

# Run a single query
codeql database analyze my-db examples/custom-queries/javascript/detect-eval-usage.ql \
  --format=sarif-latest --output=results.sarif

# Run all queries in the pack
codeql database analyze my-db examples/custom-queries/ \
  --format=sarif-latest --output=results.sarif
```

### Option 3: Use in VS Code

1. Install the [CodeQL extension for VS Code](https://marketplace.visualstudio.com/items?itemName=GitHub.vscode-codeql)
2. Open a CodeQL database
3. Right-click a `.ql` file → **Run Query**

## Further Reading

- [Writing CodeQL queries](https://codeql.github.com/docs/writing-codeql-queries/)
- [CodeQL language guides](https://codeql.github.com/docs/codeql-language-guides/)
- [CodeQL query help](https://codeql.github.com/codeql-query-help/)
- [QL language reference](https://codeql.github.com/docs/ql-language-reference/)
- [CodeQL for JavaScript](https://codeql.github.com/docs/codeql-language-guides/codeql-for-javascript/)
- [CodeQL for Python](https://codeql.github.com/docs/codeql-language-guides/codeql-for-python/)
