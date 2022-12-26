using CubeRenderer.VertexBuffer;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeRenderer
{
    class DeferredRenderer
    {
        GBuffer GBuffer;
        DepthBuffer DepthBuffer;

        ColoredNormalVertexBuffer Quad;
        ColoredNormalVertexBuffer Floor;

        readonly int Width, Height;
        
        public DeferredRenderer(int width, int height)
        {
            Width = width;
            Height = height;

            GBuffer= new GBuffer(width, height);

            Quad = new ColoredNormalVertexBuffer(new ColoredNormalVertex[]
            {
                new ColoredNormalVertex(new Vector3(-1, -1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(1, -1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(1, 1, -1), new Vector3(), new Vector4()),

                new ColoredNormalVertex(new Vector3(-1, -1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(1, 1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(-1, 1, -1), new Vector3(), new Vector4())
            }, PrimitiveType.Triangles, BufferUsageHint.DynamicDraw);

            Vector4 fColor = new Vector4(1, 1, 1, 1);
            const float LENGTH = 7;
            Floor = new ColoredNormalVertexBuffer(new ColoredNormalVertex[]
            {
                new ColoredNormalVertex(LENGTH * new Vector3(-1, -1, -1), new Vector3(0, 1, 0), fColor),
                new ColoredNormalVertex(LENGTH * new Vector3(1, -1, -1), new Vector3(0, 1, 0), fColor),
                new ColoredNormalVertex(LENGTH * new Vector3(1, -1, 1), new Vector3(0, 1, 0), fColor),

                new ColoredNormalVertex(LENGTH * new Vector3(-1, -1, -1), new Vector3(0, 1, 0), fColor),
                new ColoredNormalVertex(LENGTH * new Vector3(1, -1, 1), new Vector3(0, 1, 0), fColor),
                new ColoredNormalVertex(LENGTH * new Vector3(-1, -1, 1), new Vector3(0, 1, 0), fColor)
            }, PrimitiveType.Triangles, BufferUsageHint.DynamicDraw);

            DepthBuffer = new DepthBuffer(1024, 1024);
        }

        float x = 5;
        public void RenderCube(CubeMesh cm)
        {
            //Shadow pass
            ShaderHandler.LoadProgram("shadow");

            DepthBuffer.BindAndClear();
            x += 0.01f;
            SetShadowCam();

            cm.BindAndDraw();
            Camera.SetModelMatrix(new Vector3(0, 0, 0));
            Camera.ApplyModelMatrix();
            //Camera.ApplyMatrices();
            Floor.BindAndDrawAll();

            //Def pass 1
            ShaderHandler.LoadProgram("def");
            //ShaderHandler.SetOrtoghonalViewMatrix(new Vector3(5, 7, 3), new Vector3(0, 0, 0), new Vector3(0, 0, 1),
            //    10, 10, 1, 12);

            Camera.ApplyMatrices();
            GBuffer.BindAndClear();
            GL.Viewport(0, 0, Width, Height);

            //Camera.ApplyMatrices();
            cm.BindAndDraw();
            Camera.SetModelMatrix(new Vector3(0, 0, 0));
            Camera.ApplyModelMatrix();

            Floor.BindAndDrawAll();

            //Lighting pass
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(1, 0, 1, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ShaderHandler.SetCamPos(Camera.EyePos);

            GBuffer.BindAsTextures();
            DepthBuffer.BindAsTexture();

            ShaderHandler.LoadProgram("quad");
            ShaderHandler.SetAmbientDirection(-new Vector3(-3, 5, 3));
            SetShadowCam();
            Quad.BindAndDrawAll();
        }

        public void SetShadowCam()
        {
            ShaderHandler.SetOrtoghonalViewMatrix(new Vector3(-3, 5, 3), new Vector3(0, 0, 0), new Vector3(0, 0, 1),
                20, 20, 1, 20);
        }
    }
}
