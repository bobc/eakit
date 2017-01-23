using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RMC.Classes;

namespace CParser
{
    public class CLex
    {
        const char NUL_CHAR = '\0';
        const char EOL_CHAR = '\x0A';
        const char EOF_CHAR = '\x1A';   // 26

#if xxx
        string[] Lines;

        string CurLine;
        char CurCh;
        int LineNum;
        int Col;

        string Line;
        public int LineIndex;  // 0 based
        public int Pos;        // 0 based
#endif
        public bool IgnoreWhitespace;

        public bool IgnoreEol = false;

        public Token CurToken;

        // private
        ParsingContext CurContext;

        public CLex()
        {
        }

        public void Initialise(string Filename)
        {
            string[] lines = File.ReadAllLines(Filename);

            Initialise(lines);
        }

        public void Initialise(string[] Lines)
        {
            ParsingContext NewContext;

            NewContext = new ParsingContext();
            NewContext.FileName = "";
            NewContext.ModuleName = "";
            NewContext.LineIndex = 0;

            NewContext.Lines = Lines.ToList();

            CurContext = NewContext;

            CurContext.LineIndex = -1;
            CurContext.Line = "";
            CurContext.Pos = 1;

            GetNextChar();
            GetToken();
        }

        public void Scan (out List<Token> Tokens)
        {
            Tokens = new List<Token>();

            //
            ParseModule(ref Tokens);
        }

        // parsing functions
        private ScriptResult ParseModule(ref List<Token> Tokens)
        {
            ScriptResult Result = ScriptResult.Ok;

            while (CurToken.Type != TokenType.EOF)
            {
                Tokens.Add(CurToken);
                GetToken();
            }

            return Result;
        }


        //
        private string PrintChar (Char c)
        {
            if (c == EOL_CHAR)
                return "EOL";
            else if (c == EOF_CHAR)
                return "EOF";
            else
                return c.ToString();
        }

        // not used?
        public void GetNextToken()
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
                        if (CurContext.LineIndex >= CurContext.Lines.Count)
                        {
                            CurToken = new Token(TokenType.EOF);
                            return;
                        }
                        CurContext.Line = CurContext.Lines[CurContext.LineIndex];
                        CurContext.Pos = 0;
                    }
                }

