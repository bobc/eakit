using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Cad2D
{
    public class Shapes2D
    {
        // 2d shapes

        public static Cad2D.Polygon Square(float size)
        {
            Cad2D.Polygon poly = new Cad2D.Polygon();

            poly.Vertices.Add(new Vector2(0, 0));
            poly.Vertices.Add(new Vector2(0, size));
            poly.Vertices.Add(new Vector2(size, size));
            poly.Vertices.Add(new Vector2(size, 0));

            poly.Translate(new Vector2(-size / 2, -size / 2));
            return poly;
        }


        // A rect with truncated corners
        public static Cad2D.Polygon Rect(float x, float y, float chamfer)
        {
            Cad2D.Polygon poly2d = new Cad2D.Polygon();

            poly2d.Vertices.Add(new Vector2(0, chamfer));
            poly2d.Vertices.Add(new Vector2(0, y - chamfer));
            poly2d.Vertices.Add(new Vector2(chamfer, y));
            poly2d.Vertices.Add(new Vector2(x - chamfer, y));
            poly2d.Vertices.Add(new Vector2(x, y - chamfer));
            poly2d.Vertices.Add(new Vector2(x, chamfer));
            poly2d.Vertices.Add(new Vector2(x - chamfer, 0));
            poly2d.Vertices.Add(new Vector2(chamfer, 0));

            // center about (0,0)
            poly2d.Translate(new Vector2(-x / 2, -y / 2));
            return poly2d;
        }

        public static Cad2D.Polygon Rect(float x, float y)
        {
            Cad2D.Polygon poly = new Cad2D.Polygon();

            poly.Vertices.Add(new Vector2(0, 0));
            poly.Vertices.Add(new Vector2(0, y));
            poly.Vertices.Add(new Vector2(x, y));
            poly.Vertices.Add(new Vector2(x, 0));

            poly.Translate(new Vector2(-x / 2, -y / 2));
            return poly;
        }

        //  |\
        //  | >
        //  |/
        public static Cad2D.Polygon Triangle (float x, float y)
        {
            Cad2D.Polygon poly = new Cad2D.Polygon();

            poly.Vertices.Add(new Vector2(0, 0));
            poly.Vertices.Add(new Vector2(-x, y/2));
            poly.Vertices.Add(new Vector2(-x, -y/2));

            return poly;
        }


    }
}
