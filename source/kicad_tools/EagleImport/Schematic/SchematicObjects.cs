using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace EagleImport.Schematic
{
    [XmlRoot(ElementName = "setting")]
    public class Setting
    {
        [XmlAttribute(AttributeName = "alwaysvectorfont")]
        public string Alwaysvectorfont { get; set; }
        [XmlAttribute(AttributeName = "verticaltext")]
        public string Verticaltext { get; set; }
    }

    [XmlRoot(ElementName = "settings")]
    public class Settings
    {
        [XmlElement(ElementName = "setting")]
        public List<Setting> Setting { get; set; }
    }

    [XmlRoot(ElementName = "grid")]
    public class Grid
    {
        [XmlAttribute(AttributeName = "distance")]
        public string Distance { get; set; }
        [XmlAttribute(AttributeName = "unitdist")]
        public string Unitdist { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlAttribute(AttributeName = "style")]
        public string Style { get; set; }
        [XmlAttribute(AttributeName = "multiple")]
        public string Multiple { get; set; }
        [XmlAttribute(AttributeName = "display")]
        public string Display { get; set; }
        [XmlAttribute(AttributeName = "altdistance")]
        public string Altdistance { get; set; }
        [XmlAttribute(AttributeName = "altunitdist")]
        public string Altunitdist { get; set; }
        [XmlAttribute(AttributeName = "altunit")]
        public string Altunit { get; set; }
    }

    [XmlRoot(ElementName = "layer")]
    public class Layer
    {
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "color")]
        public string Color { get; set; }
        [XmlAttribute(AttributeName = "fill")]
        public string Fill { get; set; }
        [XmlAttribute(AttributeName = "visible")]
        public string Visible { get; set; }
        [XmlAttribute(AttributeName = "active")]
        public string Active { get; set; }
    }

    [XmlRoot(ElementName = "layers")]
    public class Layers
    {
        [XmlElement(ElementName = "layer")]
        public List<Layer> Layer { get; set; }
    }

    [XmlRoot(ElementName = "wire")]
    public class Wire
    {
        [XmlAttribute(AttributeName = "x1")]
        public string X1 { get; set; }
        [XmlAttribute(AttributeName = "y1")]
        public string Y1 { get; set; }
        [XmlAttribute(AttributeName = "x2")]
        public string X2 { get; set; }
        [XmlAttribute(AttributeName = "y2")]
        public string Y2 { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "curve")]
        public string Curve { get; set; }
        [XmlAttribute(AttributeName = "cap")]
        public string Cap { get; set; }
        [XmlAttribute(AttributeName = "style")]
        public string Style { get; set; }
    }

    [XmlRoot(ElementName = "circle")]
    public class Circle
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "radius")]
        public string Radius { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
    }

    [XmlRoot(ElementName = "smd")]
    public class Smd
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "dx")]
        public string Dx { get; set; }
        [XmlAttribute(AttributeName = "dy")]
        public string Dy { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
        [XmlAttribute(AttributeName = "roundness")]
        public string Roundness { get; set; }
    }

    [XmlRoot(ElementName = "text")]
    public class Text
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "ratio")]
        public string Ratio { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
        [XmlText]
        public string mText { get; set; }
        [XmlAttribute(AttributeName = "font")]
        public string Font { get; set; }
    }

    [XmlRoot(ElementName = "package")]
    public class Package
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        //
        [XmlElement(ElementName = "wire")]
        public List<Wire> Wire { get; set; }

        [XmlElement(ElementName = "circle")]
        public List<Circle> Circle { get; set; }

        [XmlElement(ElementName = "smd")]
        public List<Smd> Smd { get; set; }

        [XmlElement(ElementName = "text")]
        public List<Text> Text { get; set; }

        [XmlElement(ElementName = "pad")]
        public List<Pad> Pad { get; set; }

        [XmlElement(ElementName = "rectangle")]
        public List<Rectangle> Rectangle { get; set; }

        [XmlElement(ElementName = "hole")]
        public List<Hole> Hole { get; set; }

        [XmlElement(ElementName = "polygon")]
        public List<Polygon> Polygon { get; set; }
    }

    [XmlRoot(ElementName = "packages")]
    public class Packages
    {
        [XmlElement(ElementName = "package")]
        public List<Package> Package { get; set; }
    }

    [XmlRoot(ElementName = "pin")]
    public class Pin
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "visible")]
        public string Visible { get; set; }
        [XmlAttribute(AttributeName = "length")]
        public string Length { get; set; }
        [XmlAttribute(AttributeName = "direction")]
        public string Direction { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
        [XmlAttribute(AttributeName = "swaplevel")]
        public string Swaplevel { get; set; }
    }

    [XmlRoot(ElementName = "symbol")]
    public class Symbol
    {
        [XmlElement(ElementName = "wire")]
        public List<Wire> Wire { get; set; }
        [XmlElement(ElementName = "text")]
        public List<Text> Text { get; set; }
        [XmlElement(ElementName = "pin")]
        public List<Pin> Pin { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "rectangle")]
        public List<Rectangle> Rectangle { get; set; }
        [XmlElement(ElementName = "frame")]
        public Frame Frame { get; set; }
        [XmlElement(ElementName = "circle")]
        public List<Circle> Circle { get; set; }
        [XmlElement(ElementName = "polygon")]
        public Polygon Polygon { get; set; }
    }

    [XmlRoot(ElementName = "symbols")]
    public class Symbols
    {
        [XmlElement(ElementName = "symbol")]
        public List<Symbol> Symbol { get; set; }
    }

    [XmlRoot(ElementName = "gate")]
    public class Gate
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "symbol")]
        public string Symbol { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
    }

    [XmlRoot(ElementName = "gates")]
    public class Gates
    {
        [XmlElement(ElementName = "gate")]
        public List<Gate> Gate { get; set; }
    }

    [XmlRoot(ElementName = "connect")]
    public class Connect
    {
        [XmlAttribute(AttributeName = "gate")]
        public string Gate { get; set; }
        [XmlAttribute(AttributeName = "pin")]
        public string Pin { get; set; }
        [XmlAttribute(AttributeName = "pad")]
        public string Pad { get; set; }
    }

    [XmlRoot(ElementName = "connects")]
    public class Connects
    {
        [XmlElement(ElementName = "connect")]
        public List<Connect> Connect { get; set; }
    }

    [XmlRoot(ElementName = "technology")]
    public class Technology
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "technologies")]
    public class Technologies
    {
        [XmlElement(ElementName = "technology")]
        public Technology Technology { get; set; }
    }

    [XmlRoot(ElementName = "device")]
    public class Device
    {
        [XmlElement(ElementName = "connects")]
        public Connects Connects { get; set; }
        [XmlElement(ElementName = "technologies")]
        public Technologies Technologies { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "package")]
        public string Package { get; set; }

        public Device() { }

        public Device(string name, string package)
        {
            this.Name = name;
            this.Package = package;
        }
    }

    [XmlRoot(ElementName = "devices")]
    public class Devices
    {
        [XmlElement(ElementName = "device")]
        public List<Device> Device { get; set; }
    }

    [XmlRoot(ElementName = "deviceset")]
    public class Deviceset
    {
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "gates")]
        public Gates Gates { get; set; }

        [XmlElement(ElementName = "devices")]
        public Devices Devices { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "prefix")]
        public string Prefix { get; set; }

        [XmlAttribute(AttributeName = "uservalue")]
        public string Uservalue { get; set; }
    }

    [XmlRoot(ElementName = "devicesets")]
    public class Devicesets
    {
        [XmlElement(ElementName = "deviceset")]
        public List<Deviceset> Deviceset { get; set; }
    }

    [XmlRoot(ElementName = "library")]
    public class Library
    {
        [XmlElement(ElementName = "packages")]
        public Packages Packages { get; set; }
        [XmlElement(ElementName = "symbols")]
        public Symbols Symbols { get; set; }
        [XmlElement(ElementName = "devicesets")]
        public Devicesets Devicesets { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "pad")]
    public class Pad
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "drill")]
        public string Drill { get; set; }
        [XmlAttribute(AttributeName = "shape")]
        public string Shape { get; set; }
        [XmlAttribute(AttributeName = "diameter")]
        public string Diameter { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
    }

    [XmlRoot(ElementName = "rectangle")]
    public class Rectangle
    {
        [XmlAttribute(AttributeName = "x1")]
        public string X1 { get; set; }
        [XmlAttribute(AttributeName = "y1")]
        public string Y1 { get; set; }
        [XmlAttribute(AttributeName = "x2")]
        public string X2 { get; set; }
        [XmlAttribute(AttributeName = "y2")]
        public string Y2 { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
    }

    [XmlRoot(ElementName = "hole")]
    public class Hole
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "drill")]
        public string Drill { get; set; }
    }

    [XmlRoot(ElementName = "vertex")]
    public class Vertex
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "curve")]
        public string Curve { get; set; }
    }

    [XmlRoot(ElementName = "polygon")]
    public class Polygon
    {
        [XmlElement(ElementName = "vertex")]
        public List<Vertex> Vertex { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
    }

    [XmlRoot(ElementName = "frame")]
    public class Frame
    {
        [XmlAttribute(AttributeName = "x1")]
        public string X1 { get; set; }
        [XmlAttribute(AttributeName = "y1")]
        public string Y1 { get; set; }
        [XmlAttribute(AttributeName = "x2")]
        public string X2 { get; set; }
        [XmlAttribute(AttributeName = "y2")]
        public string Y2 { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public string Columns { get; set; }
        [XmlAttribute(AttributeName = "rows")]
        public string Rows { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
    }

    [XmlRoot(ElementName = "libraries")]
    public class Libraries
    {
        [XmlElement(ElementName = "library")]
        public List<Library> Library { get; set; }
    }

    [XmlRoot(ElementName = "class")]
    public class Class
    {
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "drill")]
        public string Drill { get; set; }
    }

    [XmlRoot(ElementName = "classes")]
    public class Classes
    {
        [XmlElement(ElementName = "class")]
        public Class Class { get; set; }
    }

    [XmlRoot(ElementName = "part")]
    public class Part
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "library")]
        public string Library { get; set; }
        [XmlAttribute(AttributeName = "deviceset")]
        public string Deviceset { get; set; }
        [XmlAttribute(AttributeName = "device")]
        public string Device { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlElement(ElementName = "attribute")]
        public Attribute Attribute { get; set; }
    }

    [XmlRoot(ElementName = "attribute")]
    public class Attribute
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
    }

    [XmlRoot(ElementName = "parts")]
    public class Parts
    {
        [XmlElement(ElementName = "part")]
        public List<Part> Part { get; set; }
    }

    [XmlRoot(ElementName = "plain")]
    public class Plain
    {
        [XmlElement(ElementName = "text")]
        public List<Text> Text { get; set; }
        [XmlElement(ElementName = "wire")]
        public List<Wire> Wire { get; set; }
    }

    [XmlRoot(ElementName = "instance")]
    public class Instance
    {
        [XmlElement(ElementName = "attribute")]
        public List<Attribute> Attribute { get; set; }
        [XmlAttribute(AttributeName = "part")]
        public string Part { get; set; }
        [XmlAttribute(AttributeName = "gate")]
        public string Gate { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "smashed")]
        public string Smashed { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
    }

    [XmlRoot(ElementName = "instances")]
    public class Instances
    {
        [XmlElement(ElementName = "instance")]
        public List<Instance> Instance { get; set; }
    }

    [XmlRoot(ElementName = "junction")]
    public class Junction
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
    }

    [XmlRoot(ElementName = "pinref")]
    public class Pinref
    {
        [XmlAttribute(AttributeName = "part")]
        public string Part { get; set; }
        [XmlAttribute(AttributeName = "gate")]
        public string Gate { get; set; }
        [XmlAttribute(AttributeName = "pin")]
        public string Pin { get; set; }
    }

    [XmlRoot(ElementName = "segment")]
    public class Segment
    {
        [XmlElement(ElementName = "wire")]
        public List<Wire> Wire { get; set; }

        [XmlElement(ElementName = "junction")]
        public List<Junction> Junction { get; set; }

        [XmlElement(ElementName = "pinref")]
        public List<Pinref> Pinref { get; set; }

        [XmlElement(ElementName = "label")]
        public List<Label> Label { get; set; }
    }

    [XmlRoot(ElementName = "net")]
    public class Net
    {
        [XmlElement(ElementName = "segment")]
        public List<Segment> Segment { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
    }

    [XmlRoot(ElementName = "label")]
    public class Label
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
    }

    [XmlRoot(ElementName = "nets")]
    public class Nets
    {
        [XmlElement(ElementName = "net")]
        public List<Net> Net { get; set; }
    }

    [XmlRoot(ElementName = "sheet")]
    public class Sheet
    {
        [XmlElement(ElementName = "plain")]
        public Plain Plain { get; set; }
        [XmlElement(ElementName = "instances")]
        public Instances Instances { get; set; }
        [XmlElement(ElementName = "busses")]
        public string Busses { get; set; }
        [XmlElement(ElementName = "nets")]
        public Nets Nets { get; set; }
    }

    [XmlRoot(ElementName = "sheets")]
    public class Sheets
    {
        [XmlElement(ElementName = "sheet")]
        public List<Sheet> Sheet { get; set; }
    }

    [XmlRoot(ElementName = "schematic")]
    public class Schematic
    {
        [XmlElement(ElementName = "libraries")]
        public Libraries Libraries { get; set; }
        [XmlElement(ElementName = "attributes")]
        public string Attributes { get; set; }
        [XmlElement(ElementName = "variantdefs")]
        public string Variantdefs { get; set; }
        [XmlElement(ElementName = "classes")]
        public Classes Classes { get; set; }
        [XmlElement(ElementName = "parts")]
        public Parts Parts { get; set; }

        [XmlElement(ElementName = "sheets")]
        public Sheets Sheets { get; set; }
    }

    [XmlRoot(ElementName = "drawing")]
    public class Drawing
    {
        [XmlElement(ElementName = "settings")]
        public Settings Settings { get; set; }
        [XmlElement(ElementName = "grid")]
        public Grid Grid { get; set; }
        [XmlElement(ElementName = "layers")]
        public Layers Layers { get; set; }
        [XmlElement(ElementName = "schematic")]
        public Schematic Schematic { get; set; }
    }

}
