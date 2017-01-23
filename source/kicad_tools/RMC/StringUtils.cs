//---------------------------------------------------------------------------
// Module	: StringUtils
//
// Author	: Bob Cousins
//
// Copyright: © Copyright Bob Cousins 2009
// 
// Description:
// ------------
// String related methods.
//
// Revision history:
// -----------------
// 1.0	rmc	01-Jan-2009    Initial version.
// 
// Bugs/To do:
// -----------
//---------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace RMC
{
    /// <summary>
    /// String related methods.
    /// </summary>
    public class StringUtils
    {
        public const string QuoteChar = "\"";

        //
        // General string handling
        //
	    public static string Before (string source, string delimiter, bool caseSensitive = true)
		{
            int index = source.IndexOf(delimiter, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

            if (index != -1 )
    		    return source.Substring (0, index);
    	    else
    	        return source;
		}

        public static string BeforeLast(string source, string delimiter)
        {
            if (source.IndexOf(delimiter) != -1)
                return source.Substring(0, source.LastIndexOf(delimiter));
            else
                return source;
        }

    	public static string After (string source, string delimiter)
		{
		    if (source.IndexOf (delimiter) != -1 )
    		    return source.Substring (source.IndexOf (delimiter) + delimiter.Length, 
    		        source.Length - source.IndexOf (delimiter) - delimiter.Length );
    	    else
    	        return "";
		}


        public static string AfterLast(string source, string delimiter)
        {
            if (source.LastIndexOf(delimiter) != -1)
                return source.Substring(source.LastIndexOf(delimiter) + delimiter.Length,
                    source.Length - source.LastIndexOf(delimiter) - delimiter.Length);
            else
                return "";
        }

        public static string Parse(ref string source, string delimiter)
        {
            string result;

            result = Before(source, delimiter);

            source = After(source, delimiter);
            if (delimiter == " ")
                source = source.TrimStart();

            return result;
        }

        public static string GetLast(string source, string delimiter)
        {
            if (source.LastIndexOf(delimiter) != -1)
                return source.Substring(source.LastIndexOf(delimiter) + delimiter.Length,
                    source.Length - source.LastIndexOf(delimiter) - delimiter.Length);
            else
                return source;
        }
	    
	    public static string Left (string source, int n)
		{
		    return source.Substring (0, Math.Min(n, source.Length));
        }

        public static string Right (string source, int n)
		{
            if (n >= source.Length)
                return source;
            else
		        return source.Substring (source.Length - n, n);
        }

        public static string GetToken(ref string line, string v)
        {
            string result = "";
            int j=0;
            if (!string.IsNullOrEmpty(line))
            {
                for (j = 0; j < line.Length; j++)
                    if (v.IndexOf(line[j]) == -1)
                        result += line[j];
                    else
                    {
                        break;
                    }
            }
            line = line.Substring(j);
            return result;
        }

        public static bool BeginsWith (string source, string target)
        {
            return string.Compare(source.Substring(0, Math.Min(target.Length, source.Length)),
                target, true) == 0;
        }

        public static string Capitalize(string s)
        {
            string result = "";

            if (!string.IsNullOrEmpty(s))
            {
                result = Char.ToUpper(s[0]).ToString();

                result += s.Substring(1);
            }

            return result;
        }

        public static string AddToList (string list, string s, string delim=",")
        {
            string result="";

            if (!string.IsNullOrEmpty(list))
                result += list;

            if (!string.IsNullOrEmpty(s))
                if (string.IsNullOrEmpty(result))
                    result = s;
                else
                    result += delim + s;

            return result;
        }

        /// <summary>
        /// Perform a case insensitive compare and return bool
        /// </summary>
        /// <param name="a">First string</param>
        /// <param name="b">Second string</param>
        /// <returns>True if equal</returns>
        public static bool StrEqual(string a, string b)
        {
            return String.Compare(a, b, true) == 0;
        }

        public static bool StrNotEqual(string a, string b)
        {
            return String.Compare(a, b, true) != 0;
        }

        public static int Pos(string target, string source)
        {
            return source.IndexOf(target);
        }

        public static int PosI(string target, string source)
        {
            int i;
            i=source.ToLower().IndexOf(target.ToLower());
            return i;
        }

        /// <summary>
        /// Return copy of string from "start" to end of string
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="start">start position, 0 &lt= start &lt Length</param>
        /// <returns></returns>
	    public static string Copy (string source, int start)
        {
            return Copy (source, start, source.Length - start);
        }
        
        /// <summary>
        /// Return copy of string from "start" for "length" characters
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Number of chars</param>
        /// <returns></returns>
	    public static string Copy (string source, int start, int length)
		{
		    if (start >= source.Length)
		        return "";
		    else
		        return source.Substring (start, Math.Min(length, source.Length-start));
        }

        // given a string of values, split into string array
        // typically used to split CSV string
        // e.g where DsvString = "a,b,c" and Delimiter = "," returns {"a","b","c"}
        public static string[] SplitDsvText (string DsvString, string Delimiter)
        {
            string[] Result = null;
            string Val;
            int j = 0;

            while (DsvString.Length != 0)
            {
                if (DsvString[0] == StringUtils.QuoteChar[0])
                {
                    Val = StringUtils.GetQuotedString(ref DsvString);
                }
                else
                {
                    Val = StringUtils.Before (DsvString, Delimiter);
                }
                DsvString = StringUtils.After(DsvString, Delimiter);
                Array.Resize (ref Result, j+1);
                Result[j] = Val;
                j ++;
            }

            return Result;
        }

        // join a string array into a delimiter separated string
        public static string JoinDsvText (string[] Values, string Delimiter)
        {
            string DsvString = "";

            foreach (string str in Values)
            {
                if (DsvString.Length != 0)
                    DsvString = DsvString + Delimiter;
                DsvString = DsvString + str;
            }

            return DsvString;
        }

        /// <summary>
        /// given a delimiter separated string (e.g. "a,b,c"), return the nth field (0-based)
        /// </summary>
        /// <param name="DsvString"></param>
        /// <param name="Delimiter"></param>
        /// <param name="FieldNum"></param>
        /// <returns></returns>
        public static string GetDsvField(string DsvString, string Delimiter, int FieldNum)
        {
            string Result = "";

            Result = Before(DsvString, Delimiter);
            int j = 0;
            while (j < FieldNum)
            {
                j++;
                DsvString = After(DsvString, Delimiter);
                Result = Before(DsvString, Delimiter);
            }

            return Result;
        }

        // given a string "a|b|c", return the nth field (0-based) delimited by '|' 
        public static string xxxGetField (string PsvString, int FieldNum)
		{
            return GetDsvField(PsvString, "|", FieldNum);
        }

        // given a string "a|b|c", return the index of the field containing Target
        public static int GetDsvFieldIndex (string DsvString, string Delimiter, string Target)
		{
		    string Val;
		    int Result = -1;
		    int j = 0;
		    
		    while (DsvString.Length != 0)
		    {
		        Val = Before (DsvString, Delimiter);
                DsvString = After (DsvString, Delimiter);
                if (string.Compare (Val, Target, true) == 0)
                {
                    Result = j;
                    break;
                }
		        j ++ ;
		    }
		    return Result;
        }

        // get a quoted string literal from start of str
        // return value is string literal without quotes
        // str is modified to remove literal
        // e.g. "\"fred smith\",\"engineer\"" returns "fred smith"
        //
        public static string GetQuotedString(ref string str)
        {
            string result;

            str = After(str, QuoteChar);
            result = Before(str, QuoteChar);
            str = After(str, QuoteChar);

            return result;
        }

        // remove all spaces
        public static string StripSpaces (string str)
        {
            int Index = 0;
            while (Index < str.Length)
                if (str[Index] == ' ') 
                    str = str.Remove (Index, 1);
                else
                    Index ++;
                    
            return str;
        }

        public static string TrimQuotes(string str)
        {
            return TrimQuotes (str, "\"");
        }

        public static string TrimQuotes(string str, string quoteChars)
        {
            //TODO: Deescape quotese etc?
            if (str != null)
            {
                if (str.Length >= 2)
                {
                    if ( (quoteChars.IndexOf (str[0]) != -1) && (str[0] == str[str.Length-1]) )
                    {
                        return str.Substring(1, str.Length - 2);
                    }
                    else
                        return str;
                }
                else
                    return str;
            }
            else
                return null;


        }

        public static string AddBackslash(string str)
        {
            if ( (str.Length!=0) && (str[str.Length-1] != '\\') )
                return str + '\\';
            else
                return str;
        }

        public static string ReplaceNewlines(string str)
        {
            str = str.Replace('\n', ' ');
            str = str.Replace('\r', ' ');
            return str;
        }

        //
        // Conversions: Binary to ASCII
        //
        public static string ToAscii (byte Val)
        {
            // ASCII 32..126, SPC to '~'
            if ((Val >= ' ') && (Val <= '~')) 
                return new string ((char)Val,1);
            else
                return ".";
        }
        
        public static string ToAscii (ushort Val)
        {
            return ToAscii ((byte)(Val >> 8)) + ToAscii ((byte)(Val & 0xff));
        }
        
        //
        // Tests, validation
        //
        public static bool IsHexDigit (char c)
        {
            return Char.IsDigit(c) || ((c >= 'a') && (c <= 'f')) || ((c >= 'A') && (c <= 'F'));
        }
        
        public static bool IsHexString (string s)
        {
            foreach (char c in s)
                if (!IsHexDigit (c))
                    return false;
                    
            return true;                
        }
        
        public static bool IsValidInteger (string Token)
        {
            int Base = 10;
            int Value;
            
            if (string.Compare (StringUtils.Left (Token, 2), "0x", true)==0)
            {
                Token = Token.Remove (0,2);
                Base = 16;
            }
            
            if (Token.Length != 0)
            {
                try 
                {
                    Value = Convert.ToInt32 (Token, Base);
                    return true;
                } 
                catch (FormatException)
                {
                    return false;
                }
                catch (OverflowException)
                {
                    return false;
                }
            }
            else
                return false;
        }

        public static bool IsValidDouble(string Token)
        {
            double Value;

            if (Token.Length != 0)
            {
                try
                {
                    Value = Convert.ToDouble(Token);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
                catch (OverflowException)
                {
                    return false;
                }
            }
            else
                return false;
        }

        public static bool ValidRange (long Val, long Min, long Max)
        {
            return ((Val >= Min) && (Val <= Max));
        }
        
        //
        // Conversions: string to numeric
        //
        public static int StringToInteger (string Token)
        {
            int Base = 10;
            int Value;
            
            Token = Token.Trim();

            if (string.Compare(StringUtils.Left(Token, 2), "0x", true) == 0)
            {
                Token = Token.Remove(0, 2);
                Base = 16;
            }
            
            if (Token == "")
                return 0;

            try 
            {
                Value = Convert.ToInt32 (Token, Base);
                return Value;
            } 
            catch (FormatException)
            {
                return 0;
            }
            catch (OverflowException)
            {
                return 0;
            }
        }

        //
        // Conversions: string to numeric
        //
        public static double StringToDouble(string Token)
        {
            double Value;

            Token = Token.Trim();

            try
            {
                Value = Convert.ToDouble(Token);
                return Value;
            }
            catch (FormatException)
            {
                return 0;
            }
            catch (OverflowException)
            {
                return 0;
            }
        }
        
        public static string FormatDecimal (int Value, int Width)
        {
            return string.Format ("{0}", Value).PadLeft (Width,'0');
        }

        public static string FormatDecimal(uint Value, int Width)
        {
            return string.Format("{0}", Value).PadLeft(Width, '0');
        }

        public static string FormatHex (uint Value, int Width)
        {
            return string.Format ("{0:X}", Value).PadLeft (Width,'0');
        }

        public static string FormatBin (int Value, int Width)
        {
            return Convert.ToString (Value,2).PadLeft (Width,'0');
        }

        public static string FormatHexWord (int Value)
        {
            return "0x"+string.Format("{0:X}", Value).PadLeft(4, '0');
        }

        // convert string of hex characters to array of ushort
        // left padded with zeroes, i.e. 12 = 0012
        public static int HexToWords (string Str, ushort [] Data)
        {
            string Temp;
            int Index;
            int Result;
            
            //Str = StripSpaces (Str);
            //Str = Str.PadLeft ( ( Str.Length + 3) & ~3, '0');

            Str = Str.Replace('\r', ' ');
            Str = Str.Replace('\n', ' ');

            for (Index = 0; Index < Data.Length; Index++)
                Data[Index] = 0;
                
            Str = Str.Trim();
            Index = 0;
            Result = 0;
            while ((Str.Length != 0) && (Index < Data.Length))
            {
                Temp = Before (Str, " ");
                Str = After (Str, " ");
                Str = Str.Trim();
                Data[Index] = Convert.ToUInt16(Temp, 16);
                Index++;
                Result ++;
            }

            return Result;
        }

        // convert string of hex characters to array of ushort
        public static ushort[] HexToWords(string Str)
        {
            string Temp;
            ushort [] Result = new ushort[1024];
            int NumWords;

            //Str = StripSpaces (Str);
            //Str = Str.PadLeft ( ( Str.Length + 3) & ~3, '0');

            Str = Str.Replace('\r', ' ');
            Str = Str.Replace('\n', ' ');

            Str = Str.Trim();
            NumWords = 0;
            while (Str.Length != 0)
            {
                Temp = Before(Str, " ");
                Str = After(Str, " ");
                Str = Str.Trim();
                if (NumWords + 1 >= Result.Length)
                {
                    Array.Resize(ref Result, Result.Length + 1024);
                }
                Result[NumWords] = Convert.ToUInt16(Temp, 16);
                NumWords++;
            }

            Array.Resize(ref Result, NumWords);
            return Result;
        }

        // convert string of hex characters to array of byte
        public static int HexToBytes(string Str, byte[] Data)
        {
            string Temp;
            int Index;
            int Result;

            //Str = StripSpaces (Str);
            //Str = Str.PadLeft ( ( Str.Length + 3) & ~3, '0');

            Str = Str.Replace('\r', ' ');
            Str = Str.Replace('\n', ' ');

            for (Index = 0; Index < Data.Length; Index++)
                Data[Index] = 0;

            Str = Str.Trim();
            Index = 0;
            Result = 0;
            while ((Str.Length != 0) && (Index < Data.Length))
            {
                Temp = Before(Str, " ");
                Str = After(Str, " ");
                Str = Str.Trim();
                Data[Index] = Convert.ToByte(Temp, 16);
                Index++;
                Result++;
            }

            return Result;
        }

        // convert string of hex characters to array of byte
        public static byte[] HexToBytes(string Str)
        {
            string Temp;
            int NumBytes;
            byte[] Result = new byte[1024];

            //Str = StripSpaces (Str);
            //Str = Str.PadLeft ( ( Str.Length + 3) & ~3, '0');

            Str = Str.Replace('\r', ' ');
            Str = Str.Replace('\n', ' ');

            Str = Str.Trim();
            NumBytes = 0;
            while (Str.Length != 0)
            {
                Temp = Before(Str, " ");
                Str = After(Str, " ");
                Str = Str.Trim();
                if (NumBytes + 1 >= Result.Length)
                {
                    Array.Resize(ref Result, Result.Length + 1024);
                }
                Result[NumBytes] = Convert.ToByte(Temp, 16);
                NumBytes++;
            }

            Array.Resize(ref Result, NumBytes);
            return Result;
        }
        
        //
        // Conversions: numeric to string
        //

        public static string BytesToHex (byte [] Data, int Length)
		{
			string Str;
			
			Str = "";
			for (byte j=0; j < Length; j++)
			{
				Str = Str + String.Format ("{0:X2}", Data[j]) + " ";
			}
			return Str;
		}

		public static string CharsToHex (char [] Data, int Length)
		{
			string Str;
			
			Str = "";
			for (byte j=0; j < Length; j++)
			{
			    Str = Str + String.Format ("{0:X2}", (byte)Data[j]) + " ";
			}
			return Str;
		}

		public static string WordsToHex (ushort [] Data, int Length)
		{
			string Str;
			
            if (Data.Length < Length)
                Length = Data.Length;

			Str = "";
			for (byte j=0; j < Length; j++)
			{
				Str = Str + "0x"+FormatHex (Data[j], 4) + " ";
			}
			return Str;
		}

        public static string BytesToString(byte[] Data)
        {
            string Str;

            Str = "";
            for (int j = 0; j < Data.Length; j++)
            {
                Str = Str + (char)Data[j];
            }
            return Str;
        }

        public static byte[] StringToBytes(string Data)
        {
            byte [] Result;

            Result = new byte[Data.Length];

            for (int j = 0; j < Data.Length; j++)
            {
                Result[j] = (byte)Data[j];
            }
            return Result;
        }
        
        //
        // date/time formatting
        //
        public static string IsoFormatDate(DateTime time)
        {
            return string.Format("{0}-{1}-{2}",
                FormatDecimal(time.Year, 4),
                FormatDecimal(time.Month, 2),
                FormatDecimal(time.Day, 2));
        }

        public static string IsoFormatDateTime(DateTime time)
        {
            return string.Format ("{0}-{1}-{2} {3}:{4}:{5}", 
                FormatDecimal (time.Year, 4),
                FormatDecimal (time.Month, 2),
                FormatDecimal (time.Day, 2),
                FormatDecimal (time.Hour, 2),
                FormatDecimal (time.Minute, 2),
                FormatDecimal (time.Second, 2) );
        }

        public static string IsoFormatTime(DateTime time)
        {
            return string.Format("{0}:{1}:{2}",
                FormatDecimal(time.Hour, 2),
                FormatDecimal(time.Minute, 2),
                FormatDecimal(time.Second, 2));
        }

        public static string IsoFormatDateTimeLong (DateTime time)
        {
            return string.Format ("{0}-{1}-{2} {3}:{4}:{5}.{6}", 
                FormatDecimal (time.Year, 4),
                FormatDecimal (time.Month, 2),
                FormatDecimal (time.Day, 2),
                FormatDecimal (time.Hour, 2),
                FormatDecimal (time.Minute, 2),
                FormatDecimal (time.Second, 2),
                FormatDecimal (time.Millisecond, 3));
        }

        public static string IsoFormatTimeLong (DateTime time)
        {
            return string.Format ("{0}:{1}:{2}.{3}", 
                FormatDecimal (time.Hour, 2),
                FormatDecimal (time.Minute, 2),
                FormatDecimal (time.Second, 2),
                FormatDecimal (time.Millisecond, 3));
        }

        public static string IsoFormatTimeLong(TimeSpan time)
        {
            return string.Format("{0}:{1}:{2}.{3}",
                FormatDecimal(time.Hours, 2),
                FormatDecimal(time.Minutes, 2),
                FormatDecimal(time.Seconds, 2),
                FormatDecimal(time.Milliseconds, 3));
        }


        /// <summary>
        /// Formatted elapsed time since Start in seconds
        /// </summary>
        /// <param name="Start"></param>
        /// <returns></returns>
		public static string ElapsedTimeStr (DateTime Start)
		{
		    const long TicksPerMicrosec = TimeSpan.TicksPerMillisecond / 1000;
		    DateTime now;
		    
		    now = DateTime.Now;
		    
		    long MicroSeconds = (now.Ticks - Start.Ticks) / TicksPerMicrosec;
		    
		    return String.Format ("{0:f6}", MicroSeconds / 1000000.0);
		}

		public static string ElapsedTimeStr (long Ticks)
		{
		    const long TicksPerMicrosec = TimeSpan.TicksPerMillisecond / 1000;
		    
		    long MicroSeconds = (Ticks) / TicksPerMicrosec;
		    
		    return String.Format ("+{0:f6}", MicroSeconds / 1000000.0);
		}

        public static string FormatElapsedTime (int ElapsedSeconds)
        {
            int Sec, Min, Hour, Days;

            Sec = ElapsedSeconds % 60;
            ElapsedSeconds /= 60;
            Min = ElapsedSeconds % 60;
            ElapsedSeconds /= 60;
            Hour = ElapsedSeconds % 24;
            ElapsedSeconds /= 24;
            Days = ElapsedSeconds;

            return string.Format ("{0:d3} {1}:{2}:{3}", 
                Days,
                FormatDecimal (Hour, 2),
                FormatDecimal (Min, 2),
                FormatDecimal (Sec, 2) );
                
        }


        // functions for List<string>

        public static int FindString (List<string> list, string target)
        {
            for (int index = 0; index < list.Count; index++)
                if (String.Compare (list[index], target, true) == 0)
                    return index;
            return -1;
        }

        public static int FindSubString (List<string> list, string target)
        {
            for (int index = 0; index < list.Count; index++)
                if (list[index].IndexOf (target, StringComparison.CurrentCultureIgnoreCase) != -1)
                    return index;
            return -1;
    }

        /// <summary>
        /// Converts C printf format string to C# equivalent.
        /// %d %x %f %s
        /// %4d %04d %-4d
        /// %%
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        public static string ConvertFromCFormat(string Format)
        {
            string Result;
            string FmtSign;
            string FmtWidth;
            string FmtType;
            int FmtIndex = 0;
            int Pos;

            Pos = 0;
            Result = "";
            while (Pos < Format.Length)
            {
                if (Format[Pos] == '%')
                {
                    Pos++;
                    if ((Pos < Format.Length) && (Format[Pos] == '%'))
                    {
                        Result = Result + '%';
                        Pos++;
                    }
                    else
                    {
                        FmtSign = "";
                        if ((Pos < Format.Length) && (Format[Pos] == '-'))
                        {
                            FmtSign = "-";
                            Pos++;
                        }
                        FmtWidth = "";
                        while ((Pos < Format.Length) && (Char.IsDigit(Format[Pos])))
                        {
                            FmtWidth = FmtWidth + Format[Pos];
                            Pos++;
                        }
                        if ((Pos < Format.Length) && (Char.IsLetter(Format[Pos])))
                        {
                            FmtType = Format[Pos].ToString();
                            Pos++;

                            // { index [,align] : letter prec }
                            // {0,7:f2}
                            int Width;
                            try { Width = Convert.ToInt32(FmtSign + FmtWidth); }
                            catch {Width = 0;}
                            switch (FmtType)
                            {
                                case "D":
                                case "d":
                                    Result = Result + "{" + FmtIndex;
                                    if (FmtWidth.Length != 0)
                                    {
                                        Result = Result + "," + Width;
                                    }
                                    Result = Result + ":" + FmtType + "}";
                                    FmtIndex++;
                                    break;
                                case "X":
                                case "x":
                                    Result = Result + "{" + FmtIndex;
                                    if (FmtWidth.Length != 0)
                                    {
                                        Result = Result + "," + Width;
                                    }
                                    Result = Result + ":" + FmtType;
                                    if (StringUtils.BeginsWith(FmtWidth, "0"))
                                        Result = Result + FmtWidth;
                                    Result = Result + "}";
                                    FmtIndex++;
                                    break;
                                default:
                                    Result = Result + "%" + FmtSign + FmtWidth + FmtType;
                                    break;
                            }
                        }
                        else
                        {
                            Result = Result + "%" + FmtSign + FmtWidth;
                        }
                    }
                }
                else
                {
                    Result = Result + Format[Pos];
                    Pos++;
                }
            }


            return Result;
        }

        /// <summary>
        /// Convert backslash escape sequences (e.g. \t) to equivalent control characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DeEscapeCString(string str)
        {
            string Result;

            Result = str.Replace(@"\n", Environment.NewLine);
            Result = Result.Replace(@"\t", "\t");
            Result = Result.Replace(@"\\", @"\");

            return Result;
        }

        // 
        public string[] StringListToArray (List<string> list)
        {
            string[] array = new string[list.Count];

            int index = 0;
            foreach (string s in list)
                array[index++] = s;

            return array;
        }

        public string StringListToText(List<string> list)
        {
            string text = "";

            foreach (string s in list)
                text += s + Environment.NewLine;

            return text;
        }

        public List<string> StringArrayToList(string [] array)
        {
            List<string> list = new List<string>();

            foreach (string s in array)
                list.Add (s);

            return list;
        }
    }

}
