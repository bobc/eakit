using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Polygon.cs
//Written by Saeed Afshari (www.saeedoo.com)

using System.Diagnostics;
using System.Runtime.Serialization;

using System.Drawing;

using OpenTK;
//using System.Numerics;

namespace Cad2D
{
    [Serializable]
    public partial class Polygon// : ISerializable
    {
        public enum PolygonCollisionClass
        {
            Rectangle,
            Convex,
            Concave
        }

        public bool AutoTriangulate = false;
        public List<Vector2> Vertices;
        public List<Triangle> Triangles;

        public PointF GetPoint (int n)
        {
            return new PointF (Vertices[n].X, Vertices[n].Y);
        }

        const float _epsilon = 0.0000001f;

        private int n { get { return Vertices.Count; } }

        public Polygon()
        {
            Initialize();
        }
        /*
        protected Polygon(SerializationInfo info, StreamingContext context)
        {
            Vertices = (List<Vector2>)info.GetValue("Vertices", typeof(List<Vector2>));
            AutoTriangulate = info.GetBoolean("AutoTriangulate");
            Triangles = (List<Triangle>)info.GetValue("Triangles", typeof(List<Triangle>));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Vertices", Vertices, Vertices.GetType());
            info.AddValue("AutoTriangulate", AutoTriangulate);
            info.AddValue("Triangles", Triangles, Triangles.GetType());
        }
        */
        protected virtual void Initialize()
        {
            Vertices = new List<Vector2>();
        }

        public Polygon(int InitialVertexCount)
        {
            Vertices = new List<Vector2>(InitialVertexCount);
        }

        public Polygon(List<Vector2> vs)
        {
            Vertices = new List<Vector2>(vs.Count);
            foreach (var item in vs)
                Vertices.Add(item);
        }

        public Polygon(params Vector2[] vs)
        {
            Vertices = new List<Vector2>(vs.Length);
            foreach (var item in vs)
                Vertices.Add(item);
        }

        public Polygon(Polygon source)
        {
            Vertices = new List<Vector2>(source.Vertices.Count);
            foreach (var item in source.Vertices)
                Vertices.Add(item);
            if (source.Triangles != null)
            {
                Triangles = new List<Triangle>(source.Triangles.Count);
                foreach (var item in source.Triangles)
                    Triangles.Add(new Triangle(item));
            }
            AutoTriangulate = source.AutoTriangulate;
        }

        public Polygon(Polygon source, Vector2 offset)
        {
            Vertices = new List<Vector2>(source.n);
            foreach (var item in source.Vertices)
                Vertices.Add(item+offset);
            AutoTriangulate = source.AutoTriangulate;
            Triangulate();
        }

        public static Polygon BuildRectangle(RectangleF r)
        {
            return BuildRectangle(r.X, r.Y, r.Width, r.Height);
        }

        public static Polygon BuildRectangle(float x, float y, float width, float height)
        {
            Polygon result = new Polygon(4);
            result.AddVertex(x, y);
            result.AddVertex(x + width, y);
            result.AddVertex(x + width, y + height);
            result.AddVertex(x, y + height);
            return result;
        }

        public bool IsRectangle()
        {
            if (Vertices.Count != 4) return false;
            Vector2[] v = new Vector2[4] { Vertices[0], Vertices[1], Vertices[2], Vertices[3] };
            /*
            return
                (Vertices[0].X == Vertices[1].X || Vertices[0].X == Vertices[2].X || Vertices[0].X == Vertices[3].X) && //Check for 90 deg
                (Vertices[0] - Vertices[1]).LengthSquared() == (Vertices[2] - Vertices[3]).LengthSquared() &&
                (Vertices[1] - Vertices[2]).LengthSquared() == (Vertices[3] - Vertices[0]).LengthSquared();*/
            return
                (v[0].X == v[1].X || v[0].X == v[2].X || v[0].X == v[3].X) && //Check for 90 deg
                (v[0] - v[1]).LengthSquared == (v[2] - v[3]).LengthSquared &&
                (v[1] - v[2]).LengthSquared == (v[3] - v[0]).LengthSquared;
        }

        public static Polygon BuildCircle(int vertices, Vector2 center, float radius)
        {
            Polygon result = new Polygon(vertices);
            for (double i = 0; i < MathHelper.TwoPi; i += (float)MathHelper.TwoPi / (float)vertices)
            {
                result.AddVertex(center + radius * new Vector2((float)(Math.Cos(i)), (float)(Math.Sin(i))));
            }
            return result;
        }

