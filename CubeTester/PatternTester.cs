using NUnit.Framework;
using CubeAD;
using System;

namespace CubeTester
{
	class PatternTester
	{
		[Test]
		public void CheckSquares()
		{
			Cube cube = new Cube();

			Assert.AreEqual(cube.CountSquares(), 24);

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(cube.CountSquares(), 16);

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(cube.CountSquares(), 8);
		}

		[Test]
		public void CheckBlocks()
		{
			Cube cube = new Cube();

			Assert.AreEqual(cube.CountBlocks(), 8);

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(cube.CountBlocks(), 4);

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(cube.CountBlocks(), 0);
		}

		[Test]
		public void Edges()
		{
			Cube cube = new Cube();

			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.D);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.U);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.F);
			Assert.AreEqual(cube.CountOrientedEgdes(), 8);

			cube.MakeMove(CubeMove.B);
			Assert.AreEqual(cube.CountOrientedEgdes(), 4);
			
			cube.MakeMove(CubeMove.U);
			cube.MakeMove(CubeMove.F);
			Assert.AreEqual(cube.CountOrientedEgdes(), 6);

			cube.MakeMove(CubeMove.B);
			Assert.AreEqual(cube.CountOrientedEgdes(), 8);

			cube.Reset();

			//Super flip
			cube.ApplyMoveSequenz(new MoveSequenz("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2"));

			Assert.AreEqual(cube.CountOrientedEgdes(), 0);
		}

		[Test]
		public void SingleEdges()
		{
			Cube cube = new Cube();

			Random rnd = new Random(0);

			for (int i = 0; i < 100; i++)
			{
				CubeMove cm = (CubeMove)rnd.Next(18);

				cube.MakeMove(cm);

				int counter = 0;
				for (int x = 0; x < 6; x++)
				{
					for (int y = x + 1; y < 6; y++)
					{
						if (x / 2 == y / 2) continue;

						if (cube.EdgeIsOriented(x, y))
							counter++;
					}
				}

				Assert.AreEqual(cube.CountOrientedEgdes(), counter);
			}
		}
	}
}
