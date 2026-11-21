# SharpFuzz + libFuzzer campaign driver for the MasterMemory lookups.
#
#   pwsh tests/MasterMemory.Tests.Fuzz/fuzz.ps1                       # fuzz everything until Ctrl+C
#   pwsh tests/MasterMemory.Tests.Fuzz/fuzz.ps1 -MaxTotalTimeSec 300  # bounded smoke run
#   pwsh tests/MasterMemory.Tests.Fuzz/fuzz.ps1 -Target all-keys      # one target
#
# Pipeline: publish (uninstrumented) -> sharpfuzz IL-instruments the target DLLs ->
# generate the seed corpus if absent -> libfuzzer-dotnet drives the harness exe.
# Findings land in findings/ as crash-*/timeout-*/oom-* files; reproduce one with
#   dotnet tests/MasterMemory.Tests.Fuzz/publish/MasterMemory.Tests.Fuzz.dll findings/crash-...
# (with the same FUZZ_TARGET the campaign used, if any).

#Requires -Version 7
param(
    [string]$Target = "",         # FUZZ_TARGET: exact target name, substring, or index
    [string]$Corpus = "",         # corpus dir; default <script dir>/corpus (seeded when empty)
    [int]$TimeoutSec = 10,        # per-input hang deadline
    [int]$RssLimitMb = 4096,      # generous: the CLR reserves a lot
    [int]$MaxLen = 512,           # 1 selector byte + 2 header bytes + 32 rows x 4 + 48 keys x 4, with room to spare
    [int]$MaxTotalTimeSec = 0,    # 0 = run until Ctrl+C
    [switch]$SkipBuild            # reuse the existing instrumented publish dir as-is
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$fuzzDir = $PSScriptRoot
$repoRoot = Split-Path (Split-Path $fuzzDir)
$publishDir = Join-Path $fuzzDir "publish"
$harnessDll = Join-Path $publishDir "MasterMemory.Tests.Fuzz.dll"
if (-not $Corpus) { $Corpus = Join-Path $fuzzDir "corpus" }
$findingsDir = Join-Path $fuzzDir "findings"
New-Item -ItemType Directory -Force $findingsDir | Out-Null

# libfuzzer-dotnet: the native libFuzzer<->managed bridge (github.com/Metalnem/libfuzzer-dotnet)
$libFuzzer = Join-Path $fuzzDir "libfuzzer-dotnet-windows.exe"
if (-not (Test-Path $libFuzzer)) {
    $url = "https://github.com/Metalnem/libfuzzer-dotnet/releases/download/v2025.05.02.0904/libfuzzer-dotnet-windows.exe"
    Write-Host "Downloading $url"
    Invoke-WebRequest -Uri $url -OutFile $libFuzzer
}

if (-not $SkipBuild) {
    # fresh publish every time: sharpfuzz refuses to instrument an already-instrumented
    # assembly, and an incremental publish would leave instrumented DLLs in place
    if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
    dotnet publish (Join-Path $fuzzDir "MasterMemory.Tests.Fuzz.csproj") -c Release -o $publishDir
    if ($LastExitCode -ne 0) { throw "publish failed" }

    # instrument ONLY the code under test (the searches, and the lookups generated for the tables);
    # the harness exe and the SharpFuzz runtime must stay clean or the coverage protocol breaks
    Push-Location $repoRoot
    try {
        dotnet tool restore | Out-Null
        foreach ($dll in @("MasterMemory.dll", "MasterMemory.Tests.Fuzz.Target.dll")) {
            $path = Join-Path $publishDir $dll
            if (-not (Test-Path $path)) { throw "expected $dll in publish output" }
            Write-Host "Instrumenting $dll"
            dotnet tool run sharpfuzz $path
            if ($LastExitCode -ne 0) { throw "sharpfuzz failed on $dll" }
        }
    }
    finally {
        Pop-Location
    }
}

if (-not (Test-Path $Corpus) -or -not (Get-ChildItem $Corpus -ErrorAction SilentlyContinue)) {
    dotnet $harnessDll --seeds $Corpus
    if ($LastExitCode -ne 0) { throw "seed generation failed" }
}

if ($Target) { $env:FUZZ_TARGET = $Target } else { Remove-Item Env:FUZZ_TARGET -ErrorAction SilentlyContinue }

$fuzzerArgs = @(
    "--target_path=dotnet",
    "--target_arg=$harnessDll",
    "-timeout=$TimeoutSec",
    "-rss_limit_mb=$RssLimitMb",
    "-max_len=$MaxLen",
    "-artifact_prefix=$findingsDir\",
    "-print_final_stats=1"
)
if ($MaxTotalTimeSec -gt 0) { $fuzzerArgs += "-max_total_time=$MaxTotalTimeSec" }
$fuzzerArgs += $Corpus

Write-Host "libfuzzer-dotnet $($fuzzerArgs -join ' ')"
& $libFuzzer @fuzzerArgs
exit $LastExitCode
