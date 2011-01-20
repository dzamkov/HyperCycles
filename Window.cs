using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace HC
{
    /// <summary>
    /// Main program window.
    /// </summary>
    public class Window : GameWindow
    {
        public Window() : base(640, 480, GraphicsMode.Default, "HyperCycles!")
        {
            this.WindowBorder = WindowBorder.Resizable;
            this.WindowState = WindowState.Maximized;

            this._Manifold = Manifold.Load("../../Maps/1.obj");

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        /// <summary>
        /// Program main entry point.
        /// </summary>
        public static void Main(string[] Args)
        {
            new Window().Run(60.0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.RGB(0.0, 0.0, 0.0));
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(0.7, (double)this.Width / (double)this.Height, 0.1, 100.0);
            GL.LoadMatrix(ref proj);
            Matrix4d view = Matrix4d.LookAt(3.0 * Math.Sin(this._Time), 3.0 * Math.Cos(this._Time), 3.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0);
            GL.MultMatrix(ref view);

            this._Manifold.Render();

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this._Time += e.Time;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        private double _Time;
        private Manifold _Manifold;
    }
}