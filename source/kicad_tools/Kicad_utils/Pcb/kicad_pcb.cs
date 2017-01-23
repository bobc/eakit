using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;

using SExpressions;

using Kicad_utils.Drawing;
using Kicad_utils.ModuleDef;

namespace Kicad_utils.Pcb
{
    public class kicad_pcb
    {
        public SExpression RootNode;

        public string FileVersion;     // version <number>
        public string Host_Name;       // app_name version
        public string Host_Version;    // app_name version
        public General General;
        public string Page;    // "A3", "A4"
        public List<PcbLayer> Layers;
        public Setup Setup;
        public List<Net> Nets; // list of nets #nets
        public List<NetClass> NetClasses;
        public List<Module> Modules; // #modules
        public List<graphic_base> Drawings;    // list of graphic items : #drawings


        public List<SExpression> UnParsed;

        // links?
        // no-connects?
        // tracks
        // zones

        //public List<Segment> Segments;

        // public List<Via> Vias;
        // public List<Zone> Zone;


        public kicad_pcb()
        {
            RootNode = new SExpression();

            //RootNode.Name = "kicad_pcb";
            //RootNode.Items = new List<SNodeBase>();
            //RootNode.Items.Add(new SExpression("version", new List<SNodeBase>() { new SNodeNumber(3) }));

            FileVersion = "3";
            Host_Name = "pcbnew";
            Host_Version = "(2013-07-07 BZR 4022)-stable";  //??
            General = new General();
            Page = "A4";
            Layers = PcbLayer.DefaultLayers();

            Setup = new Setup();
            Nets = new List<Net>();
            Nets.Add(new Net(0, ""));
            NetClasses = new List<NetClass>();
            NetClasses.Add(NetClass.DefaultNetclass());
            Modules = null;
            Drawings = null;
        }

        public void AddModule(Module module, PointF position)
        {
            module.position = new Position(position.X, position.Y);

            if (Modules == null)
                Modules = new List<Module>();

            Modules.Add(module);
        }

        private void Trace (string message, string data="")
        {
            // Console.WriteLine(message, data);
        }

        public bool LoadFromFile(string filename)
        {
            bool result = false;

            RootNode.LoadFromFile(filename);

            UnParsed = new List<SExpression>();

            if (RootNode.Name == "kicad_pcb")
            {
                SNodeBase pcb_node = RootNode;

                SExpression pcb_items = (pcb_node as SExpression);

                foreach (SExpression node in pcb_items.Items)
                {
                    string name = node.Name;

                    if (name == "gr_line")
                    {
                        //
                        gr_line g = gr_line.Parse(node);
                        if (Drawings == null)
                            Drawings = new List<graphic_base>();
                        Drawings.Add(g);
                    }
                    else if (name == "gr_text")
                    {
                        //
                        gr_text g = gr_text.Parse(node);
                        if (Drawings == null)
                            Drawings = new List<graphic_base>();
                        Drawings.Add(g);
                    }
                    else if (name == "gr_arc")
                    {
                        gr_arc g = gr_arc.Parse(node);
                        if (Drawings == null)
                            Drawings = new List<graphic_base>();
                        Drawings.Add(g);
                    }
                    else if (name == "gr_circle")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "module")
                    {
                        Module module = Module.Parse(node);
                        if (Modules == null)
                            Modules = new List<Module>();
                        Modules.Add(module);
                    }
                    else if (name == "version")
                    {
                        FileVersion = node.GetString();
                    }
                    else if (name == "host")
                    {
                        Host_Name = (node.Items[0] as SNodeAtom).Value;
                        Host_Version = (node.Items[1] as SNodeAtom).Value;
                    }
                    else if (name == "general")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "page")
                    {
                        Page = node.GetString();
                    }
                    else if (name == "title_block")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "layers")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "setup")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "net")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "net_class")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "dimension")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "segment")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "via")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else if (name == "zone")
                    {
                        Trace("LoadPcb: todo: {0}", name);
                    }
                    else
                    {
                        Trace("LoadPcb: unrecognised: {0}", name);
                    }
                }
            }

            return result;
        }

        public void SaveToFile(string filename)
        {
            filename = Path.ChangeExtension(filename, "kicad_pcb");

            if (Drawings == null)
                General.drawings = 0;
            else
                General.drawings = Drawings.Count;

            if (Modules == null)
                General.modules = 0;
            else
                General.modules = Modules.Count;

            if (Nets == null)
                General.nets = 0;
            else
                General.nets = Nets.Count;

            //

            RootNode = new SExpression();
            List<SExpression> sex_list;

            RootNode.Name = "kicad_pcb";
            RootNode.Items = new List<SNodeBase>();

            RootNode.Items.Add(new SExpression("version", new List<SNodeBase>() { new SNodeAtom(3) }));
            RootNode.Items.Add(new SExpression("host", new List<SNodeBase>() { new SNodeAtom(Host_Name), new SNodeAtom(Host_Version) }));
            RootNode.Items.Add(General.GetSExpression());
            RootNode.Items.Add(new SExpression("page", new List<SNodeBase>() { new SNodeAtom(Page) }));
            RootNode.Items.Add(PcbLayer.GetSExpression(Layers));
            RootNode.Items.Add(Setup.GetSExpression());

            sex_list = Net.GetSExpressionList(Nets);
            foreach (SExpression sex in sex_list)
                RootNode.Items.Add(sex);

            sex_list = NetClass.GetSExpressionList(NetClasses);
            foreach (SExpression sex in sex_list)
                RootNode.Items.Add(sex);

            if (Modules != null)
            {
                sex_list = Module.GetSExpressionList(Modules);
                foreach (SExpression sex in sex_list)
                    RootNode.Items.Add(sex);
            }

            if (Drawings != null)
            {
                sex_list = graphic_base.GetSExpressionList(Drawings);
                foreach (SExpression sex in sex_list)
                    RootNode.Items.Add(sex);
            }
            //
            RootNode.WriteToFile(filename);
        }


        //
        Color GetColorFromLayer (string layer)
        {
            switch (layer)
            {
                case "Edge.Cuts": return Color.Yellow;
                case "Dwgs.User": return Color.White;
                case "F.SilkS": return Color.Teal;
            }

            return Color.White;
        }

        public void Render(Canvas canvas)
        {
            canvas.color_background = Color.Black;

            // units in mm?
            canvas.Min = new PointF(0, 0);
            canvas.Max = new PointF(297, 210);
            canvas.grid_spacing = 10;

            canvas.Ydir = 1; // +ve Y is down the page

            canvas.Initialise();

            foreach (graphic_base node in this.Drawings)
            {
                if (node is gr_line)
                {
                    gr_line gr_line = node as gr_line;

                    Point sp1 = canvas.ToScreen(gr_line.start);
                    Point sp2 = canvas.ToScreen(gr_line.end);

                    Pen pen = new Pen(GetColorFromLayer(gr_line.layer));
                    canvas.g.DrawLine(pen, sp1, sp2);
                }
                else if (node is gr_text)
                {
                    gr_text text = node as gr_text;

                    Point sp1 = canvas.ToScreen(text.at);
                    Font font = new Font ("Arial", 10, FontStyle.Regular,GraphicsUnit.Pixel);

                    canvas.g.DrawString(text.Value, font, Brushes.White, sp1);
                }
            }
        }


    }


}
