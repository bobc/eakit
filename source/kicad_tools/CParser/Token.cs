using System;
using System.Collections.Generic;
using System.Text;

using RMC.Classes;

namespace CParser
{
    public enum TokenType { None, 
        Comment, LineBreak, 
        EOL, EOF, Whitespace,
        Name, Number, CharLiteral, 
        StringLiteral, ArrayLiteral, Symbol, 
        Value
        };

    /// <summary>
    /// Class to hold a lexical token.
    /// </summary>
    public class Token
    {
        public TokenType Type;
        public string Value;        // Source text (also value for Identifier or String literal)

        public int IntValue;        // If Type == Number
        public Variant VarValue;    // if array const or float

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
            this.OperatorValue = token.OperatorValue;
            this.Location = new Location(token.Location.FileName, token.Location.Line, token.Location.Column);
        }

        public Token(TokenType Type)
        {
            this.Type = Type;
            this.Value = "";
            this.IntValue = 0;
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
            this.OperatorValue = Operator.None;
            this.Location = new Location();
        }

        public Token(TokenType Type, Variant Value)
        {
            this.Type = Type;
            this.VarValue = Value;
            this.OperatorValue = Operator.None;
            this.Location = new Location();
        }

        public Token(Variant Value)
        {
            this.VarValue = Value;
            switch (Value.Mode)
            {
                case VariantType.Integer:
                    Type = TokenType.Number;
                    this.IntValue = Value.IntValue;
                    break;
                case VariantType.Real:
                    Type = TokenType.Number;
                    break;
                case VariantType.String:
                    Type = TokenType.StringLiteral;
                    this.Value = Value.StrValue;
                    break;
                case VariantType.IntArray:
                    Type = TokenType.ArrayLiteral;
                    break;

            }
            this.OperatorValue = Operator.None;
            this.Location = new Location();
        }

        public override string ToString()
        {
            if (Type == TokenType.Number)
            {
                if (VarValue == null)
                    return IntValue.ToString();
                else
                    return VarValue.ToString();
            }
            else if (Type == TokenType.StringLiteral)
                return '"' + Value + '"';
            else if (Type == TokenType.ArrayLiteral)
            {
                return VarValue.ToString();
            }
            else if (Type == TokenType.Symbol)
            {
                return Value;
            }
            else
                return Value;
        }
    }
}
