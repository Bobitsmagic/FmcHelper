using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.IndexCubeSets;
using System;
using System.Collections.Generic;

namespace CubeBenchmarks
{
	public class SolvedSetContainsBenchmark
	{
		const int DEPTH = 7;
		static Random rnd = new Random(0);

		static IndexCube[] Cubes = new IndexCube[10000];
		static HashSet<IndexCube> HashSet;
		static SealedHashset SealedHS;

		static SolvedSetContainsBenchmark()
		{

			IndexCube[] array = SearchingAlgorithms.GenerateSolvedTree(DEPTH).GetArray();

			Console.WriteLine("Created Array with depth: " + DEPTH);
			Console.WriteLine("Length: " + array.Length);

			HashSet = new HashSet<IndexCube>(array);
			SealedHS = new SealedHashset(array);
			Console.WriteLine("Added to other sets");

			for (int i = 0; i < Cubes.Length / 10; i++)
			{
				Cubes[i] = array[rnd.Next(array.Length)];
			}

			int count = Cubes.Length / 10;
			foreach (IndexCube index in SearchingAlgorithms.GenerateRandomCubes(rnd, 9000))
			{
				Cubes[count++] = index;
			}

			GC.Collect();

			Console.WriteLine("Finished intializing Sets");
		}

		[Benchmark]
		public void BenchHashSet()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				HashSet.Contains(Cubes[i]);
			}
		}

		[Benchmark]
		public void BenchSealedHS()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				SealedHS.Contains(Cubes[i]);
			}
		}
	}
}