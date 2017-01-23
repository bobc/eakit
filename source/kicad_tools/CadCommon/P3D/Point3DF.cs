using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CadCommon
{
    // equivalent to AMF <coordinates>
    // Like OpenGL Vector3f

    public class Point3DF
    {
        [XmlElement("x")]
        public float X;

        [XmlElement("y")]
        public float Y;
        
        [XmlElement("z")]
        public float Z;

        public Point3DF()
        { }

        public Point3DF(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", X, Y, Z);
        }

        public Point3DF Scale(float scaleX, float scaleY, float scaleZ)
        {
            Point3DF result = new Point3DF();
            result.X = this.X * scaleX;
            result.Y = this.Y * scaleY;
            result.Z = this.Z * scaleZ;

            return result;
        }

        public Point3DF Scale(Point3DF scale)
        {
            Point3DF result = new Point3DF();
            result.X = this.X * scale.X;
            result.Y = this.Y * scale.Y;
            result.Z = this.Z * scale.Z;

            return result;
        }
    }


}
