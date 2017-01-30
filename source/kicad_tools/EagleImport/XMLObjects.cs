using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace EagleImport
{
    [System.SerializableAttribute()]
    public enum Bool
    {
        no, yes,
    }

    [System.SerializableAttribute()]
    public enum GridUnit
    {
        mic, mm, mil, inch
    }

    [System.SerializableAttribute()]
    public enum GridStyle
    {
        lines, dots
    }

    [System.SerializableAttribute()]
    public enum WireStyle
    {
        continuous, longdash, shortdash, dashdot
    }

    [System.SerializableAttribute()]
    public enum WireCap
    {
        flat, round
    }

    [System.SerializableAttribute()]
    public enum PadShape
    {
        square, round, octagon, @long, offset
    }

    [System.SerializableAttribute()]
    public enum ViaShape
    {
        square, round, octagon
    }

    [System.SerializableAttribute()]
    public enum TextFont
    {
        vector, proportional, @fixed
    }

    [System.SerializableAttribute()]
    public enum AttributeDisplay
    {
        off, value, name, both
    }

    [System.SerializableAttribute()]
    public enum PolygonPour
    {
        solid, hatch, cutout
    }

    [System.SerializableAttribute()]
    public enum PinVisible
    {
        off, pad, pin, both
    }

    [System.SerializableAttribute()]
    public enum PinLength
    {
        point, @short, middle, @long,
    }

    [System.SerializableAttribute()]
    public enum PinDirection
    {
        nc, @in, @out, io, oc, pwr, pas, hiz, sup
    }

    [System.SerializableAttribute()]
    public enum PinFunction
    {
        none, dot, clk, dotclk
    }

    [System.SerializableAttribute()]
    public enum GateAddLevel
    {
        must, can, next, request, always
    }

    [System.SerializableAttribute()]
    public enum ContactRoute
    {
        all, any
    }
    [System.SerializableAttribute()]
    public enum DimensionType
    {
        parallel, horizontal, vertical, radius, diameter, leader
    }

    [System.SerializableAttribute()]
    public enum Severity
    {
        info, warning, error
    }

    [System.SerializableAttribute()]
    public enum Align
    {
        [System.Xml.Serialization.XmlEnumAttribute("bottom-left")]
        bottom_left,
        [System.Xml.Serialization.XmlEnumAttribute("bottom-center")]
        bottom_center,
        [System.Xml.Serialization.XmlEnumAttribute("bottom-right")]
        bottom_right,
        [System.Xml.Serialization.XmlEnumAttribute("center-left")]
        center_left,
        center,
        [System.Xml.Serialization.XmlEnumAttribute("center-right")]
        center_right,
        [System.Xml.Serialization.XmlEnumAttribute("top-left")]
        top_left,
        [System.Xml.Serialization.XmlEnumAttribute("top-center")]
        top_center,
        [System.Xml.Serialization.XmlEnumAttribute("top-right")]
        top_right,
    }

    [System.SerializableAttribute()]
    public enum VerticalText
    {
        up, down
    }

    [XmlRoot(ElementName = "approved")]
    public class Approved
    {
        [XmlAttribute(AttributeName = "hash")]
        public string Hash { get; set; }
    }

    [XmlRoot(ElementName = "attribute")]
    public class Attribute {

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
        [XmlAttribute(AttributeName = "font")]
        public TextFont Font { get; set; }
        [XmlAttribute(AttributeName = "ratio")]
        public string Ratio { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }

        [XmlAttribute(AttributeName = "constant")]
        public Bool Constant { get; set; }
        [XmlAttribute(AttributeName = "display")]
        public AttributeDisplay Display { get; set; }

        // display: Only in <element> or <instance> context
        // constant:Only in <device> context

        public Attribute()
        {
            Rot = "R0";
            Display = AttributeDisplay.value;
            Constant = Bool.no;
        }
    }

    [XmlRoot(ElementName = "attributes")]
    public class Attributes
    {
        [XmlElement(ElementName = "attribue")]
        public List<Attribute> Attribute { get; set; }
    }

    [XmlRoot(ElementName = "autorouter")]
    public class Autorouter {
        [XmlElement(ElementName = "pass")]
        public List<Pass> Pass { get; set; }
    }

    [XmlRoot(ElementName = "board")]
    public class Board {

        [XmlElement(ElementName = "description")]
        public Description Description { get; set; }

        [XmlElement(ElementName = "plain")]
        public Plain Plain { get; set; }

        [XmlElement(ElementName = "libraries")]
        public Libraries Libraries { get; set; }

        [XmlElement(ElementName = "attributes")]
        public Attributes Attributes { get; set; }

        [XmlElement(ElementName = "variantdefs")]
        public Variantdefs Variantdefs { get; set; }

        [XmlElement(ElementName = "classes")]
        public Classes Classes { get; set; }

        [XmlElement(ElementName = "designrules")]
        public Designrules Designrules { get; set; }

        [XmlElement(ElementName = "autorouter")]
        public Autorouter Autorouter { get; set; }

        [XmlElement(ElementName = "elements")]
        public Elements Elements { get; set; }

        [XmlElement(ElementName = "signals")]
        public Signals Signals { get; set; }

        [XmlElement(ElementName = "errors")]
        public Errors Errors { get; set; }
    }


    [XmlRoot(ElementName = "bus")]
    public class Bus
    {
        [XmlElement(ElementName = "segment")]
        public List<Segment> Segment { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "busses")]
    public class Busses
    {
        [XmlElement(ElementName = "bus")]
        public List<Bus> Bus { get; set; }
    }

    [XmlRoot(ElementName = "circle")]
    public class Circle {
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

    [XmlRoot(ElementName = "class")]
    public class Class {
        [XmlAttribute(AttributeName = "drill")]
        public string Drill { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
    }

    [XmlRoot(ElementName = "classes")]
    public class Classes
    {
        [XmlElement(ElementName = "class")]
        public Class Class { get; set; }
    }

    [XmlRoot(ElementName = "compatibility")]
    public class Compatibility
    {
        [XmlElement(ElementName = "note")]
        public List<Note> Note { get; set; }
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

    [XmlRoot(ElementName = "contactref")]
    public class Contactref {
        [XmlAttribute(AttributeName = "element")]
        public string Element { get; set; }
        [XmlAttribute(AttributeName = "pad")]
        public string Pad { get; set; }
    }

    [XmlRoot(ElementName = "description")]
    public class Description {
        [XmlAttribute(AttributeName = "language")]
        public string Language { get; set; }

        [XmlText]
        public string Text { get; set; }

        public Description()
        {
            Language = "en";
        }
    }

    [XmlRoot(ElementName = "designrules")]
    public class Designrules {
        [XmlElement(ElementName = "description")]
        public Description Description { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "param")]
        public List<Param> Param { get; set; }
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
        public Description Description { get; set; }

        [XmlElement(ElementName = "gates")]
        public Gates Gates { get; set; }

        [XmlElement(ElementName = "devices")]
        public Devices Devices { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "prefix")]
        public string Prefix { get; set; }

        [XmlAttribute(AttributeName = "uservalue")]
        public Bool Uservalue { get; set; }

        public Deviceset()
        {
            Prefix = "";
            Uservalue = Bool.no;
        }
    }

    [XmlRoot(ElementName = "devicesets")]
    public class Devicesets
    {
        [XmlElement(ElementName = "deviceset")]
        public List<Deviceset> Deviceset { get; set; }
    }

    [XmlRoot(ElementName = "dimension")]
    public class Dimension
    {
        [XmlAttribute(AttributeName = "x1")]
        public string X1 { get; set; }
        [XmlAttribute(AttributeName = "y1")]
        public string Y1 { get; set; }
        [XmlAttribute(AttributeName = "x2")]
        public string X2 { get; set; }
        [XmlAttribute(AttributeName = "y2")]
        public string Y2 { get; set; }
        [XmlAttribute(AttributeName = "x3")]
        public string X3 { get; set; }
        [XmlAttribute(AttributeName = "y3")]
        public string Y3 { get; set; }

        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }

        [XmlAttribute(AttributeName = "dtype")]
        public DimensionType Dtype { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }

        [XmlAttribute(AttributeName = "extwidth")]
        public string ExtWidth { get; set; }

        [XmlAttribute(AttributeName = "extlength")]
        public string ExtLength { get; set; }

        [XmlAttribute(AttributeName = "extoffset")]
        public string ExtOffset { get; set; }

        [XmlAttribute(AttributeName = "textsize")]
        public string TextSize { get; set; }

        [XmlAttribute(AttributeName = "textratio")]
        public string TextRatio { get; set; }

        [XmlAttribute(AttributeName = "unit")]
        public GridUnit Unit{ get; set; }

        [XmlAttribute(AttributeName = "precision")]
        public string Precision { get; set; }

        [XmlAttribute(AttributeName = "visible")]
        public Bool Visible{ get; set; }

        public Dimension()
        {
            Dtype = DimensionType.parallel;
            ExtWidth = "0";
            ExtLength = "0";
            ExtOffset = "0";
            TextRatio = "8";
            Unit = GridUnit.mm;
            Precision = "2";
            Visible = Bool.no;
        }
    }

    /// <summary>
    /// schematic drawing
    /// </summary>
    [XmlRoot(ElementName = "drawing")]
    public class Drawing
    {
        [XmlElement(ElementName = "settings")]
        public Settings Settings { get; set; }

        [XmlElement(ElementName = "grid")]
        public Grid Grid { get; set; }

        [XmlElement(ElementName = "layers")]
        public Layers Layers { get; set; }

        // only one of the following must be present
        [XmlElement(ElementName="board")]
		public Board Board { get; set; }

        [XmlElement(ElementName = "schematic")]
        public Schematic Schematic { get; set; }

        [XmlElement(ElementName = "library")]
        public Library Library { get; set; }
    }

    [XmlRoot(ElementName="element")]
	public class Element {
		[XmlElement(ElementName="attribute")]
		public List<Attribute> Attribute { get; set; }
		[XmlAttribute(AttributeName="library")]
		public string Library { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="package")]
		public string Package { get; set; }
		[XmlAttribute(AttributeName="rot")]
		public string Rot { get; set; }
		[XmlAttribute(AttributeName="smashed")]
		public string Smashed { get; set; }
		[XmlAttribute(AttributeName="value")]
		public string Value { get; set; }
		[XmlAttribute(AttributeName="x")]
		public string X { get; set; }
		[XmlAttribute(AttributeName="y")]
		public string Y { get; set; }
	}

	[XmlRoot(ElementName="elements")]
	public class Elements {
		[XmlElement(ElementName="element")]
		public List<Element> Element { get; set; }
	}

    [XmlRoot(ElementName = "errors")]
    public class Errors
    {
        [XmlElement(ElementName = "approved")]
        public List<Approved> Approved { get; set; }
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

    [XmlRoot(ElementName = "grid")]
    public class Grid
    {
        [XmlAttribute(AttributeName = "distance")]
        public string Distance { get; set; }

        [XmlAttribute(AttributeName = "unitdist")]
        public GridUnit Unitdist { get; set; }

        [XmlAttribute(AttributeName = "unit")]
        public GridUnit Unit { get; set; }

        [XmlAttribute(AttributeName = "style")]
        public GridStyle Style { get; set; }

        [XmlAttribute(AttributeName = "multiple")]
        public string Multiple { get; set; }

        [XmlAttribute(AttributeName = "display")]
        public Bool Display { get; set; }

        [XmlAttribute(AttributeName = "altdistance")]
        public string Altdistance { get; set; }

		[XmlAttribute(AttributeName="altunitdist")]
		public GridUnit Altunitdist { get; set; }

        [XmlAttribute(AttributeName = "altunit")]
        public GridUnit Altunit { get; set; }

        public Grid()
        {
            Style = GridStyle.lines;
            Multiple = "1";
            Display = Bool.no;
        }
    }

    [XmlRoot(ElementName = "hole")]
    public class Hole
    {
        [XmlAttribute(AttributeName = "drill")]
        public string Drill { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
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
        public Bool Smashed { get; set; }

        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }

        //  <!-- rot: Only 0, 90, 180 or 270 -->
        public Instance ()
        {
            Rot = "R0";
            Smashed = Bool.no;
        }
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
        public Bool Visible { get; set; }

        [XmlAttribute(AttributeName = "active")]
        public Bool Active { get; set; }

        public Layer ()
        {
            Visible = Bool.yes;
            Active = Bool.yes;
        }
    }

    [XmlRoot(ElementName = "layers")]
    public class Layers
    {
        [XmlElement(ElementName = "layer")]
        public List<Layer> Layer { get; set; }
    }

    [XmlRoot(ElementName = "libraries")]
    public class Libraries
    {
        [XmlElement(ElementName = "library")]
        public List<Library> Library { get; set; }
    }

    [XmlRoot(ElementName = "library")]
    public class Library
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "description")]
        public Description Description { get; set; }

        [XmlElement(ElementName = "packages")]
        public Packages Packages { get; set; }

        [XmlElement(ElementName = "symbols")]
        public Symbols Symbols { get; set; }

        [XmlElement(ElementName = "devicesets")]
        public Devicesets Devicesets { get; set; }
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

        public Net()
        {
            Class = "0";
        }
    }

    [XmlRoot(ElementName = "nets")]
    public class Nets
    {
        [XmlElement(ElementName = "net")]
        public List<Net> Net { get; set; }
    }

    [XmlRoot(ElementName = "note")]
    public class Note
    {
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "severity")]
        public Severity Severity { get; set; }

        [XmlText]
        public string Text;
    }

    [XmlRoot(ElementName = "package")]
    public class Package
    {
        [XmlElement(ElementName = "description")]
        public Description Description { get; set; }

        [XmlElement(ElementName = "polygon")]
        public List<Polygon> Polygon { get; set; }

        [XmlElement(ElementName = "wire")]
        public List<Wire> Wire { get; set; }

        [XmlElement(ElementName = "text")]
        public List<Text> Text { get; set; }

        [XmlElement(ElementName = "dimension")]
        public List<Dimension> Dimension { get; set; }

        [XmlElement(ElementName = "circle")]
        public List<Circle> Circle { get; set; }

        [XmlElement(ElementName = "rectangle")]
        public List<Rectangle> Rectangle { get; set; }

        [XmlElement(ElementName = "frame")]
        public List<Frame> Frame { get; set; }

        [XmlElement(ElementName = "hole")]
        public List<Hole> Hole { get; set; }

        [XmlElement(ElementName = "pad")]
        public List<Pad> Pad { get; set; }

        [XmlElement(ElementName = "smd")]
        public List<Smd> Smd { get; set; }

        //
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "packages")]
    public class Packages
    {
        [XmlElement(ElementName = "package")]
        public List<Package> Package { get; set; }
    }

    [XmlRoot(ElementName = "pad")]
    public class Pad
    {
		[XmlAttribute(AttributeName="diameter")]
		public string Diameter { get; set; }
		[XmlAttribute(AttributeName="drill")]
		public string Drill { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="rot")]
		public string Rot { get; set; }
		[XmlAttribute(AttributeName="shape")]
		public string Shape { get; set; }
		[XmlAttribute(AttributeName="x")]
		public string X { get; set; }
		[XmlAttribute(AttributeName="y")]
		public string Y { get; set; }
    }

	[XmlRoot(ElementName="param")]
	public class Param {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName="pass")]
	public class Pass {
		[XmlAttribute(AttributeName="active")]
		public string Active { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlElement(ElementName="param")]
		public List<Param> Param { get; set; }
		[XmlAttribute(AttributeName="refer")]
		public string Refer { get; set; }
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
        [XmlAttribute(AttributeName = "technology")]
        public string Technology { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        [XmlElement(ElementName = "attribute")]
        public List<Attribute> Attribute { get; set; }
        [XmlElement(ElementName = "variant")]
        public List<Variant> Variant{ get; set; }

        public Part()
        {
            Technology = "";
        }
    }

    [XmlRoot(ElementName = "parts")]
    public class Parts
    {
        [XmlElement(ElementName = "part")]
        public List<Part> Part { get; set; }
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
        public PinVisible Visible { get; set; }

        [XmlAttribute(AttributeName = "length")]
        public PinLength Length { get; set; }

        [XmlAttribute(AttributeName = "direction")]
        public PinDirection Direction { get; set; }

        [XmlAttribute(AttributeName = "function")]
        public PinFunction Function { get; set; }

        [XmlAttribute(AttributeName = "swaplevel")]
        public string Swaplevel { get; set; }

        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }

        public Pin ()
        {
            Visible = PinVisible.both;
            Length = PinLength.@long;
            Direction = PinDirection.io;
            Function = PinFunction.none;
            Swaplevel = "0";
            Rot = "R0";
        }
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

    [XmlRoot(ElementName = "plain")]
    public class Plain
    {
        [XmlElement(ElementName = "polygon")]
        public List<Polygon> Polygon { get; set; }

        [XmlElement(ElementName = "wire")]
        public List<Wire> Wire { get; set; }

        [XmlElement(ElementName = "text")]
        public List<Text> Text { get; set; }

        [XmlElement(ElementName = "dimension")]
        public List<Dimension> Dimension { get; set; }

        [XmlElement(ElementName = "circle")]
        public List<Circle> Circle { get; set; }

        [XmlElement(ElementName = "rectangle")]
        public List<Rectangle> Rectangle { get; set; }

        [XmlElement(ElementName = "frame")]
        public List<Frame> Frame { get; set; }

        [XmlElement(ElementName="hole")]
		public List<Hole> Hole { get; set; }
    }

    [XmlRoot(ElementName = "polygon")]
	public class Polygon {
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }

        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }

        [XmlAttribute(AttributeName = "spacing")]
        public string Spacing { get; set; }

        [XmlAttribute(AttributeName = "pour")]
        public PolygonPour Pour{ get; set; }

        [XmlAttribute(AttributeName="isolate")]
		public string Isolate { get; set; }

        [XmlAttribute(AttributeName = "orphans")]
        public Bool Orphans { get; set; }

        [XmlAttribute(AttributeName = "thermals")]
        public Bool Thermals { get; set; }

        [XmlAttribute(AttributeName="rank")]
		public string Rank { get; set; }


        [XmlElement(ElementName = "vertex")]
        public List<Vertex> Vertex { get; set; }

        public Polygon()
        {
            Orphans = Bool.no;
            Thermals = Bool.yes;
            Rank = "0";
        }
    }

    [XmlRoot(ElementName = "rectangle")]
    public class Rectangle
    {
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "x1")]
        public string X1 { get; set; }
        [XmlAttribute(AttributeName = "y1")]
        public string Y1 { get; set; }
        [XmlAttribute(AttributeName = "x2")]
        public string X2 { get; set; }
        [XmlAttribute(AttributeName = "y2")]
        public string Y2 { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
    }

    [XmlRoot(ElementName = "schematic")]
    public class Schematic
    {
        [XmlElement(ElementName = "description")]
        public Description Description { get; set; }

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

        [XmlElement(ElementName = "errors")]
        public Errors Errors { get; set; }

        [XmlAttribute(AttributeName = "xreflabel")]
        public string Xreflabel { get; set; }

        [XmlAttribute(AttributeName = "xrefpart")]
        public string Xrefpart { get; set; }
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

    [XmlRoot(ElementName = "setting")]
    public class Setting
    {
        [XmlAttribute(AttributeName = "alwaysvectorfont")]
        public Bool Alwaysvectorfont { get; set; }

        [XmlAttribute(AttributeName = "verticaltext")]
        public VerticalText Verticaltext { get; set; }

        public Setting ()
        {
            Verticaltext = VerticalText.up;
        }
    }

    [XmlRoot(ElementName = "settings")]
    public class Settings
    {
        [XmlElement(ElementName = "setting")]
        public List<Setting> Setting { get; set; }
    }

    [XmlRoot(ElementName = "sheet")]
    public class Sheet
    {
        [XmlElement(ElementName = "description")]
        public Description Description { get; set; }

        [XmlElement(ElementName = "plain")]
        public Plain Plain { get; set; }

        [XmlElement(ElementName = "instances")]
        public Instances Instances { get; set; }

        [XmlElement(ElementName = "busses")]
        public Busses Busses { get; set; }

        [XmlElement(ElementName = "nets")]
        public Nets Nets { get; set; }
    }

    [XmlRoot(ElementName = "sheets")]
    public class Sheets
    {
        [XmlElement(ElementName = "sheet")]
        public List<Sheet> Sheet { get; set; }
    }

	[XmlRoot(ElementName="signal")]
	public class Signal {
		[XmlElement(ElementName="contactref")]
		public List<Contactref> Contactref { get; set; }

		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }

		[XmlElement(ElementName="polygon")]
		public List<Polygon> Polygon { get; set; }

		[XmlElement(ElementName="via")]
		public List<Via> Via { get; set; }

		[XmlElement(ElementName="wire")]
		public List<Wire> Wire { get; set; }
	}

	[XmlRoot(ElementName="signals")]
	public class Signals {
		[XmlElement(ElementName="signal")]
		public List<Signal> Signal { get; set; }
	}

    [XmlRoot(ElementName = "smd")]
    public class Smd
    {
        [XmlAttribute(AttributeName = "dx")]
        public string Dx { get; set; }
        [XmlAttribute(AttributeName = "dy")]
        public string Dy { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }
        [XmlAttribute(AttributeName = "roundness")]
        public string Roundness { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
    }

    [XmlRoot(ElementName = "symbol")]
    public class Symbol
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "description")]
        public Description Description { get; set; }

        [XmlElement(ElementName = "polygon")]
        public List<Polygon> Polygon { get; set; }

        [XmlElement(ElementName = "wire")]
        public List<Wire> Wire { get; set; }

        [XmlElement(ElementName = "text")]
        public List<Text> Text { get; set; }

        [XmlElement(ElementName = "dimension")]
        public List<Dimension> Dimension { get; set; }

        [XmlElement(ElementName = "pin")]
        public List<Pin> Pin { get; set; }

        [XmlElement(ElementName = "circle")]
        public List<Circle> Circle { get; set; }

        [XmlElement(ElementName = "rectangle")]
        public List<Rectangle> Rectangle { get; set; }

        [XmlElement(ElementName = "frame")]
        public List<Frame> Frame { get; set; }
    }

    [XmlRoot(ElementName = "symbols")]
    public class Symbols
    {
        [XmlElement(ElementName = "symbol")]
        public List<Symbol> Symbol { get; set; }
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

        [XmlAttribute(AttributeName = "font")]
        public TextFont Font { get; set; }

        [XmlAttribute(AttributeName = "ratio")]
        public string Ratio { get; set; }

        [XmlAttribute(AttributeName = "rot")]
        public string Rot { get; set; }

        [XmlAttribute(AttributeName = "align")]
        public Align Align { get; set; }

        [XmlAttribute(AttributeName = "distance")]
        public string Distance { get; set; }

        [XmlText]
        public string mText { get; set; }

        public Text()
        {
            Font = TextFont.proportional;
            Ratio = "8";
            Rot = "R0";
            Align = Align.bottom_left;
            Distance = "50";
        }
    }

    [XmlRoot(ElementName = "variant")]
    public class Variant
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "populate")]
        public Bool Populate { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        [XmlAttribute(AttributeName = "technology")]
        public string Technology { get; set; }
    }

    [XmlRoot(ElementName = "variantdef")]
    public class Variantdef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "current")]
        public Bool Current { get; set; }

        public Variantdef ()
        {
            Current = Bool.no;
        }
    }

    [XmlRoot(ElementName = "variantdefs")]
    public class Variantdefs
    {
        [XmlElement (ElementName = "variantdef")]
        public List<Variantdef> Variantedef { get; set; }
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

	[XmlRoot(ElementName="via")]
	public class Via {
		[XmlAttribute(AttributeName="drill")]
		public string Drill { get; set; }

		[XmlAttribute(AttributeName="extent")]
		public string Extent { get; set; }

		[XmlAttribute(AttributeName="shape")]
		public ViaShape Shape { get; set; }

		[XmlAttribute(AttributeName="x")]
		public string X { get; set; }

		[XmlAttribute(AttributeName="y")]
		public string Y { get; set; }

        [XmlAttribute(AttributeName = "diameter")]
        public string Diameter { get; set; }

        [XmlAttribute(AttributeName = "alwaysstop")]
        public Bool AlwaysStop { get; set; }

        public Via()
        {
            Diameter = "0";
            Shape = ViaShape.round;
        }


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

        [XmlAttribute(AttributeName = "style")]
        public WireStyle Style { get; set; }

        [XmlAttribute(AttributeName = "curve")]
        public string Curve { get; set; }

        [XmlAttribute(AttributeName = "cap")]
        public WireCap Cap { get; set; }

        public Wire ()
        {
            Style = WireStyle.continuous;
            Curve = "0";
            Cap = WireCap.round;
        }
    }


}
