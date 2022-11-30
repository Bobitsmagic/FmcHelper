using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeRenderer.VertexBuffer
{
	public struct ColoredNormalVertex
	{
		public const int SizeInBytes = 10 * sizeof(float);

		public Vector3 Position;
		public Vector3 Normal;
		public Vector4 Color;

		public ColoredNormalVertex(Vector3 position, Vector3 normal, Vector4 color)
		{
			Position = position;
			Normal = normal;
			Color = color;
		}
	}
}
