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
            Console.WriteLine(SymmetryElement.Elements[0].ToString());

            List<MoveSequenz> visited = new List<MoveSequenz>();

            visited.Add(new MoveSequenz(0));

            List<MoveSequenz> openlist = new List<MoveSequenz>() { new MoveSequenz(0) };
            List<MoveSequenz> nextList = new List<MoveSequenz>();
            MoveSequenz list = new MoveSequenz();

            for(int i = 0; i < 5; i++)
            {
                Console.WriteLine(string.Join("\n", openlist.Count));
                nextList.Clear();

                int counter = 0;
                foreach(var ms in openlist)
                {
                    MoveBlocker mb = new MoveBlocker();
                    foreach (CubeMove cm in ms.Moves)
                        mb.UpdateBlockedSoft(cm);

                    if(counter++ % 100 == 0)
                    {
                        Console.WriteLine(counter);
                    }
                    for (int m = 0; m < 18; m++)
                    {
                        if (mb[(CubeMove)m])
                            continue;
                        list.Moves.Clear();
                        list.Moves.AddRange(ms.Moves);
                        list.Moves.Add((CubeMove)m);
                        int count = list.Count;
                        list.Reduce();
                        if (list.Count < count) continue;

                        bool res = true;
                        foreach(var seq in visited)
                        {
                            if (list.EqualOverSymmetry(seq))
                            {
                                res = false;
                                break;
                            }
                        }

                        if(res)
                        {
                            visited.Add(new MoveSequenz(list));
                            nextList.Add(new MoveSequenz(list));
                        }

                    }    
                }

                (nextList, openlist) = (openlist, nextList);

                Console.WriteLine( "######################");
            }
            Console.WriteLine(string.Join("\n", openlist.Count));


            //Random rnd = new Random(0);

            //checking f(f^-1(x)) = x
            //StickerCube stickerCube = new StickerCube();
            //for (int i = 0; i < 1000; i++)
            //{
            //	stickerCube.MakeMove((CubeMove)rnd.Next(18));
            //	Console.WriteLine("Exptected: ");
            //	stickerCube.PrintSideView();
            //	Console.WriteLine("Res: ");
            //	(new IndexCube(stickerCube)).GetCube().PrintSideView();
            //}


            Console.WriteLine("\nDone");
			Console.ReadLine();
		}
	}
}
