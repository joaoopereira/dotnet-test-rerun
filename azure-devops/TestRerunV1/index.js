const fs = require('fs');
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

// Options shared by every test file, mapped from the VSTest@3 input names.
function commonArguments() {
    const args = [];
    const add = (name, value) => { if (value) args.push(name, value); };
    add('--filter', tl.getInput('testFiltercriteria'));
    // An empty filePath input comes in as the sources folder, so only pass real files.
    const settings = tl.getPathInput('runSettingsFile');
    if (settings && tl.exist(settings) && fs.statSync(settings).isFile()) args.push('--settings', settings);
    if (tl.getBoolInput('codeCoverageEnabled')) args.push('--collect', 'Code Coverage');
    if (tl.getBoolInput('diagnosticsEnabled')) args.push('--blame');
    add('--logger', tl.getInput('customLoggerConfig'));
    if (tl.getBoolInput('rerunFailedTests')) {
        add('--rerunMaxAttempts', tl.getInput('rerunMaxAttempts'));
        if (tl.getInput('rerunType') === 'basedOnTestFailureCount') add('--rerunMaxFailedTests', tl.getInput('rerunFailedTestCasesMaxLimit'));
        else add('--rerunFailedThreshold', tl.getInput('rerunFailedThreshold'));
    } else {
        args.push('--rerunMaxAttempts', '0');
    }
    return args;
}

// VSTest syntax "-name value -other \"quoted value\"" becomes inline run settings after "--".
function testRunParameters() {
    const text = tl.getInput('overrideTestrunParameters') || '';
    const parameters = [...text.matchAll(/-(\S+)\s+(?:"([^"]*)"|(\S+))/g)]
        .map(([, name, quoted, plain]) => `TestRunParameters.Parameter(name="${name}", value="${quoted ?? plain}")`);
    return parameters.length ? ['--', ...parameters] : [];
}

function findTestFiles(searchFolder) {
    const patterns = tl.getDelimitedInput('testAssemblyVer2', '\n', true).map(p => p.trim()).filter(Boolean);
    const files = tl.findMatch(searchFolder, patterns, undefined, { nocase: process.platform === 'win32', matchBase: false, dot: true });
    return files.filter(file => fs.statSync(file).isFile());
}

// Distinct tests that ran (any outcome but NotExecuted), across every run of every test file.
function countExecutedTests(trxFiles) {
    const names = new Set();
    for (const file of trxFiles) {
        const xml = fs.readFileSync(file, 'utf8');
        for (const [, attrs] of xml.matchAll(/<UnitTestResult\b([^>]*)>/g)) {
            const name = /\btestName="([^"]*)"/.exec(attrs);
            const outcome = /\boutcome="([^"]*)"/.exec(attrs);
            if (name && (!outcome || outcome[1] !== 'NotExecuted')) names.add(name[1]);
        }
    }
    return names.size;
}

function publish(trxFiles) {
    if (trxFiles.length === 0) {
        tl.warning('No TRX files to publish.');
        return;
    }
    new tl.TestPublisher('VSTest').publish(
        trxFiles,
        'true', // one test run, like VSTest@3, with the reruns inside it
        tl.getInput('platform') || undefined,
        tl.getInput('configuration') || undefined,
        tl.getInput('testRunTitle') || undefined,
        tl.getBoolInput('publishRunAttachments') ? 'true' : 'false');
}

async function run() {
    const searchFolder = path.resolve(tl.getPathInput('searchFolder') || tl.getVariable('System.DefaultWorkingDirectory') || process.cwd());
    const resultsDirectory = path.resolve(searchFolder, tl.getInput('resultsFolder') || path.join(tl.getVariable('Agent.TempDirectory'), 'TestResults'));
    tl.setVariable('resultsDirectory', resultsDirectory, false, true);

    const files = findTestFiles(searchFolder);
    if (files.length === 0) throw new Error(`No test files match the patterns in ${searchFolder}.`);
    console.log(`Test files:\n  ${files.join('\n  ')}`);

    const exe = await install(tl.getInput('version') || '');
    const common = commonArguments();
    const runParameters = testRunParameters();
    const failed = [];
    try {
        for (const [index, file] of files.entries()) {
            // Each file gets its own folder, so its reruns only read its own TRX files.
            const results = path.join(resultsDirectory, `${index + 1}-${path.basename(file, path.extname(file))}`);
            const code = await tl.tool(exe)
                .arg(file)
                .arg(['--results-directory', results])
                .arg(common)
                .line(tl.getInput('otherConsoleOptions') || '')
                .arg(runParameters)
                .execAsync({ cwd: searchFolder, ignoreReturnCode: true });
            if (code !== 0) failed.push(`${path.basename(file)} (exit code ${code})`);
        }
    } finally {
        const trxFiles = tl.exist(resultsDirectory) ? tl.findMatch(resultsDirectory, '**/*.trx') : [];
        if (!tl.getBoolInput('donotPublishTestResults')) publish(trxFiles);
        if (tl.getBoolInput('failOnMinTestsNotRun')) {
            const minimum = parseInt(tl.getInput('minimumExpectedTests') || '1', 10);
            const ran = countExecutedTests(trxFiles);
            console.log(`${ran} distinct tests ran.`);
            if (ran < minimum) failed.push(`only ${ran} tests ran, ${minimum} expected`);
        }
    }

    if (failed.length === 0) tl.setResult(tl.TaskResult.Succeeded, 'All tests passed.');
    else tl.setResult(tl.TaskResult.Failed, `Tests failed: ${failed.join('; ')}.`);
}

run().catch(err => tl.setResult(tl.TaskResult.Failed, err.message));
