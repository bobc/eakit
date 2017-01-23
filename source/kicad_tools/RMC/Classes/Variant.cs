using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RMC.Classes
{
    public enum VariantType { None, Char, Integer, Real, String, IntArray, ArrayOfReal, ArrayOfString, VarArgs };

    public enum ArrayType { Fixed, Dynamic };

    /// <summary>
    /// Class for a container to hold values of various types
    /// </summary>
    public class Variant
    {
        // the current type of container
        public VariantType Mode
        {
            get { return mMode; }
            set
            {
                if (mMode == VariantType.None)
                {
                    mMode = value;
                    // convert from string
                    switch (mMode)
                    {
                        case VariantType.Integer:
                            int.TryParse(mStrValue, out mIntValue);
                            break;
                        case VariantType.Real:
                            int.TryParse(mStrValue, out mIntValue);
                            break;
                        case VariantType.String:
                            break;
                    }

                }
                else if (value != mMode)
                {
                    // convert to new mode
                    if (value == VariantType.Integer)
                        mIntValue = (int)mRealValue;
                    else
                        mRealValue = (double)mIntValue;
                    mMode = value;
                }
            }
        }

        // get/set the current value as a char
        [XmlIgnore]
        public char CharValue
        {
            get
            {
                if (mMode == VariantType.Char)
                    return mCharValue;
                else if (mMode == VariantType.Integer)
                    return (char)mIntValue;
                else
                    return '\0';
            }
            set
            {
                mCharValue = value;
                mMode = VariantType.Char;
            }
        }

        // get/set the current value as an integer
        [XmlIgnore]
        public int IntValue
        {
            get
            {
                if (mMode == VariantType.Integer)
                    return mIntValue;
                else
                    return (int)mRealValue;
            }
            set
            {
                mIntValue = value;
                mMode = VariantType.Integer;
            }
        }

        [XmlIgnore]
        public double RealValue
        {
            get
            {
                if (mMode == VariantType.Real)
                    return mRealValue;
                else
                    return (double)mIntValue;
            }
            set
            {
                mRealValue = value;
                mMode = VariantType.Real;
            }
        }

        [XmlIgnore]
        public string StrValue
        {
            get
            {
                if (mMode == VariantType.String)
                    return mStrValue;
                else if (mMode == VariantType.Integer)
                    return mIntValue.ToString();
                else // if (mMode == VariantType.Real)
                    return mRealValue.ToString();
            }
            set
            {
                mStrValue = value;
                mMode = VariantType.String;
            }
        }

        // only used to serialize scalar values
        public string XmlValue
        {
            get
            {
                switch (mMode)
                {
                    case VariantType.String:
                        {
                            return mStrValue;
                        }
                    case VariantType.Integer:
                        return mIntValue.ToString();
                    case VariantType.Real:
                        return mRealValue.ToString();
                    default:
                        return "";
                }
            }
            set
            {
                mStrValue = value;
                // Convert from string
                switch (mMode)
                {
                    case VariantType.Integer:
                        int.TryParse(mStrValue, out mIntValue);
                        break;
                    case VariantType.Real:
                        int.TryParse(mStrValue, out mIntValue);
                        break;
                    case VariantType.String:
                        break;
                }
            }
        }

        public ArrayType ArrayType
        {
            get { return mArrayType; }
            set { mArrayType = value; }
        }

        public int[] IntArrayValue
        {
            get
            {
                if (mMode == VariantType.IntArray)
                    return mIntArrayValue;
                else
                    return null;
            }
            set
            {
                if (value != null)
                {
                    // expand array size if necessary
                    if ((mIntArrayValue == null) || (mIntArrayValue.Length < value.Length))
                        mIntArrayValue = new int[value.Length];
                    value.CopyTo(mIntArrayValue, 0);
                }
                mMode = VariantType.IntArray;
                mNumElements = mIntArrayValue.Length;
            }
        }

        // NumElements reflects current size of array
        public int NumElements
        {
            get
            {
                return mNumElements;
            }
            set
            {
                if (mIntArrayValue == null)
                {
                    mNumElements = value;
                    mIntArrayValue = new int[mNumElements];
                }
                else if (value != mNumElements)
                {
                    mNumElements = value;
                    Array.Resize(ref mIntArrayValue, mNumElements);
                }
            }
        }


        public override string ToString()
        {
            switch (mMode)
            {
                case VariantType.None:
                    return "none";

                case VariantType.Integer:
                    return ""+mIntValue;

                case VariantType.Real:
                    return ""+mRealValue;

                case VariantType.String:
                    return mStrValue;

                case VariantType.IntArray:
                    string s;
                    s = "{";
                    foreach (int i in mIntArrayValue)
                        s = s + " " + i;
                    s = s + "}";
                    return s;

                case VariantType.VarArgs:
                    return "(...)";

                default:
                    return "?";
            }
        }

        #region Private data

        VariantType mMode;       

        // scalar types, numeric
        int mIntValue;
        char mCharValue;
        double mRealValue;
        //
        // scalar types, string
        string mStrValue;

        // array (int)
        int mNumElements;
        int[] mIntArrayValue;
        ArrayType mArrayType;

        #endregion

        public Variant()
        {
            this.mMode = VariantType.None;
            this.mArrayType = ArrayType.Fixed;
        }

        public Variant(VariantType mode)
        {
            this.mMode = mode;
            this.mArrayType = ArrayType.Fixed;
        }

        public Variant(VariantType mode, ArrayType arrayType)
        {
            this.mMode = mode;
            this.mArrayType = arrayType;
        }

        public Variant(VariantType mode, int numElements)
        {
            this.mMode = mode;
            this.mArrayType = ArrayType.Fixed;
            this.mNumElements = numElements;
        }

        public Variant(int IntValue)
        {
            this.IntValue = IntValue;
        }

        public Variant(string StrValue)
        {
            this.StrValue = StrValue;
        }

        public Variant(double RealValue)
        {
            this.RealValue = RealValue;
        }

        public Variant(Variant Src)
        {
            this.mNumElements = 0;
            Assign (Src);
        }

        // should be straight copy?
        // TODO: cast/conversions
        public void Assign(Variant src)
        {
            this.mMode = src.Mode;
            this.mCharValue = src.mCharValue;
            this.mIntValue = src.mIntValue;
            this.mRealValue = src.mRealValue;
            this.mStrValue = src.mStrValue;

            if ( (this.mNumElements == 0) && (src.mNumElements != 0))
            {
                this.mNumElements = src.mNumElements;
                this.mIntArrayValue = new int[this.mNumElements];
            }

            if (src.mIntArrayValue != null)
            {
                for (int j = 0; j < Math.Min(this.mNumElements, src.mIntArrayValue.Length); j++)
                {
                    this.mIntArrayValue[j] = src.mIntArrayValue[j];
                }
            }
        }
    }
}
