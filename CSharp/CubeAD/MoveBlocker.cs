using System.Collections.Generic;

namespace CubeAD
{
	public struct MoveBlocker
	{
		public bool this[int i] => ((Blocked >> i) & 1) != 0;

		private byte Blocked;

		public MoveBlocker(MoveBlocker old, CubeMove m)
		{
			Blocked = old.Blocked;
			UpdateBlocked((int)m / 3);
		}
		public MoveBlocker(MoveBlocker old, int side, bool soft = false)
		{
			Blocked = old.Blocked;

			if (soft)
				UpdateBlockedSoft(side);
			else 
				UpdateBlocked(side);
		}



		public void ResetBlocked() => Blocked = 0;
		public void UpdateBlocked(int side)
		{
			Blocked |= (byte)(1 << side);

			if (side % 2 == 0)
				Blocked |= (byte)(1 << (side + 1));

			for (int i = 0; i < 6; i++)
			{
				if (i / 2 != side / 2)
				{
					Blocked &= (byte)(~(1 << i));
				}
			}
		}

		//Ignores RL == LR
		public void UpdateBlockedSoft(int side)
		{
			Blocked |= (byte)(1 << side);

			for (int i = 0; i < 6; i++)
			{
				if (i / 2 != side / 2)
				{
					Blocked &= (byte)(~(1 << i));
				}
			}
		}

		public void AddPossibleMoves(List<CubeMove> list)
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
	}
}
