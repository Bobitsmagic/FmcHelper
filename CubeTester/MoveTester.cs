using NUnit.Framework;
using CubeAD;

namespace CubeTester
{
	class MoveTester
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
	}
}
