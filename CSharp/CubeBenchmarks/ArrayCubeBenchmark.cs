using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CubeBenchmarks
{
    public class ArrayCubeBenchmark
    {
        const int TILE_COUNT = TileCube.TILE_COUNT;
        CubeMove[] Data = new CubeMove[TILE_COUNT];
        CubeMove[] Solved = new CubeMove[TILE_COUNT];
        static List<TileCube> cubes = TileCube.GetAllCubes(3);
        static List<TileCube> uniqueCubes = TileCube.GetUniqueCubes(4).ToList();
        static HashSet<TileCube> hashset = new HashSet<TileCube>(uniqueCubes);
		bool f = false;
		bool t = true;

        public ArrayCubeBenchmark()
        {
            for(int i = 0; i < TILE_COUNT; i++)
            {
                Solved[i] = (CubeMove)(i / 9);
                Data[i] = Solved[i];
            }
        }

		#region Conversion
		//[Benchmark(Baseline = true)]
		//public void FindCornerOrientationIndex()
		//{
		//	foreach (ArrayCube cube in uniqueCubes)
		//		cube.FindCornerOrientationIndex();
		//}

		//[Benchmark]
		//public void FindEdgeOrientationIndex()
		//{
		//	foreach (ArrayCube cube in uniqueCubes)
		//		cube.FindEdgeOrientationIndex();
		//}

		//[Benchmark]
		//public void EdgePermIndex()
		//{
		//	foreach (ArrayCube cube in uniqueCubes)
		//		cube.GetEdgePermIndex();
		//}

		//[Benchmark]
		//public void CornerPermIndex()
		//{
		//	foreach (ArrayCube cube in uniqueCubes)
		//		cube.GetCornerPermIndex();
		//}

		#endregion

		#region Symmetry

		//[Benchmark]
		//public void LowestSym()
		//{
		//    foreach (ArrayCube cube in uniqueCubes)
		//        cube.FindLowestSymmetry();
		//}


		#endregion
		#region IsSolved
		//|        Method |      Mean |     Error |    StdDev |
		//|-------------- |----------:|----------:|----------:|
		//|   NormalEqual | 50.220 ns | 0.8045 ns | 0.7131 ns |
		//|   NoCondJumps | 59.609 ns | 0.8384 ns | 0.7432 ns |
		//| SequenceEqual | 30.805 ns | 0.4298 ns | 0.3810 ns |
		//|     SpanEqual |  9.199 ns | 0.0975 ns | 0.0912 ns |

		//[Benchmark]
		//public bool NormalEqual()
		//{
		//    for (int i = 0; i < TILE_COUNT; i++)
		//        if (Data[i] != Solved[i]) return false;

		//    return true;
		//}

		//[Benchmark]
		//public bool NoCondJumps()
		//{
		//    bool res = true;
		//    for (int i = 0; i < TILE_COUNT; i++)
		//        res &= Data[i] == Solved[i];

		//    return res;
		//}

		//[Benchmark]
		//public bool SequenceEqual()
		//{
		//    return Enumerable.SequenceEqual(Data, Solved);
		//}

		//[Benchmark]
		//public bool SpanEqual()
		//{
		//    return Data.AsSpan().SequenceEqual(Solved.AsSpan());
		//}

		#endregion

		#region Constructor/Reset
		//|       Method |      Mean |     Error |    StdDev |
		//|------------- |----------:|----------:|----------:|
		//|   DoubleLoop | 45.641 ns | 0.9484 ns | 1.4195 ns |
		//| DivisionLoop | 70.911 ns | 1.4318 ns | 1.9598 ns |
		//|    ArrayCopy |  6.290 ns | 0.1214 ns | 0.1076 ns |
		//[Benchmark]
		//public void DoubleLoop()
		//{
		//    int index = 0;
		//    for (int i = 0; i < 6; i++)
		//    {
		//        for (int j = 0; j < 9; j++)
		//        {
		//            Data[index++] = (CubeMove)i;
		//        }
		//    }
		//}

		//[Benchmark]
		//public void DivisionLoop()
		//{
		//    for (int i = 0; i < TILE_COUNT; i++)
		//    {
		//        Data[i] = (CubeMove)(i / 9);
		//    }
		//}

		//[Benchmark]
		//public void ArrayCopy()
		//{
		//    Array.Copy(Solved, 0, Data, 0, TILE_COUNT);
		//}

		#endregion

	}
}
