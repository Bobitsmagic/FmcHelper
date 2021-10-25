using CubeAD;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;


namespace CubeTester
{
	class CubeIndexSetTester
	{
		[Test]
		public void AddOperation()
		{
			var hashset = new HashSet<CubeIndex>();
			var radixTreeDic = new RadixTreeDictionary();
			var radixTreeArray = new RadixTreeArray();
			var radixTreeIt = new RadixTreeIterative();
			var sortCubeInd = new SortedCubeIndices();

			foreach (CubeIndex index in Cube.GetRandomCubesList(4, 1000, new Random(0)))
			{
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				
				sortCubeInd.RemoveDuplicates();

				Assert.AreEqual(hashset.Count, radixTreeArray.Count);
				Assert.AreEqual(hashset.Count, radixTreeDic.Count);
				Assert.AreEqual(hashset.Count, radixTreeIt.Count);
				Assert.AreEqual(hashset.Count, sortCubeInd.Count);
			}

			Assert.AreNotEqual(hashset.Count, 1000);
		}
	}
}
