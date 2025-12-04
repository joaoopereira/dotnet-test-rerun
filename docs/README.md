# Documentation Site

This directory contains the GitHub Pages documentation website for dotnet-test-rerun.

## Structure

- `_config.yml` - Jekyll configuration
- `_layouts/` - Custom layout templates
- `index.md` - Homepage
- `installation.md` - Installation guide
- `usage.md` - Usage documentation
- `configuration.md` - Configuration options
- `examples.md` - Real-world examples
- `docker.md` - Docker usage guide
- `contributing.md` - Contribution guidelines
- `Gemfile` - Ruby dependencies for Jekyll

## Local Development

### Prerequisites

- Ruby 3.x
- Bundler

### Setup

```bash
cd docs
bundle install
```

### Build

```bash
bundle exec jekyll build
```

The site will be built to the `_site` directory.

### Serve Locally

```bash
bundle exec jekyll serve
```

Then open http://localhost:4000/dotnet-test-rerun/ in your browser.

## Deployment

The site is automatically deployed to GitHub Pages when changes are pushed to the `main` branch via the `.github/workflows/pages.yml` workflow.

The site will be available at: https://joaoopereira.github.io/dotnet-test-rerun/

## Theme

The site uses the Cayman theme with custom navigation and styling.
