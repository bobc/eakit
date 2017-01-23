using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using RMC;

namespace Lexing
{
    public class GeneralLexer
    {
        public string Filename;
        public Token CurToken;
        public string LastError;

        // parameters, defaults for C style language
        public bool IgnoreEol = true;

        public string WhiteSpace = " \t\r\n\0";

        public string LineComment = @"\\";
        public string CommentBegin = "/*";
        public string CommentEnd = "*/";

        //public string IdentifierFirst = "[A-Za-z]";
        //public string IdentifierRest = "[A-Za-z0-9_]";
        //public string IntRegex = @"([+\-]?(([0-9]+)|(0[xX][0-9a-fA-F]+)))";

        public string IntPattern = "";
        // double, float

        // strings = ".."
        // c-style quoting

        //
        private ParsingContext CurContext;

        private Regex regInt;


        protected bool[] WhiteSpaceMap = new bool[128];
        protected bool[] IdentFirstMap = new bool[128];
        protected bool[] IdentRestMap = new bool[128];


        public GeneralLexer()
        {
            // setup typical defaults

            InitMap(ref WhiteSpaceMap);
            AddMap(ref WhiteSpaceMap, WhiteSpace);

            InitMap(ref IdentFirstMap);
            AddMapRegexStyle(ref IdentFirstMap, "[A-Za-z_]");

            InitMap(ref IdentRestMap);
            AddMapRegexStyle(ref IdentRestMap, @"[A-Za-z0-9_]");
        }

        public void SetIdentMap (string first, string rest)
        {
            InitMap(ref IdentFirstMap);
            AddMapRegexStyle(ref IdentFirstMap, first);

            InitMap(ref IdentRestMap);
            AddMapRegexStyle(ref IdentRestMap, rest);
        }

        public bool Initialise (string FileName)
        {
            ParsingContext NewContext;

            NewContext = new ParsingContext();
            NewContext.FileName = FileName;
            NewContext.ModuleName = Path.GetFileNameWithoutExtension(FileName);
            NewContext.LineIndex = 0;

            try
            {
                NewContext.Lines = File.ReadAllLines(FileName);
            }
            catch (Exception ex)
            {
                Error("loading file " + ex.Message, null);
                return false;
            }

            CurContext = NewContext;

            CurContext.Line = CurContext.Lines[CurContext.LineIndex];
            CurContext.Pos = 0;
            GetNextToken();

            return true;
        }

        public void Finish()
        {
        }

        public void ErrorAtCurToken(string s)
        {
            LastError = s + " at " + CurToken.Location.ToString();
        }

        private void Error(string Message, Token Token)
        {
            LastError = Message;
        }

        protected static void InitMap(ref bool[] CharMap)
        {
            for (int i = 0; i< CharMap.Length; i++)
            {
                CharMap[i] = false;
            }
        }

        protected static void InvertMap(ref bool[] CharMap)
        {
            for (int j=0; j < CharMap.Length; j++)
            {
                CharMap[j] = !CharMap[j];
            }
        }

        protected static void AddMap(ref bool[] CharMap, string s)
        {
            foreach (char c in s)
            {
                int i = (int)c;
                CharMap[i] = true;
            }
        }

        protected static void AddMapRange(ref bool[] CharMap, char from, char to)
        {
            for (int i = (int)from; i <= (int)to; i++)
            {
                CharMap[i] = true;
            }
        }


        protected static void AddMapRange(ref bool[] CharMap, int from, int to)
        {
            for (int i = from; i <= to; i++)
            {
                CharMap[i] = true;
            }
        }

        protected static string GetLiteral (ref string s)
        {
            string result = "";

            if (s.StartsWith ("'"))
            {
                s = s.Remove (0,1);

                while ((s.Length > 0) && (s[0] != '\''))
                {
                    result = result + s[0];
                    s = s.Remove(0, 1);
                }

                if ( (s.Length > 0) && (s[0] == '\'') )
                    s = s.Remove (0,1);
            }

            return result;
        }

