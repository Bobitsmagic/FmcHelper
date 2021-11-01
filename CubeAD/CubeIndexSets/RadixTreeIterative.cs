using System.Buffers;

namespace CubeAD.CubeIndexSets
{
	public class RadixTreeIterative
	{
		const int MAX_DEPTH = 10;
		const int BRANCHING_FACTOR = 256;
		const int ARRAY_COUNT = 1 << 25;

		static ArrayPool<int> ArrayPool = ArrayPool<int>.Create();

		public int Count => GetCount();

		int[][] Data = new int[ARRAY_COUNT][];
		int NextFree = 1;

		public RadixTreeIterative()
		{
			Data[0] = ArrayPool.Rent(BRANCHING_FACTOR);
		}

		public bool Add(CubeIndex value)
		{
			int[] currentArray = Data[0];
			byte index;
			for (int d = 0; d < MAX_DEPTH - 1; d++)
			{
				index = value[d];

				if (currentArray[index] == 0)
				{
					Data[NextFree] = ArrayPool.Rent(BRANCHING_FACTOR);

					currentArray[index] = NextFree;
					currentArray = Data[NextFree++];
				}
				else
					currentArray = Data[currentArray[index]];
			}

			index = value[MAX_DEPTH - 1];
			if (currentArray[index] == 0)
			{
				currentArray[index] = 1337;

				return true;
			}
			else
				return false;
		}

		public void Clear()
		{
			for(int i = 0; i < Data.Length; i++)
			{
				if(Data[i] != null)
				{
					ArrayPool.Return(Data[i], true);
					Data[i] = null;
				}
			}

			NextFree = 1;
			Data[0] = ArrayPool.Rent(BRANCHING_FACTOR);
		}

		public bool Contains(CubeIndex value)
		{
			int[] currentArray = Data[0];
			for (int d = 0; d < MAX_DEPTH - 1; d++)
			{
				byte index = value[d];

				if (currentArray[index] != 0)
					currentArray = Data[currentArray[index]];
				else
					return false;
			}

			return currentArray[value[MAX_DEPTH - 1]] != 0;
		}

		private int GetCount()
		{
			return RecCount(Data[0], 0);

			int RecCount(int[] array, int depth)
			{
				int sum = 0;
				if (depth < MAX_DEPTH - 1)
				{
					for (int i = 0; i < BRANCHING_FACTOR; i++)
						if (array[i] != 0)
							sum += RecCount(Data[array[i]], depth + 1);
				}
				else
				{
					for (int i = 0; i < BRANCHING_FACTOR; i++)
						if (array[i] != 0)
							sum++;
				}
				return sum;
			}
		}

		public override string ToString()
		{
			return "Count: " + Count;
		}
	}
}
