---
name: triage-issues
description: Review open GitHub issues and PRs of dotnet-test-rerun, map each issue to its PR and preview package (or legacy alpha), check whether the reporter gave feedback, propose follow-up comments, and rebase outdated PR branches. Use when asked to review/triage issues, chase feedback, or update PRs.
---

# Triage issues, PRs, previews and feedback

Repo: `joaoopereira/dotnet-test-rerun`. Use the GitHub MCP tools (`list_issues`, `list_pull_requests`, `issue_read get_comments`, `pull_request_read get_comments`, `list_releases`).

## 1. Collect (in parallel)

- Open issues: `fields: number,title,user,comments,created_at,updated_at`.
- Open PRs: `fields: number,title,draft,head,body,updated_at`. The PR body contains `Fixes #N`.
- Releases: the output is large and gets saved to a file. Summarise it with python (`tag_name`, `published_at`, `prerelease`).
- The comments of every open issue.

## 2. Map a preview to its PR

- **Current process:** preview versions are `X.Y.Z-pr.<PR>.<run>`. The PR number is in the version, and the Preview workflow bot comments on the PR with each published version. To publish a new one, add the `preview` label to the PR (or push to a PR that already has it).
- **Legacy `X.Y.Z-alpha.N` tags** were released from PR branches, not from `main`. Map them like this:

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
| Preview/alpha shared, reporter did not answer | Ask for feedback again, naming the version (and the release link for legacy alphas) |
| Reporter answered "still broken" and the maintainer never replied | Do not ask them to retest the same version. Reply and ask for diagnostics (TRX file, exit code, full output). A new fix is needed |
| Maintainer asked a question, no answer | Follow up on that question |
| Feature request with no PR | Propose implementing it (skill `add-cli-option`) |

**Always show the plan (a table plus the draft comments) to the user and wait for approval before posting.** Comments are in English, and every comment ends with the Claude Code footer.

## 4. Rebase outdated PR branches (when asked)

```bash
git checkout -B rebase-N origin/<branch> && git rebase origin/main
```

- On conflicts, keep both behaviours. Then update new `main` tests that call changed signatures (e.g. NSubstitute stubs using the old overload no longer match).
- Skip (`git rebase --skip`) legacy `chore(release): X.Y.Z-alpha.N` commits. Under the preview process, versions are never committed on PR branches.
- Run the unit tests (and the integration tests for logic changes), then:
  `git push --force-with-lease=<branch>:<old-sha> origin rebase-N:<branch>`
- Restore `global.json` before switching branches (see CLAUDE.md).
