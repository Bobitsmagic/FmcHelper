using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeRenderer
{
	public static class Camera
	{
		public static Vector3 EyePos { get { return projectionMatrix.EyePos; } }

		public struct ProjectionMatrix
		{
			public Vector3 EyePos;
			public Vector3 LookAt;
			public Vector3 Up;

			public ProjectionMatrix(Vector3 eyePos, Vector3 lookAt, Vector3 up)
			{
				EyePos = eyePos;
				LookAt = lookAt;
				Up = up;
			}
		}
		public struct ViewMatrix
		{
			public float Left, Right, Bottom, Top, Near, Far;

			public ViewMatrix(float left, float right, float bottom, float top, float near, float far)
			{
				Left = left;
				Right = right;
				Bottom = bottom;
				Top = top;
				Near = near;
				Far = far;
			}
		}
		public struct ModelMatrix
		{
			public float Angle;
			public Vector3 Axis, RCenter, Offset;

			public ModelMatrix(float angle, Vector3 axis, Vector3 rCenter, Vector3 offset)
			{
				Angle = angle;
				Axis = axis;
				RCenter = rCenter;
				Offset = offset;
			}
		}


		private static ProjectionMatrix projectionMatrix;
		private static ViewMatrix viewMatrix;
		private static ModelMatrix modelMatrix;

		public static int PortWidth, PortHeight;
		private static Matrix4 Inverse;

		public static void RefreshViewMatrix(int portWidth, int portHeight)
		{
			PortWidth = portWidth;
			PortHeight = portHeight;

			GL.Viewport(0, 0, portWidth, portHeight);
			float ratio = (float)portWidth / portHeight;
			float left = -ratio;
			float right = ratio;
			float top = 1.0f;
			float bottom = -1.0f;
			float near = 1.0f;
			float far = 100.0f;

			viewMatrix = new ViewMatrix(left, right, bottom, top, near, far);
		}

		public static (Vector3, Vector3) ScreenRaycast(float xpx, float ypx)
		{
			FindInverseMatrix();

			float mouseNDCX = 2 * xpx / PortWidth - 1;
			float mouseNDCY = 1 - 2 * ypx / PortHeight;

			Vector4 mouseNear = Inverse * new Vector4(mouseNDCX, mouseNDCY, 0, 1);
			Vector4 mouseFar = Inverse * new Vector4(mouseNDCX, mouseNDCY, 1, 1);

			mouseNear = mouseNear * (1 / mouseNear.W);
			mouseFar = mouseFar * (1 / mouseFar.W);

			Vector3 start = mouseNear.Xyz;
			Vector3 end = mouseFar.Xyz;

			return (start, end);
		}

		public static void SetProjectionMatrix(Vector3 eye, Vector3 looktAt, Vector3 up)
		{
			projectionMatrix.EyePos = eye;
			projectionMatrix.LookAt = looktAt;
			projectionMatrix.Up = up;
		}

		public static void FindInverseMatrix()
		{
			Matrix4 pMatrix = Matrix4.LookAt(projectionMatrix.EyePos, projectionMatrix.LookAt, projectionMatrix.Up);
			Matrix4 combined = pMatrix * Matrix4.CreatePerspectiveOffCenter(viewMatrix.Left, viewMatrix.Right, viewMatrix.Bottom, viewMatrix.Top, viewMatrix.Near, viewMatrix.Far);
			Inverse = Matrix4.Invert(combined);
		}

		public static void ApplyMatrices()
		{
			ShaderHandler.SetModelMatrix(modelMatrix);

			ShaderHandler.SetProjectionViewMatrix(projectionMatrix, viewMatrix);
		}

		public static void SetModelMatrix(Vector3 offset)
		{
			modelMatrix.Offset = offset;
			modelMatrix.Angle = 0;
			modelMatrix.Axis = Vector3.UnitX;
			modelMatrix.RCenter = new Vector3();
		}
		public static void SetModelMatrix(Vector3 offset, float angle, Vector3 axis)
		{
			modelMatrix.Offset = offset;
			modelMatrix.Angle = angle;
			modelMatrix.Axis = axis;
			modelMatrix.RCenter = new Vector3();
		}
		public static void SetModelMatrix(Vector3 offset, float angle, Vector3 axis, Vector3 rCenter)
		{
			modelMatrix.Offset = offset;
			modelMatrix.Angle = angle;
			modelMatrix.Axis = axis;
			modelMatrix.RCenter = rCenter;
		}
	}
}
