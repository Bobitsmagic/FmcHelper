using CubeAD.CubeRepresentation;
using CubeAD.CubeIndexSets;
using CubeAD.IndexCubeSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CubeAD.PreCompTables;
using System.ComponentModel;

namespace CubeAD
{
    /// <summary>
    /// Provides algorithms for searching through diffrent sets of cubes
    /// </summary>
    public static class SearchingAlgorithms
	{
		public static List<IndexCube> GenerateRandomCubes(Random rnd, int count)
		{
			List<IndexCube> list = new List<IndexCube>(count);

			for(int i = 0; i < count; i++)
			{
				list.Add(new IndexCube(rnd));
			}

			return list;
		}
	}
}
