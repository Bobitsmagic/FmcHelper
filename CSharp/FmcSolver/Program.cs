using CubeAD;
using CubeAD.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

for(int i = 0; i < 18; i++)
{
	PreCompTables.GenerateShortendNextEdgePermArrays((CubeMove)i);
}

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