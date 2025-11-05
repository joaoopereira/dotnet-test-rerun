# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="3.3.0-alpha.1"></a>
## [3.3.0-alpha.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.3.0-alpha.1) (2025-11-05)

### Features

* support NUnit parameterized test filtering with test case precision ([9ffcaa8](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/9ffcaa84a2a53efc4c16861b658fc305e438435a))

<a name="3.3.0-alpha.0"></a>
## [3.3.0-alpha.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.3.0-alpha.0) (2025-10-27)

### Features

* add configurable max failures threshold option ([635f63d](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/635f63d9e6150b79885939fa272508bcb80e672e))

<a name="3.2.0"></a>
## [3.2.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.2.0) (2025-10-27)

### Bug Fixes

* resolve all .NET nullable reference warnings ([c58c1b3](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/c58c1b36310ffbdc80e616caffcc80b781f07f0f))

<a name="3.2.0-alpha.2"></a>
## [3.2.0-alpha.2](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.2.0-alpha.2) (2025-10-24)

<a name="3.2.0-alpha.1"></a>
## [3.2.0-alpha.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.2.0-alpha.1) (2025-10-24)

### Features

* add multi-target Dockerfile and docker-bake configuration for .NET 8.0 and 9.0 ([ac7d797](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/ac7d79781a5361ec7ee7efb754194a670220b32f))

<a name="3.2.0-alpha.0"></a>
## [3.2.0-alpha.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.2.0-alpha.0) (2025-10-24)

### Bug Fixes

* escape spaces in FullyQualifiedName filter expressions ([9c0ef5e](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/9c0ef5e7ec8bdb3793aa44a753f3c056e63eb48f))
* update test filter from contains (~) to exact match (=) ([b42da84](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/b42da843edb6b503f22ca5bc09a280b865ea1743))

<a name="3.1.1"></a>
## [3.1.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.1.1) (2025-08-11)

### Bug Fixes

* **dotnet:** add support for dotnet 8 ([350a1c9](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/350a1c94329908d9d7ada06f4641e44d2ef5da17))

<a name="3.1.0"></a>
## [3.1.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.1.0) (2025-06-27)

### Bug Fixes

* fixed test name construction: now always uses the real test method name instead of the DisplayName ([6ff8885](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/6ff8885c668b225975252c1206961d94220f4c7d))

<a name="3.0.0"></a>
## [3.0.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v3.0.0) (2025-01-22)

### Features

* bump target framework to net9.0 ([4dc36ba](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/4dc36babfa58b39df64dd1177a12124dc01c8301))
* bump target framework to net9.0 ([37069a9](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/37069a997343caca0cbaa18df08378e199b14ef4))

### Breaking Changes

* bump target framework to net9.0 ([4dc36ba](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/4dc36babfa58b39df64dd1177a12124dc01c8301))
* bump target framework to net9.0 ([37069a9](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/37069a997343caca0cbaa18df08378e199b14ef4))

<a name="2.1.0"></a>
## [2.1.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v2.1.0) (2024-10-01)

<a name="2.0.2"></a>
## [2.0.2](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v2.0.2) (2024-07-29)

### Bug Fixes

* different .net versions test fixed ([cf37bf1](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/cf37bf1e64ae2cfa65b50dd873d1964a2c368042))
* fix test for ubuntu agent ([386e0d6](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/386e0d67176716e2a6cd65af279bd3578ce0d246))
* multiple .net versions issue fixed ([979f78c](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/979f78cb64515c516205c98ffd0da499b5a36d96))
* update test project from net48 to net 6 for ubuntu agent compatibility ([7916185](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/7916185ae1f5c6a58edd49a383c75bc640eb7c69))

<a name="2.0.1"></a>
## [2.0.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v2.0.1) (2024-07-20)

<a name="2.0.1-alpha.0"></a>
## [2.0.1-alpha.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v2.0.1-alpha.0) (2024-07-07)

### Features

* refactor in logger ([9bf4b1c](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/9bf4b1c95dbf5f2e13e0e5f0eb6fffa515174009))

