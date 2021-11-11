using CubeAD;
using CubeAD.CubeIndexSets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;



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
			var sortCubeInd = new SortedCubeIndexList();
			var bucketCubeInd = new SortedBuckets();
			var setBucket = new SetBuckets();

			foreach (CubeIndex index in Cube.GetRandomCubes(4, 1000, new Random(0)))
			{
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
				setBucket.Add(index);

				sortCubeInd.RemoveDuplicates();
				bucketCubeInd.RemoveDuplicates();

				Assert.AreEqual(hashset.Count, radixTreeArray.Count);
				Assert.AreEqual(hashset.Count, radixTreeDic.Count);
				Assert.AreEqual(hashset.Count, radixTreeIt.Count);
				Assert.AreEqual(hashset.Count, sortCubeInd.Count);
				Assert.AreEqual(hashset.Count, bucketCubeInd.Count);
				Assert.AreEqual(hashset.Count, setBucket.Count);
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
			var sortCubeInd = new SortedCubeIndexList();
			var bucketCubeInd = new SortedBuckets();
			var setBucket = new SetBuckets();

			foreach (CubeIndex index in Cube.GetRandomCubes(4, 1000, new Random(0)))
			{
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
				setBucket.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			Assert.AreNotEqual(hashset.Count, 1000);
			Assert.AreEqual(hashset.Count, radixTreeArray.Count);
			Assert.AreEqual(hashset.Count, radixTreeDic.Count);
			Assert.AreEqual(hashset.Count, radixTreeIt.Count);
			Assert.AreEqual(hashset.Count, sortCubeInd.Count);
			Assert.AreEqual(hashset.Count, bucketCubeInd.Count);
			Assert.AreEqual(hashset.Count, setBucket.Count);

			hashset.Clear();
			radixTreeArray.Clear();
			radixTreeDic.Clear();
			radixTreeIt.Clear();
			sortCubeInd.Clear();
			bucketCubeInd.Clear();
			setBucket.Clear();

			Assert.AreEqual(0, hashset.Count);
			Assert.AreEqual(0, radixTreeArray.Count);
			Assert.AreEqual(0, radixTreeDic.Count);
			Assert.AreEqual(0, radixTreeIt.Count);
			Assert.AreEqual(0, sortCubeInd.Count);
			Assert.AreEqual(0, bucketCubeInd.Count);
			Assert.AreEqual(0, setBucket.Count);

			foreach (CubeIndex index in Cube.GetRandomCubes(4, 1000, new Random(0)))
			{
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
				setBucket.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			Assert.AreNotEqual(hashset.Count, 1000);
			Assert.AreEqual(hashset.Count, radixTreeArray.Count);
			Assert.AreEqual(hashset.Count, radixTreeDic.Count);
			Assert.AreEqual(hashset.Count, radixTreeIt.Count);
			Assert.AreEqual(hashset.Count, sortCubeInd.Count);
			Assert.AreEqual(hashset.Count, bucketCubeInd.Count);
			Assert.AreEqual(hashset.Count, setBucket.Count);
		}

		[Test]
		public void ContainsOperation()
		{
			var hashset = new HashSet<CubeIndex>();
			var radixTreeDic = new RadixTreeDictionary();
			var radixTreeArray = new RadixTreeArray();
			var radixTreeIt = new RadixTreeIterative();
			var sortCubeInd = new SortedCubeIndexList();
			var bucketCubeInd = new SortedBuckets();
			var setBuckets = new SetBuckets();

			List<CubeIndex> list = Cube.GetRandomCubes(20, 1000, new Random(0));
			for (int i = 0; i < list.Count; i += 10)
			{
				CubeIndex index = list[i];
				hashset.Add(index);
				radixTreeArray.Add(index);
				radixTreeDic.Add(index);
				radixTreeIt.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
				setBuckets.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			var sealedHS = new SealedHashset(hashset.ToArray());


			foreach (CubeIndex index in list)
			{
				Assert.AreEqual(hashset.Contains(index), radixTreeArray.Contains(index));
				Assert.AreEqual(hashset.Contains(index), radixTreeDic.Contains(index));
				Assert.AreEqual(hashset.Contains(index), radixTreeIt.Contains(index));
				Assert.AreEqual(hashset.Contains(index), sortCubeInd.Contains(index));
				Assert.AreEqual(hashset.Contains(index), bucketCubeInd.Contains(index));
				Assert.AreEqual(hashset.Contains(index), setBuckets.Contains(index));
				Assert.AreEqual(hashset.Contains(index), sealedHS.Contains(index));

			}
		}
	}
}
