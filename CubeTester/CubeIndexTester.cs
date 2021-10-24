using CubeAD;
using NUnit.Framework;

namespace CubeTester
{
	class CubeIndexTester
	{
		[Test]
		public void PermIndex()
		{
			for (int n = 0; n < 10; n++)
			{
				int[] array = new int[n];

				int fac = CubeIndex.Factorial(n);

				for (int f = 0; f < fac; f++)
				{
					CubeIndex.GetIndexedPerm(array, f);

					Assert.AreEqual(f, CubeIndex.GetIndex(array));
				}
			}
		}

		[Test]
		public void EdgePermutationIndex()
		{
			Cube c = new Cube();
			CubeIndex index = new CubeIndex(c);

			Assert.AreEqual(0, index.EdgePermutation);

			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);

			index = new CubeIndex(c);
			Assert.AreEqual(0, index.EdgePermutation);

			c.ApplyMoveSequenz(MoveSequenz.CheckerBoard);
			index = new CubeIndex(c);

			//index of permutation 5 4 7 6 1 0 3 2 11 10 9 8
			Assert.AreEqual(216080063, index.EdgePermutation);
		}

		[Test]
		public void CornerPermutationIndex()
		{
			Cube c = new Cube();
			CubeIndex index = new CubeIndex(c);

			Assert.AreEqual(0, index.CornerPermutation);

			c.MakeMove(CubeMove.U);
			c.MakeMove(CubeMove.L2);
			c.MakeMove(CubeMove.R2);

			index = new CubeIndex(c);

			//index of permutation 5 4 7 6 1 0 3 2 11 10 9 8
			Assert.AreEqual(13805, index.CornerPermutation);
		}


		[Test]
		public void EdgeOrientationIndex()
		{
			Cube c = new Cube();
			CubeIndex index = new CubeIndex(c);

			Assert.AreEqual(0, index.EdgeOrientation);

			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);

			index = new CubeIndex(c);
			Assert.AreEqual(CubeIndex.MAX_EDGE_ORIENTATION - 1, index.EdgeOrientation);

			c.ApplyMoveSequenz(MoveSequenz.CheckerBoard);

			index = new CubeIndex(c);
			Assert.AreEqual(CubeIndex.MAX_EDGE_ORIENTATION - 1, index.EdgeOrientation);
		}

		[Test]
		public void CornerOrientationIndex()
		{
			Cube c = new Cube();
			CubeIndex index = new CubeIndex(c);

			Assert.AreEqual(0, index.CornerOrientation);

			//double sune on top
			//rotates corner 2 3 6 7
			c.ApplyMoveSequenz(new MoveSequenz("R U R' U R U' R' U R U2 R'"));
			index = new CubeIndex(c);

			int val = (Power(3, 7 - 2) + Power(3, 7 - 3) + Power(3, 7 - 6) + Power(3, 7 - 7));

			Assert.AreEqual(val, index.CornerOrientation);

			c.ApplyMoveSequenz(new MoveSequenz("R U R' U R U' R' U R U2 R'"));
			index = new CubeIndex(c);
			Assert.AreEqual(val * 2, index.CornerOrientation);


			int Power(int x, int y)
			{
				int ret = 1;
				for (int i = 0; i < y; i++)
				{
					ret *= x;
				}
				return ret;
			}
		}
	}
}
