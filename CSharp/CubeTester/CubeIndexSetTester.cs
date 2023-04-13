using CubeAD;
using CubeAD.CubeRepresentation;
using CubeAD.IndexCubeSets;
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
			var hashset = new HashSet<IndexCube>();
			var sortCubeInd = new SortedListSet();
			var bucketCubeInd = new SortedBucketsSet();

			Random rnd = new Random();
			List<IndexCube> cubeList = new List<IndexCube>(SearchingAlgorithms.GenerateRandomCubes(rnd, 1000));
			
			//adding duplicates
			for (int i = 0; i < 200; i++)
				cubeList[rnd.Next(cubeList.Count)] = cubeList[rnd.Next(cubeList.Count)];

			foreach (IndexCube index in cubeList)
			{
				hashset.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);

				sortCubeInd.RemoveDuplicates();
				bucketCubeInd.RemoveDuplicates();

				Assert.AreEqual(hashset.Count, sortCubeInd.Count);
				Assert.AreEqual(hashset.Count, bucketCubeInd.Count);
			}

			Assert.AreNotEqual(hashset.Count, 1000);
		}

		[Test]
		public void ClearOperation()
		{
			var hashset = new HashSet<IndexCube>();
			var sortCubeInd = new SortedListSet();
			var bucketCubeInd = new SortedBucketsSet();

			foreach (IndexCube index in SearchingAlgorithms.GenerateRandomCubes(new Random(), 1000))
			{
				hashset.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			Assert.AreEqual(hashset.Count, 1000);
			Assert.AreEqual(hashset.Count, sortCubeInd.Count);
			Assert.AreEqual(hashset.Count, bucketCubeInd.Count);

			hashset.Clear();
			sortCubeInd.Clear();
			bucketCubeInd.Clear();

			Assert.AreEqual(0, hashset.Count);
			Assert.AreEqual(0, sortCubeInd.Count);
			Assert.AreEqual(0, bucketCubeInd.Count);

			foreach (IndexCube index in SearchingAlgorithms.GenerateRandomCubes(new Random(), 1000))
			{
				hashset.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			Assert.AreEqual(hashset.Count, 1000);
			Assert.AreEqual(hashset.Count, sortCubeInd.Count);
			Assert.AreEqual(hashset.Count, bucketCubeInd.Count);
		}

		[Test]
		public void ContainsOperation()
		{
			var hashset = new HashSet<IndexCube>();
			var sortCubeInd = new SortedListSet();
			var bucketCubeInd = new SortedBucketsSet();

			List<IndexCube> list = SearchingAlgorithms.GenerateRandomCubes(new Random(), 1000).ToList();
			for (int i = 0; i < list.Count; i += 10)
			{
				IndexCube index = list[i];
				hashset.Add(index);
				sortCubeInd.Add(index);
				bucketCubeInd.Add(index);
			}

			sortCubeInd.RemoveDuplicates();
			bucketCubeInd.RemoveDuplicates();

			var sealedHS = new SealedHashset(hashset.ToArray());

			foreach (IndexCube index in list)
			{
				Assert.AreEqual(hashset.Contains(index), sortCubeInd.Contains(index));
				Assert.AreEqual(hashset.Contains(index), bucketCubeInd.Contains(index));
				Assert.AreEqual(hashset.Contains(index), sealedHS.Contains(index));
			}
		}
	}
}
