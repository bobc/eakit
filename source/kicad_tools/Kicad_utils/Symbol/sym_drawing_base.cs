using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class sym_drawing_base
    {
        public int Part = 0;
        public int DeMorganAlternate = 1;

        public float PenSize;
        public FillTypes Fill;

        public sym_drawing_base()
        {
        }

        public static FillTypes GetFillType(string fill)
        {
            if (fill == "F")
                return FillTypes.PenColor;
            else if (fill == "f")
                return FillTypes.BackgroundColor;
            else
                return FillTypes.None;
        }

        public static string FillTypeToString(FillTypes fill)
        {
            switch (fill)
            {
                case FillTypes.None: return "N";
                case FillTypes.PenColor: return "F";
                case FillTypes.BackgroundColor: return "f";
            }
            return "N";
        }

        public static sym_drawing_base Parse(List<Token> tokens)
        {
            sym_drawing_base result = null;

            switch (tokens[0].Value)
            {
                case "S":
                    result = sym_rectangle.Parse(tokens);
                    break;
                case "X":
                    result = sym_pin.Parse(tokens);
                    break;
                case "T":
                    result = sym_text.Parse(tokens);
                    break;
                case "P":
                    result = sym_polygon.Parse(tokens);
                    break;
                case "C":
                    result = sym_circle.Parse(tokens);
                    break;
                case "A":
                    result = sym_arc.Parse(tokens);
                    break;

                default:
                    Console.WriteLine("unknown {0}", tokens[0].Value);
                    result = null;
                    break;
            }

            return result;
        }
    }
}
