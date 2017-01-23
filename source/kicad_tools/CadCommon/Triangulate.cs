using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

namespace CadCommon
{
    class Triangulate
    {
        public List<Point3DF> Points;

        public Triangulate()
        {
            Points = new List<Point3DF>();
        }

#if NEW
        void process (ref List<int> indices)
        {
            int n = Points.Count;
            if (n < 3)
                return;
            int [] V = new int [n];
            if (0.0f < _area())
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }
            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if (0 >= (count--))
                    return;

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (_snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            
        }

        ///
        ///     Returns the area of the contour
        ///
        float _area()
        {
            int n = Points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Point3DF pval = Points[p];
                Point3DF qval = Points[q];
                A += pval.X * qval.Y - qval.X * pval.Y;
            }
            return(A * 0.5f);
        }

        bool _snip(int u, int v, int w, int n, int [] V)
        {
            int p;
            Point3DF A = Points[V[u]];
            Point3DF B = Points[V[v]];
            Point3DF C = Points[V[w]];
            if (C_EPSILON > (((B.X - A.X) * (C.Y - A.Y)) - ((B.Y - A.Y) * (C.x - A.x))) )
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Point3DF P = Points.get(V[p]);
                if (_insideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        ///
        ///     Tests if a point is inside the given triangle
        ///
        bool CTriangulator::_insideTriangle(const Point3DF& A, const Point3DF& B, const Point3DF& C,
            const Point3DF& P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x;  ay = C.Y - B.Y;
            bx = A.x - C.x;  by = A.Y - C.Y;
            cx = B.x - A.x;  cy = B.Y - A.Y;
            apx = P.x - A.x;  apy = P.Y - A.Y;
            bpx = P.x - B.x;  bpy = P.Y - B.Y;
            cpx = P.x - C.x;  cpy = P.Y - C.Y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
#endif

#if XXX
        public List<TriangleExt> Triangles;

        public void Triangulate()
        {
            Triangles = new List<TriangleExt>();

            if (n < 3) return;
            var V = new Polygon(GetVerticesClockwise()).Vertices;

            if (n == 3)
            {
                Triangles.Add(new TriangleExt(V[0], V[1], V[2]));
                return;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2; )
            {
                if (0 >= (count--))
                    return;

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (TestTriangle(u, v, w, V))
                {
                    Triangles.Add(new TriangleExt(V[u], V[v], V[w]));
                    m++;
                    V.RemoveAt(v);
                    nv--;
                    count = 2 * nv;
                }
            }
        }

        bool TestTriangle(int u, int v, int w, List<Vector2> V)
        {
            TriangleExt t = new TriangleExt(V[u], V[v], V[w]);
            if (((t.B.X - t.A.X) * (t.C.Y - t.A.Y)) - ((t.B.Y - t.A.Y) * (t.C.X - t.A.X)) < _epsilon)
                return false;

            for (int p = 0; p < V.Count; p++)
            {
                if ((V[p] != t.A) && (V[p] != t.B) && (V[p] != t.C))
                {
                    if (t.IsInside(V[p]))
                        return false;
                }
            }

            return true;
        }

        public bool IsInside(Vector2 p)
        {
            TriangleExt t = new TriangleExt();
            return IsInside(p, out t);
        }

        public bool IsInside(Vector2 p, out TriangleExt triangle)
        {
            triangle = null;
            foreach (var item in Triangles)
            {
                if (item.IsInside(p))
                {
                    triangle = item;
                    return true;
                }
            }
            return false;
        }
#endif

    }
}