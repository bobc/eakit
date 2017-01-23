using System;
using System.Collections.Generic;
using System.Text;

namespace CadCommon
{
    public class FileProperties
    {
        public string FileName = "";

        public UnitsSpecification Units;

        //! public VRML.Material Material;
        public MaterialProperties Material;

        public Point3DF Rotation = new Point3DF();

        public FileProperties()
        {
            Units = new UnitsSpecification();
        }
    }
}
