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
			IndexCube cube = new IndexCube();
			StickerCube sticker = cube.GetCube();
			sticker.PrintSideView();
			sticker.MakeMove(CubeMove.R);
			sticker.PrintSideView();
			StickerCube buffer = new StickerCube();
			SymmetryElement symmetry = new SymmetryElement(CubeColor.Blue, CubeColor.Yellow, CubeColor.Red);
			sticker.InsertSymmetryTransformation(symmetry, buffer);

			buffer.PrintSideView();

			buffer.MakeMove(symmetry.TransformMove(CubeMove.RP));
			
			buffer.PrintSideView();


			Console.WriteLine("\nDone");
			Console.ReadLine();
		}
	}
}
