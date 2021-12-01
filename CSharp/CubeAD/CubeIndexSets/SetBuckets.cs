using System.Collections.Generic;

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
			return Data[item.CornerPermutationIndex].Add(item);
		}

		public void Clear()
		{
			for (int i = 0; i < Data.Length; i++)
				Data[i].Clear();
		}

		public bool Contains(CubeIndex item)
		{
			return Data[item.CornerPermutationIndex].Contains(item);
		}

		public bool TryGetValue(CubeIndex item, out CubeIndex find)
		{
			return Data[item.CornerPermutationIndex].TryGetValue(item, out find);
		}
	}
}
