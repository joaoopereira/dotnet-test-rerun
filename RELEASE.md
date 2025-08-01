# Release Process

## Pre-Releases
on the repo root folder run:
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