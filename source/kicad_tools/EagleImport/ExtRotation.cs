using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EagleImport
{
    public class ExtRotation
    {
        public float Rotation;
        public bool Mirror;
        public bool Spin;

        public ExtRotation()
        {
            Rotation = 0;
            Mirror = false;
            Spin = false;
        }

        // ( [M] | [S] ) [ R 0 to 359.9 ]
        public ExtRotation(string rot)
        {
            Rotation = 0;
            Mirror = false;
            Spin = false;

            if (!string.IsNullOrEmpty(rot))
            {
                while (!string.IsNullOrEmpty(rot))
                {
                    switch (rot[0])
                    {
                        case 'M': Mirror = true;
                            rot = rot.Substring(1);
                            break;
                        case 'S':
                            Spin = true;
                            rot = rot.Substring(1);
                            break;
                        case 'R':
                            // R is assumed to be last
                            rot = rot.Substring(1);
                            float.TryParse(rot, out Rotation);
                            return;
                            
                        default:
                            // invalid?
                            return;
                    }
                }
            }
        }

        public static ExtRotation Parse (string rot)
        {
            return new ExtRotation(rot);
        }


    }
}
