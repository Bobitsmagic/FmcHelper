﻿using NUnit.Framework;
using CubeAD;


namespace CubeTester
{
	class PatternTester
	{
		[Test]
		public void CheckSquares()
		{
			Cube cube = new Cube();

			Assert.AreEqual(cube.CountSquares(), 24);

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(cube.CountSquares(), 16);

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(cube.CountSquares(), 8);
		}

		[Test]
		public void CheckBlocks()
		{
			Cube cube = new Cube();

			Assert.AreEqual(cube.CountBlocks(), 8);

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(cube.CountBlocks(), 4);

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(cube.CountBlocks(), 0);
		}

		[Test]
		public void Edges()
		{
			Cube cube = new Cube();

			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.R);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.L);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.D);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.U);
			Assert.AreEqual(cube.CountOrientedEgdes(), 12);

			cube.MakeMove(CubeMove.F);
			Assert.AreEqual(cube.CountOrientedEgdes(), 8);

			cube.MakeMove(CubeMove.B);
			Assert.AreEqual(cube.CountOrientedEgdes(), 4);
			
			cube.MakeMove(CubeMove.U);
			cube.MakeMove(CubeMove.F);
			Assert.AreEqual(cube.CountOrientedEgdes(), 6);

			cube.MakeMove(CubeMove.B);
			Assert.AreEqual(cube.CountOrientedEgdes(), 8);
		}
	}
}