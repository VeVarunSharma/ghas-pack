# Secret Scanning — Custom Patterns

GitHub secret scanning detects secrets from [supported partners](https://docs.github.com/en/code-security/secret-scanning/introduction/supported-secret-scanning-patterns) automatically. **Custom patterns** let you detect _your own_ secret formats — internal API keys, proprietary tokens, database connection strings, etc.

Custom patterns can be defined at three scopes:

| Scope | Who can create | Where alerts appear |
|-------|---------------|---------------------|
| **Repository** | Repo admin | That repo only |
| **Organization** | Org owner/security manager | All repos in the org |
| **Enterprise** | Enterprise admin | All repos in the enterprise |

---

## Example Patterns

### 1. Internal API Key

Your internal services issue API keys in a known format:

```
Pattern name:  Internal API Key
Secret format: MYCO_[A-Za-z0-9]{32}
```

**Regex breakdown:**
- `MYCO_` — a fixed prefix your key-generation service always uses.
- `[A-Za-z0-9]{32}` — 32 alphanumeric characters.

Example match: `MYCO_a1B2c3D4e5F6g7H8i9J0k1L2m3N4o5P6`

---

### 2. Custom Token Format

Your auth system produces tokens with a specific structure:

```
Pattern name:  Acme Auth Token
Secret format: acme_tok_(?:live|test)_[0-9a-f]{40}
```

**Regex breakdown:**
- `acme_tok_` — fixed prefix.
- `(?:live|test)_` — environment indicator.
- `[0-9a-f]{40}` — 40 hex characters (SHA-1-length).

Example match: `acme_tok_live_6a3f8b2c9d0e1f4a5b6c7d8e9f0a1b2c3d4e5f6a`

---

### 3. Database Connection String

Detect connection strings that embed credentials:

```
Pattern name:  Database Connection String
Secret format: (?:mysql|postgres(?:ql)?|mongodb(?:\+srv)?):\/\/[^:]+:[^@]+@[^\s]+
```

**Regex breakdown:**
- `(?:mysql|postgres(?:ql)?|mongodb(?:\+srv)?)` — common database schemes.
- `:\/\/` — scheme separator.
- `[^:]+` — username.
- `:[^@]+` — `:password`.
- `@[^\s]+` — `@host` and the rest of the URI.

Example match: `postgres://admin:s3cret@db.internal.example.com:5432/mydb`

---

### 4. Private Key Header

Catch PEM-encoded private keys:

```
Pattern name:       Private Key Header
Secret format:      -----BEGIN (?:RSA |EC |DSA |OPENSSH )?PRIVATE KEY-----
After secret:       [\s\S]+?-----END (?:RSA |EC |DSA |OPENSSH )?PRIVATE KEY-----
```

Using **"After secret"** (optional context after the match) ensures the entire key block is captured.

---

## How to Define Patterns

### Via the GitHub UI

1. Navigate to your **Repository → Settings → Code security → Secret scanning**.
   _(Or Organization → Settings → Code security → Secret scanning for org-level.)_
2. Click **New pattern**.
3. Fill in:
   - **Pattern name** — a human-readable label.
   - **Secret format** — the regex for the secret itself.
   - **Before secret** _(optional)_ — regex for content that must appear before the secret.
   - **After secret** _(optional)_ — regex for content that must appear after the secret.
4. Click **Save and dry run** (see below).

### Via the REST API

You can also create patterns programmatically:

```bash
# Create a repo-level custom pattern
curl -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  "https://api.github.com/repos/OWNER/REPO/secret-scanning/custom-patterns" \
  -d '{
    "name": "Internal API Key",
    "pattern": "MYCO_[A-Za-z0-9]{32}",
    "before_secret": "",
    "after_secret": "",
    "scope": "repository"
  }'
```

```bash
# Create an org-level custom pattern
curl -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  "https://api.github.com/orgs/ORG/secret-scanning/custom-patterns" \
  -d '{
    "name": "Acme Auth Token",
    "pattern": "acme_tok_(?:live|test)_[0-9a-f]{40}",
    "before_secret": "",
    "after_secret": "",
    "scope": "organization"
  }'
```

---

## Dry Runs — Test Before You Enable

Always use **dry runs** to validate a custom pattern before making it active:

1. After saving a new pattern, click **Dry run** in the UI.
2. GitHub will scan recent pushes and show you what _would_ have been flagged.
3. Review the results:
   - **Too many alerts?** Tighten your regex (add anchors, require longer lengths).
   - **No alerts?** Verify the format against actual secrets in your test data.
4. Once you're satisfied, toggle the pattern to **Active**.

> **Tip:** For regex development, test on [regex101.com](https://regex101.com) first, then dry-run in GitHub to confirm against real content.

---

## Scoping Patterns

| You want to… | Define the pattern at… |
|---|---|
| Protect a single repo with a unique key format | **Repository** level |
| Roll out a standard across all repos in the org | **Organization** level |
| Enforce enterprise-wide detection (e.g., shared SSO tokens) | **Enterprise** level |

Organization and enterprise patterns are automatically applied to all current _and future_ repositories within their scope.

---

## Best Practices

- **Start specific.** A tight regex (e.g., exact prefix + fixed length) produces fewer false positives than a broad one.
- **Use dry runs.** Never enable a pattern without dry-running it first.
- **Document your patterns.** Keep a registry of pattern names, regexes, and the systems they protect so teammates understand what's being detected.
- **Combine with push protection.** Custom patterns support push protection — block the push at `git push` time instead of just alerting after the fact.
- **Review periodically.** As your internal key formats evolve, update or retire old patterns.

---

## References

- [Defining custom patterns for secret scanning](https://docs.github.com/en/code-security/secret-scanning/using-advanced-secret-scanning-and-push-protection-features/custom-patterns/defining-custom-patterns-for-secret-scanning)
- [Secret scanning REST API](https://docs.github.com/en/rest/secret-scanning)
- [Push protection for custom patterns](https://docs.github.com/en/code-security/secret-scanning/using-advanced-secret-scanning-and-push-protection-features/push-protection-for-repositories-and-organizations)
