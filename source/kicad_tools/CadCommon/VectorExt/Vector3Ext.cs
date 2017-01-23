using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

using OpenTK;

namespace CadCommon
{
    // extension of Vector3f
    public class Vector3Ext
    {
        public Vector3 Position;
        public Attributes Attributes;

        //

        public Vector3Ext(float x, float y, float z)
        {
            this.Position = new Vector3(x, y, z);
            this.Attributes = new Attributes();
        }

        public Vector3Ext(float x, float y, float z, Color color)
        {
            this.Position = new Vector3(x, y, z);
            this.Attributes = new Attributes();
            this.Attributes.Color = color;
        }

        //
        public Vector3Ext(Vector3Ext vector)
        {
            this.Position = vector.Position;
            this.Attributes = new Attributes();
        }

        public Vector3Ext (Vector3Ext vector, Color color)
        {
            this.Position = vector.Position;
            this.Attributes = new Attributes();
            this.Attributes.Color = color;
        }

        //
        public Vector3Ext(Vector3 vector)
        {
            this.Attributes = new Attributes();
            this.Position = new Vector3(vector.X, vector.Y, vector.Z);
        }

        public Vector3Ext(Vector3 vector, Color color)
        {
            this.Position = new Vector3(vector.X, vector.Y, vector.Z);
            this.Attributes = new Attributes();
            this.Attributes.Color = color;
        }

        //
        public Vector3Ext(Point3DF point)
        {
            this.Attributes = new Attributes();
            this.Position = new Vector3(point.X, point.Y, point.Z);
        }

        //
        static uint ToRgba(Color color)
        {
            return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
        }
    }
}
