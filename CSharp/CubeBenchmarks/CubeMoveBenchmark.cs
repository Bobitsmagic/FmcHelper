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


//|                      Method |     Mean |    Error |   StdDev | Ratio | RatioSD |
//|---------------------------- |---------:|---------:|---------:|------:|--------:|
//|           ArrayCubeMakeMove | 30.01 us | 0.146 us | 0.122 us |  1.00 |    0.00 |
//|           IndexCubeMakeMove | 80.52 us | 1.205 us | 1.566 us |  2.68 |    0.05 |
//|            PermCubeMakeMove | 60.95 us | 0.183 us | 0.171 us |  2.03 |    0.01 |
//| PermCubeSingleArrayMakeMove | 47.52 us | 0.540 us | 0.478 us |  1.58 |    0.02 |
//|       SeperatedCubeMakeMove | 24.53 us | 0.042 us | 0.038 us |  0.82 |    0.00 |


        static CubeMoveBenchmark()
        {
            ms.ReRoll(COUNT);
            IndexCube id = new IndexCube();
            id.MakeMove(CubeMove.L);
        }

        [Benchmark(Baseline = true)]
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
	}
}
