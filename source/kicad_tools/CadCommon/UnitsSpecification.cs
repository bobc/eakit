using System;
using System.Collections.Generic;
using System.Text;

namespace CadCommon
{

    public enum Units {millimeters, meters, micron, inch, feet} ;

    public class UnitsSpecification
    {
        public Units Units;
        public double Scale;

        public static string[] UnitsDescription = new string[] { "millimeters", "meters", "micron", "inch", "feet" };

        public UnitsSpecification()
        {
            Units = Units.millimeters;
            Scale = 1.0;
        }

        public UnitsSpecification(Units units, double scale)
        {
            Units = units;
            Scale = scale;
        }

        public UnitsSpecification(UnitsSpecification source)
        {
            Units = source.Units;
            Scale = source.Scale;
        }
    }
}
