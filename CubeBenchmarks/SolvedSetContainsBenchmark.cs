using BenchmarkDotNet.Attributes;
using CubeAD;
using System;
using System.Collections.Generic;
using System.Linq;


namespace CubeBenchmarks
{
	public class SolvedSetContainsBenchmark
	{
		const int DEPTH = 6;
		static Random rnd = new Random(0);

		static CubeIndex[] Cubes = new CubeIndex[10000];
		static HashSet<CubeIndex> HashSet;
		static RadixTreeArray RadixTreeArray;
		static RadixTreeDictionary RadixTreeDic;
		static RadixTreeIterative RadixTreeIt;
		static SortedCubeIndices SortCubeInd;

		static SolvedSetContainsBenchmark()
		{
			HashSet = Cube.GetSolvedCubeIndicesHS(DEPTH);

			Console.WriteLine("Created hashset");

			RadixTreeDic = new RadixTreeDictionary();
			RadixTreeArray = new RadixTreeArray();
			RadixTreeIt = new RadixTreeIterative();
			SortCubeInd = new SortedCubeIndices();

			int count = 0;
			foreach (CubeIndex index in HashSet)
			{
				//RadixTreeArray.Add(index);
				//RadixTreeDic.Add(index);
				//RadixTreeIt.Add(index);
				SortCubeInd.Add(index);

				if (count++ % 10000 == 0)
				{
					Console.WriteLine(count + " / " + HashSet.Count);
				}
			}
			Console.WriteLine("Created dic and array");
			SortCubeInd.RemoveDuplicates();
			Console.WriteLine(HashSet.Count + " " + RadixTreeArray.Count + " " + RadixTreeDic.Count + " " + RadixTreeIt.Count + " " + SortCubeInd.Count);


			for (int i = 0; i < Cubes.Length / 10; i++)
			{
				Cubes[i] = HashSet.ElementAt(rnd.Next(HashSet.Count));
			}

			count = Cubes.Length / 10;
			foreach (CubeIndex index in Cube.GetRandomCubesHS(7, 9000, rnd))
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
	}
}


//MaxDepth = 4;
//|                   Method |     Mean |    Error |   StdDev |
//|------------------------- |---------:|---------:|---------:|
//|             BenchHashSet | 22.26 us | 0.187 us | 0.165 us |
//|      BenchRadixTreeArray | 42.25 us | 0.818 us | 0.973 us |
//| BenchRadixTreeDictionary | 98.37 us | 0.936 us | 0.876 us |
//MaxDepth = 5
//|                   Method |      Mean |    Error |   StdDev |
//|------------------------- |----------:|---------:|---------:|
//|             BenchHashSet |  23.73 us | 0.189 us | 0.167 us |
//|      BenchRadixTreeArray |  67.90 us | 0.797 us | 0.746 us |
//| BenchRadixTreeDictionary | 202.38 us | 1.212 us | 1.134 us |

