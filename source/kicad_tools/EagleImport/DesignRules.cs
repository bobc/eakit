using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EagleImport
{
    public class DesignRules
    {
        // *** NB : all sizes in mm ***

        ///<summary>percent over 100%.  0-> not elongated, 100->twice as wide as is tall
        ///Goes into making a scaling factor for "long" pads.
        ///</summary>
        int psElongationLong
        {
            get
            {
                return GetValueInt("psElongationLong");
            }
        }

        ///<summary>the offset of the hole within the "long" pad.</summary>
        int psElongationOffset
        {
            get
            {
                return GetValueInt("psElongationOffset");
            }
        }


        ///<summary>top pad size as percent of drill size</summary> 
        float rvPadTop
        {
            get
            {
                return GetValueFloat("rvPadTop");
            }
        }

        ///<summary>bottom pad size as percent of drill size</summary>
        // double   rvPadBottom;        

        ///<summary>minimum copper annulus on through hole pads</summary>
        float rlMinPadTop
        {
            get
            {
                return GetValueFloat("rlMinPadTop");
            }
        }

        ///<summary>maximum copper annulus on through hole pads</summary>
        float rlMaxPadTop
        {
            get
            {
                return GetValueFloat("rlMaxPadTop");
            }
        }

        ///<summary>copper annulus is this percent of via hole</summary>
        float rvViaOuter
        {
            get
            {
                return GetValueFloat("rvViaOuter");
            }
        }

        ///<summary> minimum copper annulus on via</summary>
        float rlMinViaOuter
        {
            get
            {
                return GetValueFloat("rlMinViaOuter");
            }
        }

        ///<summary> maximum copper annulus on via</summary>
        float rlMaxViaOuter
        {
            get
            {
                return GetValueFloat("rlMaxViaOuter");
            }
        }

        ///<summary> wire to wire spacing I presume.</summary>
        float mdWireWire
        {
            get
            {
                return GetValueFloat("mdWireWire");
            }
        }



        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        public DesignRules()
        {
            Add("psElongationLong", 100);
            Add("psElongationOffset", 0);
            Add("rvPadTop", 0.25f);
            // rvPadBottom = 0.25f;
            Add("rlMinPadTop", 0.254f);  // = 10mil
            Add("rlMaxPadTop", 0.508f);  // = 20mil
            Add("rvViaOuter", 0.25f);
            Add("rlMinViaOuter", 0.254f);  // = 10mil
            Add("rlMaxViaOuter", 0.508f);  // = 20mil
            Add("mdWireWire", 0);
        }

        public float Clamp (float val, float min, float max)
        {
            if (val < min)
                return min;
            else if (val > max)
                return max;
            else
                return val;
        }

        public float CalcPadSize(float drill)
        {
            float annulus = Clamp(drill * rvPadTop, rlMinPadTop, rlMaxPadTop);
            return 2 * annulus + drill;
        }

        public float CalcViaSize(float drill)
        {
            float annulus = Clamp(drill * rvViaOuter, rlMinViaOuter, rlMaxViaOuter);
            return 2 * annulus + drill;
        }

        public void Add(string key, float value)
        {
            Parameters[key] = value.ToString("f6");
        }

        public void Add(string key, string value)
        {
            
            Parameters[key] = value;
        }

        public string GetValueStr(string key)
        {
            string result = null;
            if (!Parameters.TryGetValue(key, out result))
                result = null;

            return result;
        }

        public float GetValueFloat(string key)
        {
            string s_val = GetValueStr(key);

            if (s_val == null)
                return 0;
            else
            {
                float conversion = 1f;
                if (s_val.Contains("mm"))
                    s_val = s_val.Substring(0, s_val.Length - 2);
                else if (s_val.Contains("mil"))
                {
                    s_val = s_val.Substring(0, s_val.Length - 3);
                    conversion = 25.4f / 1000f;
                }

                return float.Parse(s_val) * conversion;
            }
        }

        public int GetValueInt(string key)
        {
            return (int)GetValueFloat(key);
        }
    }
}