<a name="2.0.0"></a>
## [2.0.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v2.0.0) (2024-02-15)

### Features

## What's Changed
* chore(deps): bump kzrnm/get-net-sdk-project-versions-action from 1 to 2 by @dependabot in https://github.com/joaoopereira/dotnet-test-rerun/pull/134
* feat: bump target framework to net8.0 by @ricardofslp in https://github.com/joaoopereira/dotnet-test-rerun/pull/133

<a name="1.9.0"></a>
## [1.9.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.9.0) (2024-01-10)

### Features

* add environment option [#104](https://www.github.com/joaoopereira/dotnet-test-rerun/issues/104) ([3d46d16](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/3d46d1624ac6e5bb4e3fe52241b557d86c6bd4e8))

<a name="1.8.0"></a>
## [1.8.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.8.0) (2023-11-24)

### Features

* Add new input inlineRunSettings #95 ([9b7832f](https://github.com/joaoopereira/dotnet-test-rerun/commit/9b7832f7e936b1160a309e33ac856407d3ba7f71))
* Add inline runsettings as possibility to run command #95 ([2a0774d](https://github.com/joaoopereira/dotnet-test-rerun/commit/2a0774dbd87282f062859092c1117f08586cf7a2))
* Allow multiple logger values #96 ([7388973](https://github.com/joaoopereira/dotnet-test-rerun/commit/7388973e2d257cf67a098d5d56aae1f4018bbc82))

<a name="1.7.0"></a>
## [1.7.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.7.0) (2023-10-31)

<a name="1.6.1"></a>
## [1.6.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.6.1) (2023-10-11)

### Bug Fixes

* Allow MergeCoverageFormat to default to null ([6bfda1d](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/6bfda1d49b0323d8609257ae9ec6a8413707576e))

<a name="1.6.0"></a>
## [1.6.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.6.0) (2023-10-06)

<a name="1.5.0"></a>
## [1.5.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.5.0) (2023-9-9)

### Features

* allow the processing of multiple trx files ([3d82209](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/3d822098115c4f4d57a8474fef604960c2c4dae2))

<a name="1.4.2-alpha.0"></a>
## [1.4.2-alpha.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.4.2-alpha.0) (2023-8-16)

<a name="1.4.1"></a>
## [1.4.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.4.1) (2023-8-16)

### Bug Fixes

* add option to merge coverage report lost in previous rebase ([ea9c73e](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/ea9c73e01b50547bdfd5f82b2c96bf898d9a3565))

<a name="1.4.0"></a>
## [1.4.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.4.0) (2023-8-2)

### Features

* add merge coverage report capability ([86a2d4b](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/86a2d4b231779e3f02d12136bc4e1c0165b77bca))
* add option to collect coverage reports ([c0afe43](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/c0afe43b58a04821f56ee06cd67b1dbe983184e7))
* allow properties to be set (/p:) on execution of dotnet test rerun ([619e904](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/619e9042ff6767c7d53096b74a162b67580fb941))

<a name="1.3.1"></a>
## [1.3.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.3.1) (2023-7-4)

### Bug Fixes

* return status code 0 if pass after first failure #45 ([4a41541](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/4a41541519e50658a829d88995f178d069947854))

<a name="1.3.0"></a>
## [1.3.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.3.0) (2023-7-3)

### Features

* add option to delete report files ([8aa07d8](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/8aa07d882f2686bcd1ee00da8dc487041edaa4ad))

### Bug Fixes

* add additional tests for several tests inside data method failing ([790ca82](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/790ca821be6ec3f2462718e4e61d160ed30f3995))
* return failure return code to process when tests fail #45 ([b68688e](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/b68688eb3fa1a9f90d24220918c640284433a2d4))
* solve warnings and issue in tests ([cb925d0](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/cb925d0573e9cfa6172563f560ed03af768c2e1b))

<a name="1.2.1"></a>
## [1.2.1](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.2.1) (2023-6-15)

### Bug Fixes

* issue with rerun not using the filter with failed tests ([8533383](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/853338384f612ccbc6fe6fbd334238f815f32bc8))

<a name="1.2.0"></a>
## [1.2.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.2.0) (2023-6-9)

### Features

* add blame option ([a7ffe34](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/a7ffe342a84f4202a56557bd676cf9e07ef551aa))

<a name="1.1.0"></a>
## [1.1.0](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.1.0) (2023-3-21)

### Features

* add delay option to rerun command (issue #2) ([18e095c](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/18e095c895923d6596e3503689f62f6d63317bde))

<a name="1.0.13"></a>
## [1.0.13](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.13) (2023-3-2)

### Bug Fixes

* solved issue of running the rerun one less time then the given by parameter ([256248d](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/256248d314b006ef5806a57413e52ffd65d0e5fb))

<a name="1.0.12"></a>
## [1.0.12](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.12) (2023-2-23)

<a name="1.0.11"></a>
## [1.0.11](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.11) (2023-2-23)

### Bug Fixes

* **options:** set --settings as optional ([02c9d36](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/02c9d361aae5a08b6ca010d44d961f77c50212e5))
* **rerun:** handle no file is found scenario and use same folder when no results directory is given ([939111a](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/939111a98527f927bdeedfd47d2b89834703b722))

<a name="1.0.10"></a>
## [1.0.10](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.10) (2023-2-15)

### Bug Fixes

* **options:** set --settings as optional ([1fab9a8](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/1fab9a8b783d3e52dd93fcf5eaa1d18650a0032c))

<a name="1.0.9"></a>
## [1.0.9](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.9) (2023-2-14)

### Bug Fixes

* **dotnet:** use StdOut and StdErr stream asynchronously ([728e3bf](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/728e3bf73d15590adb4fea6b230ab3089c99bf40))
* **rerun:** allow filter option as optional ([3bfb93b](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/3bfb93bb9b1575341a26a91be19a1772784f9b05))

<a name="1.0.8"></a>
## [1.0.8](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.8) (2023-2-14)

### Bug Fixes

* readme typo ([82c9ef9](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/82c9ef96af37e4cad728c2bfbd9d79dafd94899d))
* **dotnet:** add default working directory ([3c84d0f](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/3c84d0fb9546b2063daa23bac97787549aa1840e))
* **entrypoint:** replace try catch with UseExceptionHandler ([7718584](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/771858450c6152e15087d573da30d40874a3ceee))
* **log:** add missing loglevel option ([bf9eb7c](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/bf9eb7cb0b39fa93159704dec4a2c4361326c8f6))
* **log:** do not use AnsiConsole if is not running in terminal ([d8179c8](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/d8179c887cf51ed99f476a86d27e3e115378e61b))
* **logging:** loglevel not setted and exitcode handling ([23985c6](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/23985c6c63c5ce94495c26b669404a959d606a94))

<a name="1.0.7"></a>
## [1.0.7](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.7) (2023-2-13)

### Bug Fixes

* **dotnet:** add default working directory ([4783ab2](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/4783ab2e7c20e57a497c6ac72c0f7419b35d2d9e))

<a name="1.0.6"></a>
## [1.0.6](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.6) (2023-2-13)

<a name="1.0.5"></a>
## [1.0.5](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.5) (2023-2-13)

### Bug Fixes

* **logging:** loglevel not setted and exitcode handling ([22d4031](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/22d403173f29e98d40ec19438c4075cdad30f7e5))

<a name="1.0.4"></a>
## [1.0.4](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.4) (2023-2-13)

### Bug Fixes

* add missing loglevel option ([d79e0ba](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/d79e0ba2b4c2309b7e01a3e2f8cdb65685376a9a))

<a name="1.0.3"></a>
## [1.0.3](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.3) (2023-2-13)

<a name="1.0.2"></a>
## [1.0.2](https://www.github.com/joaoopereira/dotnet-test-rerun/releases/tag/v1.0.2) (2023-2-13)

### Bug Fixes

* readme typo ([efb4841](https://www.github.com/joaoopereira/dotnet-test-rerun/commit/efb48417c8469b56d9833bd26570b9e114d0c75f))

