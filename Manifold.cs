using System;
using System.Collections.Generic;
using System.IO;
using CultureInfo = System.Globalization.CultureInfo;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace HC
{
    /// <summary>
    /// A fancy name for "level", or "map".
    /// </summary>
    public class Manifold
    {
        private Manifold()
        {

        }

        /// <summary>
        /// Loads (and smooths) a manifold from the individual lines of a OBJ file.
        /// </summary>
        public static Manifold Load(string[] Lines)
        {
            List<Vector> verts = new List<Vector>();
            List<List<int>> faces = new List<List<int>>();

            foreach (string line in Lines)
            {
                if (line != "")
                {
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts[0] == "v")
                    {
                        verts.Add(new Vector(
                            double.Parse(parts[1], CultureInfo.InvariantCulture),
                            double.Parse(parts[2], CultureInfo.InvariantCulture),
                            double.Parse(parts[3], CultureInfo.InvariantCulture)));
                    }
                    if (parts[0] == "f")
                    {
                        List<int> vs = new List<int>();
                        for(int t = 1; t < parts.Length; t++)
                        {
                            vs.Add(int.Parse(parts[t]) - 1);
                        }
                        faces.Add(vs);
                    }
                }
            }

            Manifold mf = new Manifold();
            mf._Vertices = verts;
            mf._Triangles = new List<Triangle<int>>();
            mf._Segments = new Dictionary<Segment<int>, Triangle<int>>();
            foreach (List<int> face in faces)
            {
                int first = face[0];
                for (int i = 2; i < face.Count; i++)
                {
                    mf._AddTriangle(new Triangle<int>(first, face[i - 1], face[i]));
                }
            }

            return mf;
        }

        /// <summary>
        /// Adds a triangle, along with segment references.
        /// </summary>
        private void _AddTriangle(Triangle<int> Tri)
        {
            this._Triangles.Add(Tri);
            foreach (Segment<int> seg in Tri.Segments)
            {
                this._Segments.Add(seg, Tri);
            }
        }

        /// <summary>
        /// Renders the manifold to the current graphics context.
        /// </summary>
        public void Render()
        {
            GL.Begin(BeginMode.Triangles);
            foreach (Triangle<int> tri in this._Triangles)
            {
                GL.Vertex3(this._Vertices[tri.A]);
                GL.Vertex3(this._Vertices[tri.B]);
                GL.Vertex3(this._Vertices[tri.C]);
            }
            GL.End();
        }

        /// <summary>
        /// Loads a manifold from the specified file path.
        /// </summary>
        public static Manifold Load(string Path)
        {
            return Load(File.ReadAllLines(Path));
        }

        private List<Vector> _Vertices;
        private List<Triangle<int>> _Triangles;
        private Dictionary<Segment<int>, Triangle<int>> _Segments;
    }
}