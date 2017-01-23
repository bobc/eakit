using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace CadCommon
{
    public class Shapes3D
    {
        // 3d shapes

        public static MeshIndexed MakeCube(float x, float y, float z, Color color)
        {
            MeshIndexed result = new MeshIndexed();
            Point3DF[] points = new Point3DF[8];

            // create vertexes
            points[0] = new Point3DF(0, 0, 0);
            points[1] = new Point3DF(0, y, 0);
            points[2] = new Point3DF(x, y, 0);
            points[3] = new Point3DF(x, 0, 0);

            points[4] = new Point3DF(0, 0, z);
            points[5] = new Point3DF(0, y, z);
            points[6] = new Point3DF(x, y, z);
            points[7] = new Point3DF(x, 0, z);

            foreach (Point3DF point in points)
                result.Vertices.Add(new Vector3Ext(point));

            // base
            result.AddFacet(new Facet(points[0], points[3], points[2]), color);
            result.AddFacet(new Facet(points[2], points[1], points[0]), color);

            // sides
            result.AddFacet(new Facet(points[0], points[4], points[3]), color);
            result.AddFacet(new Facet(points[3], points[4], points[7]), color);

            result.AddFacet(new Facet(points[0], points[1], points[5]), color);
            result.AddFacet(new Facet(points[5], points[4], points[0]), color);

            result.AddFacet(new Facet(points[2], points[6], points[5]), color);
            result.AddFacet(new Facet(points[5], points[1], points[2]), color);

            result.AddFacet(new Facet(points[7], points[6], points[2]), color);
            result.AddFacet(new Facet(points[2], points[3], points[7]), color);

            // top
            result.AddFacet(new Facet(points[4], points[5], points[6]), color);
            result.AddFacet(new Facet(points[6], points[7], points[4]), color);

            result.Translate(-x / 2, -y / 2, 0);

            return result;
        }

        // cuboid with hole
        //
        //      _   _
        //     | \ / |
        //     | | | |
        //     | |_| |
        //     |_____|
        //
        public static MeshIndexed MakeBucket(float x, float y, float z)
        {
            MeshIndexed result = new MeshIndexed();

            float hole = 1f;
            float bevel = 0.3f;

            List<CadCommon.Polygon> PolyList = new List<CadCommon.Polygon>();

            CadCommon.Polygon poly0 = new CadCommon.Polygon(new List<Point3DF> {
                new Point3DF(0, 0, 0),
                new Point3DF(0, y, 0),
                new Point3DF(x, y, 0),
                new Point3DF(x, 0, 0) });

            //PolyList.Add ();
            CadCommon.Polygon poly1 = new CadCommon.Polygon(new List<Point3DF> {
                new Point3DF(0, 0, z),
                new Point3DF(0, y, z),
                new Point3DF(x, y, z),
                new Point3DF(x, 0, z) });

            float d1 = x / 2 - hole / 2 - bevel;

            CadCommon.Polygon poly2 = new CadCommon.Polygon(new List<Point3DF> {
                new Point3DF(d1, d1, z),
                new Point3DF(d1, y - d1, z),
                new Point3DF(x - d1, y - d1, z),
                new Point3DF(x - d1, d1, z) });

            d1 = x / 2 - hole / 2;

            CadCommon.Polygon poly3 = new CadCommon.Polygon(new List<Point3DF> {
            new Point3DF(d1, d1, z-bevel),
            new Point3DF(d1, y - d1, z - bevel),
            new Point3DF(x - d1, y - d1, z - bevel),
            new Point3DF(x - d1, d1, z - bevel) });

            float z1 = 2;
            CadCommon.Polygon poly4 = new CadCommon.Polygon(new List<Point3DF> {
                new Point3DF(d1, d1, z1),
                new Point3DF(d1, y - d1, z1),
                new Point3DF(x - d1, y - d1, z1),
                new Point3DF(x - d1, d1, z1) });

            // base
            result.AddPolygon(poly0);

            // sides
            Functions3D.StitchPolygons3D(result, new List<CadCommon.Polygon> { poly0, poly1, poly2, poly3, poly4 });

            result.Translate(-x / 2, -y / 2, 0);

            return result;
        }

    }
}
