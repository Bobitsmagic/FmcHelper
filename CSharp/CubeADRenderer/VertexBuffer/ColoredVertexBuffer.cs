using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeRenderer.VertexBuffer
{
	public class ColoredNormalVertexBuffer : VertexBuffer
	{
		public ColoredNormalVertexBuffer(ColoredNormalVertex[] data, PrimitiveType mode, BufferUsageHint usage = BufferUsageHint.StaticDraw) : 
			base(mode, usage)
		{
			VertexCount = data.Length;

			CreateBuffer(data);
		}

		public void Recreate(ColoredNormalVertex[] data)
		{
			Dispose();
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();

			CreateBuffer(data);
		}

		private void CreateBuffer(ColoredNormalVertex[] data)
		{
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

			GL.BufferData(BufferTarget.ArrayBuffer, VertexCount * ColoredNormalVertex.SizeInBytes, data, usage);

			GL.VertexAttribPointer(ShaderHandler.PositionLocation, 3, VertexAttribPointerType.Float, false, ColoredNormalVertex.SizeInBytes, 0);
			GL.EnableVertexAttribArray(ShaderHandler.PositionLocation);

			GL.VertexAttribPointer(ShaderHandler.NormalLocation, 3, VertexAttribPointerType.Float, false, ColoredNormalVertex.SizeInBytes, Vector3.SizeInBytes);
			GL.EnableVertexAttribArray(ShaderHandler.NormalLocation);

			GL.VertexAttribPointer(ShaderHandler.ColorLocation, 4, VertexAttribPointerType.Float, false, ColoredNormalVertex.SizeInBytes, 2 * Vector3.SizeInBytes);
			GL.EnableVertexAttribArray(ShaderHandler.ColorLocation);

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}

		public void UpdateData(ColoredNormalVertex[] data, int index)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(index * ColoredNormalVertex.SizeInBytes), data.Length * ColoredNormalVertex.SizeInBytes, data);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}
	}
}
