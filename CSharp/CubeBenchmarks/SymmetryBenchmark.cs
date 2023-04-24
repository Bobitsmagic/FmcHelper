using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeBenchmarks
{
	public class SymmetryBenchmark
	{
		List<TileCube> RandomTileCubes = TileCube.GetUniqueCubes(4).ToList();
		List<PieceCube> RandomPieceCubes = new List<PieceCube>();

		public SymmetryBenchmark() 
		{
			Random rnd = new Random();
			for (int i = 0; i < RandomTileCubes.Count; i++)
			{
				int index = rnd.Next(i, RandomTileCubes.Count);

				(RandomTileCubes[i], RandomTileCubes[index]) = (RandomTileCubes[index], RandomTileCubes[i]);
			}
			RandomPieceCubes = RandomTileCubes.Select(x => new PieceCube(x.GetEdgePerm(), x.GetEdgeOrientation(), x.GetCornerPerm(), x.GetCornerOrientation())).ToList();
		}

		TileCube bufferTile = TileCube.GetSolved();
		PieceCube bufferPiece = new PieceCube();

		//[Benchmark]
		//public void TileCubeSymTransform()
		//{
		//	foreach(var tc in RandomTileCubes)
		//	{
		//		foreach(SymmetryElement se in SymmetryElement.Elements)
		//		{
		//			tc.TransformSymmetry(se, bufferTile);
		//		}
		//	}
		//}

		//[Benchmark]
		//public void PieceCubeSymTransform()
		//{
		//	foreach(var pc in RandomPieceCubes)
		//	{
		//		foreach (SymmetryElement se in SymmetryElement.Elements)
		//		{
		//			pc.Transform(se, bufferPiece);
		//		}
		//	}
		//}

		//[Benchmark]
		//public void TileCubeLowestSym()
		//{
		//	foreach (var tc in RandomTileCubes)
		//	{
		//		tc.FindLowestSymmetry();
		//	}
		//}

		//[Benchmark]
		//public void PieceCubeLowestSym()
		//{
		//	foreach (var pc in RandomPieceCubes)
		//	{
		//		pc.FindLowestSymmetry();
		//	}
		//}


		[Benchmark]
		public void TileCubeCompare()
		{
			for (int i = 1; i < RandomTileCubes.Count; i++)
			{
				RandomTileCubes[i].CompareTo(RandomTileCubes[i - 1]);
			}
		}

		[Benchmark]
		public void PieceCubeCompare()
		{
			for (int i = 1; i < RandomPieceCubes.Count; i++)
			{
				RandomPieceCubes[i].CompareTo(RandomPieceCubes[i - 1]);
			}
		}
	}
}