        protected static void AddMapRanges(ref bool[] CharMap, string rangeSpec)
        {
            string [] ranges = rangeSpec.Split (',');
            int from, to;

            foreach (string s in ranges)
            {
                if (s.StartsWith("'"))
                {
                    string t = s;
                    string from_s = GetLiteral(ref t);
                    string to_s = from_s;
                    if (t.StartsWith("-"))
                    {
                        t = t.Remove(0, 1);
                        to_s = GetLiteral(ref t);
                    }

                    AddMapRange(ref CharMap, (int)from_s[0], (int)to_s[0]);
                }
                else
                {
                    if (s.IndexOf('-') == -1)
                    {
                        from = StringUtils.StringToInteger(s);
                        to = from;
                        AddMapRange(ref CharMap, from, to);
                    }
                    else
                    {
                        from = StringUtils.StringToInteger(StringUtils.Before (s, "-").Trim());
                        to = StringUtils.StringToInteger(StringUtils.After(s, "-").Trim());
                        AddMapRange(ref CharMap, from, to);
                    }
                }
            }            
        }

        protected static char GetChar (ref string s)
        {
            if (s.StartsWith("\\"))
            {
                s = s.Remove(0,1);
                char c = s[0];
                s = s.Remove(0,1);
                return c;
            }
            else
            {
                char c = s[0];
                s = s.Remove(0,1);
                return c;
            }
        }

        protected static void AddMapRegexStyle(ref bool[] CharMap, string regex)
        {
            // [ ... ]
            // a-z
            // a
            // \\ \- \[ \]
            // \s                      whitespace
            // \S                      non-whitespace

            while (regex.Length != 0)
            {
                if (regex.StartsWith("["))
                    regex = regex.Remove(0, 1);
                else if (regex.StartsWith("]"))
                    regex = regex.Remove(0, 1);
                else if (regex.StartsWith("\\"))
                {
                    regex = regex.Remove(0, 1);
                    if (regex.StartsWith("s") || regex.StartsWith("S") )
                    {
                        //  \t\r\n\0"
                        CharMap[(int)' '] = true;
                        CharMap[(int)'\t'] = true;
                        CharMap[(int)'\r'] = true;
                        CharMap[(int)'\n'] = true;
                        CharMap[(int)'\0'] = true;

                        if (regex.StartsWith("S"))
                            InvertMap(ref CharMap);
                        regex = regex.Remove(0, 1);
                    }
                    else 
                    {
                        CharMap[regex[0]] = true;
                        regex = regex.Remove(0, 1);
                    }
                }
                else
                {
                    char from_c = GetChar (ref regex);
                    char to_c = from_c;
                    if (regex.StartsWith("-"))
                    {
                        regex = regex.Remove(0, 1);
                        to_c = regex[0];
                        regex = regex.Remove(0, 1);
                    }
                    AddMapRange(ref CharMap, from_c, to_c);
                }
            }
        }

        // private

        private static bool InMap(bool[] CharMap, char c)
        {
            int i = (int)c;
            if (i > CharMap.Length)
                return false;
            else
                return CharMap[i];
        }

        // return true if char c is in string s
        private bool MatchChar(string s, char c)
        {
            return s.IndexOf(c) != -1;
        }

        private void SkipWhiteSpace(string Line, ref int Pos)
        {
            while ((Pos < Line.Length) && MatchChar(WhiteSpace, Line[Pos]))
            {
                Pos++;
            }
        }

        public bool IsIdentFirst(char c)
        {
            if ((byte)c < IdentFirstMap.Length)
                return InMap(IdentFirstMap, c);
            else
                // default is valid Ident char
                return true;
        }

        public bool IsIdentRest(char c)
        {
            if ((byte)c < IdentRestMap.Length)
                return InMap(IdentRestMap, c);
            else
                // default is valid Ident char
                return true;
        }