        public void AddVertex(Vector2 v)
        {
            Vertices.Add(v);
            if (AutoTriangulate) Triangulate();
        }

        public void AddVertex(float x, float y)
        {
            AddVertex(new Vector2(x, y));
            if (AutoTriangulate) Triangulate();
        }

        public virtual Polygon Translate(Vector2 amount)
        {
            for (int i = 0; i < n; i++)
                Vertices[i] += amount;
            if (AutoTriangulate)
            {
                if (Triangles != null)
                    for (int i = 0; i < Triangles.Count; i++)
                    {
                        Triangles[i].Offset(amount);
                    }
                else Triangulate();
            }
            return this;
        }
        
        public virtual Polygon Scale(Vector2 amount)
        {
            for (int i = 0; i < n; i++)
                Vertices[i] = Vector2.Multiply(Vertices[i], amount);

            if (AutoTriangulate && Triangles != null)
            {
                if (Triangles != null)
                    for (int i = 0; i < Triangles.Count; i++)
                    {
                        Triangles[i].Scale(amount);
                    }
                else Triangulate();
            }
            return this;
        }

        public virtual void Resize(Vector2 newSize, bool dontAllowZero = true)
        {
            if (newSize.X <= _epsilon || newSize.Y <= _epsilon) return;

            Vector2 pos;
            Vector2 size;
            GetPositionAndSize(out pos, out size);
            
            Vector2 scale = Vector2.Divide (newSize, size);
            for (int i = 0; i < n; i++)
            {
                Vertices[i] -= pos;
                Vertices[i] = Vector2.Multiply(Vertices[i], scale);
                Vertices[i] += pos;
            }

            if (AutoTriangulate) Triangulate();
        }

        public virtual void FlipY()
        {
            for (int i = 0; i < n; i++)
                Vertices[i] = new Vector2 (Vertices[i].X, -Vertices[i].Y);
        }

        public Vector2 GetPosition()
        {
            Vector2 pos = new Vector2(float.MaxValue, float.MaxValue);
            foreach (var item in Vertices)
            {
                if (pos.X > item.X) pos.X = item.X;
                if (pos.Y > item.Y) pos.Y = item.Y;
            }
            return pos;
        }

        public RectangleF GetBoundingRect()
        {
            Vector2 pos, size;
            GetPositionAndSize(out pos, out size);
            return
                //Geometry2D.Vectors2Rectangle(pos, size);
                new RectangleF((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
        }

        public void GetPositionAndSize(out Vector2 pos, out Vector2 size)
        {
            pos = GetPosition();
            Vector2 big = new Vector2(float.MinValue, float.MinValue);
            foreach (var item in Vertices)
            {
                if (big.X < item.X) big.X = item.X;
                if (big.Y < item.Y) big.Y = item.Y;
            }
            size = big - pos;
        }

        public List<LineSegment> GetEdges()
        {
            List<LineSegment> result = new List<LineSegment>(n-1);
            for (int p = n - 1, q = 0; q < n; p = q++) result.Add(new LineSegment(Vertices[p], Vertices[q]));
            return result;
        }

        public virtual float GetArea()
        {
            float a = 0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
                a += Vertices[p].X * Vertices[q].Y - Vertices[q].X * Vertices[p].Y; //Cross Product
            return (a * 0.5f);
        }

        public virtual List<Vector2> GetVerticesCounterClockwise()
        {
            if (GetArea() < 0f || Vertices.Count == 1) return Vertices;
            else
            {
                var V = new List<Vector2>(n);
                for (int i = n - 1; i >= 0; i--)
                    V.Add(Vertices[i]);
                return V;
            }
        }

        public virtual List<Vector2> GetVerticesClockwise()
        {
            if (GetArea() > 0f || Vertices.Count == 1) return Vertices;
            else
            {
                var V = new List<Vector2>(n);
                for (int i = n - 1; i >= 0; i--)
                    V.Add(Vertices[i]);
                return V;
            }
        }

        public bool IsConvex()
        {
            int turn = (int)GeometryHelper.WhichSide(Vertices[0], Vertices[1], Vertices[2]);
            for (int i = 1; i < Vertices.Count; i++)
            {
                int j = (i + 1) % Vertices.Count;
                int k = (j + 1) % Vertices.Count;
                int nextTurn = (int)GeometryHelper.WhichSide(Vertices[i], Vertices[j], Vertices[k]);
                if (nextTurn != 0)
                {
                    if (turn != 0 && turn == -nextTurn)
                        return false;
                    nextTurn = turn;
                }
            }
            return true;
        }
    }
}
