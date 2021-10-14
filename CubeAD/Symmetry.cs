using System;

namespace CubeAD
{
	[Flags]
	public enum Symmetry : uint
	{
		None = 0x0,

		//Axis through center pieces
		XAxis90 = 0x1, XAxis180 = 0x2,
		YAxis90 = 0x4, YAxis180 = 0x8,
		ZAxis90 = 0x10, ZAxis180 = 0x20,

		//Axis through edge pieces
		LDAxis = 0x40, LUAxis = 0x80,
		DFAxis = 0x100, DBAxis = 0x200,
		FLAxis = 0x400, FRAxis = 0x800,

		//Axis through corner pieces
		LDFAxis = 0x1000, LDBAxis = 0x2000,
		LUFAxis = 0x4000, LUBAxis = 0x8000,

		//Mirrors with normal X, Y, Z axis
		XReflect = 0x10000, YReflect = 0x20000, ZReflect = 0x40000,

		//Mirros through cube edges
		DFReflect = 0x80000, UFReflect = 0x1000000,
		LFReflect = 0x200000, LBReflect = 0x400000,
		LDReflect = 0x800000, LUReflect = 0x1000000
	}
}