        /// <summary>
        /// GetToken reads tokens in a single line, stops at EOL
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public Token GetToken(string Line, ref int Pos)
        {
            // Match match;
            Token Token = new Token(TokenType.Comment, "");

            SkipWhiteSpace(Line, ref Pos);

            Token.Location.Column = Pos + 1;
            if (Pos < Line.Length)
            {
                // unary plus?
                // integer, float 
                if ( Char.IsDigit(Line[Pos]) ||
                     (Line[Pos] == '.') || 
                     (Line[Pos] == '-') && (Pos+1 < Line.Length) && Char.IsDigit (Line[Pos+1]))
                {
                    if (Line[Pos] == '-')
                    {
                        Token.Value = "-";
                        Pos++;
                    }

                    // TODO: invalid values?
                    Token.Type = TokenType.IntegerVal;
                    // includes 0x...                    
                    while ((Pos < Line.Length) && Char.IsLetterOrDigit(Line[Pos]))
                    {
                        Token.Value = Token.Value + Line[Pos];
                        Pos++;
                    }

                    // double/float ?
                    if ( (Pos < Line.Length) && (Line[Pos] == '.') )
                    {
                        Token.Type = TokenType.FloatVal;
                        Token.Value = Token.Value + Line[Pos];
                        Pos++;

                        // 123 . 123 e +- 123

                        while ((Pos < Line.Length) && (Char.IsDigit(Line[Pos])))
                        {
                            Token.Value = Token.Value + Line[Pos];
                            Pos++;
                        }
                    }

                    if ((Pos < Line.Length) && (Char.ToLower(Line[Pos]) == 'e') ) // or letters
                    {
                        Token.Type = TokenType.FloatVal;
                        Token.Value = Token.Value + Line[Pos];
                        Pos++;
                        
                        if ((Pos < Line.Length) && (Char.IsDigit(Line[Pos]) || Line[Pos] =='+' || Line[Pos] =='-')  )
                        {
                            Token.Value = Token.Value + Line[Pos];
                            Pos++;
                            while ((Pos < Line.Length) && Char.IsDigit(Line[Pos]))
                            {
                                Token.Value = Token.Value + Line[Pos];
                                Pos++;
                            }
                        }
                    }

                    if (Token.Type == TokenType.IntegerVal)
                    {
                        //Token.VarValue = new Variant(VariantType.Integer);
                        Token.IntValue = StringUtils.StringToInteger(Token.Value);
                    }
                    else
                    {
                        //Token.VarValue = new Variant(VariantType.Real);
                        Token.RealValue = StringUtils.StringToDouble(Token.Value);
                    }
                }

                // identifiers
                // else if (Char.IsLetter(Line[Pos]))
                else if (IsIdentFirst (Line[Pos]))
                {
                    Token.Type = TokenType.Name;

                    while ((Pos < Line.Length) && IsIdentRest (Line[Pos]) )
                    {
                        Token.Value = Token.Value + Line[Pos];
                        Pos++;
                    }
                    // dotted qualifiers e.g. record.field
                    //if ((Pos < Line.Length) && (Line[Pos] == '.'))
                    //{
                    //    Token.Value = Token.Value + Line[Pos];
                    //    Pos++;
                    //    if ((Pos < Line.Length) && Char.IsLetter(Line[Pos]))
                    //    {
                    //        while ((Pos < Line.Length) && (Char.IsLetterOrDigit(Line[Pos]) || (Line[Pos] == '_')))
                    //        {
                    //            Token.Value = Token.Value + Line[Pos];
                    //            Pos++;
                    //        }
                    //    }
                    //}

                }
                // string literals
                else if ((Line[Pos] == '"') || (Line[Pos] == '\''))
                {
                    char QuoteChar = Line[Pos];
                    Token.Type = TokenType.StringLiteral;
                    Pos++;

                    while ((Pos < Line.Length) && (Line[Pos] != QuoteChar))
                    {
                        Token.Value = Token.Value + Line[Pos];
                        Pos++;
                    }

                    if (Pos < Line.Length)
                        Pos++;

                    // C-style backslash escaping
                    Token.Value = StringUtils.DeEscapeCString(Token.Value);
                }
                else
                {
                    // symbols, C style operators

                    Token.Type = TokenType.Symbol;
                    Token.Value = Token.Value + Line[Pos];
                    Pos++;

                    // C++ style comments
                    if (Token.Value[0] == '/')
                    {
                        if ( (Pos < Line.Length) && (Line[Pos] == '/') && (LineComment == "//") )
                        {
                            // // line comment
                            Token.Type = TokenType.Comment;
                            Token.Value = StringUtils.Copy(Line, Pos - 1);
                            Pos = Line.Length;
                        }
                        else
                            Token.OperatorValue = Operator.Div;
                    }
                    else if ( (Token.Value[0] == '#') && (LineComment == "#"))
                    {
                        // pragmas?
                        // # line comment
                        Token.Type = TokenType.Comment;
                        Token.Value = StringUtils.Copy(Line, Pos - 1);
                        Pos = Line.Length;
                    }
                    // = or == C style equality
                    else if (Token.Value[0] == '=')
                    {
                        Token.OperatorValue = Operator.Equal;
                        if ((Pos < Line.Length) && (Line[Pos] == '='))
                        {
                            Token.Value = "==";
                            Pos++;
                        }
                    }
                    // ! or !=
                    else if (Token.Value[0] == '!')
                    {
                        if ((Pos < Line.Length) && (Line[Pos] == '='))
                        {
                            Token.OperatorValue = Operator.NotEqual;
                            Token.Value = "!=";
                            Pos++;
                        }
                        else
                            Token.OperatorValue = Operator.Not;

                    }
                    // < or <= or <<
                    else if (Token.Value[0] == '<')
                    {
                        if ((Pos < Line.Length) && (Line[Pos] == '='))
                        {
                            Token.OperatorValue = Operator.LessOrEqual;
                            Token.Value = "<=";
                            Pos++;
                        }
                        else if ((Pos < Line.Length) && (Line[Pos] == '<'))
                        {
                            Token.OperatorValue = Operator.LeftShift;
                            Token.Value = "<<";
                            Pos++;
                        }
                        else
                            Token.OperatorValue = Operator.LessThan;

                    }
                    // > or >= or >>
                    else if (Token.Value[0] == '>')
                    {
                        if ((Pos < Line.Length) && (Line[Pos] == '='))
                        {
                            Token.OperatorValue = Operator.GreaterOrEqual;
                            Token.Value = ">=";
                            Pos++;
                        }
                        else if ((Pos < Line.Length) && (Line[Pos] == '>'))
                        {
                            Token.OperatorValue = Operator.RightShift;
                            Token.Value = ">>";
                            Pos++;
                        }
                        else
                            Token.OperatorValue = Operator.GreaterThan;

                    }
                    // \ line continuation character
                    else if (Token.Value == "\\")
                    {
                        SkipWhiteSpace(Line, ref Pos);
                        if (Pos == Line.Length)
                        {
                            Token.Type = TokenType.LineBreak;
                        }
                    }
                    else
                    {
                        // test for single char operators
                        switch (Token.Value)
                        {
                            case "+":
                                Token.OperatorValue = Operator.Add;
                                break;
                            case "-":
                                Token.OperatorValue = Operator.Sub;
                                break;
                            case "*":
                                Token.OperatorValue = Operator.Mul;
                                break;
                            case "%":
                                Token.OperatorValue = Operator.Modulo;
                                break;
                            case "&":
                                Token.OperatorValue = Operator.BitAnd;
                                break;
                            case "|":
                                Token.OperatorValue = Operator.BitOr;
                                break;
                            case "^":
                                Token.OperatorValue = Operator.BitXor;
                                break;
                            case "[":
                                Token.OperatorValue = Operator.Index;
                                break;
                        }
                    }

                }

                SkipWhiteSpace(Line, ref Pos);
            }
            else
            {
                Token.Type = TokenType.EOL;
            }

            return Token;
        }

