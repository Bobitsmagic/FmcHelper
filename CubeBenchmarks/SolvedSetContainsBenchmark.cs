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
		//static HashSet<CubeIndex> HashSet;
		//static SetBuckets SetBuckets;
		static SealedHashset SealedHS;

		static SolvedSetContainsBenchmark()
		{
			CubeIndex[] array = CubeIndex.SolvedTree(DEPTH).GetArray();

			Console.WriteLine("Created Array");

			//HashSet = new HashSet<CubeIndex>();
			//SetBuckets = new SetBuckets();
			//foreach (CubeIndex index in array)
			//{
			//	//SetBuckets.Add(index);
			//	//HashSet.Add(index);
			//}

			SealedHS = new SealedHashset(array);
			Console.WriteLine("Added to other sets");
			Console.WriteLine(array.Length);


			for (int i = 0; i < Cubes.Length / 10; i++)
			{
				Cubes[i] = array[rnd.Next(array.Length)];
			}

			int count = Cubes.Length / 10;
			foreach (CubeIndex index in Cube.GetRandomCubesDistinct(7, 9000, rnd))
			{
				Cubes[count++] = index;

			}

			GC.Collect();

			Console.WriteLine("Finished intializing Sets");
		}

		//[Benchmark]
		//public void BenchHashSet()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		HashSet.Contains(Cubes[i]);
		//	}
		//}

		//|       Method |     Mean |     Error |    StdDev |
		//|------------- |---------:|----------:|----------:|
		//| BenchHashSet | 1.004 ms | 0.0191 ms | 0.0205 ms |
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
		public void BenchSealedHS()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				SealedHS.Contains(Cubes[i]);
			}
		}
		//[Benchmark]
		//public void BenchSetBuckets()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		SetBuckets.Contains(Cubes[i]);
		//	}
		//}
		//[Benchmark]
		//public void BenchSortedCubeIndices()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		SortCubeInd.Contains(Cubes[i]);
		//	}
		//}

		//[Benchmark]
		//public void BenchBucketCubeIndices()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		BucketCubeIndices.Contains(Cubes[i]);
		//	}
		//}
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
//|           BenchHashSet |   459.8 us |  0.37 us |   0.33 us |
//|        BenchSetBuckets |   589.3 us |  1.07 us |   1.00 us |
//| BenchSortedCubeIndices | 4,787.6 us | 77.49 us | 113.59 us |
//| BenchBucketCubeIndices | 2,991.7 us | 57.65 us |  53.92 us |