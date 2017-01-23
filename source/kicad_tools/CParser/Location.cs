using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CParser
{
    public class Location
    {
        public string FileName;
        public int Line;        // line number, 1 based
        public int Column;      // column, 1 based

        public Location()
        {
            FileName = "";
            Line = 1;
            Column = 1;
        }

        public Location(string FileName, int Line, int Column)
        {
            this.FileName = FileName;
            this.Line = Line;
            this.Column = Column;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Path.GetFileName(FileName), Line);
        }
    }
}
