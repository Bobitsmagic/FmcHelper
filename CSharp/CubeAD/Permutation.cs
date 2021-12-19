using System;
using System.Collections.Generic;

namespace CubeAD
{
	/// <summary>
	/// A collection of functions for permutations
	/// </summary>
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

		/// <returns> A permutation of <paramref name="n"/> elements with a zero-based lexicographic <paramref name="index"/> </returns>
		public static int[] GetIndexedPerm(int n, int index)
		{
			int[] ret = new int[n];
			GetIndexedPerm(ret, index);
			return ret;
		}

		private static readonly List<int> bufferList = new List<int>();
		/// <summary>
		/// Creates a permutation and inserts it into <paramref name="permutation"/> with a zero-based lexicographic <paramref name="index"/>
		/// </summary>
		public static void GetIndexedPerm(int[] permutation, int index)
		{
			int n = permutation.Length;

			bufferList.Clear();
			for (int i = 0; i < n; i++) bufferList.Add(i);

			int[] indices = new int[n];
			for (int i = n - 1; i >= 0; i--)
			{
				indices[i] = index / Factorial(i);
				index -= indices[i] * Factorial(i);
			}

			for (int i = permutation.Length - 1; i >= 0; i--)
			{
				permutation[n - i - 1] = bufferList[indices[i]];
				bufferList.RemoveAt(indices[i]);
			}
		}

		/// <returns> The lexicographic index of <paramref name="permutation"/> </returns>
		public static int GetIndex(int[] permutation)
		{
			int ret = 0;
			for (int i = 0; i < permutation.Length; i++)
			{
				int counter = 0;
				for (int j = i + 1; j < permutation.Length; j++)
				{
					if (permutation[i] > permutation[j])
						counter++;
				}

				ret += Factorial(permutation.Length - 1 - i) * counter;
			}
			return ret;
		}

		/// <returns> The inverse of <paramref name="permutation"/> </returns>
		public static int[] GetInverse(int[] permutation)
		{
			int[] ret = new int[permutation.Length];

			for (int i = 0; i < permutation.Length; i++)
			{
				ret[permutation[i]] = i;
			}

			return ret;
		}

		/// <summary>
		/// Stores the composition <paramref name="left"/> ° <paramref name="right"/> in <paramref name="result"/>
		/// </summary>
		public static void Transform(int[] left, int[] right, int[] result)
		{
			if (left.Length != right.Length || left.Length != result.Length)
				throw new ArgumentException("Arrays need to have the same length");

			for (int i = 0; i < left.Length; i++)
			{
				result[i] = left[right[i]];
			}
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
