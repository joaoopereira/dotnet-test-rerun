---
name: verify-end-to-end
description: Run the locally built dotnet-test-rerun tool against a real fixture test project to confirm a fix or feature works outside unit tests (filters, rerun behaviour, console output). Use before pushing changes to filter building, the rerun loop, or output.
---

# Verify end to end with a fixture project

Fixtures live in `test/dotnet-test-rerun.IntegrationTests/Fixtures/`. Pick one that fits the change:

| Scenario | Fixture |
|---|---|
| xUnit, always fails (Theory with string params) | `XUnitExampleFailInAllRetries` |
| xUnit, passes on 2nd run | `XUnitTestPassOnSecondRunExample` |
| NUnit parameterised, passes on 2nd run | `NUnitTestParameterizedPassOnSecondRunExample` |
| NUnit, passes on 2nd run | `NUnitTestPassOnSecondRunExample` |
| MSTest, failing | `FailingMSTestExample` |
| Multiple target frameworks | `FailingXUnitWithMultipleFrameworksExample` (needs the .NET 9 runtime, which is missing locally) |

## Steps

```bash
S=<scratchpad dir>
dotnet build src/dotnet-test-rerun.csproj            # produces src/bin/Debug/net10.0/test-rerun.dll
rm -rf $S/fx && cp -r test/dotnet-test-rerun.IntegrationTests/Fixtures/<Fixture> $S/fx
cp global.json $S/fx/                                # same SDK pin as the repo (local rewrite)
cd $S/fx
# Only if the needed runtime is missing (fixtures target net8.0):
# sed -i 's/net8.0/net10.0/' *.csproj
dotnet /home/user/dotnet-test-rerun/src/bin/Debug/net10.0/test-rerun.dll <Fixture>.csproj \
  --rerunMaxAttempts 1 -r results <options under test>
```

- Always work on a copy. "Pass on second run" fixtures keep state on disk and only work once per copy.
- If a filter is printed, paste it into a plain `dotnet test --filter "..."` and check that the expected tests (and only those) run. `No test matches the given testcase filter` means the filter is wrong.
- The full integration suite (`dotnet test test/dotnet-test-rerun.IntegrationTests/...`) takes about 6 minutes. 26/27 is the expected local result (the .NET 9 runtime is missing).
