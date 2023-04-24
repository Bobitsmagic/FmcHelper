using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeRepresentation;
using System;
using System.Linq;

namespace CubeBenchmarks
{
    public class CubeMoveBenchmark
    {
        const int COUNT = 1_000;

        static MoveSequenz ms = new MoveSequenz();

        TileCube ac = TileCube.GetSolved();
        IndexCube id = new IndexCube();
        PermCube pc = new PermCube();
        PermCubeSingleArray pcsa = new PermCubeSingleArray();
        SeperatedCube sc = new SeperatedCube();
        PieceCube piece = new PieceCube();


//|                      Method |     Mean |    Error |   StdDev | Ratio | RatioSD |
//|---------------------------- |---------:|---------:|---------:|------:|--------:|
//|           ArrayCubeMakeMove | 30.06 us | 0.279 us | 0.248 us |  1.65 |    0.02 |
//|           IndexCubeMakeMove | 76.23 us | 1.455 us | 1.732 us |  4.18 |    0.11 |
//|            PermCubeMakeMove | 60.41 us | 0.204 us | 0.170 us |  3.31 |    0.02 |
//| PermCubeSingleArrayMakeMove | 45.68 us | 0.360 us | 0.337 us |  2.50 |    0.03 |
//|       SeperatedCubeMakeMove | 25.26 us | 0.212 us | 0.198 us |  1.38 |    0.01 |
//|           PieceCubeMakeMove | 18.29 us | 0.169 us | 0.159 us |  1.00 |    0.00 |

        static CubeMoveBenchmark()
        {
            ms.ReRoll(COUNT);
            IndexCube id = new IndexCube();
            id.MakeMove(CubeMove.L);
        }

		[Benchmark]
		public void ArrayCubeMakeMove()
        {
            for (int i = 0; i < COUNT; i++)
            {
                ac.MakeMove(ms.Moves[i]);
            }
        }
        [Benchmark]
        public void IndexCubeMakeMove()
        {
            for (int i = 0; i < COUNT; i++)
            {
                id.MakeMove(ms.Moves[i]);
            }
        }
        [Benchmark]
        public void PermCubeMakeMove()
        {
            for (int i = 0; i < COUNT; i++)
            {
                pc.MakeMove(ms.Moves[i]);
            }
        }
        [Benchmark]
        public void PermCubeSingleArrayMakeMove()
        {
            for (int i = 0; i < COUNT; i++)
            {
                pcsa.MakeMove(ms.Moves[i]);
            }
        }
        [Benchmark]
		public void SeperatedCubeMakeMove()
		{
			for (int i = 0; i < COUNT; i++)
			{
				sc.MakeMove(ms.Moves[i]);
			}
		}
		[Benchmark(Baseline = true)]
		public void PieceCubeMakeMove()
		{
			for (int i = 0; i < COUNT; i++)
			{
				piece.MakeMove(ms.Moves[i]);
			}
		}
	}
}
