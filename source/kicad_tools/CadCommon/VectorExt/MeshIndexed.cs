using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

using OpenTK;

namespace CadCommon
{
    // container for mesh
    public class MeshIndexed
    {
        public float DisplayScale;

        public string Name;

        public List<Vector3Ext> Vertices;
        public List<GlPolyIndex> Polygons;
        public Attributes Attributes;

        public Point3DF Rotation
        {
            set { rotationMatrix = CalculateRotation(value); }
        }

        //
        private Matrix4 rotationMatrix;

        //
        public MeshIndexed()
        {
            this.Vertices = new List<Vector3Ext>();
            this.Polygons = new List<GlPolyIndex>();
            this.Attributes = new Attributes();
            this.DisplayScale = 1.0f;

            this.Rotation = new Point3DF(0, 0, 0);
        }

        public Matrix4 CalculateRotation(Point3DF rotation)
        {
            Matrix4 rotX = Matrix4.CreateRotationX(rotation.X);
            Matrix4 rotY = Matrix4.CreateRotationY(rotation.Y);
            Matrix4 rotZ = Matrix4.CreateRotationZ(rotation.Z);

            Matrix4.Mult(ref rotX, ref rotY, out rotationMatrix);

            return Matrix4.Mult(rotationMatrix, rotZ);
        }

        public void CalculateExtent(out Vector3 min, out Vector3 max)
        {
            min = new Vector3();
            max = new Vector3();
            foreach (Vector3Ext vector in Vertices)
            {
                if (vector.Position.X < min.X)
                    min.X = vector.Position.X;
                if (vector.Position.X > max.X)
                    max.X = vector.Position.X;

                if (vector.Position.Y < min.Y)
                    min.Y = vector.Position.Y;
                if (vector.Position.Y > max.Y)
                    max.Y = vector.Position.Y;

                if (vector.Position.Z < min.Z)
                    min.Z = vector.Position.Z;
                if (vector.Position.Z > max.Z)
                    max.Z = vector.Position.Z;
            }
        }


        public void AddTriangle(TriangleExt triangle)
        {
            TriangleIndex triangleIndex;

            int vertIndex = Vertices.Count;

            //if (triangle.HasColor)
            //    color = triangle.Color;
            //else
            //    color = this.Color;

            Vertices.Add(triangle.vertex1);
            Vertices.Add(triangle.vertex2);
            Vertices.Add(triangle.vertex3);

            triangleIndex = new TriangleIndex(vertIndex, vertIndex + 1, vertIndex + 2);

            // copy atributes
            if (triangle.Attributes.HasColor)
                triangleIndex.Attributes.Color = triangle.Attributes.Color;

            Polygons.Add(triangleIndex);
        }

        public void AddPolygon (Polygon polygon)
        {
            GlPolyIndex index;

            int vertIndex = Vertices.Count;

            foreach (Vector3Ext vector in polygon.vertices)
                Vertices.Add(vector);

            index = new GlPolyIndex();
            index.PointIndex = new int[polygon.vertices.Length];

            for (int j = 0; j < polygon.vertices.Length; j++)
                index.PointIndex[j] = vertIndex + j;

            Polygons.Add(index);
        }

        /// <summary>
        /// Add polygon from points. Points must be on a plane and
        /// form a convec ploygon.
        /// </summary>
        /// <param name="points"></param>
        public void AddPolygon(List<Point3DF> points)
        {
            GlPolyIndex index;

            int vertIndex = Vertices.Count;

            foreach (Point3DF vector in points)
                Vertices.Add(new Vector3Ext (vector));

            index = new GlPolyIndex();
            index.PointIndex = new int[points.Count];

            for (int j = 0; j < points.Count; j++)
                index.PointIndex[j] = vertIndex + j;

            Polygons.Add(index);
        }


        public void Translate(float x, float y, float z)
        {
            Vector3 Translation = new Vector3 (x,y,z);

            for (int index = 0; index < Vertices.Count; index++)
            {
                Vertices[index].Position.Add(Translation);
            }
        }

        public void RotateX(float angle)
        {
            Matrix4 rotate = Matrix4.CreateRotationX(angle);

            for (int index = 0; index < Vertices.Count; index++)
            {
                Vertices[index].Position = Mult(ref rotate, Vertices[index].Position);
            }
        }

