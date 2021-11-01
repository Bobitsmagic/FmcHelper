using CubeAD;
using CubeAD.CubeIndexSets;
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
			var bucketCubeInd = new BucketCubeIndices();

			foreach (CubeIndex index in Cube.GetRandomCubes(4, 1000, new Random(0)))
			{
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);

				sortCubeInd.RemoveDuplicates();
				bucketCubeInd.RemoveDuplicates();

				Assert.AreEqual(hashset.Count, radixTreeArray.Count);
				Assert.AreEqual(hashset.Count, radixTreeDic.Count);
				Assert.AreEqual(hashset.Count, radixTreeIt.Count);
				Assert.AreEqual(hashset.Count, sortCubeInd.Count);
				Assert.AreEqual(hashset.Count, bucketCubeInd.Count);
			}

			Assert.AreNotEqual(hashset.Count, 1000);
		}

		[Test]
		public void ClearOperation()
		{
			var hashset = new HashSet<CubeIndex>();
			var radixTreeDic = new RadixTreeDictionary();
			var radixTreeArray = new RadixTreeArray();
			var radixTreeIt = new RadixTreeIterative();
			var sortCubeInd = new SortedCubeIndices();
			var bucketCubeInd = new BucketCubeIndices();

			int count = 0;
			foreach (CubeIndex index in Cube.GetRandomCubes(4, 1000, new Random(0)))
			{
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			Assert.AreNotEqual(hashset.Count, 1000);
			Assert.AreEqual(hashset.Count, radixTreeArray.Count);
			Assert.AreEqual(hashset.Count, radixTreeDic.Count);
			Assert.AreEqual(hashset.Count, radixTreeIt.Count);
			Assert.AreEqual(hashset.Count, sortCubeInd.Count);
			Assert.AreEqual(hashset.Count, bucketCubeInd.Count);

			hashset.Clear();
			radixTreeArray.Clear();
			radixTreeDic.Clear();
			radixTreeIt.Clear();
			sortCubeInd.Clear();
			bucketCubeInd.Clear();

			Assert.AreEqual(0, hashset.Count);
			Assert.AreEqual(0, radixTreeArray.Count);
			Assert.AreEqual(0, radixTreeDic.Count);
			Assert.AreEqual(0, radixTreeIt.Count);
			Assert.AreEqual(0, sortCubeInd.Count);
			Assert.AreEqual(0, bucketCubeInd.Count);

			foreach (CubeIndex index in Cube.GetRandomCubes(4, 1000, new Random(0)))
			{
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			Assert.AreNotEqual(hashset.Count, 1000);
			Assert.AreEqual(hashset.Count, radixTreeArray.Count);
			Assert.AreEqual(hashset.Count, radixTreeDic.Count);
			Assert.AreEqual(hashset.Count, radixTreeIt.Count);
			Assert.AreEqual(hashset.Count, sortCubeInd.Count);
			Assert.AreEqual(hashset.Count, bucketCubeInd.Count);
		}
	}
}
