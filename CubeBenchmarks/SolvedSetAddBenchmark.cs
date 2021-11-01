using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeBenchmarks
{
	[MemoryDiagnoser]
	public class SolvedSetAddBenchmark
	{
		static Random rnd = new Random(0);

		static CubeIndex[] Cubes = new CubeIndex[10000000];
		static HashSet<CubeIndex> hashset;
		//static RadixTreeArray radixTreeArray;
		//static RadixTreeDictionary radixTreeDic;
		//static RadixTreeIterative radixTreeIt;
		static SortedCubeIndices sortCubeInd;
		static BucketCubeIndices bucketCubeIndices;

		static SolvedSetAddBenchmark()
		{
			hashset = new HashSet<CubeIndex>();
			//radixTreeDic = new RadixTreeDictionary();
			////radixTreeArray = new RadixTreeArray();
			//radixTreeIt = new RadixTreeIterative();
			sortCubeInd = new SortedCubeIndices();
			bucketCubeIndices = new BucketCubeIndices();

			Console.WriteLine("Created other sets");

			int count = 0;
			foreach (CubeIndex index in Cube.GetRandomCubes(7, Cubes.Length, rnd))
			{
				Cubes[count++] = index;
			}

			Console.WriteLine("Finished intializing Sets");
		}

		[IterationSetup]
		public void Setup()
		{
			hashset = new HashSet<CubeIndex>();
			//radixTreeDic.Clear();
			//radixTreeArray.Clear();
			//radixTreeIt.Clear();
			sortCubeInd.Clear();
			bucketCubeIndices.Clear();

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
	//|            Method |    Mean |    Error |   StdDev |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
	//|------------------ |--------:|---------:|---------:|-----------:|-----------:|----------:|----------:|
	//|           HashSet | 1.532 s | 0.0157 s | 0.0139 s |  3000.0000 |  1000.0000 | 1000.0000 |    556 MB |
	//| SortedCubeIndices | 1.728 s | 0.0051 s | 0.0045 s |  1000.0000 |  1000.0000 | 1000.0000 |    384 MB |
	//| BucketCubeIndices | 1.334 s | 0.0250 s | 0.0245 s | 45000.0000 | 24000.0000 | 5000.0000 |    328 MB |

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
