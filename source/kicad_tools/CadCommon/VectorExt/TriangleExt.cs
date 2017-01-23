using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

using OpenTK;

namespace CadCommon
{
    public class TriangleExt : Polygon
    {
        public Vector3Ext vertex1
        {
            get { return vertices[0]; }
            set { vertices[0] = value; }
        }
        public Vector3Ext vertex2
        {
            get { return vertices[1]; }
            set { vertices[1] = value; }
        }
        public Vector3Ext vertex3
        {
            get { return vertices[2]; }
            set { vertices[2] = value; }
        }

        //
        public TriangleExt()
        {
            vertices = new Vector3Ext[3];
        }

        public TriangleExt(Vector3Ext v1, Vector3Ext v2, Vector3Ext v3)
        {
            vertices = new Vector3Ext[3];
            this.vertex1 = new Vector3Ext(v1);
            this.vertex2 = new Vector3Ext(v2);
            this.vertex3 = new Vector3Ext(v3);
        }

        public TriangleExt(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            vertices = new Vector3Ext[3];
            this.vertex1 = new Vector3Ext(v1);
            this.vertex2 = new Vector3Ext(v2);
            this.vertex3 = new Vector3Ext(v3);
        }

        public TriangleExt(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            vertices = new Vector3Ext[3];
            this.vertex1 = new Vector3Ext(v1);
            this.vertex2 = new Vector3Ext(v2);
            this.vertex3 = new Vector3Ext(v3);
            this.Attributes.Color = color;
        }
    }
}
