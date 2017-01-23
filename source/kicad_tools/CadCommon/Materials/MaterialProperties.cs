using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace CadCommon
{
    public class MaterialProperties
    {
        public string Name;

        public ColorF diffuseColor;
        public double transparency; // 0 = opaque

        public ColorF emissiveColor;
        public ColorF specularColor;
        public double ambientIntensity;
        public double shininess;

        public MaterialProperties()
        {
            diffuseColor = new ColorF(0.8, 0.8, 0.8, 0.0);
            ambientIntensity = 1.0;
            emissiveColor = new ColorF(0, 0, 0, 0);
            specularColor = new ColorF(0, 0, 0, 0);
            shininess = 0.2;
            transparency = 0.0;
        }

        public MaterialProperties(MaterialProperties sourceProperties)
        {
            if (sourceProperties == null)
            {
                diffuseColor = new ColorF(0.8, 0.8, 0.8, 0.0);
                ambientIntensity = 1.0;
                emissiveColor = new ColorF(0, 0, 0, 0);
                specularColor = new ColorF(0, 0, 0, 0);
                shininess = 0.2;
                transparency = 0.0;
            }
            else
            {
                Name = sourceProperties.Name;
                diffuseColor = new ColorF(sourceProperties.diffuseColor);
                ambientIntensity = sourceProperties.ambientIntensity;
                emissiveColor = new ColorF(sourceProperties.emissiveColor);
                specularColor = new ColorF(sourceProperties.specularColor);
                shininess = sourceProperties.shininess;
                transparency = sourceProperties.transparency;
            }
        }

        // Get the diffuse color as ARGB
        public Color GetColor()
        {
            int r, g, b, a;

            r = ColorF.ByteRange(this.diffuseColor.R);
            g = ColorF.ByteRange(this.diffuseColor.G);
            b = ColorF.ByteRange(this.diffuseColor.B);
            a = 255 - ColorF.ByteRange(this.transparency);  // 255 = opaque  //??

            return Color.FromArgb(a, r, g, b);
        }

    }
}
