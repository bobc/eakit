using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using Cad2D;


namespace CadCommon
{
    public class Path3DNode
    {
        public Cad2D.Polygon Poly2d
        {
            get { return Polygons[0]; }
        }
        public Vector3 Position;  // relative to current  
        public Vector3 Direction; // angles relative to current
        public Vector2 Scale;

        public List<Cad2D.Polygon> Polygons;

        public Path3DNode()
        {
            Polygons = new List<Cad2D.Polygon>();
        }

        public Path3DNode(Cad2D.Polygon poly2d, Vector3 position)
        {
            Polygons = new List<Cad2D.Polygon>();

            this.Polygons.Add(poly2d);
            this.Position = position;
            this.Direction = new Vector3(0, 0, 0);
            this.Scale = new Vector2(1, 1);
        }

        public Path3DNode(Cad2D.Polygon poly2d, Vector3 position, Vector3 direction)
        {
            Polygons = new List<Cad2D.Polygon>();

            this.Polygons.Add(poly2d);
            this.Position = position;
            this.Direction = direction;
            this.Scale = new Vector2(1, 1);
        }

        public Path3DNode(Cad2D.Polygon poly2d, Vector3 position, Vector3 direction, Vector2 scale)
        {
            Polygons = new List<Cad2D.Polygon>();

            this.Polygons.Add(poly2d);
            this.Position = position;
            this.Direction = direction;
            this.Scale = scale;
        }

        //

        public Path3DNode(List<Cad2D.Polygon> polygons)
        {
            this.Polygons = polygons;
            this.Position = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.Scale = new Vector2(1, 1);
        }

        public Path3DNode(List<Cad2D.Polygon> polygons, Vector3 position)
        {
            this.Polygons = polygons;
            this.Position = position;
            this.Direction = new Vector3(0, 0, 0);
            this.Scale = new Vector2(1, 1);
        }

    }
}
