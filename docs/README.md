# Documentation site

This folder is the source of https://joaoopereira.github.io/dotnet-test-rerun/. GitHub Pages builds it with Jekyll and the [Just the Docs](https://just-the-docs.com/) theme (`remote_theme` in `_config.yml`). No workflow in this repository builds it: GitHub Pages builds the site from this folder (repository Settings → Pages).

## Pages

| File | Page | Contents |
|---|---|---|
| `index.md` | Home | What the tool does, quick start, links |
| `installation.md` | Installation | Global and local tool, versions, `PATH` |
| `usage.md` | Command reference | **Every option, once.** Exit codes |
| `how-it-works.md` | How it works | Rerun loop, failure detection, filters, results folders |
| `configuration.md` | Guides | Advice per topic: reruns, limits, logging, coverage, run settings |
| `ci.md` | CI integration | GitHub Actions, Azure DevOps, GitLab CI |
| `docker.md` | Docker | Image tags and usage |
| `troubleshooting.md` | Troubleshooting | Common problems |
| `contributing.md` | Contributing | Development setup, tests, commit format |

Keep each fact in one place and link to it. The option table belongs in `usage.md`; the other pages and the repository `README.md` link to it instead of copying it. When an option or behaviour changes, check `usage.md` first, then search the folder for the option name.

Pages are ordered in the sidebar by `nav_order` in their front matter. Long pages start with a table of contents:

```markdown
<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>
```

Callouts (`{: .note }`, `{: .warning }`) are defined in `_config.yml`.

## Preview locally

Requires Ruby 3.1 or later and Bundler.

```bash
cd docs
bundle install
bundle exec jekyll serve
```

Open http://localhost:4000/dotnet-test-rerun/. The site rebuilds when a file changes (not `_config.yml`: restart the server for that).
