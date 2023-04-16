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

        ArrayCube ac = ArrayCube.GetSolved();
        IndexCube id = new IndexCube();
        static CubeMoveBenchmark()
        {
            ms.ReRoll(COUNT);
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

    }
}
