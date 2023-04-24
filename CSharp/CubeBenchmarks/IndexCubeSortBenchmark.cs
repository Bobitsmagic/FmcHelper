﻿using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeRepresentation;
using System;
using System.Linq;

namespace CubeBenchmarks
{
    [MemoryDiagnoser]
	public class IndexCubeSortBenchmark
	{
		const int COUNT = 1_000;
		Random Rnd = new Random(0);

		IndexCube[] Indices = SearchingAlgorithms.GenerateRandomCubes(new Random(0), COUNT).ToArray();
		//IndexCube[] Buffer = new IndexCube[COUNT];

		//[IterationSetup]
		//public void Setup()
		//{
		//	for (int i = 0; i < COUNT - 1; i++)
		//	{
		//		int index = Rnd.Next(i + 1, COUNT);

		//		IndexCube buffer = Indices[i];
		//		Indices[i] = Indices[index];
		//		Indices[index] = buffer;
		//	}

		//	Console.WriteLine("Run Setup");
		//}



		//[Benchmark]
		//public void MakeMove()
		//{
		//	for(int i = 0; i < COUNT; i++)
		//	{
		//		for(int m = 0; m < 18; m++)
		//		{
		//			Indices[i].MakeMove((CubeMove)m);
		//		}
		//	}
		//}


		//[Benchmark]
		//public void StandardSort()
		//{
		//	Array.Sort(Indices);
		//}
		//[Benchmark]
		//public void RadixSort()
		//{
		//	IndexCube.RadixSortCubeIndices(Indices, Buffer);
		//}
	}
}