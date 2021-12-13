using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;

namespace CubeBenchmarks
{
	[MemoryDiagnoser]
	public class SolvedSetAddBenchmark
	{
		static Random rnd = new Random(0);

		static IndexCube[] Cubes = new IndexCube[10000000];
		static HashSet<IndexCube> hashset;
		//static RadixTreeArray radixTreeArray;
		//static RadixTreeDictionary radixTreeDic;
		//static RadixTreeIterative radixTreeIt;
		static SortedCubeIndexSet sortCubeInd;
		static SortedBucketsSet bucketCubeIndices;
		static SetBuckets setBuckets;

		static SolvedSetAddBenchmark()
		{
			hashset = new HashSet<IndexCube>();
			//radixTreeDic = new RadixTreeDictionary();
			////radixTreeArray = new RadixTreeArray();
			//radixTreeIt = new RadixTreeIterative();
			sortCubeInd = new SortedCubeIndexSet();
			bucketCubeIndices = new SortedBucketsSet();
			setBuckets = new SetBuckets(0);

			Console.WriteLine("Created other sets");

			int count = 0;
			foreach (IndexCube index in StickerCube.GetRandomCubes(7, Cubes.Length, rnd))
			{
				Cubes[count++] = index;
			}

			Console.WriteLine("Finished intializing Sets");
		}

		[IterationSetup]
		public void Setup()
		{
			hashset = new HashSet<IndexCube>();
			//radixTreeDic.Clear();
			//radixTreeArray.Clear();
			//radixTreeIt.Clear();
			sortCubeInd.Clear();
			bucketCubeIndices.Clear();
			setBuckets.Clear();

			Console.WriteLine("Run Setup");
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
		public void SetBucket()
		{
			for (int i = 0; i < Cubes.Length; i++)
			{
				setBuckets.Add(Cubes[i]);
			}
		}
		//[Benchmark]
		//public void RadixTreeArray()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		radixTreeArray.Add(Cubes[i]);
		//	}
		//}
		//[Benchmark]
		//public void RadixTreeDictionary()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		radixTreeDic.Add(Cubes[i]);
		//	}
		//}
		//[Benchmark]
		//public void RadixTreeIterative()
		//{
		//	for (int i = 0; i < Cubes.Length; i++)
		//	{
		//		radixTreeIt.Add(Cubes[i]);
		//	}
		//}

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

	//N=10000000
	//|            Method |    Mean |    Error |   StdDev |  Median |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
	//|------------------ |--------:|---------:|---------:|--------:|-----------:|-----------:|----------:|----------:|
	//|           HashSet | 1.692 s | 0.0308 s | 0.0367 s | 1.679 s |  2000.0000 |          - |         - |    556 MB |
	//|         SetBucket | 2.024 s | 0.0403 s | 0.0551 s | 2.005 s |  1000.0000 |          - |         - |     16 MB |
	//| SortedCubeIndices | 1.483 s | 0.0392 s | 0.1155 s | 1.533 s |          - |          - |         - |    384 MB |
	//| BucketCubeIndices | 2.052 s | 0.0406 s | 0.0679 s | 2.059 s | 50000.0000 | 25000.0000 | 5000.0000 |    368 MB |

	//N=100000
	//|              Method |         Mean |      Error |     StdDev |       Median |       Gen 0 |      Gen 1 |     Gen 2 | Allocated |
	//|-------------------- |-------------:|-----------:|-----------:|-------------:|------------:|-----------:|----------:|----------:|
	//|             HashSet |     5.311 ms |  0.1031 ms |  0.1227 ms |     5.244 ms |           - |          - |         - |      7 MB |
	//|      RadixTreeArray | 2,008.671 ms | 33.7616 ms | 26.3588 ms | 2,001.685 ms | 155000.0000 | 82000.0000 | 6000.0000 |  1,193 MB |
	//| RadixTreeDictionary |   431.788 ms |  8.5450 ms | 18.7566 ms |   424.166 ms |  19000.0000 | 10000.0000 | 1000.0000 |    151 MB |
	//|  RadixTreeIterative |   557.262 ms | 10.9987 ms | 14.6830 ms |   557.867 ms |  79000.0000 | 43000.0000 | 5000.0000 |    592 MB |
	//|   SortedCubeIndices |    11.922 ms |  0.0661 ms |  0.0586 ms |    11.903 ms |           - |          - |         - |      3 MB |
	//|   BucketCubeIndices |     5.022 ms |  0.0227 ms |  0.0190 ms |     5.033 ms |           - |          - |         - |      3 MB |
}
