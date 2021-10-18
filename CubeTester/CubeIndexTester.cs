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
	}
}
