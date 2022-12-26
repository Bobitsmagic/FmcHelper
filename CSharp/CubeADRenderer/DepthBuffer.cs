using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeRenderer
{
    class DepthBuffer
    {
        readonly int DepthFBO;
        readonly int DepthMap;
        readonly int Width, Height;

        public DepthBuffer(int width, int height)
        {
            Width = width;
            Height = height;

            DepthFBO = GL.GenFramebuffer();

            DepthMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 
                width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, DepthFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void BindAndClear()
        {
            GL.Viewport(0, 0, Width, Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, DepthFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        public void BindAsTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, DepthMap);
        }
    }
}
