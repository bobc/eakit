using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CadCommon
{
    public class Materials
    {
        // Material properties from http://sourceforge.net/p/kicad3dmodels/code/ci/master/tree/
        // # Copyright 2012 Cirilo Bernardo (cjh.bernardo@gmail.com)
        // # License: GPLv.3 http://www.gnu.org/licenses/gpl.html

        public static MaterialProperties BlackPlastic()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "mat_black";
            result.diffuseColor = new ColorF(0.0676, 0.0676, 0.06760, 0);
            result.emissiveColor = new ColorF(0.001, 0.001, 0.001, 0);
            result.specularColor = new ColorF(1, 1, 1, 0);
            result.ambientIntensity = 1;
            result.transparency = 0;
            result.shininess = 1;
            return result;
        }

        public static MaterialProperties BlackPlastic2()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "mat_black";
            result.diffuseColor = new ColorF(0.2, 0.2, 0.2, 0);
            result.emissiveColor = new ColorF(0.001, 0.001, 0.001, 0);
            result.specularColor = new ColorF(0.2, 0.2, 0.2, 0);
            result.ambientIntensity = 0.2;
            result.transparency = 0;
            result.shininess = 1;
            return result;
        }

        public static double CalcIntensity (MaterialProperties props)
        {
            return (0.212671 * props.emissiveColor.R + 0.715160 * props.emissiveColor.G + 0.072169 * props.emissiveColor.B) /
                (0.212671 * props.emissiveColor.R + 0.715160 * props.emissiveColor.G + 0.072169 * props.emissiveColor.B);
        }




        public static MaterialProperties Gold()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "mat_gold";
            result.diffuseColor = new ColorF(1, 0.8, 0, 0);
            result.emissiveColor = new ColorF(0.24742, 0.24742, 0.00097, 0);
            result.specularColor = new ColorF(1, 0.8, 0, 0);
            result.ambientIntensity = 0.86536;
            result.transparency = 0;
            result.shininess = 1;
            return result;
        }


        public static MaterialProperties Tin()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "mat_tin";
            result.diffuseColor = new ColorF(0.92008f, 0.92008f, 0.92008f, 0);
            result.emissiveColor = new ColorF(0, 0, 0, 0);
            result.specularColor = new ColorF(1, 1, 1, 0);
            result.ambientIntensity = 0.82018;
            result.transparency = 0;
            result.shininess = 1;
            return result;
        }

        public static MaterialProperties Pcb_Green ()
        {
            MaterialProperties result = new MaterialProperties();

            result.diffuseColor = new ColorF(0.33f, 0.67f, 0f, 0);
            result.emissiveColor = new ColorF(0, 0, 0, 0);
            result.specularColor = new ColorF(0.33f, 0.67f, 0f, 0);
            result.ambientIntensity = 0.5f;
            result.transparency = 0;
            result.shininess = 0.5f;

            return result;
        }

        public static MaterialProperties ModifyColor(MaterialProperties material, ColorF color)
        {
            MaterialProperties result = new MaterialProperties(material);
            result.diffuseColor = new ColorF(color);
            return result;
        }

        // Imported from http://devernay.free.fr/cours/opengl/materials.html

        public static MaterialProperties emerald()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "emerald";
            result.diffuseColor = new ColorF(0.07568, 0.61424, 0.07568, 0);
            result.emissiveColor = new ColorF(0.0215, 0.1745, 0.0215, 0);
            result.specularColor = new ColorF(0.633, 0.727811, 0.633, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.6;
            return result;
        }
        public static MaterialProperties jade()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "jade";
            result.diffuseColor = new ColorF(0.54, 0.89, 0.63, 0);
            result.emissiveColor = new ColorF(0.135, 0.2225, 0.1575, 0);
            result.specularColor = new ColorF(0.316228, 0.316228, 0.316228, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.1;
            return result;
        }
        public static MaterialProperties obsidian()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "obsidian";
            result.diffuseColor = new ColorF(0.18275, 0.17, 0.22525, 0);
            result.emissiveColor = new ColorF(0.05375, 0.05, 0.06625, 0);
            result.specularColor = new ColorF(0.332741, 0.328634, 0.346435, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.3;
            return result;
        }
        public static MaterialProperties pearl()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "pearl";
            result.diffuseColor = new ColorF(1, 0.829, 0.829, 0);
            result.emissiveColor = new ColorF(0.25, 0.20725, 0.20725, 0);
            result.specularColor = new ColorF(0.296648, 0.296648, 0.296648, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.088;
            return result;
        }
        public static MaterialProperties ruby()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "ruby";
            result.diffuseColor = new ColorF(0.61424, 0.04136, 0.04136, 0);
            result.emissiveColor = new ColorF(0.1745, 0.01175, 0.01175, 0);
            result.specularColor = new ColorF(0.727811, 0.626959, 0.626959, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.6;
            return result;
        }
        public static MaterialProperties turquoise()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "turquoise";
            result.diffuseColor = new ColorF(0.396, 0.74151, 0.69102, 0);
            result.emissiveColor = new ColorF(0.1, 0.18725, 0.1745, 0);
            result.specularColor = new ColorF(0.297254, 0.30829, 0.306678, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.1;
            return result;
        }
        public static MaterialProperties brass()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "brass";
            result.diffuseColor = new ColorF(0.780392, 0.568627, 0.113725, 0);
            result.emissiveColor = new ColorF(0.329412, 0.223529, 0.027451, 0);
            result.specularColor = new ColorF(0.992157, 0.941176, 0.807843, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.21794872;
            return result;
        }
        public static MaterialProperties bronze()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "bronze";
            result.diffuseColor = new ColorF(0.714, 0.4284, 0.18144, 0);
            result.emissiveColor = new ColorF(0.2125, 0.1275, 0.054, 0);
            result.specularColor = new ColorF(0.393548, 0.271906, 0.166721, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.2;
            return result;
        }
        public static MaterialProperties chrome()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "chrome";
            result.diffuseColor = new ColorF(0.4, 0.4, 0.4, 0);
            result.emissiveColor = new ColorF(0.25, 0.25, 0.25, 0);
            result.specularColor = new ColorF(0.774597, 0.774597, 0.774597, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.6;
            return result;
        }
        public static MaterialProperties copper()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "copper";
            result.diffuseColor = new ColorF(0.7038, 0.27048, 0.0828, 0);
            result.emissiveColor = new ColorF(0.19125, 0.0735, 0.0225, 0);
            result.specularColor = new ColorF(0.256777, 0.137622, 0.086014, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.1;
            return result;
        }
        public static MaterialProperties gold()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "gold";
            result.diffuseColor = new ColorF(0.75164, 0.60648, 0.22648, 0);
            result.emissiveColor = new ColorF(0.24725, 0.1995, 0.0745, 0);
            result.specularColor = new ColorF(0.628281, 0.555802, 0.366065, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.4;
            return result;
        }
        public static MaterialProperties silver()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "silver";
            result.diffuseColor = new ColorF(0.50754, 0.50754, 0.50754, 0);
            result.emissiveColor = new ColorF(0.19225, 0.19225, 0.19225, 0);
            result.specularColor = new ColorF(0.508273, 0.508273, 0.508273, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = 0.4;
            return result;
        }
        public static MaterialProperties black_plastic()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "black_plastic";
            result.diffuseColor = new ColorF(0.01, 0.01, 0.01, 0);
            result.emissiveColor = new ColorF(0.0, 0.0, 0.0, 0);
            result.specularColor = new ColorF(0.50, 0.50, 0.50, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .25;
            return result;
        }
        public static MaterialProperties cyan_plastic()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "cyan_plastic";
            result.diffuseColor = new ColorF(0.0, 0.50980392, 0.50980392, 0);
            result.emissiveColor = new ColorF(0.0, 0.1, 0.06, 0);
            result.specularColor = new ColorF(0.50196078, 0.50196078, 0.50196078, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .25;
            return result;
        }
        public static MaterialProperties green_plastic()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "green_plastic";
            result.diffuseColor = new ColorF(0.1, 0.35, 0.1, 0);
            result.emissiveColor = new ColorF(0.0, 0.0, 0.0, 0);
            result.specularColor = new ColorF(0.45, 0.55, 0.45, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .25;
            return result;
        }
        public static MaterialProperties red_plastic()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "red_plastic";
            result.diffuseColor = new ColorF(0.5, 0.0, 0.0, 0);
            result.emissiveColor = new ColorF(0.0, 0.0, 0.0, 0);
            result.specularColor = new ColorF(0.7, 0.6, 0.6, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .25;
            return result;
        }

        public static MaterialProperties white_plastic()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "white_plastic";
            result.diffuseColor = new ColorF(0.55, 0.55, 0.55, 0);
            result.emissiveColor = new ColorF(0.0, 0.0, 0.0, 0);
            result.specularColor = new ColorF(0.70, 0.70, 0.70, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .25;
            return result;
        }
        public static MaterialProperties yellow_plastic()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "yellow_plastic";
            result.diffuseColor = new ColorF(0.5, 0.5, 0.0, 0);
            result.emissiveColor = new ColorF(0.0, 0.0, 0.0, 0);
            result.specularColor = new ColorF(0.60, 0.60, 0.50, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .25;
            return result;
        }
        public static MaterialProperties black_rubber()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "black_rubber";
            result.diffuseColor = new ColorF(0.01, 0.01, 0.01, 0);
            result.emissiveColor = new ColorF(0.02, 0.02, 0.02, 0);
            result.specularColor = new ColorF(0.4, 0.4, 0.4, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .078125;
            return result;
        }
        public static MaterialProperties cyan_rubber()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "cyan_rubber";
            result.diffuseColor = new ColorF(0.4, 0.5, 0.5, 0);
            result.emissiveColor = new ColorF(0.0, 0.05, 0.05, 0);
            result.specularColor = new ColorF(0.04, 0.7, 0.7, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .078125;
            return result;
        }
        public static MaterialProperties green_rubber()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "green_rubber";
            result.diffuseColor = new ColorF(0.4, 0.5, 0.4, 0);
            result.emissiveColor = new ColorF(0.0, 0.05, 0.0, 0);
            result.specularColor = new ColorF(0.04, 0.7, 0.04, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .078125;
            return result;
        }
        public static MaterialProperties red_rubber()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "red_rubber";
            result.diffuseColor = new ColorF(0.5, 0.4, 0.4, 0);
            result.emissiveColor = new ColorF(0.05, 0.0, 0.0, 0);
            result.specularColor = new ColorF(0.7, 0.04, 0.04, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .078125;
            return result;
        }
        public static MaterialProperties white_rubber()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "white_rubber";
            result.diffuseColor = new ColorF(0.5, 0.5, 0.5, 0);
            result.emissiveColor = new ColorF(0.05, 0.05, 0.05, 0);
            result.specularColor = new ColorF(0.7, 0.7, 0.7, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .078125;
            return result;
        }
        public static MaterialProperties yellow_rubber()
        {
            MaterialProperties result = new MaterialProperties();
            //result.Name = "yellow_rubber";
            result.diffuseColor = new ColorF(0.5, 0.5, 0.4, 0);
            result.emissiveColor = new ColorF(0.05, 0.05, 0.0, 0);
            result.specularColor = new ColorF(0.7, 0.7, 0.04, 0);
            result.ambientIntensity = CalcIntensity(result);
            result.transparency = 0;
            result.shininess = .078125;
            return result;
        }

    }
}
