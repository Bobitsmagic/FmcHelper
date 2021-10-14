using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
			set{
				switch (i)
				{
					case 0: Orange = value; break;
					case 1: Yellow = value; break;
					case 2: Green = value; break;
				}
			}
		}

		public bool IsIdentity => Orange == CubeColor.Orange && Yellow == CubeColor.Yellow && Green == CubeColor.Green;

		public CubeColor Orange, Yellow, Green;


		public SymmetryElement(CubeColor orange, CubeColor yellow, CubeColor green)
		{
			Orange = orange;
			Yellow = yellow;
			Green = green;
		}

		public static SymmetryElement[] AllSymmetryElements;
		static SymmetryElement()
		{
			AllSymmetryElements = new SymmetryElement[48];

			int index = 0;
			for(int x = 0; x < 6; x++)
			{
				for(int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					for(int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;

						AllSymmetryElements[index++] = new SymmetryElement((CubeColor)x, (CubeColor)y, (CubeColor)z);
					}
				}
			}
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
		140}
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
