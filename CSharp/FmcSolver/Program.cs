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
			for(int i = 0; i < 18; i++)
			{
				CubeMove m = (CubeMove)i;

				Console.WriteLine(m + " -> " + MoveSequenz.ReverseMove(m));
			}


			SearchingAlgorithms.CheckTree();

			//Cube c = new Cube();
			//MoveSequenz ms = MoveSequenz.FmcWr;
			//c.ApplyMoveSequenz(ms);
			//c.PrintSideView();
			//c.MakeMove(CubeMove.D2);
			//c.MakeMove(CubeMove.FP);

			//CubeIndex kek = new CubeIndex(c);

			//CubeIndex.FindSolutionBruteForce(kek);
			//CubeIndex.FindCommutator(kek, 10); 

			Console.WriteLine("\nDone");
			Console.ReadLine();
		}
	}
}
