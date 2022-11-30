using CubeRenderer.VertexBuffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CubeAD;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;

namespace CubeRenderer
{
	class CubeMesh
	{
		private static Vector4[] CubeColors =
		{
			new Vector4(1, 0.5f, 0, 0), //Orange
			new Vector4(1, 0, 0, 0), //Red
			new Vector4(1, 1, 0, 0), //Yellow
			new Vector4(1, 1, 1, 0), //White
			new Vector4(0, 1, 0, 0), //Green
			new Vector4(0, 0, 1, 0), //Blue
			new Vector4(0, 0, 0, 0), //Black
		};
		static Vector3[] Dimensions = new Vector3[]
		{
			new Vector3(1, 0, 0),
			new Vector3(0, 1, 0),
			new Vector3(0, 0, 1),
		};

		private ColoredNormalVertexBuffer[] CuboidVb = new ColoredNormalVertexBuffer[27];
		StickerCube SC;
		public CubeMove CurrentMove = CubeMove.None;
		public float Angle = 0;
		public MoveSequenz moves = new MoveSequenz("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2");
		int MoveIndex = 0;


		public CubeMesh(StickerCube sc)
		{
			SC = sc;
			CubeColor[] colors = new CubeColor[6];
			int index = 0;
			int oneCount = 0;
			int sideX, sideY, zSideCoord;
			for(int cx = 0; cx < 3; cx++)
			{
				sideX = cx switch
				{
					0 => 0,
					1 => -1,
					2 => 1
				};
				for (int cy = 0; cy < 3; cy++)
				{
					sideY = cy switch
					{
						0 => 2,
						1 => -1,
						2 => 3
					};
					for (int cz = 0; cz < 3; cz++)
					{
						zSideCoord = cz switch
						{
							0 => 4,
							1 => -1,
							2 => 5
						};
						int zSideCube = cz switch
						{
							0 => 5,
							1 => -1,
							2 => 4
						};
						oneCount = 0;
						if (cx == 1) oneCount++;
						if (cy == 1) oneCount++;
						if (cz == 1) oneCount++;

						Array.Fill(colors, CubeColor.None);

						//Center
						if(oneCount == 2)
						{
							if (cx == 0) colors[0] = (CubeColor)0;
							if (cx == 2) colors[1] = (CubeColor)1;
							if (cy == 0) colors[2] = (CubeColor)2;
							if (cy == 2) colors[3] = (CubeColor)3;
							if (cz == 0) colors[4] = (CubeColor)5;
							if (cz == 2) colors[5] = (CubeColor)4;
						}

						//Edge
						if (oneCount == 1)
						{
							if (cx == 1)
							{
								colors[sideY] = sc.GetEdgeColor(sideY, zSideCube);
								colors[zSideCoord] = sc.GetEdgeColor(zSideCube, sideY);
							}
							if (cy == 1)
							{
								colors[sideX] = sc.GetEdgeColor(sideX, zSideCube);
								colors[zSideCoord] = sc.GetEdgeColor(zSideCube, sideX);
							}
							if (cz == 1)
							{
								colors[sideY] = sc.GetEdgeColor(sideY, sideX);
								colors[sideX] = sc.GetEdgeColor(sideX, sideY);
							}
						}

						//Corner
						if (oneCount == 0)
						{
							colors[sideX] = sc.GetCornerColor(sideX, sideY, zSideCube);
							colors[sideY] = sc.GetCornerColor(sideY, zSideCube, sideX);
							colors[zSideCoord] = sc.GetCornerColor(zSideCube, sideX, sideY);
							
						}

						CuboidVb[index++] = new ColoredNormalVertexBuffer(CreateCuboid(new Vector3(cx, cy, cz) * 1.1f - new Vector3(1, 1, 1) * 1.5f * 1.05f, colors), 
							PrimitiveType.Triangles, BufferUsageHint.DynamicDraw);
					}
				}
			}

			CurrentMove = moves.Moves[MoveIndex++];
		}

		
		public void Update(StickerCube sc)
		{
			CubeColor[] colors = new CubeColor[6];
			int index = 0;
			int oneCount = 0;
			int sideX, sideY, zSideCoord;
			for (int cx = 0; cx < 3; cx++)
			{
				sideX = cx switch
				{
					0 => 0,
					1 => -1,
					2 => 1
				};
				for (int cy = 0; cy < 3; cy++)
				{
					sideY = cy switch
					{
						0 => 2,
						1 => -1,
						2 => 3
					};
					for (int cz = 0; cz < 3; cz++)
					{
						zSideCoord = cz switch
						{
							0 => 4,
							1 => -1,
							2 => 5
						};
						int zSideCube = cz switch
						{
							0 => 5,
							1 => -1,
							2 => 4
						};
						oneCount = 0;
						if (cx == 1) oneCount++;
						if (cy == 1) oneCount++;
						if (cz == 1) oneCount++;

						Array.Fill(colors, CubeColor.None);

						//Center
						if (oneCount == 2)
						{
							if (cx == 0) colors[0] = (CubeColor)0;
							if (cx == 2) colors[1] = (CubeColor)1;
							if (cy == 0) colors[2] = (CubeColor)2;
							if (cy == 2) colors[3] = (CubeColor)3;
							if (cz == 0) colors[4] = (CubeColor)5;
							if (cz == 2) colors[5] = (CubeColor)4;
						}

						//Edge
						if (oneCount == 1)
						{
							if (cx == 1)
							{
								colors[sideY] = sc.GetEdgeColor(sideY, zSideCube);
								colors[zSideCoord] = sc.GetEdgeColor(zSideCube, sideY);
							}
							if (cy == 1)
							{
								colors[sideX] = sc.GetEdgeColor(sideX, zSideCube);
								colors[zSideCoord] = sc.GetEdgeColor(zSideCube, sideX);
							}
							if (cz == 1)
							{
								colors[sideY] = sc.GetEdgeColor(sideY, sideX);
								colors[sideX] = sc.GetEdgeColor(sideX, sideY);
							}
						}

						//Corner
						if (oneCount == 0)
						{
							colors[sideX] = sc.GetCornerColor(sideX, sideY, zSideCube);
							colors[sideY] = sc.GetCornerColor(sideY, zSideCube, sideX);
							colors[zSideCoord] = sc.GetCornerColor(zSideCube, sideX, sideY);

						}

						CuboidVb[index++].UpdateData(CreateCuboid(new Vector3(cx, cy, cz) * 1.1f - new Vector3(1, 1, 1) * 1.5f * 1.05f, colors), 0);
					}
				}
			}
		}


