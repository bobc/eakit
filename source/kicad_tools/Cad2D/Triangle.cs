using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

using OpenTK;
//using System.Numerics;

namespace Cad2D
{
    [Serializable]
    public class Triangle : Polygon
    {
        public Vector2 A { get { return Vertices[0]; } set { Vertices[0] = value; } }
        public Vector2 B { get { return Vertices[1]; } set { Vertices[1] = value; } }
        public Vector2 C { get { return Vertices[2]; } set { Vertices[2] = value; } }

        public Triangle()
        {
            Initialize();
        }
        /*
        protected Triangle(SerializationInfo info, StreamingContext context)
        {
            Vertices = (List<Vector2>)info.GetValue("Vertices", typeof(List<Vector2>));
            AutoTriangulate = info.GetBoolean("AutoTriangulate");
            Triangles = new List<Triangle>{ this };// (List<Triangle>)info.GetValue("Triangles", typeof(List<Triangle>));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Vertices", Vertices, Vertices.GetType());
            info.AddValue("AutoTriangulate", AutoTriangulate);
            //info.AddValue("Triangles", Triangles, Triangles.GetType());
        }
        */
        protected override void Initialize()
        {
            Vertices = new List<Vector2>(3);
            Triangles = new List<Triangle> { this };
        }

        public Triangle(Triangle t)
        {
            Initialize();
            Vertices[0] = t.Vertices[0];
            Vertices[1] = t.Vertices[1];
            Vertices[2] = t.Vertices[2];
        }

        public Triangle(Polygon p)
        {
            Initialize();
            if (p.Vertices.Count > 1)
            {
                Vertices[0] = p.Vertices[0];
                if (p.Vertices.Count > 2)
                {
                    Vertices[1] = p.Vertices[1];
                    if (p.Vertices.Count > 3)
                    {
                        Vertices[2] = p.Vertices[2];
                    }
                }
            }
        }

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            Vertices = new List<Vector2>(3) { a, b, c };
            Triangles = new List<Triangle> { this };
        }

        public Triangle(float xa, float ya, float xb, float yb, float xc, float yc)
        {
            Vertices = new List<Vector2>(3) { 
                new Vector2(xa, ya),
                new Vector2(xb, yb),
                new Vector2(xc, yc)
            };
            Triangles = new List<Triangle> { this };
        }

        public new Triangle Offset(Vector2 amount)
        {
            Vertices[0] += amount;
            Vertices[1] += amount;
            Vertices[2] += amount;
            return this;
        }

        public new Triangle Scale(Vector2 amount)
        {
            // Vertices[n] *= amount;
            Vertices[0] = Vector2.Multiply(Vertices[0], amount);
            Vertices[1] = Vector2.Multiply(Vertices[1], amount);
            Vertices[2] = Vector2.Multiply(Vertices[2], amount);
            return this;
        }

        public override List<Vector2> GetVerticesClockwise()
        {
            if (GetArea() >= 0) return new List<Vector2>(3) { Vertices[2], Vertices[1], Vertices[0] };
            return Vertices;
        }

        public override List<Vector2> GetVerticesCounterClockwise()
        {
            if (GetArea() <= 0) return new List<Vector2>(3) { Vertices[2], Vertices[1], Vertices[0] };
            return Vertices;
        }

        public override float GetArea()
        {
            return
                ((Vertices[0].X * Vertices[1].Y - Vertices[1].X * Vertices[0].Y) +
                (Vertices[1].X * Vertices[2].Y - Vertices[2].X * Vertices[1].Y) +
                (Vertices[2].X * Vertices[0].Y - Vertices[0].X * Vertices[2].Y)) * 0.5f;
        }

        public Vector2 Center()
        {
            return (Vertices[0] + Vertices[1] + Vertices[2]) / 3.0f;
        }

        public override bool IsInside(Vector2 point)
        {
            Vector2 A = new Vector2(Vertices[2].X - Vertices[1].X, Vertices[2].Y - Vertices[1].Y);
            Vector2 B = new Vector2(Vertices[0].X - Vertices[2].X, Vertices[0].Y - Vertices[2].Y);
            Vector2 C = new Vector2(Vertices[1].X - Vertices[0].X, Vertices[1].Y - Vertices[0].Y);
            Vector2 ap = new Vector2(point.X - Vertices[0].X, point.Y - Vertices[0].Y);
            Vector2 bp = new Vector2(point.X - Vertices[1].X, point.Y - Vertices[1].Y);
            Vector2 cp = new Vector2(point.X - Vertices[2].X, point.Y - Vertices[2].Y);

            var aCbp = A.X * bp.Y - A.Y * bp.X;
            var cCap = C.X * ap.Y - C.Y * ap.X;
            var bCcp = B.X * cp.Y - B.Y * cp.X;

            return ((aCbp >= 0f) && (bCcp >= 0f) && (cCap >= 0f));
        }

        public override void Triangulate()
        {
            //Do nothing
        }
    }
}