using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeAD.CubeIndexSets
{
	public class SealedHashset
	{
		const int LENGTH = (int)CubeIndex.MAX_EDGE_PERMUTATION;
		CubeIndex[] Data;
		int[] StartIndex = new int[LENGTH + 1];

		//Input array is not preserved!
		public SealedHashset(CubeIndex[] cubes)
		{
			Data = new CubeIndex[cubes.Length];
			cubes.CopyTo(Data, 0);

			CubeIndex.RadixSortCubeIndices(Data, cubes);

			for(int i = 1; i < Data.Length; i++)
				if (Data[i] == Data[i - 1])
					throw new ArgumentException("cubes must be distinct");
			

			//count cubes
			for (int i = 0; i < cubes.Length; i++)
				StartIndex[cubes[i].EdgePermutation]++;

			double Mean = (double)cubes.Length / LENGTH;
			double Std = 0;
			for (int i = 0; i < StartIndex.Length; i++)
				Std += (StartIndex[i] - Mean) * (StartIndex[i] - Mean);

			Std /= LENGTH;


			Console.WriteLine("Maximum bucket size: " + StartIndex.Max());
			Console.WriteLine("Minimum bucket size: " + StartIndex.Min());
			Console.WriteLine("Mean: " + Mean);
			Console.WriteLine("Std: " + Std);

			//Sum up the first i cubes
			for (int i = 1; i < StartIndex.Length; i++)
				StartIndex[i] += StartIndex[i - 1];

			//calculated indices
			for (int i = StartIndex.Length - 1; i >= 1; i--)
				StartIndex[i] = StartIndex[i - 1];
			
			StartIndex[0] = 0;
		}

		public bool Contains(CubeIndex index)
		{
			int start = StartIndex[index.EdgePermutation];
			int end = StartIndex[index.EdgePermutation + 1];

			for (int i = start; i < end; i++)
			{
				if (Data[i] == index)
					return true;
			}

			return false;
		}


	}
}