        public void RotateY(float angle)
        {
            Matrix4 rotate = Matrix4.CreateRotationY(angle);

            for (int index = 0; index < Vertices.Count; index++)
            {
                Vertices[index].Position = Mult(ref rotate, Vertices[index].Position);
            }
        }

        public void RotateZ(float angle)
        {
            Matrix4 rotate = Matrix4.CreateRotationZ(angle);

            for (int index = 0; index < Vertices.Count; index++)
            {
                Vertices[index].Position = Mult(ref rotate, Vertices[index].Position);
            }
        }

        public Vector3 Mult(ref Matrix4 matrix, Point3DF vector)
        {
            Vector3 result = new Vector3();
            result.X = matrix.M11 * vector.X + matrix.M12 * vector.Y + matrix.M13 * vector.Z;
            result.Y = matrix.M21 * vector.X + matrix.M22 * vector.Y + matrix.M23 * vector.Z;
            result.Z = matrix.M31 * vector.X + matrix.M32 * vector.Y + matrix.M33 * vector.Z;

            return result;
        }

        public Vector3 Mult(ref Matrix4 matrix, Vector3 vector)
        {
            Vector3 result = new Vector3();
            result.X = matrix.M11 * vector.X + matrix.M12 * vector.Y + matrix.M13 * vector.Z;
            result.Y = matrix.M21 * vector.X + matrix.M22 * vector.Y + matrix.M23 * vector.Z;
            result.Z = matrix.M31 * vector.X + matrix.M32 * vector.Y + matrix.M33 * vector.Z;

            return result;
        }

        public void AddFacets(List<Facet> facets)
        {
            foreach (Facet facet in facets)
                AddFacet(facet);
        }

        public void AddFacet(Facet facet)
        {
            int vertIndex = Vertices.Count;
            GlPolyIndex polyIndex = new GlPolyIndex();

            foreach (Point3DF point in facet.Points)
            {
                //
                Vector3Ext vector = new Vector3Ext(point);

                vector.Position = Mult(ref rotationMatrix, point);
                Vertices.Add(vector);
            }

            polyIndex.PointIndex = new int[facet.Points.Length];
            for (int j = 0; j < facet.Points.Length; j++)
            {
                polyIndex.PointIndex[j] = vertIndex + j;
            }

            Polygons.Add(polyIndex);
        }

        public void AddFacet(Facet facet, Color color)
        {
            int vertIndex = Vertices.Count;
            GlPolyIndex polyIndex = new GlPolyIndex();

            foreach (Point3DF point in facet.Points)
            {
                //
                Vector3Ext vector = new Vector3Ext(point);

                vector.Position = Mult(ref rotationMatrix, point);
                Vertices.Add(vector);
            }

            polyIndex.PointIndex = new int[facet.Points.Length];
            for (int j = 0; j < facet.Points.Length; j++)
            {
                polyIndex.PointIndex[j] = vertIndex + j;
            }

            polyIndex.Attributes.Color = color;
            Polygons.Add(polyIndex);
        }

        public void AddMesh(MeshIndexed sourceMesh, Color color)
        {
            int vertIndex = Vertices.Count;

            foreach (Vector3Ext point in sourceMesh.Vertices)
            {
                //
                Vector3Ext vector = new Vector3Ext(point.Position);

                vector.Position = Mult(ref rotationMatrix, point.Position);
                Vertices.Add(vector);
            }

            foreach (GlPolyIndex poly in sourceMesh.Polygons)
            {
                GlPolyIndex polyIndex = new GlPolyIndex();

                polyIndex.PointIndex = new int[poly.VertexCount];
                for (int j = 0; j < poly.VertexCount; j++)
                {
                    polyIndex.PointIndex[j] = vertIndex + poly.PointIndex[j];
                }

                if (poly.Attributes.HasColor)
                    polyIndex.Attributes.Color = poly.Attributes.Color;
                else
                    polyIndex.Attributes.Color = color;

                Polygons.Add(polyIndex);
            }
        }

        public void AddToMesh(IndexedFaceSet faces, MaterialProperties material)
        {
            //TODO: triangle (done?)
            foreach (PolyIndex poly in faces.Polygons)
            {
                this.AddFacet(poly.GetFacet(faces.Points), material.GetColor());
            }
        }

    }
}
