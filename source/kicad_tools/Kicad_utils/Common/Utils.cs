using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RMC;
using Lexing;


namespace Kicad_utils
{
    public class Utils
    {

        // set text size of Gloal labels to standard size
        public static bool ProcessSchematic(string filename)
        {
            return false; //todo

            string[] lines;

            try
            {
                lines = File.ReadAllLines(filename);
            }
            catch
            {
                return false;
            }

            string[] output = new string[lines.Length];

            int index = 0;
            foreach (string s in lines)
            {
                string s2;

                // Text GLabel 8900 13300 0    39   BiDi ~ 0
                if (s.StartsWith("Text GLabel"))
                {
                    string[] fields = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    fields[5] = "45";
                    s2 = StringUtils.JoinDsvText(fields, " ");
                }
                else
                    s2 = s;

                output[index++] = s2;
            }

            File.WriteAllLines(filename, output);

            return true;
        }


        public static ulong GetUnixSeconds(DateTime date)
        {
            TimeSpan time = date - new DateTime(1970, 1, 1, 0, 0, 0);

            return (ulong)time.TotalSeconds;
        }

        public static string GetTimeStamp (DateTime date)
        {
            ulong secs = GetUnixSeconds(date);

            return secs.ToString("X");
        }

        public static string GetToken (ref string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            else
            {
                string tok="";

                if (s.StartsWith("\""))
                {
                    //tok += s[0];
                    s = s.Remove(0, 1);
                    while ((s.Length>0) && (s[0] != '\"'))
                    {
                        tok += s[0];
                        s = s.Remove(0, 1);
                    }
                    if ((s.Length > 0) && (s[0] == '\"'))
                        s = s.Remove(0, 1);
                    //tok += "\"";
                }
                else
                {
                    while ((s.Length > 0) && !Char.IsWhiteSpace (s[0]))
                    {
                        tok += s[0];
                        s = s.Remove(0, 1);
                    }
                }

                while ((s.Length>0) && Char.IsWhiteSpace(s[0]) )
                {
                    s = s.Remove(0, 1);
                }

                return tok;
            }
        }

        // tokenize a line assuming "space" delimiters
        // used by legacy file parser 
        public static List<Token> Tokenise(string line)
        {
            List<Token> result = new List<Token>();

            string token = GetToken(ref line);
            while (token != null)
            {
                int int_val;
                double real_val;
                Token tok = new Token();

                tok.Type = TokenType.Name;
                tok.Value = token;

                if (token.StartsWith("\"") && token.EndsWith("\""))
                {
                    token = token.Substring(1, token.Length - 2);
                    // descape " and \
                }
                else if (int.TryParse(token, out int_val))
                {
                    tok.Type = TokenType.IntegerVal;
                    tok.IntValue = int_val;
                }
                else if (double.TryParse(token, out real_val))
                {
                    tok.Type = TokenType.FloatVal;
                    tok.RealValue = real_val;
                }

                result.Add(tok);

                token = GetToken(ref line);
            }

            return result;
        }

        public static int GetInt(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int result = 0;
                if (int.TryParse(value, out result))
                    return result;
                else
                    return 0;
            }
            else
                return 0;
        }

        //
        public static string EscapedValue(string Value)
        {
            if (String.IsNullOrEmpty(Value))
                return "\"\"";
            else if (NeedsQuoting(Value))
                return "\"" + Value + "\"";
            else
                return Value;
        }

        public static bool NeedsQuoting(string Value)
        {
            return (Value.IndexOfAny(new char[] { '#', ' ', '\t', '(', ')', '%', '{', '}', }) >= 0)
                || (Value.IndexOf("-") > 0)
                ;
        }


    }


}
