# GHAS Rollout Checklist

A step-by-step checklist for rolling out GitHub Advanced Security (GHAS) across your organization. Each phase builds on the previous one — complete the pilot before expanding.

---

## Phase 1: Planning

Lay the groundwork before enabling any features.

- [ ] **Identify 3–5 pilot repositories** — choose repos that are actively developed, cover your main languages, and have willing teams
- [ ] **Assign a security champion for each pilot team** — this person is the point of contact for GHAS questions and alert triage
- [ ] **Review GHAS licensing requirements** — GHAS requires GitHub Enterprise Cloud or GitHub Enterprise Server 3.0+; verify your plan includes GHAS seats
- [ ] **Audit current security tooling** — document existing SAST/SCA/secret scanning tools so you can plan migration or integration
- [ ] **Define alert severity thresholds** — decide which severities will block merges (recommended: start with `critical` and `high`)
- [ ] **Establish SLAs for alert remediation** — e.g., Critical: 48 hours, High: 1 week, Medium: 30 days, Low: best effort
- [ ] **Set up a communication channel** — create a Slack/Teams channel or mailing list for GHAS rollout coordination
- [ ] **Schedule kickoff meeting with pilot teams** — walk through GHAS features, the rollout timeline, and expectations

> **💡 Tip:** Don't try to enable everything at once. Start with secret scanning (lowest friction, highest immediate value), then Dependabot, then CodeQL.

> **📝 Note:** GHAS features are free for public repositories on GitHub.com. You only need a GHAS license for private/internal repositories.

---

## Phase 2: Pilot Deployment

Enable GHAS features on your pilot repositories and gather feedback.

### Secret Scanning
- [ ] **Enable secret scanning** on all pilot repos (Settings → Code security and analysis)
- [ ] **Enable push protection** — blocks pushes containing detected secrets before they enter history
- [ ] **Review initial secret scanning alerts** — remediate any existing leaked secrets found in history
- [ ] **Document the process for handling blocked pushes** — share with pilot teams (see [push protection reference](../.github/skills/secret-scanning/references/push-protection.md))

### Dependabot
- [ ] **Enable Dependabot alerts** on pilot repos
- [ ] **Enable Dependabot security updates** — auto-generates PRs for vulnerable dependencies
- [ ] **Add `dependabot.yml`** with version updates configured for primary ecosystems (see [example configs](../.github/skills/dependabot/references/example-configs.md))
- [ ] **Configure dependency grouping** to reduce PR volume (see [Dependabot skill guide](../.github/skills/dependabot/SKILL.md))

