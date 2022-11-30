using CubeRenderer.VertexBuffer;
using OpenTK.Mathematics;
using CubeRenderer;

Window3D wd = null;

Task.Factory.StartNew(DoStuff);

wd = new Window3D();
wd.Run();

void DoStuff()
{
	while (wd is null)
	{
		Thread.Sleep(10);
	}
	while (!wd.IsReady)
	{
		Thread.Sleep(10);
	}
}