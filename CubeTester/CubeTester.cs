using CubeAD;
using NUnit.Framework;
using System;

namespace CubeTester
{
	class CubeTester
	{
		[Test]
		public void RotateSingleSide()
		{
			Cube cube = new Cube();
			Assert.IsTrue(cube.IsSolved);

			for (int i = 0; i < 6; i++)
			{
				cube.MakeMove((CubeMove)(i * 3 + 0));
				for (int j = 0; j < 3; j++)
				{
					Assert.IsFalse(cube.IsSolved);
					cube.MakeMove((CubeMove)(i * 3 + 0));
				}

				Assert.IsTrue(cube.IsSolved);

				cube.MakeMove((CubeMove)(i * 3 + 1));
				Assert.IsFalse(cube.IsSolved);
				cube.MakeMove((CubeMove)(i * 3 + 1));

				Assert.IsTrue(cube.IsSolved);

				cube.MakeMove((CubeMove)(i * 3 + 2));
				for (int j = 0; j < 3; j++)
				{
					Assert.IsFalse(cube.IsSolved);
					cube.MakeMove((CubeMove)(i * 3 + 2));
				}

				Assert.IsTrue(cube.IsSolved);
			}
		}

		[Test]
		public void Rotate()
		{
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 6; j++)
				{
					if (i / 2 == j / 2) continue;

					CubeMove[] cubeMoves = new CubeMove[4 * 6];
					for (int k = 0; k < cubeMoves.Length; k += 4)
					{
						cubeMoves[k + 0] = (CubeMove)(i * 3);
						cubeMoves[k + 1] = (CubeMove)(j * 3);
						cubeMoves[k + 2] = (CubeMove)(i * 3 + 2);
						cubeMoves[k + 3] = (CubeMove)(j * 3 + 2);
					}

					Cube cube = new Cube();
					Assert.IsTrue(cube.IsSolved);

					for (int k = 0; k < cubeMoves.Length - 1; k++)
					{
						cube.MakeMove(cubeMoves[k]);
						Assert.IsFalse(cube.IsSolved);
					}

					cube.MakeMove(cubeMoves[cubeMoves.Length - 1]);
					Assert.IsTrue(cube.IsSolved);

				}
			}

		}

		[Test]
		public void GetEdges()
		{
			Cube c = new Cube();
			for (int x = 0; x < 6; x++)
			{
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					Assert.AreEqual((CubeColor)x, c.GetEdgeColor(x, y));
				}
			}

			//Super flip
			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);

			for (int x = 0; x < 6; x++)
			{
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					Assert.AreEqual((CubeColor)y, c.GetEdgeColor(x, y));
				}
			}
		}
		[Test]
		public void GetCorners()
		{
			Cube c = new Cube();
			//Super flip
			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);
			for (int x = 0; x < 6; x++)
			{
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					for (int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;

						Assert.AreEqual((CubeColor)x, c.GetCornerColor(x, y, z));
					}
				}
			}
		}

		[Test]
		public void HasSymmetry()
		{
			Cube c = new Cube();

			foreach (SymmetryElement se in SymmetryElement.Elements)
			{
				Assert.True(c.HasSymmetry(se));
			}

			//Super flip
			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);
			foreach (SymmetryElement se in SymmetryElement.Elements)
			{
				Assert.True(c.HasSymmetry(se));
			}

			c.Reset();

			SymmetryElement mirrorX = new SymmetryElement(CubeColor.Red, CubeColor.Yellow, CubeColor.Green);

			c.MakeMove(CubeMove.R);

			Assert.False(c.HasSymmetry(mirrorX));

			c.MakeMove(CubeMove.LP);

			Assert.True(c.HasSymmetry(mirrorX));
		}

		[Test]
		public void GetSymmetrySet()
		{
			Cube c = new Cube();

			BitArray sym = c.GetSymmetrySet();
			Assert.AreEqual(SymmetryElement.FullSymmetry, sym);

			//Super flip
			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);
			sym = c.GetSymmetrySet();
			Assert.AreEqual(SymmetryElement.FullSymmetry, sym);

			c.Reset();

			//H perm
			c.ApplyMoveSequenz(new MoveSequenz("R2 U2 R' U2 R2 U2 R2 U2 R' U2 R2"));
			sym = c.GetSymmetrySet();

			//4 rotations (identity, 90, 180, 270) * 2 reflections (idenity, refelcted)
			Assert.AreEqual(8, sym.BitCount);

			//H perm of bottom side
			c.ApplyMoveSequenz(new MoveSequenz("R2 D2 R' D2 R2 D2 R2 D2 R' D2 R2"));
			sym = c.GetSymmetrySet();

			//4 Y-rotations (identity, 90, 180, 270) * 2 Z-Rotation (0, 180) * 2 reflections (idenity, refelcted)
			Assert.AreEqual(16, sym.BitCount);


			c.Reset();
			//checkerboard pattern
			c.ApplyMoveSequenz(new MoveSequenz("U2 D2 R2 L2 F2 B2"));
			sym = c.GetSymmetrySet();
			Assert.AreEqual(SymmetryElement.FullSymmetry, sym);

			c.Reset();
			c.ApplyMoveSequenz(new MoveSequenz("R2 L2 U2 R2 L2 D2"));
			sym = c.GetSymmetrySet();
			Assert.AreEqual(8, sym.BitCount);

		}

		[Test]
		public void IsEqualWithSymmetry()
		{
			Cube c1 = new Cube();
			Cube c2 = new Cube();

			Random rnd = new Random(0);

			//H perm
			c1.ApplyMoveSequenz(new MoveSequenz("R2 U2 R' U2 R2 U2 R2 U2 R' U2 R2"));
			c2.ApplyMoveSequenz(new MoveSequenz("R2 U2 R' U2 R2 U2 R2 U2 R' U2 R2"));

			BitArray sym = c1.GetSymmetrySet();
			SymmetryElement[] elements = new SymmetryElement[sym.BitCount];

			int counter = 0;
			for (int i = 0; i < 48; i++)
			{
				if (sym[i])
				{
					elements[counter++] = SymmetryElement.Elements[i];
				}
			}

			Assert.IsTrue(c1.IsEqualWithSymmetry(c2));
			Assert.IsTrue(c2.IsEqualWithSymmetry(c1));

			SymmetryElement se = elements[rnd.Next(elements.Length)];
			for (int i = 0; i < 100; i++)
			{
				CubeMove cm = (CubeMove)rnd.Next(18);

				c1.MakeMove(cm);
				c2.MakeMove(se.TransformMove(cm));

				Assert.IsTrue(c1.IsEqualWithSymmetry(c2, se));
			}


		}

		[Test]
		public void CountSquares()
		{
			Cube cube = new Cube();

			Assert.AreEqual(24, cube.CountSquares());

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(16, cube.CountSquares());

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(8, cube.CountSquares());
		}

		[Test]
		public void CheckBlocks()
		{
			Cube cube = new Cube();

			Assert.AreEqual(8, cube.CountBlocks());

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(4, cube.CountBlocks());

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(0, cube.CountBlocks());
		}

		[Test]
		public void CountOrientedEdges()
		{
			Cube cube = new Cube();

			Assert.AreEqual(12, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(12, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(12, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.D);
			Assert.AreEqual(12, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.U);
			Assert.AreEqual(12, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.F);
			Assert.AreEqual(8, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.B);
			Assert.AreEqual(4, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.U);
			cube.MakeMove(CubeMove.F);
			Assert.AreEqual(6, cube.CountOrientedEgdes());

			cube.MakeMove(CubeMove.B);
			Assert.AreEqual(8, cube.CountOrientedEgdes());

			cube.Reset();

			//Super flip
			cube.ApplyMoveSequenz(MoveSequenz.SuperFlip);

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

		[Test]
		public void GetSolvedCubeIndice()
		{
			
		}
	}
}
