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
			IndexCube[] cubes = new IndexCube[COUNT];

			for (int i = 0; i < COUNT; i++)
			{
				cubes[i] = new IndexCube(
					(uint)(i % IndexCube.MAX_EDGE_PERMUTATION),
					(ushort)(i % IndexCube.MAX_CORNER_PERMUTATION),
					(ushort)(i % IndexCube.MAX_EDGE_ORIENTATION),
					(ushort)(i % IndexCube.MAX_CORNER_ORIENTATION),
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

	//| Method |       Mean |     Error |    StdDev |     Median |
	//|------- |-----------:|----------:|----------:|-----------:|
	//| Unsafe |   314.0 ms |   6.53 ms |  19.06 ms |   304.5 ms |
	//|   Safe | 5,555.5 ms | 110.09 ms | 250.73 ms | 5,561.8 ms |
}
