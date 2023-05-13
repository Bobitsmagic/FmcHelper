using CubeAD;
using CubeAD.CubeIndexSets;
using CubeAD.CubeRepresentation;
using CubeAD.IndexCubeSets;
using Microsoft.CodeAnalysis;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.ExceptionServices;
using System.Security.Principal;

//Console.WriteLine("Order: " + Permutation.Order(new int[] { 1, 2, 0, 4, 3 }));



//PieceCube[] pc = new PieceCube[48];

//for (int i = 0; i < 48; i++)
//{
//	pc[i] = new PieceCube();
//}


PieceCube pc = new PieceCube();

Dictionary<int, int> set = new Dictionary<int, int>();

MoveBlocker mb = new MoveBlocker();
List<int> list = new List<int>();
Random rnd = new Random();
for(long i = 0; i < 1_000_000_00; i++)
{	
	list.Clear();
	for (int j = 0; j < 6; j++)
	{
		if (!mb[j])
			list.Add(j);
	}

	int index = rnd.Next(list.Count);
	int side = list[index];

	CubeMove move = (CubeMove)(side * 3 + rnd.Next(3));
	mb.UpdateBlocked(move);
	
	int hash = pc.GetSymHash();
	if (set.ContainsKey(hash))
	{
		set[hash]++;
	}
	else
	{
		set.Add(hash, 1);
	}
	
	pc.MakeMove(move);

	if (i % 10_000_000 == 0)
        Console.WriteLine(i);
}


Console.WriteLine(set.Count);

Console.WriteLine("Keys: ");
//Console.WriteLine(string.Join("\n", new SortedList<int, int>(set).Keys));
Console.WriteLine("Values: ");
Console.WriteLine(string.Join("\n", new SortedList<int, int>(set).Values));


//Console.WriteLine(ms);

//for (int i = 0; i < 48; i++)
//{
//	pc[i] = new PieceCube();

//	pc[i].ApplySequence(ms.TransForm(SymmetryElement.Elements[i]).Moves);

//	Console.WriteLine(pc[i].GetSymHash());
//}

Console.WriteLine("Done");
Console.ReadLine();
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
