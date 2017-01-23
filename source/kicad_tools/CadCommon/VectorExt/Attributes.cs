using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace CadCommon
{
    // bitmap
    public enum AttributeFlags
    {
        Color = 1,
        Texture = 2,
        Normal = 4
    }

    public class Attributes
    {
        public bool HasColor { get { return (mAttributes & AttributeFlags.Color) != 0; } }

        public Color Color
        {
            set
            {
                mColor = value;
                mAttributes |= AttributeFlags.Color;
            }
            get { return mColor; }
        }

        private AttributeFlags mAttributes;
        private Color mColor;
    }
}