        /// <summary>
        /// GetNextToken tokenizes lines until EOF
        /// handles line continuation, skippig EOL
        /// </summary>
        public virtual void GetNextToken()
        {
            do
            {
                if (CurToken != null)
                {
                    if (CurToken.Type == TokenType.EOF)
                    {
                        CurToken = new Token(TokenType.EOF);
                        return;
                    }
                    else if ((CurToken.Type == TokenType.EOL) || (CurToken.Type == TokenType.LineBreak))
                    {
                        CurContext.LineIndex++;
                        if (CurContext.LineIndex >= CurContext.Lines.Length)
                        {
                            CurToken = new Token(TokenType.EOF);
                            return;
                        }
                        CurContext.Line = CurContext.Lines[CurContext.LineIndex];
                        CurContext.Pos = 0;
                    }
                }

                CurToken = GetToken(CurContext.Line, ref CurContext.Pos);

                CurToken.Location.FileName = CurContext.FileName;
                CurToken.Location.Line = CurContext.LineIndex + 1;

                // since a comment must be at end of a line, then treat Comment as Eol
                // if inline comments were allowed then we would just skip them.
                if (CurToken.Type == TokenType.Comment)
                {
                    CurToken.Type = TokenType.EOL;
                }
            }
            while ( (CurToken.Type == TokenType.LineBreak) || (IgnoreEol && (CurToken.Type == TokenType.EOL) ) );
        }


