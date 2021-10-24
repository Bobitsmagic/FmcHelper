using System;
using System.Collections.Generic;
using System.Text;

namespace CubeAD
{
	public class RadixTreeArray
	{
		const int MAX_DEPTH = 10;
		const int BRANCHING_FACTOR = 256;

		public int Count => GetCount();

		public readonly int Depth;
		RadixTreeArray[] SubTree;

		public RadixTreeArray()
		{
			Depth = 0;
			SubTree = new RadixTreeArray[BRANCHING_FACTOR];
		}
		private RadixTreeArray(int depth, CubeIndex value)
		{
			Depth = depth;

			if(Depth < MAX_DEPTH)
			{
				SubTree = new RadixTreeArray[BRANCHING_FACTOR];

				SubTree[value[Depth]] = new RadixTreeArray(Depth + 1, value);
			}
		}

		public bool Add(CubeIndex value)
		{
			if (Depth < MAX_DEPTH)
			{
				byte index = value[Depth];
				if (SubTree[index] is null)
				{
					SubTree[index] = new RadixTreeArray(Depth + 1, value);
					return true;
				}
				else
					return SubTree[index].Add(value);
			}
			else 
				return false;
		}


		private int GetCount()
		{
			if (Depth == MAX_DEPTH)
				return 1;

			int sum = 0;
			for (int i = 0; i < SubTree.Length; i++)
			{
				if(SubTree[i] != null)
				{
					sum += SubTree[i].GetCount();
				}
			}

			return sum;
		}
		public bool Contains(CubeIndex value)
		{
			if (Depth < MAX_DEPTH)
			{
				if (SubTree[value[Depth]] is null)
					return false;
				else
					return SubTree[value[Depth]].Contains(value);
			}
			else
				return true;
		}

		public long Size()
		{
			long sum = 8;

			if(SubTree != null)
			{
				sum += SubTree.Length * 4;

				for(int i = 0; i < SubTree.Length; i++)
				{
					if(SubTree[i] != null)
					{
						sum += SubTree[i].Size();
					}
				}
			}

			return sum;
		}
	}
}
