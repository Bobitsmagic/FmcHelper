using System;
using System.Collections.Generic;

namespace CubeAD
{
	//A class that compresses a sequenz of moves into a single data permutation
	//[TODO] unit tests
	public static class Permutation
	{
		public static int Factorial(int n)
		{
			int ret = 1;
			for (int i = 2; i <= n; i++)
			{
				ret *= i;
			}

			return ret;
		}
		public static int[] GetIndexedPerm(int n, int index)
		{
			List<int> list = new List<int>(n);
			for (int i = 0; i < n; i++) list.Add(i);

			int[] indices = new int[n];
			for (int i = n - 1; i >= 0; i--)
			{
				indices[i] = index / Factorial(i);
				index -= indices[i] * Factorial(i);
			}

			int[] ret = new int[n];

			for (int i = ret.Length - 1; i >= 0; i--)
			{
				ret[n - i - 1] = list[indices[i]];
				list.RemoveAt(indices[i]);
			}

			return ret;
		}

		private static readonly List<int> bufferList = new List<int>();
		public static int[] GetIndexedPerm(int[] ret, int index)
		{
			int n = ret.Length;

			bufferList.Clear();
			for (int i = 0; i < n; i++) bufferList.Add(i);

			int[] indices = new int[n];
			for (int i = n - 1; i >= 0; i--)
			{
				indices[i] = index / Factorial(i);
				index -= indices[i] * Factorial(i);
			}

			for (int i = ret.Length - 1; i >= 0; i--)
			{
				ret[n - i - 1] = bufferList[indices[i]];
				bufferList.RemoveAt(indices[i]);
			}

			return ret;
		}
		public static int GetIndex(int[] perm)
		{
			int ret = 0;
			for (int i = 0; i < perm.Length; i++)
			{
				int counter = 0;
				for (int j = i + 1; j < perm.Length; j++)
				{
					if (perm[i] > perm[j])
						counter++;
				}

				ret += Factorial(perm.Length - 1 - i) * counter;
			}
			return ret;
		}

		public static int[] GetInverse(int[] perm)
		{
			int[] ret = new int[perm.Length];

			for (int i = 0; i < perm.Length; i++)
			{
				ret[perm[i]] = i;
			}

			return ret;
		}
		public static int GetInversIndex(int[] perm)
		{
			int ret = 0;
			for (int i = 0; i < perm.Length; i++)
			{
				int counter = 0;
				for (int j = i - 1; j >= 0; j--)
				{
					if (perm[i] > perm[j])
						counter++;
				}

				ret += Factorial(i) * counter;
			}
			return ret;
		}
		public static int TransfromToFacNumber(int N, int value)
		{
			int ret = 0;
			int factor = 1;
			for (int i = 2; i < N; i++)
			{
				int dec = value % i;
				ret += dec * factor;

				value /= i;
				factor *= 10;
			}
			return ret;
		}
	}
}
