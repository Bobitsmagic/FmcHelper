using System.Collections.Generic;

namespace CubeAD
{
	/// <summary>
	/// A struct that helps reducing redundancy on longer move sequenzes,
	/// blocked moves: L-L2 = LP, R-L-R = R2-L, RL = LR 
	/// </summary>
	public struct MoveBlocker
	{
		public bool this[int i] => ((Blocked >> i) & 1) != 0;
		public bool this[CubeMove m] => this[(int)m / 3];

		//Contains information on which side is blocked
		private byte Blocked;

		/// <summary>
		/// Creates copy of a <see cref="MoveBlocker"/> and updates it with <paramref name="m"/> afterwards
		/// </summary>
		public MoveBlocker(MoveBlocker old, CubeMove m, bool soft = false)
		{
			Blocked = old.Blocked;

			if (soft)
				UpdateBlockedSoft(m);
			else 
				UpdateBlocked(m);
		}
		
		public void ResetBlocked() => Blocked = 0;

		public void UpdateBlocked(CubeMove m)
		{
			int side = (int)m / 3;
			Blocked |= (byte)(1 << side);

			if (side % 2 == 1)
				Blocked |= (byte)(1 << (side - 1));

			for (int i = 0; i < 6; i++)
			{
				if (i / 2 != side / 2)
				{
					Blocked &= (byte)(~(1 << i));
				}
			}
		}

		//Ignores RL = LR
		public void UpdateBlockedSoft(CubeMove m)
		{
			int side = (int)m / 3;
			Blocked |= (byte)(1 << side);

			for (int i = 0; i < 6; i++)
			{
				if (i / 2 != side / 2)
				{
					Blocked &= (byte)(~(1 << i));
				}
			}
		}

		public void InsertPossibleMoves(List<CubeMove> list)
		{
			list.Clear();
			for (int i = 0; i < 6; i++)
			{
				if (((Blocked >> i) & 1) == 0)
				{
					for (int j = 0; j < 3; j++)
					{
						list.Add((CubeMove)(i * 3 + j));
					}
				}
			}
		}

		public override string ToString()
		{
			string s = "";
			for(int i = 0; i < 18; i+= 3)
			{
				CubeMove m = (CubeMove)i;
				s += m + ":" + (this[m] ? 1 : 0) + ", ";
			}

			return s;
		}
	}
}
