using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD
{
    public class ArrayCube
    {
        const int TILE_COUNT = 6 * 9;
        CubeColor[] Data = new CubeColor[TILE_COUNT];
        CubeColor[] ArrayBuffer = new CubeColor[TILE_COUNT];

        static int GetTileIndex(int side, int x, int y)
        {
            return side * 9 + x + 3 * y;
        }
        static int[][] MoveArray = new int[18][];

        static ArrayCube()
        {                               //6, 12
            int[,] affectedTiles = new int[,]
            {
                //Orange
                {33, 30, 27, 51, 48, 45, 24, 21, 18, 38, 41, 44},
                //Red
                {20, 23, 26, 47, 50, 53, 29, 32, 35, 42, 39, 36},
                //Yellow
                {0, 1, 2, 45, 46, 47, 9, 10, 11, 36, 37, 38},
                //White
                {44, 43, 42, 17, 16, 15, 53, 52, 51, 8, 7, 6 },
                //Blue
                {11, 14, 17, 35, 34, 33, 6, 3, 0, 18, 19, 20 },
                //Green
                {2, 5, 8, 27, 28, 29, 15, 12, 9, 26, 25, 24},
            };
            int[][] RotationArray = new int[3][]
            {
                new int[9]{6, 3, 0, 7, 4, 1, 8, 5, 2},
                new int[9]{2, 5, 8, 1, 4, 7, 0, 3, 6},
                new int[9]{8, 7, 6, 5, 4, 3, 2, 1, 0},
            };

            for (int side = 0; side < 6; side++)
            {
                int[] template = Enumerable.Range(0, TILE_COUNT).ToArray();

                for(int i = 0; i < 9; i++)
                {
                    template[side * 9 + i] = RotationArray[0][i] + side * 9;
                }

                for(int i = 0; i < 12; i++)
                {
                    template[affectedTiles[side, i]] = affectedTiles[side, (i + 3) % 12];
                }

                int[] buffer = template.ToArray();

                MoveArray[side * 3 + 0] = template.ToArray();

                for (int i = 0; i < template.Length; i++)
                    template[i] = buffer[buffer[i]];
                MoveArray[side * 3 + 1] = template.ToArray();

                for (int i = 0; i < template.Length; i++)
                    template[i] = buffer[buffer[buffer[i]]];
                MoveArray[side * 3 + 2] = template.ToArray();
            }
        }

        public ArrayCube()
        {
            int index = 0;
            for(int i = 0; i < 6; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    Data[index++] = (CubeColor)i;
                }
            }
        }

        public void MakeMove(CubeMove cm)
        {
            Array.Copy(Data, ArrayBuffer, TILE_COUNT);

            int[] move = MoveArray[(int)cm];

            for(int i = 0; i < TILE_COUNT; i++)
            {
                Data[move[i]] = ArrayBuffer[i];
            }
        }

        public string GetSideView()
        {
            int[] sides = new int[]{ 0, 5, 1, 4 };
            string[] tiles = new string[]
            {
                "o", "r", "y", "w", "b", "g"
            };

            string s = "#color_viewer(0.6cm,\n";
            
            for (int y = 3 - 1; y >= 0; y--)
            {
                for (int i = 0; i < 3; i++)
                    s += "n, ";

                for (int x = 0; x < 3; x++)
                {
                    s += tiles[(int)Data[GetTileIndex(3, x, y)]] + ", ";
                }

                for (int i = 0; i < 6; i++)
                    s += "n, ";

                s += "\n";
            }
            for(int y = 3 - 1; y >= 0; y--)
            {
                for(int i = 0; i < sides.Length; i++)
                {
                    for(int x = 0; x < 3; x++)
                    {
                        s += tiles[(int)Data[GetTileIndex(sides[i], x, y)]] + ", ";
                    }
                }

                s += "\n";
            }

            for (int y = 3 - 1; y >= 0; y--)
            {
                for (int i = 0; i < 3; i++)
                    s += "n, ";

                for (int x = 0; x < 3; x++)
                {
                    s += tiles[(int)Data[GetTileIndex(2, x, y)]] + ", ";
                }

                for (int i = 0; i < 6; i++)
                    s += "n, ";

                s += "\n";
            }


            return s + ")";

        }

        public override string ToString()
        {
            int index = 0;
            string s = "";
            for(int side = 0; side < 6; side++)
            {
                for(int i = 0; i < 9; i++)
                {
                    s += Data[index++].ToString()[0] + " ";
                }
                s += "\n";
            }

            return s;
        }
    }
}
