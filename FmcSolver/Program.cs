﻿using BenchmarkDotNet.Running;
using CubeAD;
using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace FmcSolver
{
	class Program
	{
		static void Main(string[] args)
		{


			//long sum = 0;
			//for (int i = 0; i < 18; i += 3)
			//{
			//	sum +=  CubeIndex.FindCycle((CubeMove)i);
			//	GC.Collect();
			//}

			//Console.WriteLine("Sum: " + sum);
			CubeIndex.Factorial(1);
			//CubeIndex.GenerateSolvedEdgeSet(7);

			MoveSequenz ms = new MoveSequenz(20);
			Console.WriteLine(ms);
			Cube c = new Cube();
			c.ApplyMoveSequenz(ms);
			c.PrintSideView();

			var list =  CubeIndex.FindSolution(new CubeIndex(c));


			Console.WriteLine(string.Join(" ", list));
			//CubeIndex.DoPermThings(4);

			//OrientEdges();

			Console.WriteLine("\nDone");
			Console.ReadLine();
		}

		private static string ToBase(int value, int newBase)
		{
			string s = "";
			do
			{
				s = value % newBase + s;
				value /= newBase;
			}
			while (value != 0);

			return s;
		}

		private static void DFSComparison()
		{
			Stopwatch sw = new Stopwatch();
			for (int i = 0; i < 9; i++)
			{
				Console.WriteLine("Max depth: " + i);

				sw.Restart();
				var indices = CubeIndex.SolvedTreeOrientedEdges(i);
				sw.Stop();
				Console.WriteLine("Index: " + indices.Count + " in " + sw.ElapsedMilliseconds + " ms");

				//sw.Restart();
				//HashSet<CubeIndex> set = Cube.GetSolvedCubeIndicesDFS(i);
				//sw.Stop();

				//Console.WriteLine("Cube:  " + set.Count + " in " + sw.ElapsedMilliseconds + " ms");
			}
		}

		private static void EdgeOrientationDistribution()
		{
			List<int> list = new List<int>();
			int counter = 0;
			Stopwatch sw = new Stopwatch();
			for (int i = 0; i <= 12; i += 2)
			{
				ushort value = 0;
				for (int j = 0; j < i; j++)
				{
					value |= (ushort)(1 << j);
				}

				Console.WriteLine(i);
				while (value < 4096)
				{
					counter++;
					////Console.WriteLine(Convert.ToString(value, 2));
					//CubeIndex ci = new CubeIndex(0, 0, value, 0);
					//ci.ApplyToCube(c);
					sw.Start();

					list.Add(CubeIndex.FindEdgeOrientationMoves(value).Count);
					//bins[OrientEdges(c)]++;


					sw.Stop();

					value = NextLexicographic(value);
				}
			}



			Console.WriteLine(string.Join("\n", list));

			Console.WriteLine("Sum: " + counter);
			Console.WriteLine(sw.ElapsedMilliseconds.ToString("000 000"));
			Console.WriteLine((double)sw.ElapsedMilliseconds / counter);
		}

		static ushort NextLexicographic(ushort x)
		{
			ushort t = (ushort)(x | (x - 1));
			return (ushort)((t + 1) | ((~t & -(~t)) - 1) >> (BSF(x) + 1));
		}
		static ushort BSF(ushort x)
		{
			return (ushort) System.Runtime.Intrinsics.X86.Bmi1.X64.TrailingZeroCount(x);
		}

		private static void SaveSetGeneration(int depth)
		{
			string path = Directory.GetCurrentDirectory() + "\\fullset.bin";

			Console.WriteLine("Generating buckets");
			BucketCubeIndices bucket = new BucketCubeIndices(109043123 / CubeIndex.MAX_CORNER_PERMUTATION);
			//HashSet<CubeIndex> bucket = new HashSet<CubeIndex>();

			Console.WriteLine(GC.GetTotalMemory(true).ToString("000 000 000 000"));

			Console.WriteLine("Finding cubes of depth: " + depth);
			Cube[] set = Cube.GetSolvedCubesDFS(depth).ToArray();

			Console.WriteLine(GC.GetTotalMemory(true).ToString("000 000 000 000"));

			Console.WriteLine("Set count: " + set.Length.ToString("0 000 000 000"));
			//CubeIndex.WriteCubeIndicesToFile(set, path);

			Console.WriteLine("Inserting into buckets");
			for (int i = 0; i < set.Length; i++)
				bucket.Add(new CubeIndex(set[i]));

			Console.WriteLine(GC.GetTotalMemory(true).ToString("000 000 000 000"));

			bucket.RemoveDuplicates();
			Console.WriteLine("Bucket count:  " + bucket.Count.ToString("0 000 000 000"));

			Console.WriteLine("Expanding");
			Cube bufferCube = new Cube();


			for (int i = 0; i < set.Length; i++)
			{
				Cube c = set[i];
				for (int j = 0; j < 18; j++)
				{
					bufferCube.CopyValuesFrom(c);

					bufferCube.MakeMove((CubeMove)j);

					bucket.Add(new CubeIndex(bufferCube));

				}
				set[i] = null;

				if(i % 100_000 == 0)
				{
					Console.WriteLine(i.ToString("000 000 000") + " / " + set.Length);
					bucket.RemoveDuplicates();
					GC.Collect();
				}
				//Console.WriteLine((CubeMove)i + " done");
			}
			Console.WriteLine(GC.GetTotalMemory(true).ToString("000 000 000 000"));

			bucket.RemoveDuplicates();
			Console.WriteLine("Bucket count:  " + bucket.Count.ToString("0 000 000 000"));

			bucket.SaveDistribution(Directory.GetCurrentDirectory() + "\\dist_" + depth + ".txt");
			Console.WriteLine("Saved file");

			bucket.SaveData(Directory.GetCurrentDirectory() + "\\data_" + depth + ".bin");
		}

		private static void SolvedSetBenchmarking()
		{
			Stopwatch sw = new Stopwatch();
			for (int i = 1; i < 12; i++)
			{
				sw.Restart();
				Console.WriteLine(i + " " + Cube.GetSolvedCubesBFSNaive(i).Count);
				Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000"));
			}
		}

		static void PrintValues()
		{
			MoveSequenz s = new MoveSequenz("B2 L' R F2 R B L2 D B U L2 D' F' U' L' F' L D' B' U");
			Cube c = new Cube();
			Console.WriteLine(c.GetValue().ToString("00 00 00"));

			c.ApplyMoveSequenz(s);

			MoveSequenz solution = new MoveSequenz("FP U2 DP BP DP R D R2 D RP D2 L2 UP R");


			foreach (CubeMove m in solution.Moves)
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
				foreach (Cube c in CloseList)
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

		static int OrientEdges(Cube c)
		{
			const int MAX_DEPTH = 7;
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
			int depthStop = 0;
			for (int i = 1; i <= MAX_DEPTH && !FoundSolution; i++)
			{
				depthStop = i;
				//Console.WriteLine(depthStop);
				FindBestCube(c, 0);
			}


			long end = Environment.TickCount64;
			return depthStop;

			void FindBestCube(Cube cube, int depth)
			{
				counter++;

				if (cube.CountOrientedEgdes() == 12)
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
			if (solution.Count > 5)
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
			for (int i = 1; i <= MAX_DEPTH; i++)
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
			Console.WriteLine(string.Join(" ", solution));
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
