using CubeAD;
using System;
using System.Collections.Generic;
using System.Diagnostics;


ArrayCube cube = new ArrayCube();
HashSet<ArrayCube> visited = new HashSet<ArrayCube>() { cube };
HashSet<ArrayCube> inverSet = new HashSet<ArrayCube>() { cube };

List<ArrayCube> openlist = new List<ArrayCube>() { cube };
List<ArrayCube> nextList = new List<ArrayCube>();
Stopwatch sw = Stopwatch.StartNew();
for (int i = 0; i < 8; i++)
{
	Console.WriteLine("OpenListCount: " + openlist.Count);

	inverSet.Clear();
	foreach(var c in openlist)
	{
		ArrayCube buffer = c.FindLowestSymmetryInverse();
		inverSet.Add(buffer);
	}

    Console.WriteLine("Invers count: " + inverSet.Count);

    nextList.Clear();
	int counter = 0;
	foreach (var c in openlist)
	{
		if (++counter % 10000 == 0)
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

//Console.WriteLine(string.Join("kkekekekkkkkkkkkkkkkkkkkkkkkkkkkkkk\n", openlist.Select(x => x.GetSideView())));

//1
//2
//9
//75
//934
//12 077
//159 131
//2 101 575
//27 762 103