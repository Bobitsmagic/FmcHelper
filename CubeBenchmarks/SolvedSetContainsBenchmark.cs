using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.Linq;


namespace CubeBenchmarks
{
	public class SolvedSetContainsBenchmark
	{
		const int DEPTH = 7;
		static Random rnd = new Random(0);

		static CubeIndex[] Cubes = new CubeIndex[10000];
		static HashSet<CubeIndex> HashSet;
		static RadixTreeArray RadixTreeArray;
		static RadixTreeDictionary RadixTreeDic;
		static RadixTreeIterative RadixTreeIt;
		static SortedCubeIndices SortCubeInd;
		static BucketCubeIndices BucketCubeIndices;

		static SolvedSetContainsBenchmark()
		{
			HashSet = Cube.GetSolvedCubeIndicesDFS(DEPTH);

			Console.WriteLine("Created hashset");

			RadixTreeDic = new RadixTreeDictionary();
			RadixTreeArray = new RadixTreeArray();
			RadixTreeIt = new RadixTreeIterative();
			SortCubeInd = new SortedCubeIndices();
			BucketCubeIndices = new BucketCubeIndices();

			int count = 0;
			foreach (CubeIndex index in HashSet)
			{
				//RadixTreeArray.Add(index);
				//RadixTreeDic.Add(index);
				//RadixTreeIt.Add(index);
				SortCubeInd.Add(index);
				BucketCubeIndices.Add(index);

				//if (count++ % 10000 == 0)
				//{
				//	Console.WriteLine(count + " / " + HashSet.Count);
				//}
			}
			Console.WriteLine("Created dic and array");
			SortCubeInd.RemoveDuplicates();
			BucketCubeIndices.RemoveDuplicates();
			Console.WriteLine(HashSet.Count + " " + RadixTreeArray.Count + " " + RadixTreeDic.Count + " " + RadixTreeIt.Count + " " + SortCubeInd.Count + " " + BucketCubeIndices.Count);


			for (int i = 0; i < Cubes.Length / 10; i++)
			{
				Cubes[i] = HashSet.ElementAt(rnd.Next(HashSet.Count));
			}

			count = Cubes.Length / 10;
			foreach (CubeIndex index in Cube.GetRandomCubesDistinct(7, 9000, rnd))
			{
				Cubes[count++] = index;

			}

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
		//[Benchmark]
		//public void BenchRadixTreeArray()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		RadixTreeArray.Contains(Cubes[i]);
		//	}
		//}
		//[Benchmark]
		//public void BenchRadixTreeDictionary()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		RadixTreeDic.Contains(Cubes[i]);
		//	}
		//}
		//[Benchmark]
		//public void BenchRadixTreeIterative()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		RadixTreeIt.Contains(Cubes[i]);
		//	}
		//}

		[Benchmark]
		public void BenchSortedCubeIndices()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				SortCubeInd.Contains(Cubes[i]);
			}
		}

		[Benchmark]
		public void BenchBucketCubeIndices()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				BucketCubeIndices.Contains(Cubes[i]);
			}
		}
	}
}
//7
//|                 Method |       Mean |     Error |    StdDev |
//|----------------------- |-----------:|----------:|----------:|
//|           BenchHashSet |   977.9 us |  12.89 us |  12.66 us |
//| BenchSortedCubeIndices | 9,285.7 us | 111.95 us | 104.72 us |
//| BenchBucketCubeIndices | 6,775.8 us |  11.26 us |   9.40 us |
//6
//|                 Method |       Mean |    Error |    StdDev |
//|----------------------- |-----------:|---------:|----------:|
//|           BenchHashSet |   462.5 us |  1.12 us |   0.94 us |
//| BenchSortedCubeIndices | 5,070.6 us | 98.74 us | 147.80 us |
//| BenchBucketCubeIndices | 3,158.7 us | 60.01 us |  69.11 us |