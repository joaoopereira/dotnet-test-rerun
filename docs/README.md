# Documentation Development Guide

This directory contains the GitHub Pages documentation for dotnet-test-rerun.

## Theme

The documentation uses the [Just the Docs](https://just-the-docs.com/) theme, which provides:
- 🌙 Dark mode by default (configured in `_config.yml`)
- 📚 Documentation-focused layout with navigation sidebar
- 🔍 Built-in search functionality
- 📱 Responsive design
- 📋 Copy code button for code blocks
- ⬆️ Back to top button

## Prerequisites

To work with the documentation locally, you need:
- Ruby 3.1 or higher
- Bundler gem

## Setup

### Using the Devcontainer

The easiest way is to use the devcontainer which comes pre-configured with all required tools:

1. Open the repository in VS Code
2. Click "Reopen in Container" when prompted (or use Command Palette: "Dev Containers: Reopen in Container")
3. Wait for the container to build and initialize
4. The devcontainer will automatically install Ruby and run `bundle install` in the docs directory

### Manual Setup

If you're not using the devcontainer:

1. Install Ruby 3.1 or higher:
   ```bash
   # On Ubuntu/Debian
   sudo apt-get update
   sudo apt-get install ruby-full build-essential zlib1g-dev
   
   # On macOS
   brew install ruby
   ```

2. Install Bundler:
   ```bash
   gem install bundler
   ```

3. Install dependencies:
   ```bash
   cd docs
   bundle install
   ```

## Building and Serving Locally

### Start the Jekyll Server

```bash
cd docs
bundle exec jekyll serve
```

The documentation will be available at `http://localhost:4000/dotnet-test-rerun/`

### Live Reload

Jekyll automatically watches for file changes and rebuilds the site. Just refresh your browser to see changes.

### Build Only (No Server)

```bash
cd docs
bundle exec jekyll build
```

The static site will be generated in the `docs/_site` directory.

## Making Changes

### Adding a New Page

1. Create a new `.md` file in the `docs/` directory
2. Add front matter with layout, title, and nav_order:
   ```yaml
   ---
   layout: default
   title: My New Page
   nav_order: 8
   ---
   ```
3. Write your content in Markdown

### Updating Navigation Order

Edit the `nav_order` in the front matter of each page. Pages are sorted by this value in the sidebar.

### Customizing the Theme

The theme is configured in `_config.yml`. Key settings:

- `color_scheme: dark` - Enables dark mode
- `search_enabled: true` - Enables search
- `enable_copy_code_button: true` - Adds copy button to code blocks
- `back_to_top: true` - Shows back to top button

For more customization options, see the [Just the Docs documentation](https://just-the-docs.com/).

## Deployment

The documentation is automatically deployed to GitHub Pages when changes are pushed to the `main` branch:

1. The `.github/workflows/pages.yml` workflow is triggered
2. Jekyll builds the site
3. The static site is deployed to GitHub Pages

The live site is available at: https://joaoopereira.github.io/dotnet-test-rerun/

## Troubleshooting

### Bundle Install Fails

If `bundle install` fails, try updating Bundler:
```bash
gem update --system
gem install bundler
```

### Jekyll Server Won't Start

Make sure you're in the `docs` directory and all dependencies are installed:
```bash
cd docs
bundle install
bundle exec jekyll serve
```

### Changes Not Appearing

- Make sure Jekyll is running with `bundle exec jekyll serve`
- Check the terminal for build errors
- Hard refresh your browser (Ctrl+Shift+R or Cmd+Shift+R)

### Port 4000 Already in Use

Kill the existing process or use a different port:
```bash
bundle exec jekyll serve --port 4001
```

## File Structure

```
docs/
├── _config.yml           # Jekyll configuration and theme settings
├── _layouts/             # Custom layouts (not used with remote theme)
│   └── default.html
├── Gemfile               # Ruby dependencies
├── Gemfile.lock          # Locked dependency versions
├── index.md              # Home page (nav_order: 1)
├── installation.md       # Installation guide (nav_order: 2)
├── usage.md              # Usage guide (nav_order: 3)
├── configuration.md      # Configuration guide (nav_order: 4)
├── examples.md           # Examples (nav_order: 5)
├── docker.md             # Docker guide (nav_order: 6)
├── contributing.md       # Contributing guide (nav_order: 7)
└── README.md             # This file
```

## Resources

- [Just the Docs Theme Documentation](https://just-the-docs.com/)
- [Jekyll Documentation](https://jekyllrb.com/docs/)
- [GitHub Pages Documentation](https://docs.github.com/en/pages)
- [Kramdown Syntax](https://kramdown.gettalong.org/syntax.html)
