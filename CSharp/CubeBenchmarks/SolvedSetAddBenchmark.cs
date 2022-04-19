using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.IndexCubeSets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeBenchmarks
{
	[MemoryDiagnoser]
	public class SolvedSetAddBenchmark
	{
		static Random rnd = new Random(0);

		static IndexCube[] Cubes = new IndexCube[10000000];
		static HashSet<IndexCube> hashset;
		static SortedListSet sortCubeInd;
		static SortedBucketsSet bucketCubeIndices;

		static SolvedSetAddBenchmark()
		{
			hashset = new HashSet<IndexCube>();
			sortCubeInd = new SortedListSet();
			bucketCubeIndices = new SortedBucketsSet();

			Console.WriteLine("Created sets");

			Cubes = SearchingAlgorithms.GenerateRandomCubes(rnd, Cubes.Length).ToArray();

			Console.WriteLine("Finished intializing Sets");
		}

		[IterationSetup]
		public void Setup()
		{
			hashset = new HashSet<IndexCube>();
			sortCubeInd.Clear();
			bucketCubeIndices.Clear();

			Console.WriteLine("Ran Setup");
		}

		[Benchmark]
		public void HashSet()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				hashset.Add(Cubes[i]);
			}
		}
		[Benchmark]
		public void SortedCubeIndices()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				sortCubeInd.Add(Cubes[i]);
			}

			sortCubeInd.RemoveDuplicates();
		}

		[Benchmark]
		public void BucketCubeIndices()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				bucketCubeIndices.Add(Cubes[i]);
			}

			bucketCubeIndices.RemoveDuplicates();
		}
	}
}
