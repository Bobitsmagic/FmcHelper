using BenchmarkDotNet.Running;

namespace CubeBenchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<CubeIndexSerilization>();
		}
	}
}
