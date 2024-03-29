﻿namespace CubeAD
{
	public enum CubeColor : byte
	{
		Orange, Red,
		Yellow, White,
		Green, Blue,
		None
	}

	/// <summary>
	/// All moves in half-turn metric
	/// </summary>
	public enum CubeMove : byte
	{
		L, L2, LP,
		R, R2, RP,
		D, D2, DP,
		U, U2, UP,
		F, F2, FP,
		B, B2, BP,
		None
	}
}
