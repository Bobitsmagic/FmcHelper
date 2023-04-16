using BenchmarkDotNet.Running;
using CubeAD.CubeRepresentation;
using System;

namespace CubeBenchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<ArrayCubeBenchmark>();

			//ArrayCube.GetAllCubes(4);

			Console.ReadLine();
		}
	}
}

//|          Method |        Mean |     Error |    StdDev |
//|---------------- |------------:|----------:|----------:|
//|       LowestSym | 3,006.69 ms | 31.391 ms | 29.363 ms |
//| LowestSymUnique |    50.53 ms |  0.906 ms |  0.803 ms |