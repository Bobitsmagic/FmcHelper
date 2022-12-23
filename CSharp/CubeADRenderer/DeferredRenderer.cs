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
        readonly int Width, Height;
        readonly int gBuffer;
        readonly int gPosition, gNormal, gColorSpec;
        readonly int gDepth;    

        ColoredNormalVertexBuffer Quad;
        DrawBuffersEnum[] Attachments;

        public DeferredRenderer(int width, int height)
        {
            Width = width;
            Height = height;

            Quad = new ColoredNormalVertexBuffer(new ColoredNormalVertex[]
            {
                new ColoredNormalVertex(new Vector3(-1, -1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(1, -1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(1, 1, -1), new Vector3(), new Vector4()),

                new ColoredNormalVertex(new Vector3(-1, -1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(1, 1, -1), new Vector3(), new Vector4()),
                new ColoredNormalVertex(new Vector3(-1, 1, -1), new Vector3(), new Vector4())
            }, PrimitiveType.Triangles, BufferUsageHint.DynamicDraw);

            gBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, gBuffer);

            //Position
            gPosition = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gPosition, 0);

            //Normal
            gNormal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gNormal, 0);

            //RGB + Spec
            gColorSpec = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gColorSpec);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, gColorSpec, 0);


            //Depth
            gDepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gDepth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, gDepth, 0);
            
            Attachments = new DrawBuffersEnum[3] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 };
            GL.DrawBuffers(Attachments.Length, Attachments);
        }

        public void RenderCube(CubeMesh cm)
        {
            ShaderHandler.LoadProgram("def");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, gBuffer);
            GL.ClearColor(0, 0, 0.3f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Camera.ApplyMatrices();
            cm.BindAndDraw();

            //Lighting pass
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0, 0.4f, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, gColorSpec);

            ShaderHandler.LoadProgram("quad");

            //Console.WriteLine(GL.GetError());
            Quad.BindAndDrawAll();
        }
    }
}
