using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Polygon_Triangulator.cs
//Written by Saeed Afshari (www.saeedoo.com)

//Based on "CTriangulator" class which can be found here (as of November 2010):
//      http://members.gamedev.net/ysaneya/Code/CTriangulator.cpp
//      http://members.gamedev.net/ysaneya/Code/CTriangulator.h

using OpenTK;
//using System.Numerics;

namespace Cad2D
{
    public partial class Polygon
    {
        public virtual void Triangulate()
        {
            Triangles = new List<Triangle>();

            if (n < 3) return;
            var V = new Polygon( GetVerticesClockwise() ).Vertices;
            
            if (n == 3)
            {
                Triangles.Add(new Triangle(V[0],V[1],V[2]));
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
                    Triangles.Add(new Triangle(V[u], V[v], V[w]));
                    m++;
                    V.RemoveAt(v);
                    nv--;
                    count = 2 * nv;
                }
            }
        }

        bool TestTriangle(int u, int v, int w, List<Vector2> V)
        {
            Triangle t = new Triangle(V[u], V[v], V[w]);
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

        public virtual bool IsInside(Vector2 p)
        {
            Triangle t;
            return IsInside(p, out t);
        }

        public bool IsInside(Vector2 p, out Triangle triangle)
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
    }
}