using System.Collections.Generic;

namespace CubeAD.CubeIndexSets
{
	public class RadixTreeDictionary
	{
		const int MAX_DEPTH = 10;

		public int Count => GetCount();

		public readonly int Depth;
		Dictionary<byte, RadixTreeDictionary> SubTree = new Dictionary<byte, RadixTreeDictionary>();

		public RadixTreeDictionary()
		{
			Depth = 0;
		}
		private RadixTreeDictionary(int depth, CubeIndex value)
		{
			Depth = depth;

			if (Depth < MAX_DEPTH)
			{
				SubTree.Add(value[Depth], new RadixTreeDictionary(Depth + 1, value));
			}
		}

		public bool Add(CubeIndex value)
		{
			if (Depth < MAX_DEPTH)
			{
				byte index = value[Depth];
				if (SubTree.ContainsKey(index))
				{
					return SubTree[index].Add(value);
				}
				else
				{
					SubTree.Add(index, new RadixTreeDictionary(Depth + 1, value));
					return true;
				}

			}
			else
				return false;
		}

		public void Clear()
		{
			foreach(var tree in SubTree.Values)
			{
				tree.Clear();
			}

			SubTree.Clear();
		}

		private int GetCount()
		{
			if (Depth == MAX_DEPTH)
				return 1;

			int sum = 0;
			foreach (RadixTreeDictionary tree in SubTree.Values)
				sum += tree.GetCount();

			return sum;
		}
		public bool Contains(CubeIndex value)
		{
			if (Depth < MAX_DEPTH)
			{
				if (SubTree.ContainsKey(value[Depth]))
					return SubTree[value[Depth]].Contains(value);
				else
					return false;
			}
			else
				return true;
		}

		public override string ToString()
		{
			return "Count: " + Count;
		}
	}
}
