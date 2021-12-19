using CubeAD;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeTester
{
	public class PermutationTester
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
		public void PermutationTransform()
		{
			int[] perm = new int[4] { 1, 0, 2, 3};
			int[] move = new int[4] { 0, 3, 1, 2};
			int[] expected = new int[4] { 1, 3, 0, 2 };

			int[] result = new int[4];
			Permutation.Transform(perm, move, result);

			for(int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], result[i]);
			}
		}
	}
}
