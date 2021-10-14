namespace CubeAD
{
	public struct SymmetryGroup
	{
		public const int ORDER = 48;
		public static SymmetryElement[] Elements = new SymmetryElement[ORDER];
		public static SymmetryElement[] Generators = new SymmetryElement[33]
		{
			//Center pieces
			//X-Axis
			new SymmetryElement(CubeColor.Orange, CubeColor.Green, CubeColor.White),
			new SymmetryElement(CubeColor.Orange, CubeColor.White, CubeColor.Blue),

			//Y-Axis
			new SymmetryElement(CubeColor.Blue, CubeColor.Yellow, CubeColor.Orange),
			new SymmetryElement(CubeColor.Red, CubeColor.Yellow, CubeColor.Blue),
			
			//Z-Axis
			new SymmetryElement(CubeColor.Yellow, CubeColor.Red, CubeColor.Green),
			new SymmetryElement(CubeColor.Red, CubeColor.White, CubeColor.Green),

			//Edge pieces 
			//Orange-Yellow (0 2) LD
			new SymmetryElement(CubeColor.Yellow, CubeColor.Orange, CubeColor.Blue),
			//Orange-White (0 3) LU
			new SymmetryElement(CubeColor.White, CubeColor.Red, CubeColor.Blue),

			//Yellow-Green (2 4) DF
			new SymmetryElement(CubeColor.Red, CubeColor.Green, CubeColor.Yellow),
			//Yellow-Blue (2 5) DB
			new SymmetryElement(CubeColor.Red, CubeColor.Blue, CubeColor.White),

			//Green-Orange (4 0) FL
			new SymmetryElement(CubeColor.Green, CubeColor.White, CubeColor.Orange),
			//Green-Red (4 1) FR
			new SymmetryElement(CubeColor.Blue, CubeColor.White, CubeColor.Red),

			//Corner Pieces
			//Orange-Yellow-Green
			new SymmetryElement(CubeColor.Yellow, CubeColor.Green, CubeColor.Orange),
			//Orange-Yellow-Blue
			new SymmetryElement(CubeColor.Blue, CubeColor.Orange, CubeColor.White),
			//Orange-White-Green
			new SymmetryElement(CubeColor.Green, CubeColor.Red, CubeColor.White),
			//Orange-White-Blue
			new SymmetryElement(CubeColor.White, CubeColor.Green, CubeColor.Red),

			//Reflections with axis normal
			//X-Axis
			new SymmetryElement(CubeColor.Red, CubeColor.Yellow, CubeColor.Green),
			//Y-Axis
			new SymmetryElement(CubeColor.Orange, CubeColor.White, CubeColor.Green),
			//Z-Axis
			new SymmetryElement(CubeColor.Orange, CubeColor.Yellow, CubeColor.Blue),

			//Mirros through cube edges
			//Orange-Yellow (0 2) LD
			new SymmetryElement(CubeColor.Yellow, CubeColor.Orange, CubeColor.Green),
			//Orange-White (0 3) LU
			new SymmetryElement(CubeColor.White, CubeColor.Red, CubeColor.Green),

			//Yellow-Green (2 4) DF
			new SymmetryElement(CubeColor.Orange, CubeColor.Green, CubeColor.Yellow),
			//Yellow-Blue (2 5) DB
			new SymmetryElement(CubeColor.Orange, CubeColor.Blue, CubeColor.White),

			//Green-Orange (4 0) FL
			new SymmetryElement(CubeColor.Green, CubeColor.Yellow, CubeColor.Orange),
			//Green-Red (4 1) FR
			new SymmetryElement(CubeColor.Blue, CubeColor.Yellow, CubeColor.Red),

			//PunktSym Zentrum
			new SymmetryElement(CubeColor.Red, CubeColor.White, CubeColor.Blue),

			//Rotate like "Orange-Yellow-Green" and reflect like "X-Axis"
			new SymmetryElement(CubeColor.White, CubeColor.Green, CubeColor.Orange),

			//Rotate like "Orange-Yellow-Green" and reflect like "Y-Axis"
			new SymmetryElement(CubeColor.Yellow, CubeColor.Blue, CubeColor.Orange),

			//Rotate like "Orange-Yellow-Green" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Yellow, CubeColor.Green, CubeColor.Red),

			//Rotate like "Yellow-Green" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Red, CubeColor.Green, CubeColor.White),

			//Rotate around Z-Axis and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Yellow, CubeColor.Red, CubeColor.Blue),

			//Rotate like "Orange-White-Green" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.White, CubeColor.Blue, CubeColor.Red),

			//Rotate like "Green-Red (4 1) FR" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Blue, CubeColor.White, CubeColor.Orange),
		};

		static SymmetryGroup()
		{
			int index = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					for (int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;

						Elements[index++] = new SymmetryElement((CubeColor)x, (CubeColor)y, (CubeColor)z);
					}
				}
			}
		}

		public static CubeColor FlipColor(CubeColor c)
		{
			return (CubeColor)(((int)c) ^ 1);
		}

		//public static CubeColor TransformColor(Symmetry s, CubeColor c)
		//{
		//	return ColorMap[Log2((uint)s)].TransformColor(c);
		//}

		public static uint Log2(uint val)
		{
			val |= (val >> 1);
			val |= (val >> 2);
			val |= (val >> 4);
			val |= (val >> 8);
			val |= (val >> 16);

			val -= ((val >> 1) & 0x55555555);
			val = (val & 0x33333333) + ((val >> 2) & 0x33333333);
			return ((((val + (val >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24) - 1;
		}

		Symmetry Symmetries;


	}
}
