using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using LibTessDotNet;

namespace Cad2D
{
    public class PolygonTesselator
    {
        // The data array contains 4 values, it's the associated data of the vertices that resulted in an intersection.
        private static object VertexCombine(LibTessDotNet.Vec3 position, object[] data, float[] weights)
        {
            // Fetch the vertex data.
            var colors = new Color[] { (Color)data[0], (Color)data[1], (Color)data[2], (Color)data[3] };
            // Interpolate with the 4 weights.
            var rgba = new float[] {
                colors[0].R * weights[0] + colors[1].R * weights[1] + colors[2].R * weights[2] + colors[3].R * weights[3],
                colors[0].G * weights[0] + colors[1].G * weights[1] + colors[2].G * weights[2] + colors[3].G * weights[3],
                colors[0].B * weights[0] + colors[1].B * weights[1] + colors[2].B * weights[2] + colors[3].B * weights[3],
                colors[0].A * weights[0] + colors[1].A * weights[1] + colors[2].A * weights[2] + colors[3].A * weights[3]
            };
            // Return interpolated data for the new vertex.
            return Color.FromArgb((int)rgba[3], (int)rgba[0], (int)rgba[1], (int)rgba[2]);
        }


        public static Cad2D.Polygon Tesselate(List <Cad2D.Polygon> Polygons)
        {

            // Create an instance of the tessellator. Can be reused.
            var tess = new LibTessDotNet.Tess();

            // Construct the contours from input polygons

            // A polygon can be composed of multiple contours which are all tessellated at the same time.

            foreach (Cad2D.Polygon poly in Polygons)
            {
                int numPoints = poly.Vertices.Count;

                var contour = new LibTessDotNet.ContourVertex[numPoints];
                for (int i = 0; i < numPoints; i++)
                {
                    // NOTE : Z is here for convenience if you want to keep a 3D vertex position throughout the tessellation process but only X and Y are important.
                    contour[i].Position = new LibTessDotNet.Vec3 { X = poly.Vertices[i].X, Y = poly.Vertices[i].Y, Z = 0.0f };

                    // Data can contain any per-vertex data, here a constant color.
                    contour[i].Data = Color.Azure;
                }

                // Add the contour with a specific orientation, use "Original" if you want to keep the input orientation.
                tess.AddContour(contour, LibTessDotNet.ContourOrientation.Clockwise);
            }


            // Tessellate!
            // The winding rule determines how the different contours are combined together.
            // See http://www.glprogramming.com/red/chapter11.html (section "Winding Numbers and Winding Rules") for more information.
            // If you want triangles as output, you need to use "Polygons" type as output and 3 vertices per polygon.
            tess.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3, VertexCombine);

            // Same call but the last callback is optional. Data will be null because no interpolated data would have been generated.
            //tess.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3); // Some vertices will have null Data in this case.

            Cad2D.Polygon result = new Cad2D.Polygon();

            int numTriangles = tess.ElementCount;
            result.Triangles = new List<Cad2D.Triangle>();
            for (int i = 0; i < numTriangles; i++)
            {
                var v0 = tess.Vertices[tess.Elements[i * 3]].Position;
                var v1 = tess.Vertices[tess.Elements[i * 3 + 1]].Position;
                var v2 = tess.Vertices[tess.Elements[i * 3 + 2]].Position;

                result.Triangles.Add (new Cad2D.Triangle (v0.X, v0.Y, v1.X, v1.Y, v2.X, v2.Y));
            }

            return result;
        }

    }
}
