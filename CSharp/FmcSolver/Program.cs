using CubeAD;
using CubeAD.IndexCubeSets;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FmcSolver
{
	class Program
    {


//1
//2
//10
//83
//1180
//17025
		static void Main(string[] args)
		{
			ArrayCube cube = new ArrayCube();
			ArrayCube superFLip = new ArrayCube();
			foreach(var move in MoveSequenz.SuperFlip.Moves)
			{
				superFLip.MakeMove(move);
			}
			
            for(int x = 0; x < 6; x++)
			{
				for(int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2)
						continue;

					if (cube.GetEdgeColor(x, y) != (CubeColor)x)
                        Console.WriteLine("Alarm");

                    if (superFLip.GetEdgeColor(x, y) != (CubeColor)y)
                        Console.WriteLine("Superflip Alarm");

					for(int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2)
							continue;
						if (superFLip.GetCornerColor(x, y, z) != (CubeColor)x)
						{
                            Console.WriteLine("Corner Alarm");
                        }

					}

                }
			}

            Console.WriteLine("\nDone");
			Console.ReadLine();
		}
	}
}
