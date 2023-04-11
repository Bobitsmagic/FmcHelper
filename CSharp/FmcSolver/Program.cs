using CubeAD;
using System;
using System.Collections.Generic;
using System.Diagnostics;

ArrayCube cube = new ArrayCube();

//MoveSequenz ms = new MoveSequenz();
//ms.ReRoll(20);

//foreach(var move in ms.Moves)
//	cube.MakeMove(move);

//Console.WriteLine(ms);

Console.WriteLine(cube.GetSideView());
cube = cube.FindLowestSymmetry();

HashSet<ArrayCube> visited = new HashSet<ArrayCube>() { cube };

List<ArrayCube> openlist = new List<ArrayCube>() { cube };
List<ArrayCube> nextList = new List<ArrayCube>();
Stopwatch sw = Stopwatch.StartNew();	
for (int i = 0; i < 8; i++)
{
	Console.WriteLine("OpenListCount: " + openlist.Count);
	nextList.Clear();
	int counter = 0;
	foreach (var c in openlist)
	{
		if(++counter % 10000 == 0)
            Console.WriteLine(counter.ToString("000 000 000"));

		ArrayCube buffer = new ArrayCube(c);
        for (int m = 0; m < 18; m++)
		{
			buffer.MakeMove((CubeMove)m);

			ArrayCube lowSym = buffer.FindLowestSymmetry();
			if (visited.Add(lowSym))
			{
				nextList.Add(lowSym);
			}
			
			buffer.MakeMove(MoveSequenz.ReverseMove((CubeMove)m));
		}
	}

    Console.WriteLine(sw.ElapsedMilliseconds + " ms");
	sw.Restart();
    (nextList, openlist) = (openlist, nextList);
}
Console.WriteLine("Last list count: " + openlist.Count);

Console.WriteLine("\nDone");
Console.ReadLine();

//1.5 sec, 20 sec
//0.35 sec, 5.3 sec, 272 sec, 36120 sec