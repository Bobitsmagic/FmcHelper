using BenchmarkDotNet.Attributes;
using CubeAD;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeBenchmarks
{
	public class PermutationBenchmark
	{
		[Params(4, 8, 12)]
		public int N;
		int[] Perm;
		int[] InversePerm;

		[GlobalSetup]
		public void Setup()
		{
			Perm = new int[N];
			InversePerm = new int[N];

			for(int i = 0; i < N; i++)
			{
				Perm[i] = i;
				InversePerm[i] = N - i - 1;
            }
		}
		[Benchmark]
		public int PermIndex()
		{
			return Permutation.GetIndex(Perm) + Permutation.GetIndex(InversePerm);
		}
	}
}
