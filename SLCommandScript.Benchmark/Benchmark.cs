using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SLCommandScript.Core;

namespace SLCommandScript.Benchmark;

public static class Runner
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SLCSBenchmark>();
    }
}

[MemoryDiagnoser]
public class SLCSBenchmark
{
    public const string Script = @"
    bc 1 Hello World
";

    [Benchmark]
    public void BenchmarkScript()
    {
        _ = ScriptUtils.Execute(Script, new(["benchmark"], 1, 0), null);
    }
}
