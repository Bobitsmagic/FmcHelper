using CubeRenderer.VertexBuffer;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;

namespace CubeRenderer
{
	public class Window3D : GameWindow
	{
		public bool IsReady { get; private set; } = false;

		ColoredNormalVertexBuffer TriangleBuffer;

		float Ratio;
		float angle = 0;
		int FPSCount = 0;
		double FPSTimeSum = 0;
		public Vector3 ClearColor = new Vector3(0.5f, 0.5f, 0.5f);
		DeferredRenderer DR;

		CubeMesh cm;

		public Window3D(string title = "kek", double renderFreq = 1337, int updateFreq = 1337, int width = 800, int height = 800) :
			base(new GameWindowSettings() { RenderFrequency = 60, UpdateFrequency = updateFreq },
				new NativeWindowSettings() { Title = title, Size = new Vector2i(width, height) })
		{

		}

		protected override void OnLoad()
		{
			base.OnLoad();

			ShaderHandler.Initialize();

			Ratio = Size.X / (float)Size.Y;

			Console.WriteLine("Ratio: " + Ratio);

			GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
			//GL.CullFace(CullFaceMode.Front);
			//GL.FrontFace(FrontFaceDirection.Ccw);
			VSync = VSyncMode.Off;


			cm = new CubeMesh(new CubeAD.StickerCube());
			DR = new DeferredRenderer(Size.X, Size.Y);

			Console.WriteLine(GL.GetString(StringName.Version));
			ShaderHandler.LoadProgram("color");

			Camera.RefreshViewMatrix(Size.X, Size.Y);
			Camera.SetProjectionMatrix(new Vector3(0, 3, 4), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
			Camera.SetModelMatrix(new Vector3(0, 0, 0));
			Camera.ApplyMatrices();
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			base.OnUpdateFrame(args);

			IsReady = true;
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			Camera.RefreshViewMatrix(Size.X, Size.Y);
			base.OnResize(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			FPSCount++;
			FPSTimeSum += e.Time;

			if(FPSTimeSum > 1)
			{
				//Console.WriteLine("Fps: " + FPSCount);
				FPSCount = 0;
				FPSTimeSum -= 1;
			}
			//26, 26, 52
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, 1);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Camera.SetProjectionMatrix(new Vector3(MathF.Cos(angle), 1, MathF.Sin(angle)) * 5, new Vector3(), new Vector3(0, 1, 0));
			Camera.ApplyMatrices();
			angle += .01f;


			//cm.BindAndDraw();
			DR.RenderCube(cm);

			Context.SwapBuffers();
			base.OnRenderFrame(e);
		}
	}
}
