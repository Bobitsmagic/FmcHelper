using CubeAD;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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

				int fac = Permutation.Factorial(n);

				for (int f = 0; f < fac; f++)
				{
					Permutation.GetIndexedPerm(array, f);

					Assert.AreEqual(f, Permutation.GetIndex(array));
				}
			}
		}

		[Test]
		public void EdgePermutationIndex()
		{
			StickerCube c = new StickerCube();
			IndexCube index = new IndexCube(c);

			Assert.AreEqual(0, index.EdgePermutationIndex);

			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);

			index = new IndexCube(c);
			Assert.AreEqual(0, index.EdgePermutationIndex);

			c.ApplyMoveSequenz(MoveSequenz.CheckerBoard);
			index = new IndexCube(c);

			//index of permutation 5 4 7 6 1 0 3 2 11 10 9 8
			Assert.AreEqual(216080063, index.EdgePermutationIndex);
		}

		[Test]
		public void CornerPermutationIndex()
		{
			StickerCube c = new StickerCube();
			IndexCube index = new IndexCube(c);

			Assert.AreEqual(0, index.CornerPermutationIndex);

			c.MakeMove(CubeMove.U);
			c.MakeMove(CubeMove.L2);
			c.MakeMove(CubeMove.R2);

			index = new IndexCube(c);

			//index of permutation 5 4 7 6 1 0 3 2 11 10 9 8
			Assert.AreEqual(13805, index.CornerPermutationIndex);
		}


		[Test]
		public void EdgeOrientationIndex()
		{
			StickerCube c = new StickerCube();
			IndexCube index = new IndexCube(c);

			Assert.AreEqual(0, index.EdgeOrientationIndex);

			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);

			index = new IndexCube(c);
			Assert.AreEqual(IndexCube.MAX_EDGE_ORIENTATION - 1, index.EdgeOrientationIndex);

			c.ApplyMoveSequenz(MoveSequenz.CheckerBoard);

			index = new IndexCube(c);
			Assert.AreEqual(IndexCube.MAX_EDGE_ORIENTATION - 1, index.EdgeOrientationIndex);
		}

		[Test]
		public void CornerOrientationIndex()
		{
			StickerCube c = new StickerCube();
			IndexCube index = new IndexCube(c);

			Assert.AreEqual(0, index.CornerOrientationIndex);

			//double sune on top
			//rotates corner 2 3 6 7
			c.ApplyMoveSequenz(new MoveSequenz("R U R' U R U' R' U R U2 R'"));
			index = new IndexCube(c);

			int val = (Power(3, 7 - 2) + Power(3, 7 - 3) + Power(3, 7 - 6) + Power(3, 7 - 7));

			Assert.AreEqual(val, index.CornerOrientationIndex);

			c.ApplyMoveSequenz(new MoveSequenz("R U R' U R U' R' U R U2 R'"));
			index = new IndexCube(c);
			Assert.AreEqual(val * 2, index.CornerOrientationIndex);


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

		[Test]
		public void CubeIndexSorting()
		{
			HashSet<IndexCube> indices = StickerCube.GetRandomCubesDistinct(10, 1000, new System.Random(0));

			IndexCube[] array1 = indices.ToArray();
			IndexCube[] array2 = array1.ToArray();
			IndexCube[] buffer = new IndexCube[array1.Length];

			Array.Sort(array1);

			for (int i = 0; i < array1.Length - 1; i++)
			{
				Assert.IsTrue(array1[i].Index < array1[i + 1].Index);
			}

			IndexCube.RadixSortCubeIndices(array2, buffer);

			Assert.IsTrue(array1.SequenceEqual(array2));
		}

		[Test]
		public void CubeEdgeColor()
		{
			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					System.Diagnostics.Debug.WriteLine(counter + " x " + x + " y " + y);
					Assert.AreEqual(PreCompTables.CubeEdgeColors[counter, 0], x);
					Assert.AreEqual(PreCompTables.CubeEdgeColors[counter++, 1], y);
				}
			}
		}
		[Test]
		public void CubeGenerator()
		{
			Random rnd = new Random(0);

			StickerCube c = new StickerCube();
			for (int i = 0; i < 1000; i++)
			{
				c.MakeMove((CubeMove)rnd.Next(18));
				Assert.AreEqual(c, (new IndexCube(c)).GetCube());
			}
		}

		[Test]
		public void MakeMove()
		{
			Random rnd = new Random(0);

			StickerCube cube = new StickerCube();
			IndexCube index = new CubeIndex();
			Assert.AreEqual(new IndexCube(cube), index);

			for (int i = 0; i < 18; i++)
			{
				index = new CubeIndex();
				cube.Reset();

				CubeMove m = (CubeMove)i;
				index.MakeMove(m);
				cube.MakeMove(m);

				IndexCube buffer = new IndexCube(cube);

				Assert.AreEqual(buffer.CornerOrientationIndex, index.CornerOrientationIndex);
				Assert.AreEqual(buffer.CornerPermutationIndex, index.CornerPermutationIndex);
				Assert.AreEqual(buffer.EdgeOrientationIndex, index.EdgeOrientationIndex);
				Assert.AreEqual(buffer.EdgePermutationIndex, index.EdgePermutationIndex);

				Assert.AreEqual(buffer, index);
			}

			for (int i = 0; i < 1000; i++)
			{
				CubeMove m = (CubeMove)rnd.Next(18);
				cube.MakeMove(m);
				index.MakeMove(m);

				System.Diagnostics.Debug.WriteLine(i + " -> " + m);

				IndexCube buffer = new IndexCube(cube);


				Assert.AreEqual(cube, index.GetCube());

				Assert.AreEqual(buffer.CornerOrientationIndex, index.CornerOrientationIndex);
				Assert.AreEqual(buffer.CornerPermutationIndex, index.CornerPermutationIndex);
				Assert.AreEqual(buffer.EdgeOrientationIndex, index.EdgeOrientationIndex);
				Assert.AreEqual(buffer.EdgePermutationIndex, index.EdgePermutationIndex);

				Assert.AreEqual(buffer, index);
			}
		}

		[Test]
		public void HasSymmetry()
		{
			IndexCube c = new CubeIndex();

			foreach (SymmetryElement se in SymmetryElement.Elements)
			{
				Assert.True(c.HasSymmetry(se), "Solved cube should have symmetry: " + se.ToString());
			}

			//Super flip
			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);
			foreach (SymmetryElement se in SymmetryElement.Elements)
			{
				Assert.True(c.HasSymmetry(se));
			}

			c = new CubeIndex();

			SymmetryElement mirrorX = new SymmetryElement(CubeColor.Red, CubeColor.Yellow, CubeColor.Green);

			c.MakeMove(CubeMove.R);

			Assert.False(c.HasSymmetry(mirrorX));

			c.MakeMove(CubeMove.LP);

			Assert.True(c.HasSymmetry(mirrorX));

			c.MakeMove(CubeMove.D2);

			//Skip idendity
			for (int i = 1; i < SymmetryElement.Elements.Length; i++)
			{
				SymmetryElement se = SymmetryElement.Elements[i];

				Assert.AreEqual(se == mirrorX, c.HasSymmetry(se));
			}
		}

		[Test]
		public void IsEqualWithSymmetry()
		{
			IndexCube c1 = new CubeIndex();
			IndexCube c2 = new CubeIndex();			

			Random rnd = new Random(0);
			
			for(int i = 0; i < 48; i++)
			{
				SymmetryElement se = SymmetryElement.Elements[i];

				c1.Reset();
				c2.Reset();

				for (int j = 0; j < 100; j++)
				{
					CubeMove cm = (CubeMove)rnd.Next(18);

					c1.MakeMove(cm);
					CubeMove transformed = se.TransformMove(cm);

					for(int k = 0; k < 18; k++)
					{
						CubeMove wrongMove = (CubeMove)k;
						if (wrongMove == transformed) continue;

						IndexCube c3 = new IndexCube(c2, wrongMove);

						Assert.IsFalse(c1.IsEqualWithSymmetry(c3, se));

					}

					c2.MakeMove(transformed);
					Assert.IsTrue(c1.IsEqualWithSymmetry(c2, se));
				}

			}
		}
	}
}
