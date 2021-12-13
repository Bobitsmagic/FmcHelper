using BenchmarkDotNet.Attributes;
using CubeAD;
using System;
using System.Linq;

namespace CubeBenchmarks
{
	[MemoryDiagnoser]
	public class CubeIndexSortBenchmark
	{
		const int COUNT = 10_000_000;
		Random Rnd = new Random(0);

		IndexCube[] Indices = StickerCube.GetRandomCubesDistinct(15, COUNT, new Random(0)).ToArray();
		IndexCube[] Buffer = new IndexCube[COUNT];

		[IterationSetup]
		public void Setup()
		{
			for (int i = 0; i < COUNT - 1; i++)
			{
				int index = Rnd.Next(i + 1, COUNT);

				IndexCube buffer = Indices[i];
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
			IndexCube.RadixSortCubeIndices(Indices, Buffer);
		}


	}
}
