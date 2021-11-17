using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeAD
{
	public class MoveSequenz
	{
		public static Random Rnd = new Random();

		public static readonly MoveSequenz SuperFlip = new MoveSequenz("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2");
		public static readonly MoveSequenz CheckerBoard = new MoveSequenz("L2 R2 D2 U2 F2 B2");
		public static readonly MoveSequenz APerm = new MoveSequenz("R B' R F2 R' B R F2 R2");
		public static readonly MoveSequenz GPerm = new MoveSequenz("R U R' U' D R2 U' R U' R' U R' U R2 D'");
		public static readonly MoveSequenz UPerm = new MoveSequenz("R' U R' U' R' U' R' U R U R2");
		public static readonly MoveSequenz TPerm = new MoveSequenz("R U R' U' R' F R2 U' R' U' R U R' F'");
		public int Length { get { return Moves.Count; } }
		public readonly List<CubeMove> Moves;

		public MoveSequenz(IEnumerable<CubeMove> moves)
		{
			Moves = moves.ToList();
		}
		public MoveSequenz(List<CubeMove> moves)
		{
			Moves = moves;
		}
		public MoveSequenz(params CubeMove[] cm)
		{
			Moves = cm.ToList();
		}

		public MoveSequenz(string s)
		{
			Moves = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => FromString(x)).ToList();

			CubeMove FromString(string s)
			{
				int sum = (int)(CubeMove)Enum.Parse(typeof(CubeMove), s[0].ToString(), true);

				if (s.Length == 2)
					sum += s[1] == '2' ? 1 : 2;

				return (CubeMove)sum;
			}
		}

		public MoveSequenz(int length)
		{
			Moves = new List<CubeMove>(length);

			bool[] blocked = new bool[6];

			List<int> list = new List<int>(6);
			for (int i = 0; i < length; i++)
			{
				list.Clear();

				for (int j = 0; j < 6; j++)
				{
					if (!blocked[j])
						list.Add(j);
				}

				int index = Rnd.Next(list.Count);

				int side = list[index];
				blocked[side] = true;

				for (int j = 0; j < 6; j++)
				{
					if (j / 2 != side / 2)
					{
						blocked[j] = false;
					}
				}

				Moves.Add((CubeMove)(side * 3 + Rnd.Next(3)));
			}
		}

		public MoveSequenz GetReverse()
		{
			return new MoveSequenz(Moves.Select(x => ReverseMove(x)).Reverse().ToList());
		}

		public void Reduce()
		{
			bool reduced = false;

			do
			{
				for(int i = 0; i < Moves.Count - 1; i++)
				{
					int v1 = (int)Moves[i];
					int v2 = (int)Moves[i + 1];

					if (v1 / 3 == v2 / 3)
					{
						if((v1 % 3) + (v2 % 3) == 4)
						{
							Moves.RemoveRange(i, 2);
							break;
						}
						else
						{
							Moves.RemoveAt(i);
							Moves[i] = (CubeMove)((v1 % 3) + (v2 % 3) + (v1 / 3));
						}
					}
				}
			} while (reduced);
		}

		public MoveSequenz Rotate(SymmetryElement se)
		{
			List<CubeMove> ret = new List<CubeMove>();

			foreach (CubeMove m in Moves)
			{
				int side = ((int)m) / 3;
				int nextSide = se.TransformColor(side);
				CubeMove nextMove = (CubeMove)(((int)m) - side + nextSide);

				if (se.HasReflection)
					ret.Add(ReverseMove(nextMove));
				else
					ret.Add(nextMove);
			}

			return new MoveSequenz(ret);
		}

		public static CubeMove ReverseMove(CubeMove m)
		{
			int val = (int)m;
			return (CubeMove)(val - val % 3 + (2 * val + 2) % 3); 
		}
		public void ReRoll()
		{
			Moves.Clear();
			bool[] blocked = new bool[6];

			List<int> list = new List<int>(6);
			for (int i = 0; i < Moves.Capacity; i++)
			{
				list.Clear();

				for (int j = 0; j < 6; j++)
				{
					if (!blocked[j])
						list.Add(j);
				}

				int index = Rnd.Next(list.Count);

				int side = list[index];
				blocked[side] = true;

				for (int j = 0; j < 6; j++)
				{
					if (j / 2 != side / 2)
					{
						blocked[j] = false;
					}
				}

				Moves.Add((CubeMove)(side * 3 + Rnd.Next(3)));
			}
		}
		public override string ToString()
		{
			return string.Join(" ", Moves.Select(x => x.ToString().Replace('P', '\'')));
		}
	}
}
