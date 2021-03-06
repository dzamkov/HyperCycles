﻿using System;
using System.Collections.Generic;

namespace HC
{
    /// <summary>
    /// A point's relation to a boundary.
    /// </summary>
    public enum BoundaryRelation
    {
        Front,
        On,
        Back
    }

    /// <summary>
    /// A point's relation to an area.
    /// </summary>
    public enum AreaRelation
    {
        Inside,
        On,
        Outside
    }

    /// <summary>
    /// A group of three homogenous items arranged to form a triangle. Triangles are equal to
    /// all other triangles with the same values that are arranged in the same direction.
    /// </summary>
    public struct Triangle<T> : IEquatable<Triangle<T>>
        where T : IEquatable<T>
    {
        public Triangle(T A, T B, T C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public Triangle(T Vertex, Segment<T> Base)
        {
            this.A = Vertex;
            this.B = Base.A;
            this.C = Base.B;
        }

        public bool Equals(Triangle<T> Triangle)
        {
            return this == Triangle;   
        }

        public override int GetHashCode()
        {
            int a = this.A.GetHashCode();
            int b = this.B.GetHashCode();
            int c = this.C.GetHashCode();
            if ((a > b) ^ (b > c) ^ (c > a))
            {
                return a ^ b ^ c;
            }
            else
            {
                return ~(a ^ b ^ c);
            }
        }

        public static bool operator ==(Triangle<T> A, Triangle<T> B)
        {
            // Allow rotational symmetric equality.
            if (A.A.Equals(B.A))
            {
                return A.B.Equals(B.B) && A.C.Equals(B.C);
            }
            if (A.A.Equals(B.B))
            {
                return A.B.Equals(B.C) && A.C.Equals(B.A);
            }
            if (A.A.Equals(B.C))
            {
                return A.B.Equals(B.A) && A.C.Equals(B.B);
            }
            return false;
        }

        public static bool operator !=(Triangle<T> A, Triangle<T> B)
        {
            return !(A == B);
        }

        public override bool Equals(object obj)
        {
            Triangle<T>? tri = obj as Triangle<T>?;
            if (tri.HasValue)
            {
                return this == tri.Value;
            }
            return false;
        }

        /// <summary>
        /// Gets the points (hopefully 3) in the triangle.
        /// </summary>
        public T[] Points
        {
            get
            {
                return new T[]
                {
                    this.A,
                    this.B,
                    this.C
                };
            }
        }

        /// <summary>
        /// Gets the edges of the triangle.
        /// </summary>
        public Segment<T>[] Segments
        {
            get
            {
                return new Segment<T>[]
                {
                    new Segment<T>(this.A, this.B),
                    new Segment<T>(this.B, this.C),
                    new Segment<T>(this.C, this.A)
                };
            }
        }

        /// <summary>
        /// Gets the primary vertex of the triangle (A).
        /// </summary>
        public T Vertex
        {
            get
            {
                return this.A;
            }
        }

        /// <summary>
        /// Gets the primary base of the triangle (B, C).
        /// </summary>
        public Segment<T> Base
        {
            get
            {
                return new Segment<T>(this.B, this.C);
            }
        }

        /// <summary>
        /// Gets a flipped form of the triangle (same points, different order).
        /// </summary>
        public Triangle<T> Flip
        {
            get
            {
                return new Triangle<T>(this.A, this.C, this.B);
            }
        }

        public override string ToString()
        {
            return this.A.ToString() + ", " + this.B.ToString() + ", " + this.C.ToString();
        }

        public T A;
        public T B;
        public T C;
    }

    /// <summary>
    /// Triangle (2-simplex) related functions.
    /// </summary>
    public static class Triangle
    {
        /// <summary>
        /// Gets the normal of the specified triangle.
        /// </summary>
        public static Vector Normal(Triangle<Vector> Triangle)
        {
            return Vector.Normalize(Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A));
        }

        /// <summary>
        /// Gets the order of the specified triangle. Swapping any two points in the triangle negates the order. A triangle
        /// with a positive order has all segments pointing outward.
        /// </summary>
        public static bool Order(Triangle<Point> Triangle)
        {
            return Segment.Relation(Triangle.Vertex, Triangle.Base) == BoundaryRelation.Back;
        }

        /// <summary>
        /// Gets the relation a point has to the plane defined by the triangle.
        /// </summary>
        public static BoundaryRelation Relation(Vector Point, Triangle<Vector> Triangle)
        {
            Vector norm = Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A);
            double dota = Vector.Dot(Point - Triangle.A, norm);
            if (dota > 0.0)
            {
                return BoundaryRelation.Front;
            }
            if (dota < 0.0)
            {
                return BoundaryRelation.Back;
            }
            return BoundaryRelation.On;
        }

