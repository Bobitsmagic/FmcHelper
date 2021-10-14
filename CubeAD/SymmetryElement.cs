using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CubeAD
{
	public struct SymmetryElement : IComparable<SymmetryElement>
	{
		public CubeColor this[int i]
		{
			get
			{
				return i switch
				{
					0 => Orange,
					1 => Yellow,
					2 => Green,
					_ => CubeColor.None,
				};
			}
			set
			{
				switch (i)
				{
					case 0: Orange = value; break;
					case 1: Yellow = value; break;
					case 2: Green = value; break;
				}
			}
		}

		public bool IsIdentity => Orange == CubeColor.Orange && Yellow == CubeColor.Yellow && Green == CubeColor.Green;

		public int Index { get
			{
				return (int)Orange * 8 +
					((int)Orange < 2
						? ((int)Yellow - 2)
						: ((int)Orange < 4
							? ((int)Yellow >> 1) | ((int)Yellow & 1)
							: (int)Yellow)) * 2 +
					((int)Green & 1);
			} }

		public CubeColor Orange, Yellow, Green;


		public SymmetryElement(CubeColor orange, CubeColor yellow, CubeColor green)
		{
			Orange = orange;
			Yellow = yellow;
			Green = green;
		}



		public CubeColor TransformColor(CubeColor c)
		{
			if ((((int)c) & 1) == 0)
				return this[(int)c >> 1];
			else
				//flip color twice
				return (CubeColor)((int)this[(((int)c) ^ 1) >> 1] ^ 1);
		}

		public List<SymmetryElement> GenerateMultGroup()
		{
			SymmetryElement state = this;

			List<SymmetryElement> subGroup = new List<SymmetryElement>();
			subGroup.Add(state);
			while (!state.IsIdentity)
			{
				state *= this;
				subGroup.Add(state);
			}

			subGroup.Sort();

			return subGroup;
		}
		public void GenerateMultGroup(List<SymmetryElement> subGroup)
		{
			SymmetryElement state = this;

			subGroup.Clear();
			subGroup.Add(state);
			while (!state.IsIdentity)
			{
				state *= this;
				subGroup.Add(state);
			}

			subGroup.Sort();
		}

		public static SymmetryElement operator *(SymmetryElement a, SymmetryElement b)
		{
			return new SymmetryElement(b.TransformColor(a.Orange), b.TransformColor(a.Yellow), b.TransformColor(a.Green));
		}

		public override bool Equals(object obj)
		{
			return obj is SymmetryElement element &&
				   Orange == element.Orange &&
				   Yellow == element.Yellow &&
				   Green == element.Green;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Orange, Yellow, Green);
		}

		public int CompareTo([AllowNull] SymmetryElement other)
		{
			if (Orange != other.Orange)
				return Orange.CompareTo(other.Orange);

			if (Yellow != other.Yellow)
				return Yellow.CompareTo(other.Yellow);

			return Green.CompareTo(other.Green);
		}

		public override string ToString()
		{
			return Orange + " " + Yellow + " " + Green;
		}
	}
}