//!                CurToken = GetToken(CurContext.Line, ref CurContext.Pos);

                CurToken.Location.FileName = CurContext.FileName;
                CurToken.Location.Line = CurContext.LineIndex + 1;

                // since a comment must be at end of a line, then treat Comment as Eol
                // if inline comments were allowed then we would just skip them.
                if (CurToken.Type == TokenType.Comment)
                {
                    CurToken.Type = TokenType.EOL;
                }
            }
            while (
                    (CurToken.Type == TokenType.LineBreak) ||
                    (IgnoreEol && (CurToken.Type == TokenType.EOL) )
                );
        }

        //
        public Token GetToken()
        {
            _GetToken();

            if (IgnoreWhitespace || IgnoreEol)
            {
                while ( (CurToken.Type != TokenType.EOF) &&
                        (
                            (IgnoreWhitespace && (CurToken.Type == TokenType.Whitespace) )
                            || (IgnoreEol && (CurToken.Type == TokenType.EOL) )
                        )
                       )
                {
                    _GetToken();
                }
            }

            return CurToken;
        }

        private Token _GetToken()
        {
            CurToken = new Token(TokenType.Comment, "");

            CurToken.Location.Column = CurContext.Pos + 1;

            if (CurContext.CurCh != EOF_CHAR)
            {
                if (CurContext.CurCh == '\n')
                {
                    CurContext.InDirective = false;
                    CurToken.Type = TokenType.EOL;
                    GetNextChar();

                }
                else if ((CurContext.CurCh == ' ') || (CurContext.CurCh == '\0') || (CurContext.CurCh == '\t'))
                {
                    CurToken.Type = TokenType.Whitespace;
                    while ((CurContext.CurCh == ' ') || (CurContext.CurCh == '\0') || (CurContext.CurCh == '\t'))
                    {
                        CurToken.Value = CurToken.Value + CurContext.CurCh;
                        GetNextChar();
                    }

                }
                //else if (Char.IsDigit(CurContext.CurCh))
                //{
                //    // TODO: invalid values?
                //    CurToken.Type = TokenType.Number;
                //    // includes 0x...                    
                //    while (Char.IsLetterOrDigit(CurContext.CurCh))
                //    {
                //        CurToken.Value = CurToken.Value + CurContext.CurCh;
                //        GetNextChar();
                //    }
                //    if (CurContext.CurCh == '.')
                //    {
                //        CurToken.IntValue = StringUtils.StringToInteger(CurToken.Value);
                //        CurToken.Value = CurToken.Value + CurContext.CurCh;
                //        GetNextChar();
                //        // 123.123e+-123
                //        while (Char.IsLetterOrDigit(CurContext.CurCh))
                //        {
                //            CurToken.Value = CurToken.Value + CurContext.CurCh;
                //            GetNextChar();
                //        }
                //        CurToken.VarValue = new Variant(VariantType.Real);
                //        CurToken.VarValue.RealValue = StringUtils.StringToDouble(CurToken.Value);
                //    }
                //    else
                //    {
                //        string lastCh = StringUtils.Right(CurToken.Value, 1);
                //        if ((string.Compare(lastCh, "u", true) == 0) || (string.Compare(lastCh, "l", true) == 0))
                //            CurToken.Value = CurToken.Value.Substring(0, CurToken.Value.Length - 1);

                //        CurToken.IntValue = StringUtils.StringToInteger(CurToken.Value);
                //        CurToken.VarValue = new Variant(VariantType.Integer);
                //        CurToken.VarValue.IntValue = CurToken.IntValue;
                //    }
                //}
                //else if (Char.IsLetter(CurContext.CurCh))
                //{
                //    CurToken.Type = TokenType.Name;
                //    while (Char.IsLetterOrDigit(CurContext.CurCh) || (CurContext.CurCh == '_'))
                //    {
                //        CurToken.Value = CurToken.Value + CurContext.CurCh;
                //        GetNextChar();
                //    }
                //    if (CurContext.CurCh == '.')
                //    {
                //        CurToken.Value = CurToken.Value + CurContext.CurCh;
                //        GetNextChar();
                //        if (Char.IsLetter(CurContext.CurCh))
                //        {
                //            while (Char.IsLetterOrDigit(CurContext.CurCh) || (CurContext.CurCh == '_'))
                //            {
                //                CurToken.Value = CurToken.Value + CurContext.CurCh;
                //                GetNextChar();
                //            }
                //        }
                //    }
                //}
                else if ((CurContext.CurCh == '"') 
                    //|| (CurContext.CurCh == '\'')
                    )
                {
                    char QuoteChar = CurContext.CurCh;

                    if (QuoteChar == '"')
                        CurToken.Type = TokenType.StringLiteral;
                    else
                        CurToken.Type = TokenType.CharLiteral;
                    GetNextChar();

                    while ( (CurContext.CurCh != QuoteChar) && (CurContext.CurCh != EOL_CHAR) )
                    {
                        CurToken.Value = CurToken.Value + CurContext.CurCh;
                        GetNextChar();
                    }

                    GetNextChar();

                    //
                    //CurToken.Value = StringUtils.DeEscapeCString(CurToken.Value);
                    CurToken.Value = QuoteChar + CurToken.Value + QuoteChar;
                }
                else if (CurContext.CurCh == '(') 
                {
                    CurToken.Type = TokenType.Symbol;
                    CurToken.Value = CurContext.CurCh.ToString();
                    GetNextChar();
                }
                else if (CurContext.CurCh == ')')
                {
                    CurToken.Type = TokenType.Symbol;
                    CurToken.Value = CurContext.CurCh.ToString();
                    GetNextChar();
                }
                else
                {
                    CurToken.Type = TokenType.Value;
                    CurToken.Value = CurToken.Value + CurContext.CurCh;
                    GetNextChar();

                    while (IsPrint(CurContext.CurCh) &&
                        (CurContext.CurCh != ')') &&
                        (CurContext.CurCh != '(')
                        ) 
                    {
                        CurToken.Value = CurToken.Value + CurContext.CurCh;
                        GetNextChar();
                    }
                }
            }
            else
            {
                CurToken.Type = TokenType.EOF;
                GetNextChar();
            }

            return CurToken;
        }

        private bool IsWhiteSpace(char c)
        {
            return (c == ' ') || (c == '\t') || (c == '\0');
        }

        static void SkipSpaces(string Line, ref int Pos)
        {
            while ((Pos < Line.Length) && ((Line[Pos] == ' ') || (Line[Pos] == '\t') || (Line[Pos] == '\0')))
            {
                Pos++;
            }
        }

        private bool IsPrint(char c)
        {
            return (c > ' ');
        }

#if XXX
        private Token GetToken()
        {
            Token result = new Token(LineNum, Col, TokenType.WhiteSpace, "");;

            if (CurCh == NULCHAR)
                return null;
            else
            {
                if (Char.IsWhiteSpace(CurCh))
                {
                    while (Char.IsWhiteSpace(CurCh))
                    {
                        result.Data += CurCh;
                        GetNextChar();
                    }
                }
                else
                {
                    result.Type = TokenType.Token;
                    while ((CurCh != NULCHAR) && !Char.IsWhiteSpace(CurCh))
                    {
                        result.Data += CurCh;
                        GetNextChar();
                    }
                }

                return result;
            }
        }
#endif

        private void GetNextChar()
        {
            if (CurContext.Pos > CurContext.Line.Length)   // at EOL
            {
                // next line
                if (CurContext.LineIndex == CurContext.Lines.Count)
                {
                    CurContext.CurCh = EOF_CHAR;
                    return;
                }
                CurContext.LineIndex++;

                if (CurContext.LineIndex == CurContext.Lines.Count)
                {
                    CurContext.CurCh = EOF_CHAR;
                    return;
                }

                // Console.WriteLine("Line : " + CurContext.LineIndex);

                CurContext.Line = CurContext.Lines[CurContext.LineIndex];
                CurContext.Pos = 0;

                GetNextChar();
            }
            else
            {
                if (CurContext.Pos == CurContext.Line.Length)
                    CurContext.CurCh = EOL_CHAR;
                else
                   CurContext.CurCh = CurContext.Line [CurContext.Pos];

                CurContext.Pos++;
            }

        }


    }
}