        /// <summary>
        /// Gets the relation a point has to a triangle.
        /// </summary>
        public static AreaRelation Relation(Point Point, Triangle<Point> Triangle)
        {
            Point ra = new Point();
            Point rb = Triangle.B - Triangle.A;
            Point rc = Triangle.C - Triangle.A;
            Point rp = Point - Triangle.A;
            BoundaryRelation ab = Segment.Relation(rp, new Segment<Point>(ra, rb));
            BoundaryRelation bc = Segment.Relation(rp, new Segment<Point>(rb, rc));
            BoundaryRelation ca = Segment.Relation(rp, new Segment<Point>(rc, ra));
            if (ab == BoundaryRelation.Back && bc == BoundaryRelation.Back && ca == BoundaryRelation.Back)
            {
                return AreaRelation.Inside;
            }
            bool abonorinside = ab == BoundaryRelation.On || ab == BoundaryRelation.Back;
            bool bconorinside = bc == BoundaryRelation.On || bc == BoundaryRelation.Back;
            bool caonorinside = ca == BoundaryRelation.On || ca == BoundaryRelation.Back;
            if (
                (ab == BoundaryRelation.On && bconorinside && caonorinside) || 
                (bc == BoundaryRelation.On && caonorinside && abonorinside) ||
                (ca == BoundaryRelation.On && abonorinside && bconorinside))
            {
                return AreaRelation.On;
            }
            return AreaRelation.Outside;
        }

        /// <summary>
        /// Gets the center of the circumsphere encompassing the triangle.
        /// </summary>
        public static Vector Circumcenter(Triangle<Vector> Triangle)
        {
            // From http://www.ics.uci.edu/~eppstein/junkyard/circumcenter.html
            Vector ba = Triangle.B - Triangle.A;
            Vector ca = Triangle.C - Triangle.A;
            double balen = ba.SquareLength;
            double calen = ca.SquareLength;
            Vector crossbc = Vector.Cross(ba, ca);

            double denominator = 0.5 / crossbc.SquareLength;

            return new Vector(
                ((balen * ca.Y - calen * ba.Y) * crossbc.Z - (balen * ca.Z - calen * ba.Z) * crossbc.Y) * denominator,
                ((balen * ca.Z - calen * ba.Z) * crossbc.X - (balen * ca.X - calen * ba.X) * crossbc.Z) * denominator,
                ((balen * ca.X - calen * ba.X) * crossbc.Y - (balen * ca.Y - calen * ba.Y) * crossbc.X) * denominator) + Triangle.A;
        }

        /// <summary>
        /// Aligns the base (B, C) of the source triangle so that it equals the base. Returns
        /// null if the triangle does not include the specified base.
        /// </summary>
        public static Triangle<T>? Align<T>(Triangle<T> Source, Segment<T> Base)
            where T : IEquatable<T>
        {
            if (new Segment<T>(Source.A, Source.B) == Base)
            {
                return new Triangle<T>(Source.C, Base);
            }
            if (new Segment<T>(Source.B, Source.C) == Base)
            {
                return new Triangle<T>(Source.A, Base);
            }
            if (new Segment<T>(Source.C, Source.A) == Base)
            {
                return new Triangle<T>(Source.B, Base);
            }
            return null;
        }

