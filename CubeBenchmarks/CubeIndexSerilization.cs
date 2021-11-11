using BenchmarkDotNet.Attributes;
using CubeAD;
using CubeAD.CubeIndexSets;
namespace CubeBenchmarks
{
	public class CubeIndexSerilization
	{
		const int COUNT = 1 << 27;
		static SealedHashset set;
		static CubeIndexSerilization()
		{
			CubeIndex[] cubes = new CubeIndex[COUNT];

			for (int i = 0; i < COUNT; i++)
			{
				cubes[i] = new CubeIndex(
					(uint)(i % CubeIndex.MAX_EDGE_PERMUTATION),
					(ushort)(i % CubeIndex.MAX_CORNER_PERMUTATION),
					(ushort)(i % CubeIndex.MAX_EDGE_ORIENTATION),
					(ushort)(i % CubeIndex.MAX_CORNER_ORIENTATION),
					(CubeMove)(i % 18));
			}

			set = new SealedHashset(cubes);
		}

		[Benchmark]
		public void Unsafe()
		{
			set.ToByteArrayUnsafe();
		}

		[Benchmark]
		public void Safe()
		{
			set.ToByteArray();
		}

	}
}
