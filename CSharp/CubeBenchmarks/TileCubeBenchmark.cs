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
    public class TileCubeBenchmark
    {
        const int TILE_COUNT = TileCube.TILE_COUNT;
        CubeMove[] Data = new CubeMove[TILE_COUNT];
        CubeMove[] Solved = new CubeMove[TILE_COUNT];
        static List<TileCube> cubes = TileCube.GetAllCubes(3);
        static List<TileCube> uniqueCubes = TileCube.GetUniqueCubes(4).ToList();
        static HashSet<TileCube> hashset = new HashSet<TileCube>(uniqueCubes);
		IndexCube IndexCube = new IndexCube();
		int[][] Arrays = new int[4][];

		bool f = false;
		bool t = true;

        public TileCubeBenchmark()
        {
            for(int i = 0; i < TILE_COUNT; i++)
            {
                Solved[i] = (CubeMove)(i / 9);
                Data[i] = Solved[i];
            }

			IndexCube.ApplyMoveSequenz(MoveSequenz.FmcWr);
			Arrays[0] = IndexCube.GetEdgePermutation();
			Arrays[1] = IndexCube.GetEdgeOrientation();
			Arrays[2] = IndexCube.GetCornerPermuation();
			Arrays[3] = IndexCube.GetCornerOrientation();
		}
		#region Constructor

		[Benchmark] 
		public void ConstructorFromArrays()
		{
			new TileCube(Arrays[0], Arrays[1], Arrays[2], Arrays[3]);
		}

		#endregion

		#region Conversion
		//|                     Method |      Mean |     Error |    StdDev | Ratio | RatioSD |
		//|--------------------------- |----------:|----------:|----------:|------:|--------:|
		//| FindCornerOrientationIndex |  1.759 ms | 0.0114 ms | 0.0101 ms |  1.00 |    0.00 |
		//|   FindEdgeOrientationIndex |  1.539 ms | 0.0034 ms | 0.0027 ms |  0.87 |    0.00 |
		//|              EdgePermIndex | 11.397 ms | 0.1493 ms | 0.1397 ms |  6.47 |    0.09 |
		//|            CornerPermIndex |  6.072 ms | 0.0455 ms | 0.0404 ms |  3.45 |    0.03 |
		//[Benchmark(Baseline = true)]
		//public void FindCornerOrientationIndex()
		//{
		//	foreach (TileCube cube in uniqueCubes)
		//		cube.FindCornerOrientationIndex();
		//}

		//[Benchmark]
		//public void FindEdgeOrientationIndex()
		//{
		//	foreach (TileCube cube in uniqueCubes)
		//		cube.FindEdgeOrientationIndex();
		//}

		//[Benchmark]
		//public void EdgePermIndex()
		//{
		//	foreach (TileCube cube in uniqueCubes)
		//		cube.GetEdgePermIndex();
		//}

		//[Benchmark]
		//public void CornerPermIndex()
		//{
		//	foreach (TileCube cube in uniqueCubes)
		//		cube.GetCornerPermIndex();
		//}

		#endregion

		#region Symmetry

		//[Benchmark]
		//public void LowestSym()
		//{
		//    foreach (TileCube cube in uniqueCubes)
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
