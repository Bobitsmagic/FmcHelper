using NUnit.Framework;
using CubeAD;

namespace CubeTester
{
	class CubeTester
	{
		[Test]
		public void RotateSingleSide()
		{
			Cube cube = new Cube();
			Assert.IsTrue(cube.IsSolved);

			for(int i = 0; i < 6; i++)
			{
				cube.MakeMove((CubeMove)(i * 3 + 0));
				for(int j = 0; j < 3; j++)
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
			for(int i = 0; i < 6; i++)
			{
				for(int j = 0; j < 6; j++)
				{
					if (i / 2 == j / 2) continue;

					CubeMove[] cubeMoves = new CubeMove[4 * 6];
					for(int k = 0; k < cubeMoves.Length; k += 4)
					{
						cubeMoves[k + 0] = (CubeMove)(i * 3);
						cubeMoves[k + 1] = (CubeMove)(j * 3);
						cubeMoves[k + 2] = (CubeMove)(i * 3 + 2);
						cubeMoves[k + 3] = (CubeMove)(j * 3 + 2);
					}

					Cube cube = new Cube();
					Assert.IsTrue(cube.IsSolved);

					for(int k = 0; k < cubeMoves.Length - 1; k++)
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
			c.ApplyScramble(new MoveSequenz("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2"));

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
			c.ApplyScramble(new MoveSequenz("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2"));
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

			foreach(SymmetryElement se in SymmetryGroup.Elements)
			{
				Assert.True(c.HasSymmetry(se));
			}

			//Super flip
			c.ApplyScramble(new MoveSequenz("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2"));
			foreach (SymmetryElement se in SymmetryGroup.Elements)
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
	}
}