        public void SkipTo (string token)
        {
            while ( (CurToken.Type != TokenType.EOL) && (CurToken.Value != token) )
                GetNextToken();
        }
    }



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
            return string.Format("{0}({1}, {2})", Path.GetFileName(FileName), Line, Column);
        }
    }

    public enum TokenType { 
        None, 
        Comment,        // line or context
        Name,           // identifier, keyword
        IntegerVal,     // integer, decimal, hex etc
        FloatVal,       // 
        StringLiteral,  //
        ArrayLiteral,   // 
        Symbol,         // operators, any single-char special char?
        LineBreak,      // ?
        EOL,            // end of line
        EOF };

    public enum Operator
    {
        None,
        UnaryNegate,
        // + - * / % << >> & | ^
        Add, 
        Sub, 
        Mul, 
        Div, 
        Modulo, 
        LeftShift, 
        RightShift, 
        BitAnd, 
        BitOr, 
        BitXor,
        // and or 
        LogicalAnd, 
        LogicalOr, 
        Not,   // ! (boolean/bitwise)
        // = != < <= > >=
        Equal, 
        NotEqual, 
        LessThan, 
        LessOrEqual, 
        GreaterThan, 
        GreaterOrEqual, 
        Index   // "[" array index
    };

    public class Token 
    {
        public TokenType Type;
        public string Value;

        public int IntValue;
        public double RealValue;

        public Operator OperatorValue;

        public Location Location;

        public Token()
        {
            Type = TokenType.None;
            Value = "";
            IntValue = 0;
            OperatorValue = Operator.None;
            Location = new Location();
        }

        public Token(Token token)
        {
            this.Type = token.Type;
            this.Value = token.Value;
            this.IntValue = token.IntValue;
            this.RealValue = token.RealValue;
            this.OperatorValue = token.OperatorValue;
            this.Location = new Location(token.Location.FileName, token.Location.Line, token.Location.Column);
        }

        public Token(TokenType Type)
        {
            this.Type = Type;
            this.Value = "";
            this.IntValue = 0;
            this.RealValue = 0.0;
            this.OperatorValue = Operator.None;
            this.Location = new Location();
        }

        public Token(TokenType Type, string Value)
        {
            this.Type = Type;
            this.Value = Value;
            this.IntValue = 0;
            this.OperatorValue = Operator.None;
            this.Location = new Location();
        }

        public Token(TokenType Type, int IntValue)
        {
            this.Type = Type;
            this.Value = IntValue.ToString();
            this.IntValue = IntValue;
            this.RealValue = 0.0;
            this.OperatorValue = Operator.None;
            this.Location = new Location();
        }

        //public Token(TokenType Type, Variant Value)
        //{
        //    this.Type = Type;
        //    this.VarValue = Value;
        //    this.OperatorValue = Operator.None;
        //    this.Location = new Location();
        //}

        //public Token(Variant Value)
        //{
        //    this.VarValue = Value;
        //    switch (Value.Mode)
        //    {
        //        case VariantType.Integer:
        //            Type = TokenType.Number;
        //            this.IntValue = Value.IntValue;
        //            break;
        //        case VariantType.Real:
        //            Type = TokenType.Number;
        //            break;
        //        case VariantType.String:
        //            Type = TokenType.StringLiteral;
        //            this.Value = Value.StrValue;
        //            break;
        //        case VariantType.IntArray:
        //            Type = TokenType.ArrayLiteral;
        //            break;

        //    }
        //    this.OperatorValue = Operator.None;
        //    this.Location = new Location();
        //}

        public bool IsANumber()
        {
            return (Type == TokenType.IntegerVal) || (Type == TokenType.FloatVal);
        }

        public double GetValueAsDouble()
        {
            if (Type == TokenType.FloatVal)
                return RealValue;
            else if (Type == TokenType.IntegerVal)
                return IntValue;
            else
                return 0f;
        }

        public float AsFloat()
        {
            if (Type == TokenType.FloatVal)
                return (float)RealValue;
            else if (Type == TokenType.IntegerVal)
                return IntValue;
            else
                return 0f;
        }

        public int GetValueAsInt()
        {
            if (Type == TokenType.FloatVal)
                return (int)RealValue;
            else if (Type == TokenType.IntegerVal)
                return IntValue;
            else
                return 0;
        }

        public override string ToString()
        {
            string result;

            result = Type.ToString() + "<";

            if (Type == TokenType.IntegerVal)
            {
                result += IntValue.ToString();
            }
            else if (Type == TokenType.FloatVal)
                result += RealValue.ToString();
            else if (Type == TokenType.StringLiteral)
                result += '"' + Value + '"';
            else if (Type == TokenType.ArrayLiteral)
            {
                // return VarValue.ToString();
                result += "";
            }
            else
                result += Value;

            result += ">";

            return result;
        }
    }

    public class ParsingContext
    {
        public string [] Lines;
        public string Line;

        public string ModuleName;

        //todo: replace with Location
        public string FileName;
        public int LineIndex;  // 0 based
        public int Pos;        // 0 based
        // public Token CurToken;
    }

}
