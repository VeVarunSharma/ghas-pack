# GHAS Frequently Asked Questions

Common questions from teams adopting GitHub Advanced Security (GHAS), organized by feature area.

---

## Table of Contents

- [General GHAS](#general-ghas)
- [CodeQL Code Scanning](#codeql-code-scanning)
- [Dependabot](#dependabot)
- [Secret Scanning](#secret-scanning)
- [Organization & Enterprise](#organization--enterprise)

---

## General GHAS

### What's the difference between GHAS and GitHub's free security features?

GitHub provides several security features for free on all plans: Dependabot alerts, Dependabot security updates, secret scanning for public repos, and the dependency graph. **GHAS adds** CodeQL code scanning, secret scanning for private repos, push protection, custom secret scanning patterns, dependency review action, and the Security Overview dashboard. GHAS requires a separate license on GitHub Enterprise Cloud or GitHub Enterprise Server.

### Do I need a GitHub Enterprise license?

Yes. GHAS is an add-on to **GitHub Enterprise Cloud** or **GitHub Enterprise Server** (3.0+). You cannot purchase GHAS separately — you need an Enterprise plan first, then add GHAS seats. However, all GHAS features are free and automatically available on **public repositories** on GitHub.com, regardless of your plan.

### Can I use GHAS with GitHub.com Free/Team plans?

Partially. On **Free and Team plans**, you can use GHAS features on **public repositories only** (CodeQL, secret scanning with push protection, dependency review). For **private repositories**, you need GitHub Enterprise Cloud with GHAS licensing. Dependabot alerts and security updates are available on all plans for all repositories.

---

## CodeQL Code Scanning

### How long does CodeQL analysis take?

Analysis time depends on repository size, language, and build complexity. **Typical ranges:** small repos (JavaScript/Python) take 2–5 minutes; medium repos with compiled languages (Java/C#) take 5–15 minutes; large monorepos can take 20–45 minutes. You can reduce time by using `paths` filters to scope analysis to specific directories, using `build-mode: none` for interpreted languages, and splitting monorepos into parallel matrix jobs. See the [monorepo scanning guide](advanced-scenarios.md#1-monorepo-code-scanning) for optimization strategies.

### What languages does CodeQL support?

CodeQL supports: **C/C++**, **C#**, **Go**, **Java/Kotlin**, **JavaScript/TypeScript**, **Python**, **Ruby**, **Rust** (preview), and **Swift**. It also scans **GitHub Actions** workflow files for misconfigurations. Each language has a default query suite covering common vulnerabilities including the OWASP Top 10 and CWE Top 25. See the [compiled languages reference](../.github/skills/codeql/references/compiled-languages.md) for build mode details per language.

### Why am I getting false positives?

False positives usually occur because CodeQL's analysis is conservative — it flags potential issues even when the code has mitigations that are hard to detect statically. **To reduce false positives:** dismiss alerts with the "false positive" reason (this helps GitHub improve), exclude test/generated code using `paths-ignore` in your CodeQL config, and consider switching from `security-and-quality` to `security-extended` or the default `security` suite. You can also write custom query overrides to exclude specific patterns. See [alert management](../.github/skills/codeql/references/alert-management.md).

### How do I handle CodeQL timeouts?

CodeQL has a default timeout of ~2 hours per language. If you hit this, try: (1) Use `paths` to limit the analysis scope to relevant source directories. (2) Exclude vendored/generated code with `paths-ignore`. (3) Split the repository into multiple analysis jobs using a matrix strategy. (4) For self-hosted runners, ensure adequate resources — CodeQL recommends at least 8 GB RAM and 2 CPU cores. See the [troubleshooting reference](../.github/skills/codeql/references/troubleshooting.md) for detailed guidance.

### Can I run CodeQL on pull requests only?

Yes. Configure your workflow triggers to run only on `pull_request` events. However, **GitHub recommends also running on `push` to the default branch** so that the Security tab has a baseline to compare against. Without a baseline, PR checks can't distinguish new alerts from pre-existing ones. A common pattern is to run on both `push` (for baseline) and `pull_request` (for PR checks), with a weekly `schedule` as a fallback.

### What's the difference between default and advanced CodeQL setup?

**Default setup** is configured entirely through the GitHub UI — no workflow file needed. GitHub automatically detects languages, selects runners, and runs the standard query suite. **Advanced setup** uses a workflow YAML file (`.github/workflows/codeql.yml`) that gives you full control over triggers, languages, build steps, query packs, path filters, and runner selection. Start with default setup; switch to advanced only when you need custom build commands, monorepo path filtering, or custom query packs.

---

## Dependabot

### How do I reduce the number of Dependabot PRs?

Several strategies: (1) **Group updates** — combine related dependency updates into fewer PRs using the `groups` option in `dependabot.yml`. (2) **Reduce frequency** — set `schedule.interval` to `weekly` or `monthly` instead of `daily`. (3) **Set `open-pull-requests-limit`** — cap the number of simultaneous open Dependabot PRs (default is 5). (4) **Ignore minor/patch updates** for stable dependencies using `ignore` rules. (5) **Focus on security updates only** — disable version updates if they create too much noise. See the [Dependabot skill guide](../.github/skills/dependabot/SKILL.md) for grouping examples.

### Can Dependabot update private/internal dependencies?

Yes, but it requires additional configuration. You need to set up **private registry credentials** in your `dependabot.yml` using the `registries` option. Dependabot supports private registries for npm, Maven, NuGet, PyPI, Docker, and more. Credentials are stored as encrypted secrets in the repository or organization settings. See the [Dependabot YAML reference](../.github/skills/dependabot/references/dependabot-yml-reference.md) for registry configuration syntax.

### How do I handle breaking dependency updates?

When a Dependabot PR introduces breaking changes: (1) **Review the changelog** — Dependabot includes a changelog link and compatibility score in each PR. (2) **Run your test suite** — if CI passes, the update is likely safe. (3) **Use `@dependabot ignore this major version`** to skip major updates you're not ready for. (4) **Pin the dependency** to a specific version range if needed. (5) For grouped updates, merge the non-breaking updates first, then address breaking ones individually.

### What's the difference between Dependabot security updates and version updates?

**Security updates** are triggered automatically when GitHub detects a known vulnerability (CVE/advisory) in one of your dependencies — these PRs update only the affected dependency to the minimum safe version. **Version updates** are proactive — they keep all your dependencies up to date on a schedule you define in `dependabot.yml`, regardless of whether vulnerabilities exist. Security updates are enabled in repository settings; version updates require a `dependabot.yml` configuration file.

---

## Secret Scanning

### What happens when push protection blocks my push?

When push protection detects a secret in your commit, the `git push` is rejected with a message identifying the secret type and location. You have three options: (1) **Remove the secret** from your commit history (using `git rebase` or `git filter-branch`) and push again. (2) **Mark the secret as a false positive** or test credential through the provided URL — this allows the push and creates a record. (3) **Request a bypass** if your organization has delegated bypass configured — an approver must review and approve the push. The blocked secret and your chosen action are logged for audit purposes. See the [push protection reference](../.github/skills/secret-scanning/references/push-protection.md).

### How do I handle false positives in secret scanning?

If secret scanning flags a string that isn't actually a secret (e.g., an example token in documentation): (1) **Close the alert as "false positive"** or "used in tests" in the Security tab — this dismisses it and prevents re-alerting. (2) **For push protection blocks**, choose "it's a false positive" in the bypass flow. (3) **Add the file to exclusions** in `.github/secret_scanning.yml` if the file consistently contains test/example secrets. (4) **For custom patterns**, refine the regex to reduce false matches. Note that dismissed alerts are tracked for audit compliance.

### Can I create custom patterns for internal secrets?

Yes. Custom patterns let you detect secrets specific to your organization — internal API keys, database connection strings, or proprietary token formats. You can define custom patterns at the **repository**, **organization**, or **enterprise** level. Each pattern requires a regex, and optionally "before" and "after" context patterns to improve precision. GitHub also offers **Copilot-assisted pattern generation** to help you write regex for your secret format. Custom patterns can optionally be enabled for push protection. See the [custom patterns reference](../.github/skills/secret-scanning/references/custom-patterns.md) for syntax and examples.

### Does secret scanning work on private repositories?

Secret scanning works on private repositories **only with a GHAS license**. Without GHAS, secret scanning is limited to public repositories. When enabled on a private repo, secret scanning checks the entire Git history (all branches and tags) for supported secret types. Partner patterns (e.g., AWS keys, Stripe tokens) trigger notifications to the issuing provider so they can revoke the secret. Push protection for private repos also requires GHAS.

---

## Organization & Enterprise

### How do I roll out GHAS across my organization?

Follow a phased approach: (1) **Pilot** — enable GHAS on 3–5 representative repos, train teams, collect feedback for 2–4 weeks. (2) **Refine** — tune CodeQL config, adjust Dependabot PR volume, create custom secret patterns based on pilot learnings. (3) **Broad rollout** — enable features at the org level, distribute standard configs, and set up monitoring. (4) **Mature** — add custom queries, automated triage, and metrics tracking. The entire process typically takes 3–5 months. See the [rollout checklist](rollout-checklist.md) for a detailed step-by-step guide.

### How do I track GHAS adoption across repositories?

Use the **Security Overview** dashboard at `https://github.com/orgs/{org}/security`. The **Coverage** view shows which repositories have each GHAS feature (CodeQL, Dependabot, secret scanning) enabled or disabled. The **Risk** view ranks repos by open alert count. You can filter by team, topic, visibility, and language. For programmatic tracking, use the REST API to query alert counts across repos — see [GHAS API and Automation](advanced-scenarios.md#6-ghas-api-and-automation). Export data to dashboards like Grafana or Power BI for executive reporting.

### Can I enforce GHAS policies with repository rulesets?

Yes. **Repository rulesets** (available at the org level) let you enforce security policies across all repositories without relying on individual repo settings. You can require CodeQL and dependency review as mandatory status checks, prevent merging PRs with critical/high security alerts, and apply these rules across all repos or a filtered subset. Rulesets are particularly useful because they can't be overridden by repository administrators (unlike branch protection rules, which repo admins can modify). Configure rulesets at **Organization → Settings → Rulesets**.

---

## Still Have Questions?

- **Skill guides:** Detailed setup and reference docs are in [`.github/skills/`](../.github/skills/)
- **Advanced scenarios:** Monorepos, custom queries, SARIF upload, and more in [advanced-scenarios.md](advanced-scenarios.md)
- **Rollout planning:** Step-by-step checklist in [rollout-checklist.md](rollout-checklist.md)
- **GitHub Docs:** [docs.github.com/code-security](https://docs.github.com/en/code-security)
- **GitHub Community:** [github.community](https://github.community)
