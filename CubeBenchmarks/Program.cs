using BenchmarkDotNet.Running;
using System;

namespace CubeBenchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<SolvedSetAddBenchmark>();
		}
	}
}
