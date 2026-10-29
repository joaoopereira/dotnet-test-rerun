---
name: triage-issues
description: Review open GitHub issues and PRs of dotnet-test-rerun, map each issue to its PR and alpha pre-release, check whether the reporter gave feedback, propose follow-up comments, and rebase outdated PR branches. Use when asked to review/triage issues, chase feedback, or update PRs.
---

# Triage issues, PRs, alphas and feedback

Repo: `joaoopereira/dotnet-test-rerun`. Use the GitHub MCP tools (`list_issues`, `list_pull_requests`, `issue_read get_comments`, `pull_request_read get_comments`, `list_releases`).

## 1. Collect (in parallel)

- Open issues: `fields: number,title,user,comments,created_at,updated_at`.
- Open PRs: `fields: number,title,draft,head,body,updated_at`. The PR body contains `Fixes #N`.
- Releases: the output is large and gets saved to a file. Summarise it with python (`tag_name`, `published_at`, `prerelease`).
- The comments of every open issue.

## 2. Map an alpha to its PR

Alphas are often released from the PR branch, not from `main`:

```bash
git fetch --unshallow origin 2>/dev/null; git fetch --tags -q origin
git fetch -q origin <pr-branches> main
for t in <alpha tags>; do for b in <pr-branches>; do
  git merge-base --is-ancestor $t origin/$b && echo "$t in $b"; done; done
git rev-list --left-right --count origin/main...origin/<branch>   # behind / ahead
```

## 3. Classify each issue

| Situation | Action |
|---|---|
| Alpha shared, reporter did not answer | Ask for feedback again, naming the alpha with a link to its release |
| Reporter answered "still broken" and the maintainer never replied | Do not ask them to retest the same alpha. Reply and ask for diagnostics (TRX file, exit code, full output). A new fix is needed |
| Maintainer asked a question, no answer | Follow up on that question |
| Feature request with no PR | Propose implementing it (skill `add-cli-option`) |

**Always show the plan (a table plus the draft comments) to the user and wait for approval before posting.** Comments are in English, and every comment ends with the Claude Code footer.

## 4. Rebase outdated PR branches (when asked)

```bash
git checkout -B rebase-N origin/<branch> && git rebase origin/main
```

- On conflicts, keep both behaviours. Then update new `main` tests that call changed signatures (e.g. NSubstitute stubs using the old overload no longer match).
- Skip (`git rebase --skip`) a `chore(release): X.Y.Z-alpha.N` commit whose version is lower than `main`'s, because it would downgrade the csproj. A release commit that is still newer than `main` normally applies cleanly; keep it.
- Run the unit tests (and the integration tests for logic changes), then:
  `git push --force-with-lease=<branch>:<old-sha> origin rebase-N:<branch>`
- Restore `global.json` before switching branches (see CLAUDE.md).