        /// <summary>
        /// Gets if the two vectors are on opposite sides of the plane defined by the triangle.
        /// </summary>
        public static bool Oppose(Vector A, Vector B, Triangle<Vector> Triangle)
        {
            Vector norm = Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A);
            double dota = Vector.Dot(A - Triangle.A, norm);
            double dotb = Vector.Dot(B - Triangle.A, norm);
            return (dota > 0.0) ^ (dotb > 0.0);
        }

        /// <summary>
        /// Gets if the specified vector is on the front side of the plane defined by the triangle.
        /// </summary>
        public static bool Front(Vector A, Triangle<Vector> Triangle)
        {
            Vector norm = Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A);
            double dota = Vector.Dot(A - Triangle.A, norm);
            return (dota > 0.0);
        }

        /// <summary>
        /// Gets the area of the specified triangle.
        /// </summary>
        public static double Area(Triangle<Vector> Triangle)
        {
            return Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A).Length / 2.0;
        }

        /// <summary>
        /// Determines if a segment intersects the triangle and if so, outputs the relative distance along
        /// the segment the hit is at and the actual position of the hit. This will only check for a hit on the front side of
        /// the triangle.
        /// </summary>
        public static bool Intersect(Triangle<Vector> Triangle, Segment<Vector> Segment, out double Length, out Vector Position)
        {
            Point uv;
            if (Intersect(Triangle, Segment, out Length, out Position, out uv))
            {
                if (Length >= 0.0 && Length <= 1.0)
                {
                    if (uv.X >= 0.0 && uv.X <= 1.0)
                    {
                        if (uv.Y >= 0.0 && (uv.Y + uv.X) <= 1.0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Finds where the segment intersects the plane and ouputs the point where the intersection
        /// is made, the length along the segment the intersection is at, and the uv coordinates relative to the triangle the intersection is at. Returns
        /// if the segment hit the triangle on its front face.
        /// </summary>
        public static bool Intersect(Triangle<Vector> Triangle, Segment<Vector> Segment, out double Length, out Vector Position, out Point UV)
        {
            Vector u = Triangle.B - Triangle.A;
            Vector v = Triangle.C - Triangle.A;
            Vector n = Vector.Cross(u, v);

            // Test intersection of segment and triangle plane.
            Vector raydir = Segment.B - Segment.A;
            Vector rayw = Segment.A - Triangle.A;
            double a = -Vector.Dot(n, rayw);
            double b = Vector.Dot(n, raydir);
            double r = a / b;

            Length = r;
            Position = Segment.A + (raydir * r);

            // Check if point is in triangle.
            Vector w = Position - Triangle.A;
            double uu = Vector.Dot(u, u);
            double uv = Vector.Dot(u, v);
            double vv = Vector.Dot(v, v);
            double wu = Vector.Dot(w, u);
            double wv = Vector.Dot(w, v);
            double d = (uv * uv) - (uu * vv);
            UV = new Point(((uv * wv) - (vv * wu)) / d, ((uv * wu) - (uu * wv)) / d);

            return b < 0.0;
        }

        /// <summary>
        /// Gets the uv coordinates of the specified position on the triangle.
        /// </summary>
        public static Point UV(Triangle<Vector> Triangle, Vector Position)
        {
            Vector u = Triangle.B - Triangle.A;
            Vector v = Triangle.C - Triangle.A;
            Vector w = Position - Triangle.A;
            double uu = Vector.Dot(u, u);
            double uv = Vector.Dot(u, v);
            double vv = Vector.Dot(v, v);
            double wu = Vector.Dot(w, u);
            double wv = Vector.Dot(w, v);
            double d = (uv * uv) - (uu * vv);
            return new Point(((uv * wv) - (vv * wu)) / d, ((uv * wu) - (uu * wv)) / d);
        }

        /// <summary>
        /// Calculates the relative coordinates that result from the intersection of the plane of triangle A and the plane of triangle B. Returns
        /// if the two triangles are not coplanar.
        /// </summary>
        /// <param name="ABLength">Length along (A.A, A.B) the plane of B is at.</param>
        /// <param name="ACLength">Length along (A.A, A.C) the plane of B is at.</param>
        /// <param name="ABHit">Place on B where (A.A, A.B) hit.</param>
        /// <param name="ACHit">Place on B where (A.A, A.C) hit.</param>
        /// <param name="ABUV">UV point on B where (A.A, A.B) hit.</param>
        /// <param name="ACUV">UV point on B where (A.A, A.C) hit.</param>
        public static bool Intersect(
            Triangle<Vector> A, Triangle<Vector> B, 
            out double ABLength, out double ACLength, 
            out Vector ABHit, out Vector ACHit, 
            out Point ABUV, out Point ACUV)
        {
            Intersect(B, new Segment<Vector>(A.A, A.B), out ABLength, out ABHit, out ABUV);
            Intersect(B, new Segment<Vector>(A.A, A.C), out ACLength, out ACHit, out ACUV);
            return !double.IsNaN(ABUV.X);
        }
    }
}