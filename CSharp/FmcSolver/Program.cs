using CubeAD;
using CubeAD.IndexCubeSets;
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
			Random rnd = new Random(0);

			//checking f(f^-1(x)) = x
			StickerCube stickerCube = new StickerCube();
			for (int i = 0; i < 1000; i++)
			{
				stickerCube.MakeMove((CubeMove)rnd.Next(18));
				Console.WriteLine("Exptected: ");
				stickerCube.PrintSideView();
				Console.WriteLine("Res: ");
				(new IndexCube(stickerCube)).GetCube().PrintSideView();
			}


			Console.WriteLine("\nDone");
			Console.ReadLine();
		}
	}
}
