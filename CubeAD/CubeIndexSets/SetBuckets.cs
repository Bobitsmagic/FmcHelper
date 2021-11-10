using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CubeAD.CubeIndexSets
{
	public class SetBuckets
	{
		public int Count
		{
			get
			{
				int sum = 0;
				for (int i = 0; i < Data.Length; i++)
					sum += Data[i].Count;

				return sum;
			}
		}

		HashSet<CubeIndex>[] Data = new HashSet<CubeIndex>[CubeIndex.MAX_CORNER_PERMUTATION];

		public SetBuckets(int bucketCapacity = 0)
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i] = new HashSet<CubeIndex>(bucketCapacity);
			}
		}

		public bool Add(CubeIndex item)
		{
			return Data[item.CornerPermutation].Add(item);
		}

		public void Clear()
		{
			for (int i = 0; i < Data.Length; i++)
				Data[i].Clear();
		}

		public bool Contains(CubeIndex item)
		{
			return Data[item.CornerPermutation].Contains(item);
		}

		public bool TryGetValue(CubeIndex item, out CubeIndex find)
		{
			return Data[item.CornerPermutation].TryGetValue(item, out find);
		}
	}
}
