# Release Process

## Preview Packages for Pull Requests
To let a reporter try a fix or feature before it is merged, add the **`preview`** label to the pull request.

The [Preview workflow](.github/workflows/preview.yml) runs the unit tests and packs the PR head. It publishes a prerelease to NuGet and comments on the PR with the install command. Nothing is committed to the branch.

- Version format: `<next patch>-pr.<PR number>.<run number>`, for example `4.2.1-pr.397.12`. It sorts above the current release and below the next one.
- Every push to a PR that has the label publishes a new build with a higher run number.
- Only PRs from branches of this repository can publish (forks have no access to secrets).
- Share the version from the bot comment on the issue:
  ```text
  Version 4.2.1-pr.397.12 released. Can you please try and give feedback?
  ```

Do **not** run `dotnet r bump` on PR branches. Release commits in PR branches produce duplicated versions and rebase conflicts.

## Pre-Releases
Only on `main`, when a pre-release of what is already merged is needed. On the repo root folder run:
```sh
dotnet r bump
```

this will output something like this:
```text
V bumping version from 1.4.1 to 1.4.2-alpha.0 in projects

---
<a name="1.4.2-alpha.0"></a>
## [1.4.2-alpha.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.4.2-alpha.0) (2023-8-16)
---

V updated CHANGELOG.md
```

on github, go to [Releases](https://www.github.com/joaoopereira/dotnet-test-rerun/releases) and follow these steps:
1. Click on **Draft a new Release**
2. Select the new pushed tag: **1.4.2-alpha.0** (example)
3. Click on **Generate release notes**
4. :exclamation:IMPORTANT:exclamation: Click on **Set as a pre-release**
5. Click on **Publish release**

## Releases
on the repo root folder run:
```sh
dotnet r bump:live
```

this will output something like this:
```text
V bumping version from 1.4.2-alpha.0 to 1.4.2 in projects

---
<a name="1.4.2"></a>
## [1.4.2](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.4.2) (2023-8-16)
---

V updated CHANGELOG.md
```

on github, go to [Releases](https://www.github.com/joaoopereira/dotnet-test-rerun/releases) and follow these steps:
1. Click on **Draft a new Release**
2. Select the new pushed tag: **v1.4.2** (example)
3. Click on **Generate release notes**
4. Click on **Publish release**