		public void BindAndDraw()
		{
			if(CurrentMove == CubeMove.None)
			{
				Camera.SetModelMatrix(new Vector3());
				Camera.ApplyMatrices();
				
				for (int i = 0; i < CuboidVb.Length; i++)
					CuboidVb[i].BindAndDrawAll();
			}
			else
			{
				int index = 0;
				for (int cx = 0; cx < 3; cx++)
				{
					for (int cy = 0; cy < 3; cy++)
					{
						for (int cz = 0; cz < 3; cz++)
						{
							bool rot = false;
							switch((int)CurrentMove / 3)
							{
								case 0: rot = cx == 0; break;
								case 1: rot = cx == 2; break;
								case 2: rot = cy == 0; break;
								case 3: rot = cy == 2; break;
								case 4: rot = cz == 2; break; //!!
								case 5: rot = cz == 0; break;
							}

							if(rot)
							{

								Camera.SetModelMatrix(new Vector3(), ((int)CurrentMove / 3 == 0 || (int)CurrentMove / 3 == 2 ? -1 : 1) * (((int)CurrentMove) % 3 == 0 ? -1 : 1) * Angle, Dimensions[(int)CurrentMove / 6]);
							}
							else
							{
								Camera.SetModelMatrix(new Vector3());
							}
							Camera.ApplyMatrices();

							CuboidVb[index++].BindAndDrawAll();
						}
					}
				}

				Angle += 0.2f;
				if (Angle >= (((int)CurrentMove) % 3 == 1 ? 2 : 1) * MathF.PI / 2)
				{
					Angle = 0;

					SC.MakeMove(CurrentMove);
					Update(SC);

					if (MoveIndex == moves.Count)
						CurrentMove = CubeMove.None;
					else 
						CurrentMove = moves.Moves[MoveIndex++];

					Console.WriteLine(CurrentMove);
				}
			}
		}

		private static ColoredNormalVertex[] CreateCuboid(Vector3 offset, CubeColor[] colors)
		{
			int index = 0;
			ColoredNormalVertex[] array = new ColoredNormalVertex[6 * 6];
			for(int d = 0; d < 3; d++)
			{
				AddQuad(offset, Dimensions[(d + 1) % 3], Dimensions[(d + 2) % 3], -Dimensions[d], CubeColors[(int)colors[d * 2 + 0]]);
				AddQuad(offset + Dimensions[d], Dimensions[(d + 1) % 3], Dimensions[(d + 2) % 3], Dimensions[d], CubeColors[(int)colors[d * 2 + 1]]);
			}

			return array;

			void AddQuad(Vector3 offset, Vector3 dir1, Vector3 dir2, Vector3 normal, Vector4 color)
			{
				array[index++] = new ColoredNormalVertex(offset, normal, color);
				array[index++] = new ColoredNormalVertex(offset + dir1, normal, color);
				array[index++] = new ColoredNormalVertex(offset + dir1 + dir2, normal, color);

				array[index++] = new ColoredNormalVertex(offset, normal, color);
				array[index++] = new ColoredNormalVertex(offset + dir1 + dir2, normal, color);
				array[index++] = new ColoredNormalVertex(offset + dir2, normal, color);
			}
		}
	}
}
