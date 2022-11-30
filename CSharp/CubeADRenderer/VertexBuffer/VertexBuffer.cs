using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeRenderer.VertexBuffer
{
	public abstract class VertexBuffer
	{
		private static List<VertexBuffer> allBuffers = new List<VertexBuffer>();


		public int VertexCount { get; protected set; }
		protected int VBO, VAO;

		protected PrimitiveType mode;
		protected BufferUsageHint usage;

		//stride byte
		//index, size in float
		public VertexBuffer(PrimitiveType mode, BufferUsageHint usage)
		{
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();

			this.mode = mode;
			this.usage = usage;

			allBuffers.Add(this);
		}

		public static void DisposeAll()
		{
			foreach (VertexBuffer vb in allBuffers)
			{
				vb.Dispose();
			}
		}

		public void Dispose()
		{
			GL.DeleteVertexArray(VAO);
			GL.DeleteBuffer(VBO);
		}
		public void BindAndDrawAll()
		{
			GL.BindVertexArray(VAO);
			GL.DrawArrays(mode, 0, VertexCount);
		}
		public void BindAndDrawAll(int start, int count)
		{
			GL.BindVertexArray(VAO);
			GL.DrawArrays(mode, start, count);
		}
	}
}
