using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeAD
{
	public class MoveSequenz
	{
		public static Random Rnd = new Random();

		public int Length { get { return Moves.Count; } }
		public readonly List<CubeMove> Moves;

		public MoveSequenz(List<CubeMove> moves)
		{
			Moves = moves;
		}

		public MoveSequenz(string s)
		{
			Moves = s.Split(' ').Select(x => FromString(x)).ToList();

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

			CubeMove ReverseMove(CubeMove m)
			{
				int val = (int)m;
				return (CubeMove)(val +
					((val % 3) == 0 ? 2 :
					(val % 3) == 1 ? 0 : -2));
			}
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
