﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;
using Kicad_utils.ModuleDef;

namespace Kicad_utils.Pcb
{
    public class Via
    {
        public PointF at;
        public float size;
        public float drill;
        public string topmost_layer;   // topmost
        public string backmost_layer;
        public int net;
        public uint tstamp;

        public Via()
        {
        }

        public Via(PointF pos, float size, float drill, string topmost_layer, string backmost_layer, int net)
        {
            this.at = pos;
            this.size = size;
            this.drill = drill;
            this.topmost_layer = topmost_layer;
            this.backmost_layer = backmost_layer;
            this.net = net;
        }


        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "via";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SExpression("at", new List<SNodeBase>{
                new SNodeAtom(at.X),
                new SNodeAtom(at.Y)
            }));
            result.Items.Add(new SExpression("size", size));
            result.Items.Add(new SExpression("drill", drill));
            result.Items.Add(new SExpression("layers", new List<SNodeBase>{
                new SNodeAtom(topmost_layer),
                new SNodeAtom(backmost_layer)
                }));
            result.Items.Add(new SExpression("net", net));
            result.Items.Add(new SExpression("tstamp", tstamp.ToString("X8")));

            return result;
        }

        public static List<SExpression> GetSExpressionList(List<Via> list)
        {
            List<SExpression> result = new List<SExpression>();
            foreach (Via item in list)
            {
                result.Add(item.GetSExpression());
            }
            return result;
        }


        public static Via Parse(SExpression root_node)
        {
            Via result;

            if ((root_node is SExpression) && ((root_node as SExpression).Name == "via"))
            {
                result = new Via();
                int index = 0;

                //
                while (index < root_node.Items.Count)
                {
                    SNodeBase node = root_node.Items[index];
                    SExpression sub = node as SExpression;

                    switch (sub.Name)
                    {
                        case "at":
                            {
                                float x = float.Parse((sub.Items[0] as SNodeAtom).Value);
                                float y = float.Parse((sub.Items[1] as SNodeAtom).Value);
                                result.at = new PointF(x, y);
                            }
                            break;
                        case "size":
                            result.size = sub.GetFloat();
                            break;
                        case "drill":
                            result.drill = sub.GetFloat();
                            break;
                        case "layers":
                            result.topmost_layer = sub.GetString(0);
                            result.topmost_layer = sub.GetString(1);
                            break;
                        case "net":
                            result.net = sub.GetInt();
                            break;
                        case "tstamp":
                            result.tstamp = sub.GetUintHex();
                            break;
                    }
                    index++;
                }

                return result;
            }
            else
                return null;  // error
        }

    }
}