### CodeQL
- [ ] **Enable CodeQL with default setup** on pilot repos (Settings → Code security and analysis → Code scanning)
- [ ] **Verify CodeQL runs successfully** — check the Actions tab for the initial analysis
- [ ] **Review initial code scanning alerts** — triage obvious false positives and true positives
- [ ] **Set up the dependency review action** on PRs (see [advanced scenarios](advanced-scenarios.md#5-dependency-review-in-cicd))

### Training & Feedback
- [ ] **Conduct a 30-minute training session** for pilot team members — cover alert triage, dismissal workflows, and Dependabot PR commands
- [ ] **Share quick-reference docs** — point teams to the skill guides in `.github/skills/`
- [ ] **Collect feedback for 2–4 weeks** — track pain points, false positive rates, and developer experience
- [ ] **Hold a retrospective** at the end of the pilot — document what worked, what didn't, and what needs tuning

> **💡 Tip:** The most common pilot feedback is "too many Dependabot PRs" and "CodeQL false positives." Both are addressed in Phase 3.

> **📝 Note:** Default CodeQL setup works well for most repositories. Only switch to advanced setup if you need custom queries, non-standard build steps, or path filtering.

---

## Phase 3: Configuration Refinement

Tune GHAS settings based on pilot feedback before rolling out broadly.

### CodeQL Tuning
- [ ] **Review false positives** — dismiss with appropriate reasons and note patterns
- [ ] **Switch to advanced setup** if needed — for custom build steps, path filtering, or query suite changes (see [workflow configuration](../.github/skills/codeql/references/workflow-configuration.md))
- [ ] **Adjust query suites** — use `security-extended` for more coverage or `security-and-quality` for code quality rules
- [ ] **Add path filters** to exclude generated code, vendored dependencies, or test fixtures
- [ ] **Set analysis `category`** for monorepos to separate alerts by service (see [monorepo scanning](advanced-scenarios.md#1-monorepo-code-scanning))

### Dependabot Tuning
- [ ] **Group dependency updates** by type (production vs. development) and update type (minor/patch vs. major)
- [ ] **Adjust schedule** — weekly is recommended; avoid daily unless PR volume is manageable
- [ ] **Configure `open-pull-requests-limit`** to cap the number of open Dependabot PRs (default: 5)
- [ ] **Set up ignore rules** for dependencies that can't be updated (e.g., pinned for compatibility)
- [ ] **Add labels** to Dependabot PRs for easier filtering (e.g., `dependencies`, `security`)

### Secret Scanning Tuning
- [ ] **Create custom secret scanning patterns** for internal secrets (API keys, internal tokens, etc.) — see [custom patterns reference](../.github/skills/secret-scanning/references/custom-patterns.md)
- [ ] **Define push protection bypass policies** — decide who can bypass and what approval process is required
- [ ] **Enable non-provider patterns** if available — catches generic high-entropy strings
- [ ] **Configure exclusion paths** in `.github/secret_scanning.yml` for test fixtures or example files

### Branch Protection
- [ ] **Require CodeQL status checks** to pass before merging (Settings → Branches → Branch protection rules)
- [ ] **Require dependency review** as a status check on PRs
- [ ] **Set merge restrictions** based on alert severity — block merges with critical/high findings

> **💡 Tip:** Start by requiring security checks in "warning" mode (not blocking) for a sprint, then switch to blocking once teams are comfortable.

> **📝 Note:** Keep a shared document of all tuning decisions — this becomes your "GHAS Configuration Standard" for Phase 4.

---

## Phase 4: Broad Rollout

Expand GHAS to all repositories with your refined configuration.

### Organization-Wide Enablement
- [ ] **Enable secret scanning + push protection** at the org level (Organization → Settings → Code security and analysis)
- [ ] **Enable Dependabot alerts + security updates** at the org level
- [ ] **Enable CodeQL default setup** at the org level for supported languages
- [ ] **Apply your standard `dependabot.yml`** across all repos (see [org-wide Dependabot config](advanced-scenarios.md#4-organization-wide-dependabot-config))

### Standardization
- [ ] **Create repository rulesets** to enforce security checks on all repos
- [ ] **Distribute the dependency review workflow** to all repos
- [ ] **Publish an internal GHAS onboarding guide** based on your pilot learnings
- [ ] **Add GHAS setup to your repository template** so new repos are configured from day one

### Monitoring & Process
- [ ] **Set up Security Overview dashboard** monitoring (see [Security Overview](advanced-scenarios.md#7-github-security-overview))
- [ ] **Configure alert notifications** — route critical alerts to the appropriate team channels
- [ ] **Establish a recurring security review cadence** — weekly for critical alerts, monthly for overall trends
- [ ] **Document runbooks for common alert types** — how to fix SQL injection, XSS, hardcoded secrets, etc.
- [ ] **Set up automated alert triage** if needed (see [GHAS API and Automation](advanced-scenarios.md#6-ghas-api-and-automation))

### Communication
- [ ] **Announce the rollout** to all engineering teams with a timeline and FAQ link
- [ ] **Schedule "office hours"** for the first 2–4 weeks — let teams ask questions in real time
- [ ] **Share a weekly rollout status update** — coverage %, alert trends, and common issues

> **💡 Tip:** Roll out to teams in waves (e.g., 20% per week) rather than enabling everything at once. This lets you catch org-wide issues early.

> **📝 Note:** Some repos will need special handling — monorepos, repos with complex build systems, or repos with legacy code that generates many alerts. Identify these early and plan accordingly.

---

## Phase 5: Maturity

Mature your GHAS program with advanced capabilities and metrics.

### Advanced Features
- [ ] **Develop custom CodeQL query packs** for org-specific patterns (see [custom queries](advanced-scenarios.md#2-custom-codeql-queries-for-org-specific-rules))
- [ ] **Integrate GHAS with your ticketing system** (Jira, Azure DevOps, etc.) — see [ticketing integration](advanced-scenarios.md#8-integration-with-ticketing-systems)
- [ ] **Set up third-party scanner integration** if needed — upload SARIF from additional tools (see [SARIF upload](advanced-scenarios.md#3-third-party-scanner-integration-sarif-upload))
- [ ] **Implement automated alert triage workflows** — auto-dismiss test-only findings, auto-assign alerts to code owners

### Metrics & KPIs
- [ ] **Track mean time to remediation (MTTR)** — measure how quickly alerts are fixed
- [ ] **Track alert backlog trends** — open vs. closed alerts over time
- [ ] **Track GHAS coverage** — percentage of repos with each feature enabled
- [ ] **Track false positive rate** — percentage of alerts dismissed as false positives
- [ ] **Track developer experience** — survey teams quarterly on friction and satisfaction
- [ ] **Report to leadership** — monthly security posture summary with trends and key metrics

### Continuous Improvement
- [ ] **Review and update custom query packs** quarterly
- [ ] **Review `dependabot.yml` configurations** quarterly — adjust grouping, schedules, and ignore rules
- [ ] **Review secret scanning custom patterns** as new internal secret formats are introduced
- [ ] **Update branch protection rules and rulesets** as your security bar rises
- [ ] **Conduct advanced CodeQL training** for interested developers — query writing, pack development
- [ ] **Participate in the GitHub security community** — contribute custom queries, share patterns
- [ ] **Run periodic security retrospectives** — review incidents and near-misses to improve detection

> **💡 Tip:** Mature organizations typically maintain a "Security Champions" program where one developer per team acts as a GHAS expert and liaison.

> **📝 Note:** GHAS features evolve rapidly. Subscribe to the [GitHub changelog](https://github.blog/changelog/) and review new features quarterly for adoption opportunities.

---

## Quick Reference: Rollout Timeline

| Phase | Duration | Key Milestone |
|-------|----------|---------------|
| 1. Planning | 1–2 weeks | Pilot repos selected, SLAs defined |
| 2. Pilot Deployment | 2–4 weeks | All features running on pilot repos |
| 3. Configuration Refinement | 2–4 weeks | False positives addressed, PR volume manageable |
| 4. Broad Rollout | 4–8 weeks | All repos covered, monitoring active |
| 5. Maturity | Ongoing | Custom queries, metrics, continuous improvement |

**Total estimated time from start to broad coverage: 3–5 months**

---

## Related Resources

- [Advanced Scenarios](advanced-scenarios.md) — monorepo scanning, custom queries, SARIF upload, and more
- [FAQ](faq.md) — common questions from teams adopting GHAS
- [CodeQL skill guide](../.github/skills/codeql/SKILL.md) — step-by-step CodeQL setup
- [Dependabot skill guide](../.github/skills/dependabot/SKILL.md) — step-by-step Dependabot setup
- [Secret scanning skill guide](../.github/skills/secret-scanning/SKILL.md) — step-by-step secret scanning setup
