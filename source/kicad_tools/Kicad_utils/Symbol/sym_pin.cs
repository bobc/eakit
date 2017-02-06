using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class sym_pin : sym_drawing_base
    {
        public string Name;
        public string PinNumber;

        // replace with Position?
        public PointF Pos;
        /// <summary>
        /// Up Down Left Right
        /// </summary>
        public string Orientation;

        public int Length;

        public float SizeNum;
        public float SizeName;
        /// <summary>
        /// I Input
        /// O Output
        /// B Bidirectional
        /// T Tristate
        /// P Passive
        /// C Open Collector
        /// E Open Emitter
        /// N NC
        /// U Unspecified
        /// W Power input
        /// w Power Output
        /// </summary>
        public string Type;
        /// <summary>
        /// " "     simple line (default)
        /// I       Inverted (circle)
        /// C       Clock
        /// CI      Clock, inverted
        /// CL      Clock, low
        /// F       Falling edge clock
        /// L       Input, active low
        /// V       Output, active low
        /// X       Non-logic
        /// N...    Prefix for Invisible
        /// </summary>
        public string Shape;
        public bool Visible;

        public const string dir_input = "I";
        public const string dir_output = "O";
        public const string dir_power_in = "W";
        public const string dir_power_out = "w";

        public sym_pin()
        {
            Visible = true;
        }

        public sym_pin(int unit, string name, string pinNumber, PointF pos, int length, string orientation,
            float sizeNum, float sizeName, string type, string shape, bool pinVisible)
        {
            this.Name = name;
            this.PinNumber = pinNumber;
            this.Unit = unit;
            this.DeMorganAlternate = 0;

            this.Pos = pos;
            this.Length = length;
            this.Orientation = orientation;
            this.SizeNum = sizeNum;
            this.SizeName = sizeName;
            this.Type = type;
            this.Shape = shape;
            this.Visible = pinVisible;
        }

        public static sym_pin Clone (sym_pin src)
        {
            sym_pin result = new sym_pin(src.Unit, src.Name, src.PinNumber, src.Pos, src.Length, src.Orientation, src.SizeNum, src.SizeName, src.Type, src.Shape, src.Visible);
            return result;
        }

        // Legacy format
        public override string ToString()
        {
            // X name pin X Y length orientation sizenum sizename part dmg type shape
            return string.Format("X {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}{12}",
                Name,                       // 0
                PinNumber,                  // 1
                (int)Pos.X, (int)Pos.Y,     // 2,3
                Length, Orientation,        // 4,5
                (int)SizeNum, (int)SizeName,// 6,7
                Unit, DeMorganAlternate,    // 8,9
                Type,                       // 10
                Visible ? "" : "N",         // 11
                Shape                       // 12
                );
        }

        public new static sym_pin Parse(List<Token> tokens)
        {
            sym_pin result = new sym_pin();

            result.Name = tokens[1].Value;
            result.PinNumber = tokens[2].Value;
            result.Pos.X = tokens[3].IntValue;
            result.Pos.Y = tokens[4].IntValue;
            result.Length = tokens[5].IntValue;
            result.Orientation = tokens[6].Value;
            result.SizeNum = (float)tokens[7].GetValueAsDouble();
            result.SizeName = (float)tokens[8].GetValueAsDouble();
            result.Unit = tokens[9].IntValue;
            result.DeMorganAlternate = tokens[10].IntValue;
            result.Type = tokens[11].Value;
            if (tokens.Count >= 13)
                result.Shape = tokens[12].Value;
            else
                result.Shape = " ";

            result.Visible = true;
            if (result.Shape.StartsWith("N"))
                result.Visible = false;

            return result;
        }
    }
}
