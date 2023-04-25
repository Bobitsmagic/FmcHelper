using CubeAD.CubeIndexSets;
using CubeAD.Pieces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace CubeAD.CubeRepresentation
{
	struct CornerState
	{
		public const int MAX_STATE = 8 * 3;

		public int Position => State / 3;
		public int Orientation => State % 3;

		public byte State;

		public CornerState(byte state)
		{
			State = state;
		}
		public CornerState(int position, int orientation)
		{
			State = (byte)(position * 3 + orientation);
		}

		public static bool operator ==(CornerState left, CornerState right)
		{
			return left.State == right.State;
		}
		public static bool operator !=(CornerState left, CornerState right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			return Position + " " + Orientation;
		}
	}
	struct EdgeState
	{
		public const int MAX_STATE = 12 * 2;

		public int Position => State / 2;
		public int Orientation => State % 2;

		public byte State;

		public EdgeState(byte state)
		{
			State = state;
		}
		public EdgeState(int position, int orientation)
		{
			State = (byte)(position * 2 + orientation);
		}
	}

	public class PieceCube : IComparable<PieceCube>
	{
		static CornerState[][] MoveCornerPiece = new CornerState[18][];
		static EdgeState[][] MoveEdgePiece = new EdgeState[18][];
		static CornerState[][,] SymmetryCornerPiece = new CornerState[48][,]; //(sym) x (corner, state) -> new state
		static EdgeState[][,] SymmetryEdgePiece = new EdgeState[48][,];       //(sym) x (edge, state) -> new state
		static int[][] SymmetryCornerPerm = new int[48][];
		static int[][] SymmetryEdgePerm = new int[48][];

		public static HashSet<PieceCube> GetUniqueSymCubes(int maxDepth)
		{
			HashSet<PieceCube> set = new HashSet<PieceCube>() { new PieceCube() };
			List<PieceCube> currentList = new List<PieceCube>();
			List<PieceCube> next = new List<PieceCube>();

			PieceCube lowSym = new PieceCube();

			currentList.Add(new PieceCube());
			for (int d = 0; d < maxDepth; d++)
			{
				next.Clear();
				foreach (var cube in currentList)
				{
					lowSym.CopyValuesFrom(cube);
					for (int i = 0; i < 18; i++)
					{
						lowSym.MakeMove((CubeMove)i);

						PieceCube trans = lowSym.FindLowestSymmetry();

						if (set.Add(trans))
						{
							next.Add(trans);
						}

						lowSym.MakeMove(MoveSequenz.ReverseMove((CubeMove)i));
					}
				}


				(currentList, next) = (next, currentList);
			}
			Console.WriteLine("Ram usage: " + GC.GetTotalMemory(true).ToString("000 000 000 000"));

			return set;
		}

		public static SortedBucketsSet GetUniqueSymCubesFast(int maxDepth)
		{
			SortedBucketsSet set = new SortedBucketsSet();
			set.Add(new IndexCube());

			//int[] edgePerm = new int[12];
			//int[] edgeOrient= new int[12];
			//int[] cornerPerm = new int[8];
			//int[] cornerOrient = new int[8];

			for (int d = 0; d < maxDepth; d++)
			{
				set.Foreach(x =>
				{
					PieceCube pc = new PieceCube(x.GetEdgePermutation(), x.GetEdgeOrientation(), x.GetCornerPermuation(), x.GetCornerOrientation());
					for(int  i = 0; i < 18; i++)
					{
						CubeMove cm = (CubeMove)i;

						pc.MakeMove(cm);

						PieceCube lowSym = pc.FindLowestSymmetry();

						set.Add(new IndexCube((uint)lowSym.GetEdgePermIndex(), (ushort)lowSym.GetCornerPermIndex(), (ushort)lowSym.GetEdgeOrientationIndex(), (ushort)lowSym.GetCornerOrientationIndex()));

						pc.MakeMove(MoveSequenz.ReverseMove(cm));	
					}
				});


				set.RemoveDuplicates();
			}

            Console.WriteLine("Ram usage: " + GC.GetTotalMemory(true).ToString("000 000 000 000"));

			

			return set;
		}

		static PieceCube()
		{
			int[] FrontEdges = new int[] { 3, 7, 9, 11 };
			int[] BackEdges = new int[] { 2, 6, 8, 10 };

			int[][] SideCorners = new int[][]
			{
				new int[]{0, 1, 2, 3},
				new int[]{4, 5, 6, 7},
				new int[]{0, 1 ,4, 5},
				new int[]{2, 3, 6, 7},
				new int[]{0, 2, 4, 6},
				new int[]{1, 3, 5, 7},
			};

			for (int move = 0; move < 18; move++)
			{
				TileCube ac = TileCube.GetSolved();
				CubeMove m = (CubeMove)move;
				ac.MakeMove((CubeMove)move);
				int[] cornerPerm = ac.GetCornerPerm();
				int[] edgePerm = ac.GetEdgePerm();


				if (m == CubeMove.D2)
				{
					Console.WriteLine("kek");
				}

				MoveCornerPiece[move] = new CornerState[CornerState.MAX_STATE];
				for (int s = 0; s < CornerState.MAX_STATE; s++)
				{
					CornerState corner = new CornerState((byte)s);
					int nextOrient = corner.Orientation;
					if (SideCorners[move / 3].Contains(corner.Position) && move % 3 != 1)
						 nextOrient = (byte)((2 * corner.Orientation - (move / 6) + 3) % 3);
					int nextPos = cornerPerm[corner.Position];

					MoveCornerPiece[move][s] = new CornerState(nextPos, nextOrient);
				}

				MoveEdgePiece[move] = new EdgeState[EdgeState.MAX_STATE];
				for (int s = 0; s < EdgeState.MAX_STATE; s++)
				{
					EdgeState edge = new EdgeState((byte)s);
					int nextOrient = edge.Orientation;

					//Flip edges on F, FP, B, BP
					if (m == CubeMove.F || m == CubeMove.FP)
					{
						if (FrontEdges.Contains(edge.Position))
						{
							nextOrient = (byte)(1 - nextOrient);
						}
					}

					if (m == CubeMove.B || m == CubeMove.BP)
					{
						if (BackEdges.Contains(edge.Position))
						{
							nextOrient = (byte)(1 - nextOrient);
						}
					}

					int nextPos = edgePerm[edge.Position];

					MoveEdgePiece[move][s] = new EdgeState(nextPos, nextOrient);
				}
			}

			for (int sym = 0; sym < 48; sym++)
			{
				SymmetryElement se = SymmetryElement.Elements[sym];

				SymmetryCornerPerm[sym] = new int[8];
				for (int i = 0; i < 8; i++)
				{
					SymmetryCornerPerm[sym][i] = SortedCorner.Corners[i].Transform(se).GetIndex;
				}

				SymmetryEdgePerm[sym] = new int[12];
				for (int i = 0; i < 12; i++)
				{
					SymmetryEdgePerm[sym][i] = SortedEdge.Edges[i].Transform(se).GetIndex;
				}

				SymmetryCornerPiece[sym] = new CornerState[8, CornerState.MAX_STATE];
				for (int cornerIndex = 0; cornerIndex < 8; cornerIndex++)
				{
					for (int s = 0; s < CornerState.MAX_STATE; s++)
					{
						CornerState corner = new CornerState((byte)s);
						int nextOrient = TileCube.CornerOrientAfterTransformation(cornerIndex, corner.Position, corner.Orientation, se);

						int nextPos = SymmetryCornerPerm[sym][corner.Position];

						SymmetryCornerPiece[sym][cornerIndex, s] = new CornerState(nextPos, nextOrient);
					}
				}

				SymmetryEdgePiece[sym] = new EdgeState[12, EdgeState.MAX_STATE];
				for (int edgeIndex = 0; edgeIndex < 12; edgeIndex++)
				{
					for (int s = 0; s < EdgeState.MAX_STATE; s++)
					{
						EdgeState edge = new EdgeState((byte)s);
						int nextOrient = TileCube.EdgeOrientAfterTransformation(edgeIndex, edge.Position, edge.Orientation, se);

						int nextPos = SymmetryEdgePerm[sym][edge.Position];

						SymmetryEdgePiece[sym][edgeIndex, s] = new EdgeState(nextPos, nextOrient);
					}
				}
			}

			for (int sym = 0; sym < 1; sym++)
			{
				SymmetryElement se = SymmetryElement.Elements[sym];

				
				
				for(int cornerIndex = 0; cornerIndex < 8; cornerIndex++)
				{
					for (int s = 0; s < CornerState.MAX_STATE; s++)
					{
						CornerState cs = new CornerState((byte)s);

						if (SymmetryCornerPiece[sym][cornerIndex, cs.State] != cs)
						{
							Console.WriteLine("kekkek");
						}
					}

				}
			}
		}

		CornerState[] Corners = new CornerState[8];
		EdgeState[] Edges = new EdgeState[12];

		public PieceCube()
		{
			for (int i = 0; i < 8; i++)
			{
				Corners[i] = new CornerState(i, 0);
			}
			for (int i = 0; i < 12; i++)
			{
				Edges[i] = new EdgeState(i, 0);
			}
		}

		public PieceCube(IndexCube src) : this(src.GetEdgePermutation(), src.GetEdgeOrientation(), src.GetCornerPermuation(), src.GetCornerOrientation())
		{
			
		}
		public PieceCube(int[] edgePerm, int[] edgeOrientations, int[] cornerPerm, int[] cornerOrientations)
		{
			for (int i = 0; i < 8; i++)
			{
				Corners[i] = new CornerState(cornerPerm[i], cornerOrientations[cornerPerm[i]]);
			}
			for (int i = 0; i < 12; i++)
			{
				Edges[i] = new EdgeState(edgePerm[i], edgeOrientations[edgePerm[i]]);
			}
		}
		public PieceCube(PieceCube src)
		{
			Array.Copy(src.Corners, Corners, 8);
			Array.Copy(src.Edges, Edges, 12);
		}
		public void CopyValuesFrom(PieceCube src)
		{
			Array.Copy(src.Corners, Corners, 8);
			Array.Copy(src.Edges, Edges, 12);
		}

		public void MakeMove(CubeMove cm)
		{
			var nextCornerArray = MoveCornerPiece[(int)cm];
			var nextEdgeArray = MoveEdgePiece[(int)cm];

			for (int i = 0; i < 8; i++)
				Corners[i] = nextCornerArray[Corners[i].State];

			for (int i = 0; i < 12; i++)
				Edges[i] = nextEdgeArray[Edges[i].State];
		}
		public void ApplySequence(List<CubeMove> moves)
		{
			foreach(CubeMove m in moves)
			{
				MakeMove(m);
			}
		}


		public void Transform(SymmetryElement se, PieceCube res)
		{
			var cPerm = SymmetryCornerPerm[se.Index];
			var cTrans = SymmetryCornerPiece[se.Index];

			for (int i = 0; i < 8; i++)
			{
				res.Corners[cPerm[i]] = cTrans[i, Corners[i].State];
			}

			var ePerm = SymmetryEdgePerm[se.Index];
			var eTrans = SymmetryEdgePiece[se.Index];

			for (int i = 0; i < 12; i++)
			{
				res.Edges[ePerm[i]] = eTrans[i, Edges[i].State];
			}
		}

		public IndexCube GetIndexCube()
		{
			return new IndexCube(GetEdgePermIndex(), GetCornerPermIndex(), GetEdgeOrientationIndex(), GetCornerOrientationIndex());
		}

		public PieceCube FindLowestSymmetry()
		{
			PieceCube best = new PieceCube(this);
			PieceCube buffer = new PieceCube();

			foreach (var se in SymmetryElement.Elements)
			{
				Transform(se, buffer);

				if (buffer.CompareTo(best) < 0)
				{
					best = new PieceCube(buffer);
				}
			}

			return best;
		}

		public int[] GetEdgePerm()
		{
			return Edges.Select(x => x.Position).ToArray();
		}
		public int GetEdgePermIndex()
		{
			Span<int> perm = stackalloc int[12];
			for (int i = 0; i < 12; i++)
				perm[i] = Edges[i].Position;

			return Permutation.GetIndex(perm);
		}

		public int[] GetCornerPerm()
		{
			return Corners.Select(x => x.Position).ToArray();
		}
		public int GetCornerPermIndex()
		{
			Span<int> perm = stackalloc int[8];
			for (int i = 0; i < 8; i++)
				perm[i] = Corners[i].Position;

			return Permutation.GetIndex(perm);
		}

		public int[] GetEdgeOrient()
		{
			int[] ret = new int[12];

			for(int i = 0; i < 12; i++)
			{
				ret[Edges[i].Position] = Edges[i].Orientation;
			}

			return ret;
		}
		public int GetEdgeOrientationIndex()
		{
			int ret = 0;
			Span<int> vals = stackalloc int[12];
			for(int i = 0; i < 12; i++)
			{
				vals[Edges[i].Position] = Edges[i].Orientation;
			}

			for (int i = 0; i < 12; i++)
			{
				ret = (ret << 1) | vals[i];
			}

			return ret;
		}

		public int[] GetCornerOrient()
		{
			int[] ret = new int[8];

			for (int i = 0; i < 8; i++)
			{
				ret[Corners[i].Position] = Corners[i].Orientation;
			}

			return ret;
		}
		public int GetCornerOrientationIndex()
		{
			int ret = 0;
			Span<int> vals = stackalloc int[8];
			for (int i = 0; i < 8; i++)
			{
				vals[Corners[i].Position] = Corners[i].Orientation;
			}
			for (int i = 0; i < 8; i++)
			{
				ret = ret * 3 + vals[i];
			}

			return (ushort)ret;
		}

		public static bool operator ==(PieceCube left, PieceCube right)
		{
			return left.Corners.AsSpan().SequenceEqual(right.Corners) &&
				left.Edges.AsSpan().SequenceEqual(right.Edges);
		}
		public static bool operator !=(PieceCube left, PieceCube right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return (PieceCube)obj == this;
		}

		public override int GetHashCode()
		{
			int hash = 0;
			unsafe
			{
				fixed (void* ptr = Edges)
				{
					for (int i = 0; i < 3; i++)
					{
						hash = HashCode.Combine(hash, ((int*)ptr)[i]);
					}
				}
				fixed (void* ptr = Corners)
				{
					for (int i = 0; i < 2; i++)
					{
						hash = HashCode.Combine(hash, ((int*)ptr)[i]);
					}
				}
			}

			return hash;
		}

		public int CompareTo(PieceCube other)
		{
			for (int i = 0; i < 12; i++)
			{
				if (Edges[i].State != other.Edges[i].State)
				{
					return Edges[i].State.CompareTo(other.Edges[i].State);
				}
			}

			for (int i = 0; i < 8; i++)
			{
				if (Corners[i].State != other.Corners[i].State)
				{
					return Corners[i].State.CompareTo(other.Corners[i].State);
				}
			}

			//Console.WriteLine("This should never happen");
			return 0;
		}
	}
}
