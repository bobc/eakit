using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CParser
{
    public class ParsingContext
    {
        public string ModuleName;

        public List<string> Lines;
        public string Line;
        public char CurCh;

        //todo: replace with Location
        public string FileName;
        public int LineIndex;  // 0 based
        public int Pos;        // 0 based
        // public Token CurToken;

        public bool InDirective;
    }
}
