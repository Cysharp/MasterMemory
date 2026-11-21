using System.Runtime.InteropServices;
using MasterMemory.Tests.Fuzz.Target;
using SharpFuzz;

// Coverage-guided fuzzing (SharpFuzz + libFuzzer) of the lookups.
// This exe stays UNinstrumented; the code under test (MasterMemory.dll, and the Fuzz.Target assembly that holds the
// tables and so the generated lookups) gets IL-instrumented by the sharpfuzz tool. fuzz.ps1 orchestrates the whole thing.
//
// Input format: byte[0] selects the target from FindFuzzTargets.Targets (mod the active set), the rest is what the
// target builds its table and its keys from. One corpus therefore explores every target at once, and libFuzzer learns
// the selector byte like any other. FUZZ_TARGET (exact name, name substring, or index) narrows the active set for a
// focused campaign; the selector byte is still consumed either way, so corpus files mean the same thing in every mode.
//
// Crash oracle: a target compares every answer of a lookup with a linear pass over the rows and throws
// FindMismatchException when they differ. That exception, like any other, escapes into Fuzzer.LibFuzzer.Run and is
// recorded as a crash with its input. Hangs and OOM are covered by libFuzzer itself (-timeout / -rss_limit_mb).
//
// Modes:
//   (no args)       fuzzing loop under libfuzzer-dotnet (what fuzz.ps1 launches)
//   <file>          reproduce a finding: SharpFuzz's no-libFuzzer fallback runs the one input and lets the exception
//                   fly (set FUZZ_TARGET as in the campaign)
//   --seeds <dir>   write the multiplexed seed corpus and exit

// Instrumented branches write through SharpFuzz.Common.Trace.SharedMem, which stays a null pointer until
// Fuzzer.LibFuzzer.Run attaches the real shared memory. Everything this process does before/outside Run (--seeds mode)
// executes instrumented code too, so park the pointer on a dummy map first. Run replaces it with the real one; the
// 64KB is intentionally leaked for the process lifetime.
unsafe
{
    SharpFuzz.Common.Trace.SharedMem = (byte*)NativeMemory.AllocZeroed(65536);
}

if (args.Length >= 2 && args[0] == "--seeds")
{
    WriteSeeds(args[1]);
    return;
}

var active = ResolveActiveTargets();

Fuzzer.LibFuzzer.Run(span =>
{
    if (span.Length == 0)
    {
        return;
    }
    var (name, run) = active[span[0] % active.Length];
    var payload = span[1..].ToArray();
    try
    {
        run(payload);
    }
    catch (Exception ex) when (Report(name, payload, ex))
    {
        // Report always returns false: it logs the target and the input, and the exception still propagates for
        // libFuzzer to record as a crash
    }
});

static (string Name, Action<byte[]> Run)[] ResolveActiveTargets()
{
    var targets = FindFuzzTargets.Targets;
    var spec = Environment.GetEnvironmentVariable("FUZZ_TARGET");
    if (string.IsNullOrEmpty(spec))
    {
        return targets;
    }
    if (int.TryParse(spec, out var index) && index >= 0 && index < targets.Length)
    {
        return [targets[index]];
    }
    var matches = targets.Where(t => t.Name.Contains(spec, StringComparison.OrdinalIgnoreCase)).ToArray();
    if (matches.Length == 0)
    {
        Console.Error.WriteLine($"FUZZ_TARGET '{spec}' matches nothing. Targets:");
        for (var i = 0; i < targets.Length; i++)
        {
            Console.Error.WriteLine($"  [{i}] {targets[i].Name}");
        }
        Environment.Exit(1);
    }
    return matches;
}

static bool Report(string name, byte[] payload, Exception ex)
{
    Console.Error.WriteLine($"FINDING {ex.GetType().Name} on {name}: {ex.Message}");
    Console.Error.WriteLine($"  payload {Convert.ToHexString(payload)}");
    return false;
}

static void WriteSeeds(string dir)
{
    Directory.CreateDirectory(dir);
    var targets = FindFuzzTargets.Targets;
    var seeds = FindFuzzTargets.SeedCorpus();
    var written = 0;
    for (var t = 0; t < targets.Length; t++)
    {
        for (var s = 0; s < seeds.Length; s++)
        {
            byte[] input = [(byte)t, .. seeds[s]];
            File.WriteAllBytes(Path.Combine(dir, $"seed-t{t:D2}-s{s:D2}.bin"), input);
            written++;
        }
    }
    Console.WriteLine($"{written} seed inputs ({targets.Length} targets x {seeds.Length} payloads) -> {dir}");
}
