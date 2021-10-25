using BenchmarkDotNet.Attributes;
using CubeAD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeBenchmarks
{
	[MemoryDiagnoser]
	public class CubeIndexSortBenchmark
	{
		const int COUNT = 100_000_000;
		Random Rnd = new Random(0);

		CubeIndex[] Indices = Cube.GetRandomCubesHS(15, COUNT, new Random(0)).ToArray();
		CubeIndex[] Buffer = new CubeIndex[COUNT];

		[IterationSetup]
		public void Setup()
		{
			for(int i = 0; i < COUNT - 1; i++)
			{
				int index = Rnd.Next(i + 1, COUNT);

				CubeIndex buffer = Indices[i];
				Indices[i] = Indices[index];
				Indices[index] = buffer;
			}

			Console.WriteLine("Run Setup");
		}

		[Benchmark]
		public void StandardSort()
		{
			Array.Sort(Indices);
		}
		[Benchmark]
		public void RadixSort()
		{
			CubeIndex.RadixSortCubeIndices(Indices, Buffer);
		}


	}
}
