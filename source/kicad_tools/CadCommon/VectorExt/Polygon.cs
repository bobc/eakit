using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

using Cad2D;

namespace CadCommon
{
    public class Polygon
    {
        public Vector3Ext[] vertices;
        public Attributes Attributes;

        public int VertexCount
        {
            get
            {
                if (vertices != null)
                    return vertices.Length;
                else
                    return 0;
            }
        }

        public Polygon()
        {
            this.Attributes = new Attributes();
        }

        public Polygon(Cad2D.Polygon polygon)
        {
            this.Attributes = new Attributes();
            this.vertices = new Vector3Ext[polygon.Vertices.Count];

            for (int index = 0; index < polygon.Vertices.Count; index++)
            {
                Vector2 vector = polygon.Vertices[index];
                this.vertices[index] = new Vector3Ext(vector.X, vector.Y, 0);
            }
        }

        public Polygon(List<Point3DF> points)
        {
            this.Attributes = new Attributes();
            this.vertices = new Vector3Ext[points.Count];
            //
            for (int index = 0; index < points.Count; index++)
            {
                this.vertices[index] = new Vector3Ext(points[index]);
            }
        }


        [Obsolete("deprecated")]
        public Polygon(Cad2D.Polygon polygon, float z)
        {
            this.Attributes = new Attributes();
            this.vertices = new Vector3Ext[polygon.Vertices.Count];

            for (int index=0; index < polygon.Vertices.Count; index++)
            {
                Vector2 vector = polygon.Vertices[index];
                this.vertices[index] = new Vector3Ext(vector.X, vector.Y, z);
            }
        }

        public void ReverseOrder()
        {
            Vector3Ext[] new_vert = new Vector3Ext[vertices.Length];

            for (int j = 0; j < vertices.Length; j++)
                new_vert[vertices.Length-1-j] = vertices[j];

            vertices = new_vert;
        }

        public virtual void RotateY(float angle)
        {
            double ang = Math.PI * angle / 180;

            for (int index = 0; index < vertices.Length; index++)
            {
                vertices[index] = new Vector3Ext(
                    (float)(vertices[index].Position.X * Math.Cos(ang) + vertices[index].Position.Z * Math.Sin(ang)),
                    vertices[index].Position.Y,
                    (float)(-vertices[index].Position.X * Math.Sin(ang) + vertices[index].Position.Z * Math.Cos(ang))
                    );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle">angle in degrees</param>
        public virtual void RotateX(float angle)
        {
            double ang = Math.PI * angle / 180;

            for (int index = 0; index < vertices.Length; index++)
            {
                vertices[index] = new Vector3Ext(
                    vertices[index].Position.X,
                    (float)(vertices[index].Position.Y * Math.Cos(ang) - vertices[index].Position.Z * Math.Sin(ang)),
                    (float)(vertices[index].Position.Y * Math.Sin(ang) + vertices[index].Position.Z * Math.Cos(ang))
                    );
            }
        }

        public void Translate (Vector3 v)
        {
            for (int index = 0; index < vertices.Length; index++)
            {
                vertices[index] = new Vector3Ext(Vector3.Add (vertices[index].Position, v));
            }

        }


    }
}
