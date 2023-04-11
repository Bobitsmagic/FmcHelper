using CubeAD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

ArrayCube cube = new ArrayCube();

MoveSequenz ms = new MoveSequenz("U2 R' B2 D2 U2 F2 L' U2 L2 R F' L' U' B U2 F D' F R B'");

cube.ApplySequence(ms.Moves);

Console.WriteLine(ms);
Console.WriteLine(string.Join(" ", Enumerable.Range(0, 12)));
Console.WriteLine(string.Join(" ", cube.GetEdgePerm()));
Console.WriteLine(string.Join(" ", Permutation.GetInverse(cube.GetEdgePerm())));
Console.WriteLine(cube.GetSideView());

cube.Reset();

ms = ms.GetReverse();

cube.ApplySequence(ms.Moves);
Console.WriteLine(ms);
Console.WriteLine(string.Join(" ", cube.GetEdgePerm()));
Console.WriteLine(cube.GetSideView());

cube = cube.GetInverse();
Console.WriteLine(string.Join(" ", cube.GetEdgePerm()));
Console.WriteLine(cube.GetSideView());


//1.5 sec, 20 sec
//0.35 sec, 5.3 sec, 272 sec, 36120 sec