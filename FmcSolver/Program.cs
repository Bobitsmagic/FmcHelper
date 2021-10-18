using CubeAD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FmcSolver
{
	class Program
	{
		static void Main(string[] args)
		{
			for(int i = 0; i < 12; i++)
			{
				Console.WriteLine(i + " " +  Cube.GetSolvedCubes(i).Count);
			}
			Console.ReadLine();

			Cube c = new Cube();
			c.ApplyMoveSequenz(MoveSequenz.SuperFlip);
			c.PrintSideView();

			Console.ReadLine();

			PLLAlgos.Init();

			Console.ReadLine();
			ZBLLAlgos.FromFile();

			//Console.WriteLine(FindMinmalCycle());

			c = new Cube();
			MoveSequenz s = new MoveSequenz(20);
			Console.WriteLine(s);
			c.ApplyMoveSequenz(s);
			c.PrintSideView();
			c = FindSolution(c);
			c.ResetBlocked();
			c = FindSolution(c);
			c.ResetBlocked();
			c = FindSolution(c);
			//NissHelper();

			//PrintValues();

			Console.WriteLine("\nDone");
			Console.ReadLine();
		}

		static void PrintValues()
		{
			MoveSequenz s = new MoveSequenz("B2 L' R F2 R B L2 D B U L2 D' F' U' L' F' L D' B' U");
			Cube c = new Cube();
			Console.WriteLine(c.GetValue().ToString("00 00 00"));

			c.ApplyMoveSequenz(s);

			MoveSequenz solution = new MoveSequenz("FP U2 DP BP DP R D R2 D RP D2 L2 UP R");


			foreach(CubeMove m in solution.Moves)
			{
				Console.WriteLine(c.GetValue().ToString("00 00 00"));

				c.MakeMove(m);
			}

			Console.WriteLine(c.GetValue().ToString("00 00 00"));

		}

		static int FindMinmalCycle()
		{
			HashSet<Cube> Openlist = new HashSet<Cube>();
			HashSet<Cube> CloseList = new HashSet<Cube>();

			Cube start = new Cube();

			CloseList.Add(start);
			int depth = 1;
			List<CubeMove> moves = new List<CubeMove>(18);
			while (true)
			{
				Console.WriteLine("Depth: " + depth);

				Console.WriteLine("Count: " + CloseList.Count);
				foreach(Cube c in CloseList)
				{
					c.AddPossibleMoves(moves);

					for (int i = 0; i < moves.Count; i++)
					{
						//if (((int)moves[i] / 3) % 2 == 0) continue;

						Cube buffer = new Cube(c, moves[i]);

						if (buffer.IsSolved)
						{
							return depth + 2;
						}

						Openlist.Add(buffer);
					}
				}

				Console.WriteLine(Openlist.Count + " / " + CloseList.Count + " = " + (float)Openlist.Count / CloseList.Count);

				CloseList.Clear();

				HashSet<Cube> bufferList = Openlist;
				Openlist = CloseList;
				CloseList = bufferList;
				depth++;
			}

			return depth;
		}

		static void OrientEdges()
		{
			const int MAX_DEPTH = 7;

			Cube c = new Cube();
			MoveSequenz s = new MoveSequenz(20);
			//Console.WriteLine(s);
			c.ApplyMoveSequenz(s);
			//c.PrintSideView();

			bool FoundSolution = false;
			Cube best = null;
			List<CubeMove>[] moveBuffer = new List<CubeMove>[MAX_DEPTH + 1];
			for (int i = 0; i < moveBuffer.Length; i++)
				moveBuffer[i] = new List<CubeMove>(18);

			//Console.WriteLine("EdgeCount: " + c.CountOrientedEgdes());
			Stack<CubeMove> currentMoves = new Stack<CubeMove>();
			List<CubeMove> solution = new List<CubeMove>();

			long counter = 0;
			long solutionCounter = 0;
			long start = Environment.TickCount64;
			int depthStop;
			for (int i = 1; i <= MAX_DEPTH && !FoundSolution; i++)
			{
				depthStop = i;
				//Console.WriteLine(depthStop);
				FindBestCube(c, 0);
			}

			long end = Environment.TickCount64;

			void FindBestCube(Cube cube, int depth)
			{
				counter++;

				if(cube.CountOrientedEgdes() == 12)
				{
					if (!FoundSolution)
					{
						FoundSolution = true;
						best = cube;
						solution.Clear();
						solution.AddRange(currentMoves);
					}
					
					solutionCounter++;
				}

				if (depth < depthStop)
				{
					List<CubeMove> list = moveBuffer[depth];
					list.Clear();
					cube.AddPossibleMoves(list);

					for (int i = 0; i < list.Count; i++)
					{
						currentMoves.Push(list[i]);
						FindBestCube(new Cube(cube, list[i]), depth + 1);
						currentMoves.Pop();
					}
				}

			}

			solution.Reverse();
			//Console.WriteLine(string.Join(" ", solution));
			//Console.WriteLine("Time: " + (end - start).ToString("000 000"));
			if(solution.Count > 5)
				Console.WriteLine("Solutions found: " + solutionCounter + "\tLength: " + solution.Count);
			//Console.WriteLine("Positions searched: " + counter.ToString("000 000 000"));
			//best.PrintSideView();
		}
		static void FindBlocks()
		{
			const int MAX_DEPTH = 6;

			Cube c = new Cube();
			MoveSequenz s = new MoveSequenz(20);
			Console.WriteLine(s);
			c.ApplyMoveSequenz(s);
			c.PrintSideView();

			int bestVal = 0;
			Cube best = null;
			List<CubeMove>[] moveBuffer = new List<CubeMove>[MAX_DEPTH + 1];
			for (int i = 0; i < moveBuffer.Length; i++) 
				moveBuffer[i] = new List<CubeMove>(18);


			Stack<CubeMove> currentMoves = new Stack<CubeMove>();
			List<CubeMove> solution = new List<CubeMove>();

			long counter = 0;
			long start = Environment.TickCount64;
			int depthStop;
			for(int i = 1; i <= MAX_DEPTH; i++)
			{
				depthStop = i;
				Console.WriteLine(depthStop);
				FindBestCube(c, 0);
			}

			long end = Environment.TickCount64;

			void FindBestCube(Cube cube, int depth)
			{
				counter++;

				int value = cube.GetValue();
				if (value > bestVal)
				{
					bestVal = value;
					best = cube;
					//Console.WriteLine("NewBest: " + value);

					solution.Clear();
					solution.AddRange(currentMoves);
				}

				if (depth < depthStop)
				{
					List<CubeMove> list = moveBuffer[depth];
					list.Clear();
					cube.AddPossibleMoves(list);

					for (int i = 0; i < list.Count; i++)
					{
						currentMoves.Push(list[i]);
						FindBestCube(new Cube(cube, list[i]), depth + 1);
						currentMoves.Pop();
					}
				}

			}

			solution.Reverse();
			Console.WriteLine(string.Join(" " , solution));
			Console.WriteLine("Time: " + (end - start).ToString("000 000"));
			Console.WriteLine("Positions searched: " + counter.ToString("000 000 000 000"));
			Console.WriteLine("Bestval: " + bestVal);
			best.PrintSideView();

			best.GetValue();
		}

		static Cube FindSolution(Cube c)
		{
			const int MAX_DEPTH = 7;

			long counter = 0;

			Dictionary<Cube, List<CubeMove>> EOSolutions = FindEOSolution(c);
			Console.WriteLine("Finished EO: " + EOSolutions.Count);

			(Cube Cube, List<CubeMove> Moves)[] solutionBuffer = new (Cube, List<CubeMove>)[EOSolutions.Count];

			long start = Environment.TickCount64;
			int threadCounter = 0;

			Parallel.For(0, EOSolutions.Count, (i) =>
			{
				solutionBuffer[i] = FindBlockSolutions(EOSolutions.Keys.ElementAt(i));
			});
			//for (int i = 0; i < EOSolutions.Count; i++)
			//{
			//	Console.WriteLine(string.Join(" ", EOSolutions.ElementAt(i).Value));
			//	solutionBuffer[i] = FindBlockSolutions(EOSolutions.ElementAt(i).Key);
			//}

			long end = Environment.TickCount64;

			int bestIndex = -1;
			int bestValue = -1;
			for (int i = 0; i < solutionBuffer.Length; i++)
			{
				int val = solutionBuffer[i].Cube.GetValue();

				if (val > bestValue)
				{
					bestValue = val;
					bestIndex = i;
				}
			}

			c = solutionBuffer[bestIndex].Cube;
			List<CubeMove> FinalSolution = solutionBuffer[bestIndex].Moves;
			FinalSolution.AddRange(EOSolutions.Values.ElementAt(bestIndex));
			FinalSolution.Reverse();

			Console.WriteLine(string.Join(" ", FinalSolution) + " (" + FinalSolution.Count + ")");
			Console.WriteLine("Time: " + (end - start).ToString("000 000"));
			Console.WriteLine("Positions searched: " + counter.ToString("000 000 000"));
			Console.WriteLine("Bestval: " + bestValue.ToString("00 00 00"));
			c.PrintSideView();

			return c;
			Dictionary<Cube, List<CubeMove>> FindEOSolution(Cube cube)
			{
				bool FoundSolution = false;

				Stack<CubeMove> currentMoves = new Stack<CubeMove>();
				List<CubeMove> solution = new List<CubeMove>();
				Dictionary<Cube, List<CubeMove>> solutions = new Dictionary<Cube, List<CubeMove>>(500);

				//Avoid GC Pressure
				List<CubeMove>[] moveBuffer = new List<CubeMove>[20];
				for (int i = 0; i < moveBuffer.Length; i++)
					moveBuffer[i] = new List<CubeMove>(18);

				long start = Environment.TickCount64;
				int eoDepthStop = 0;
				for (int i = 1; i <= 6 && !FoundSolution; i++)
				{
					eoDepthStop = i;
					Solve(c, 0);
				}

				return solutions;

				void Solve(Cube cube, int depth)
				{
					counter++;

					if (cube.CountOrientedEgdes() == 12)
					{
						if (!FoundSolution)
						{
							FoundSolution = true;
							Console.WriteLine(depth);

						}
						solutions.Add(cube, new List<CubeMove>(currentMoves));
					}
					else
					{

						if (depth < eoDepthStop)
						{
							List<CubeMove> list = moveBuffer[depth];
							list.Clear();
							cube.AddPossibleMoves(list);

							for (int i = 0; i < list.Count; i++)
							{
								currentMoves.Push(list[i]);
								Solve(new Cube(cube, list[i]), depth + 1);
								currentMoves.Pop();
							}
						}
					}
				}
			}
			(Cube, List<CubeMove>) FindBlockSolutions(Cube cube)
			{
				int bestVal = 0;
				Cube best = null;

				Stack<CubeMove> currentMoves = new Stack<CubeMove>();
				List<CubeMove> solution = new List<CubeMove>();

				//Avoid GC Pressure
				List<CubeMove>[] moveBuffer = new List<CubeMove>[20];
				for (int i = 0; i < moveBuffer.Length; i++)
					moveBuffer[i] = new List<CubeMove>(18);

				int depthStop = 0;
				for (int i = 1; i <= MAX_DEPTH; i++)
				{
					depthStop = i;
					Solve(cube, 0);
				}

				Console.WriteLine("Thread " + threadCounter++ + " Complete: " + bestVal.ToString("00 00 00"));
				return (best, solution);

				void Solve(Cube cube, int depth)
				{
					counter++;

					int value = cube.GetValue();
					if (value > bestVal)
					{
						bestVal = value;
						best = cube;

						solution.Clear();
						solution.AddRange(currentMoves);
					}

					if (depth < depthStop)
					{
						List<CubeMove> list = moveBuffer[depth];
						list.Clear();
						cube.AddPossibleMoves(list);

						for (int i = 0; i < list.Count; i++)
						{
							//Keep orientation of edges
							if ((int)list[i] / 6 == 2 && (int)list[i] % 3 != 1)
								continue;

							currentMoves.Push(list[i]);
							Solve(new Cube(cube, list[i]), depth + 1);
							currentMoves.Pop();
						}
					}
				}
			}
		}

		static void NissHelper()
		{
			while (true)
			{
				Console.Clear();
				Console.SetCursorPosition(0, 0);
				MoveSequenz s = new MoveSequenz(Console.ReadLine());
				Cube c = new Cube();
				c.ApplyMoveSequenz(s);
				c.PrintSideView();

				Console.WriteLine("\nReverse: ");
				c = new Cube();
				c.ApplyMoveSequenz(s.GetReverse());
				Console.WriteLine(s.GetReverse());
				c.PrintSideView();

				Console.ReadLine();
			}
		}
	}
}
