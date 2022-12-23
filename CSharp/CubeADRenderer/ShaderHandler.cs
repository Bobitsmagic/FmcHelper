using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CubeRenderer
{
	public static class ShaderHandler
	{
		public const int PositionLocation = 0;
		public const int ColorLocation = 1;
		public const int TextureCoordLocation = 2;
		public const int NormalLocation = 3;
		public static int ShaderCount { get { return ProgramPointer.Count; } }

		#region Locations
		public static int PVMatrixLocation, ModelMatrixLocation;
		public static int AmbientLocation;
		public static int GPos, GNormal, GColorSpec;
		#endregion

		private static Dictionary<string, int> ProgramPointer = new Dictionary<string, int>();
		public static int ActiveLightCount = 0;
		public static string LoadedShader = "";
		public static void Initialize()
		{
			string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Shader")).Select(x => x.Split('\\').Last()).ToArray();

			HashSet<string> names = new HashSet<string>();
			Dictionary<string, int> ShaderPointer = new Dictionary<string, int>();

			foreach (string name in files)
			{
				names.Add(name.Split('.')[0]);
				string contend = File.ReadAllText(Path.Combine("Shader", name));

				ShaderPointer.Add(name, LoadShader(name, contend));
			}

			foreach (string name in names)
			{
				ProgramPointer.Add(name, InitProgramm(ShaderPointer[name + ".vert"], ShaderPointer[name + ".frag"]));
				GL.DeleteShader(ShaderPointer[name + ".vert"]);
				GL.DeleteShader(ShaderPointer[name + ".frag"]);
			}

			int LoadShader(string name, string source)
			{
				ShaderType type = name.EndsWith("frag") ? ShaderType.FragmentShader : ShaderType.VertexShader;
				int shader = GL.CreateShader(type);
				if (shader == 0)
					throw new InvalidOperationException("Unable to create shader");

				int length = 0;
				GL.ShaderSource(shader, 1, new string[] { source }, (int[])null);
				GL.CompileShader(shader);

				GL.GetShader(shader, ShaderParameter.CompileStatus, out int compiled);
				if (compiled == 0)
				{
					length = 0;
					GL.GetShader(shader, ShaderParameter.InfoLogLength, out length);
					if (length > 0)
					{
						GL.GetShaderInfoLog(shader, length, out length, out string log);
					}

					GL.DeleteShader(shader);
					throw new InvalidOperationException("Unable to compile shader of type : " + type.ToString());
				}

				return shader;
			}

			int InitProgramm(int vShader, int fShader)
			{
				int program = GL.CreateProgram();
				if (program == 0)
					throw new InvalidOperationException("Unable to create program");

				GL.AttachShader(program, vShader);
				GL.AttachShader(program, fShader);

				GL.LinkProgram(program);

				GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linked);
				if (linked == 0)
				{
					// link failed
					GL.GetProgram(program, GetProgramParameterName.InfoLogLength, out int length);
					if (length > 0)
					{
						GL.GetProgramInfoLog(program, length, out length, out string log);
					}

					GL.DeleteProgram(program);
					throw new InvalidOperationException("Unable to link program");
				}

				return program;
			}
		}


		public static void Dispose()
		{
			foreach (int pointer in ProgramPointer.Values)
			{
				GL.DeleteProgram(pointer);
			}

			ProgramPointer.Clear();
		}

		public static void LoadProgram(string name)
		{
			if (LoadedShader == name) return;

			int pointer = ProgramPointer[name];
			GL.UseProgram(pointer);

			PVMatrixLocation = GL.GetUniformLocation(pointer, "PVMatrix");
			ModelMatrixLocation = GL.GetUniformLocation(pointer, "ModelMatrix");
			AmbientLocation = GL.GetUniformLocation(pointer, "AmbientDirection");

			GPos = GL.GetUniformLocation(pointer, "gPosition");
			GNormal = GL.GetUniformLocation(pointer, "gNormal");
			GColorSpec = GL.GetUniformLocation(pointer, "gColorSpec");

			GL.Uniform1(GPos, 0);
			GL.Uniform1(GNormal, 1);
			GL.Uniform1(GColorSpec, 2);

            SetAmbientDirection(new Vector3(1, 4, 2));
		}

		public static void SetAmbientDirection(Vector3 dir)
		{
			GL.Uniform3(AmbientLocation, Vector3.NormalizeFast(dir));
		}
		public static void SetModelMatrix(Camera.ModelMatrix matrix)
		{
			Vector3 nCenter = -new Vector3(matrix.RCenter.X, matrix.RCenter.Y, matrix.RCenter.Z);
			Matrix4 modelMatrix = Matrix4.CreateTranslation(nCenter);
			Vector3 sum = matrix.Offset + matrix.RCenter;

			modelMatrix = modelMatrix *
				Matrix4.CreateFromAxisAngle(matrix.Axis, matrix.Angle) *
			Matrix4.CreateTranslation(sum);

			GL.UniformMatrix4(ModelMatrixLocation, false, ref modelMatrix);
		}
		public static void SetProjectionViewMatrix(Camera.ProjectionMatrix pm, Camera.ViewMatrix vm)
		{
			Matrix4 pMatrix = Matrix4.LookAt(pm.EyePos, pm.LookAt, pm.Up);
            Matrix4 combined = pMatrix * Matrix4.CreatePerspectiveOffCenter(vm.Left, vm.Right, vm.Bottom, vm.Top, vm.Near, vm.Far);
			GL.UniformMatrix4(PVMatrixLocation, false, ref combined);
		}
	}
}
