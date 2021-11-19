using CubeAD;
using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FmcSolver
{
	class Program
	{
		static void Main(string[] args)
		{
			Cube c = new Cube();
			MoveSequenz ms = MoveSequenz.FmcWr;
			c.ApplyMoveSequenz(ms);
			c.PrintSideView();

			CubeIndex kek = new CubeIndex(c);

			SearchingAlgorithms.FindSolutionBruteForceMultithreaded(kek);

			Console.WriteLine("\nDone");
			Console.ReadLine();
		}
	}
}
