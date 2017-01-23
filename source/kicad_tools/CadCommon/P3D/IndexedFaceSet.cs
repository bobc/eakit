using System;
using System.Collections.Generic;
using System.Text;

namespace CadCommon
{
    // compatible with AMF (vertex.coordinates, triangles)
    // and VRML geometry.IndexedFaceSet

    // similar to OpenGL (Vector3[], TriangleIndexed[])

    // == MeshIndexedTriangles

    public class IndexedFaceSet
    {
        public List<Point3DF> Points;  // all the points used

        // polygon facets
        public List<PolyIndex> Polygons;

        public IndexedFaceSet()
        {
            Points = new List<Point3DF>();
            Polygons = new List<PolyIndex>();
        }

        // create from a list of Facets (direct coords)
        // does not eliminate duplicate coords
        public IndexedFaceSet(List<Facet> Facets, float Scaling)
        {
            int pointIndex = 0;
            Points = new List<Point3DF>();
            Polygons = new List<PolyIndex>();

            foreach (Facet facet in Facets)
            {
                PolyIndex poly = new PolyIndex();

                // add points
                for (int j = 0; j < facet.Points.Length; j++)
                {
                    Points.Add(facet.Points[0].Scale(Scaling, Scaling, Scaling));
                    
                //    Points.Add(facet.Points[1].Scale(Scaling, Scaling, Scaling));
                //    Points.Add(facet.Points[2].Scale(Scaling, Scaling, Scaling));
                }

                Polygons.Add(new PolyIndex(pointIndex, pointIndex + 1, pointIndex + 2));

                pointIndex += 3;
            }
        }

        public IndexedFaceSet(List<Facet> Facets)
        {
            int pointIndex = 0;
            Points = new List<Point3DF>();
            Polygons = new List<PolyIndex>();

            foreach (Facet facet in Facets)
            {
                // add points
                Points.Add(facet.Points[0]);
                Points.Add(facet.Points[1]);
                Points.Add(facet.Points[2]);

                Polygons.Add(new PolyIndex(pointIndex, pointIndex + 1, pointIndex + 2));

                pointIndex += 3;
            }
        }


        public void Scale(Point3DF scale)
        {
            for (int j = 0; j < Points.Count; j++)
            {
                Points[j].X *= scale.X;
                Points[j].Y *= scale.Y;
                Points[j].Z *= scale.Z;
            }
        }

        public void Translate(Point3DF translation)
        {
            for (int j = 0; j < Points.Count; j++)
            {
                Points[j].X += translation.X;
                Points[j].Y += translation.Y;
                Points[j].Z += translation.Z;
            }
        }

    }
}
