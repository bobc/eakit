using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{

    // schematic sheet elements

    public class NetListSheet
    {
        public int Number;
        public string Name;
        public string Tstamps;
        public TitleBlock Title_block;

    }

    public class TitleBlock
    {
        public string Title;
        public string Company;
        public string Rev;
        public string Date;
        public string Source;
        public List<Comment> Comment;
    }

    public class Comment
    {
        public int Number;
        public string Value;
    }
}
