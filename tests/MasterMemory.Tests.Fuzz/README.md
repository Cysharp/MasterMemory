# MasterMemory.Tests.Fuzz

Coverage-guided fuzzing of the lookups (`FindBy` / `TryFindBy` / `FindClosestBy` / `FindRangeBy`, the searches under them, and the `RangeView` they return) with SharpFuzz + libFuzzer.

The fuzz targets live in `MasterMemory.Tests.Fuzz.Target` (`FindFuzzTargets`). Each one turns its input into a table and a list of keys, asks the table every kind of lookup for every key, and compares each answer with what a linear pass over the rows gives. A wrong answer throws `FindMismatchException`, so a finding is "a lookup answered wrongly", not only "something crashed".

There are two tiers over the same targets:

- `FindFuzzTest` in `MasterMemory.Tests` runs every target on the seeds and on 3,000 inputs from a fixed random sequence. It is an ordinary test and runs with the others.
- This project is the escalation: the same targets under libFuzzer, with execution feedback steering the inputs. Run it before a release, and after any change to the searches or to the generated lookups.

## Run

```powershell
# everything, until Ctrl+C
pwsh tests/MasterMemory.Tests.Fuzz/fuzz.ps1

# bounded run
pwsh tests/MasterMemory.Tests.Fuzz/fuzz.ps1 -MaxTotalTimeSec 600

# one target: FUZZ_TARGET is an exact target name, a substring, or an index
pwsh tests/MasterMemory.Tests.Fuzz/fuzz.ps1 -Target all-keys
```

The first run downloads `libfuzzer-dotnet-windows.exe` (the native libFuzzer bridge, github.com/Metalnem/libfuzzer-dotnet) next to the script and restores the `sharpfuzz` local dotnet tool.

## What it does

1. `dotnet publish` the harness (Release) into `publish/`.
2. `sharpfuzz` IL-instruments the code under test: `MasterMemory.dll` (the searches) and `MasterMemory.Tests.Fuzz.Target.dll` (the tables, and so the lookups generated for them). The harness exe and the SharpFuzz runtime stay uninstrumented.
3. Generates the seed corpus into `corpus/` if it is empty: every `FindFuzzTargets.SeedCorpus()` payload, once per target, with the one-byte target selector prepended.
4. `libfuzzer-dotnet-windows.exe` drives the harness in persistent mode.

Input format: `input[0]` selects the target, the rest is what the target builds its table and its keys from. Past its end an input reads as zero, so every input is a valid table.

| Target | What it builds | What it checks |
|---|---|---|
| `table/all-keys` | up to 32 `FuzzRow`s: ids with or without gaps starting at one of several values (the ends of `int` among them), and a group, a name, a kind, a code and a score per row | every lookup of all nine keys: unique `int` (primary), unique `long`, shared `int`, unique `(int, int)`, shared ordinal string, shared `(string, enum)` (through the comparer generated for a composite key), shared enum, unique string ignoring case, shared `double` |
| `search/int` | a sorted `int[]` of up to 40 elements with repeated values | `Find`, `FindBranchless`, `FindClosest`, `LowerBound`, `UpperBound`, `LowerBoundClosest`, `UpperBoundClosest` of `Internal.BinarySearch` |
| `pk/long`, `pk/uint`, `pk/byte`, `pk/sbyte`, `pk/short`, `pk/ushort`, `pk/char` | a table whose unique primary key is of that type, ids going up by 1 to 4 from the minimum, zero, near the maximum, or 10 | every lookup of the primary key, which reads a table without gaps at `key - first key` |

## Findings

libFuzzer writes `crash-*` / `timeout-*` / `oom-*` files into `findings/`; the harness also logs `FINDING <ExceptionType> on <target>: <the question, the answer, the expected answer>` and the payload in hex to stderr just before the crash is recorded. Reproduce under a debugger-friendly single run:

```powershell
dotnet tests/MasterMemory.Tests.Fuzz/publish/MasterMemory.Tests.Fuzz.dll findings/crash-<hash>
```

Use the same `FUZZ_TARGET` environment value the campaign ran with (the selector byte indexes into the *filtered* target set). After fixing a bug, keep the input as a deterministic regression test in `MasterMemory.Tests`.

`publish/`, `corpus/`, `findings/`, and the downloaded exe are gitignored; the corpus regenerates deterministically and grows per machine.

## Campaign log

| Date | HEAD | Duration | Targets | Executions | Findings |
|---|---|---|---|---|---|
| 2026-11-21 | the commit that adds this project | 181 s bounded (`-MaxTotalTimeSec 180`), Windows x64, .NET 9 | all | 498,116 (about 2.7k exec/s), corpus grew to 653 inputs | none. To see that a finding looks like one, `LowerBound` was then broken on purpose (`c < 0` for `c <= 0`): libFuzzer reported it on the first seed as `Group FindClosestBy(-2, upper) -> 0 rows [], expected 1 rows [-1]`, and by four of the `FindFuzzTest` cases. |
| 2026-11-21 | the commit that reads tables from symbols | 151 s bounded (`-MaxTotalTimeSec 150`), Windows x64, .NET 9, on the corpus of the campaign above | all, with `pk/short`, `pk/ushort` and `pk/char` added and the composite key of `table/all-keys` going through its generated comparer | 636,002 (about 4.2k exec/s), corpus grew to 1,528 inputs | none |
