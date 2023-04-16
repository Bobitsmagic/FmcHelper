using CubeAD;
using CubeAD.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;

for(int i = 0; i < 18; i++)
{
    if (i % 3 == 0) continue;

    uint[] array = PreCompTables.GenerateShortendNextEdgePermArrays((CubeMove)i);
    byte[] bytes = new byte[array.Length * 4];
    Buffer.BlockCopy(array, 0, bytes, 0, bytes.Length);

    File.WriteAllBytes("next_edge_perm_" + (CubeMove)i + ".bin", bytes);
}

Console.ReadLine();

string ToBase(int val, int newBase, int minLength)
{
    string s = Convert.ToString(val, newBase);
    return new string('0', minLength - s.Length) + s; 
}
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