const path = require('path');
const tl = require('azure-pipelines-task-lib/task');

function toolDirectory(version) {
    // A pinned version is cached on the agent; "latest" goes to a fresh folder so it is never stale.
    return version
        ? path.join(tl.getVariable('Agent.ToolsDirectory'), 'dotnet-test-rerun', version)
        : path.join(tl.getVariable('Agent.TempDirectory'), 'dotnet-test-rerun', `latest-${Date.now()}`);
}

async function install(version) {
    const dir = toolDirectory(version);
    const exe = path.join(dir, process.platform === 'win32' ? 'test-rerun.exe' : 'test-rerun');
    if (tl.exist(exe)) {
        console.log(`dotnet-test-rerun ${version} found in ${dir}`);
        return exe;
    }
    const dotnet = tl.tool(tl.which('dotnet', true)).arg(['tool', 'install', 'dotnet-test-rerun', '--tool-path', dir]);
    if (version) dotnet.arg(['--version', version]);
    await dotnet.execAsync();
    return exe;
}

function publish(resultsDirectory) {
    const files = tl.findMatch(resultsDirectory, '**/*.trx');
    if (files.length === 0) {
        tl.warning(`No TRX files found in ${resultsDirectory}.`);
        return;
    }
    const runTitle = tl.getInput('testRunTitle') || undefined;
    new tl.TestPublisher('VSTest').publish(files, 'false', undefined, undefined, runTitle, 'true');
}

async function run() {
    const cwd = tl.getPathInput('workingDirectory') || tl.getVariable('System.DefaultWorkingDirectory') || process.cwd();
    const resultsDirectory = path.resolve(cwd, tl.getInput('resultsDirectory') || path.join(tl.getVariable('Agent.TempDirectory'), 'TestResults'));
    tl.setVariable('resultsDirectory', resultsDirectory, false, true);
    let code = 1;
    try {
        const exe = await install(tl.getInput('version') || '');
        const testPath = tl.getInput('path');
        const testRerun = tl.tool(exe);
        if (testPath) testRerun.arg(testPath);
        testRerun.arg(['--results-directory', resultsDirectory]).line(tl.getInput('arguments') || '');
        code = await testRerun.execAsync({
            cwd,
            ignoreReturnCode: true
        });
    } finally {
        if (tl.getBoolInput('publishTestResults')) publish(resultsDirectory);
    }
    if (code === 0) tl.setResult(tl.TaskResult.Succeeded, 'All tests passed.');
    else tl.setResult(tl.TaskResult.Failed, `test-rerun exited with code ${code}.`);
}

run().catch(err => tl.setResult(tl.TaskResult.Failed, err.message));
