using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Kicad_utils.Schema
{
    public class LegacyComponent : ComponentBase
    {
        //
        //public string Name;

        public override string Reference
        {
            get { return fReference.Value; }
            set { fReference.Value = value; }
        }

        public override string Value
        {
            get { return fValue.Value; }
            set { fValue.Value = value; }
        }

        public override string Footprint
        {
            get { return fPcbFootprint.Value; }
            set { fPcbFootprint.Value = value; }
        }

        //
        public int N; // 1  // unit number in multipart components
        public int mm; // 1 // Indicates DeMorgan?

        public PointF Position;

        public List<AlternateRef> AltRefs;

        public LegacyField fReference;       // 0
        public LegacyField fValue;           // 1
        public LegacyField fPcbFootprint;    // 2
        public LegacyField fUserDocLink;     // 3

        public List<LegacyField> UserFields; // 4...

        public int Rotation; // 0,90,180,270
        public bool Mirror;

        // private int A, B, C, D; // orientation

        //F 0 "P4"                     V 4300 6200 60  0000 C CNN
        //F 1 "LOWER_PINS"             V 4400 6200 60  0000 C CNN
        //F 2 "pin_header_2.54mm_1x18" V 4350 6200 60  0001 C CNN
        //F 3 ""                       H 4350 6200 60  0001 C CNN
        //F 4 "never"                  V 4350 6200 60  0001 C CNN "populate"


        static int[] rotate0 = new int[4] { 1, 0, 0, -1 };
        static int[] rotate90 = new int[4] { 0, -1, -1, 0 };
        static int[] rotate180 = new int[4] { -1, 0, 0, 1 };
        static int[] rotate270 = new int[4] { 0, 1, 1, 0 };

        static int[] rotate0_mirror = new int[4] { 1, 0, 0, 1 };
        static int[] rotate90_mirror = new int[4] { 0, -1, 1, 0 };
        static int[] rotate180_mirror = new int[4] { -1, 0, 0, -1 };
        static int[] rotate270_mirror = new int[4] { 0, 1, -1, 0 };

        private List<int[]> orientations = new List<int[]> {
            rotate0, rotate90, rotate180, rotate270,
            rotate0_mirror, rotate90_mirror, rotate180_mirror, rotate270_mirror
        };

        public LegacyComponent()
        {
            fReference = new LegacyField(LegacyField.FieldReference);
            fValue = new LegacyField(LegacyField.FieldValue);
            fPcbFootprint = new LegacyField(LegacyField.FieldPcbFootprint);
            fUserDocLink = new LegacyField(LegacyField.FieldUserDocLink);

            N = 1;
            mm = 1;
            Timestamp = Utils.GetTimeStamp(DateTime.Now);

            UserFields = new List<LegacyField>();
        }

        public override List<Field> GetFields()
        {
            List<Field> result = new List<Field>();
            foreach (LegacyField lf in UserFields)
            {
                result.Add(new Field(lf.UserName, lf.Value));
            }
            return result;
        }


        public override void AddOrSetField(string Name, string Value, TextFormat format)
        {
            //
            LegacyField field = null;

            switch (Name)
            {
                case "Ref":
                    field = fReference;
                    break;
                case "Value":
                    field = fValue;
                    break;
                case "Footprint":
                    field = fPcbFootprint;
                    break;

                default:
                    if (UserFields != null)
                        foreach (LegacyField ufield in UserFields)
                            if (string.Compare(ufield.UserName, Name, true) == 0)
                            {
                                field = ufield;
                                break;
                            }
                    break;

            }

            if (field == null)
            {
                // add field
                if (UserFields == null)
                    UserFields = new List<LegacyField>();

                field = new LegacyField();
                UserFields.Add(field);
                field.UserName = Name;
                field.Pos = new PointF(Position.X, Position.Y);
            }

            field.Value = Value;
            field.Hidden = ! format.Visible;
        }

        public override void Write(List<string> data)
        {
            data.Add("$Comp");

            data.Add(string.Format("L {0} {1}", Symbol, Reference));
            data.Add(string.Format("U {0} {1} {2}", N, mm, Timestamp));
            data.Add(string.Format("P {0} {1}", (int)Position.X, (int)Position.Y));

            // AR
            if (AltRefs != null)
            {
                foreach (AlternateRef aref in AltRefs)
                    data.Add(string.Format("AR Path={0} Ref={1}  Part={2}", aref.Path, aref.Ref, aref.Part));
            }

            // F
            data.Add(fReference.ToString());
            data.Add(fValue.ToString());
            if (fPcbFootprint != null)
                data.Add(fPcbFootprint.ToString());
            if (fUserDocLink != null)
                data.Add(fUserDocLink.ToString());

            if (UserFields != null)
            {
                int j = 4;
                foreach (LegacyField lf in UserFields)
                {
                    lf.Number = j++;
                    data.Add(lf.ToString());
                }
            }

            //
            data.Add(string.Format("\t1 {0} {1}", (int)Position.X, (int)Position.Y));

            int index = Rotation / 90;
            if (Mirror)
                index += 4;

            //data.Add(string.Format("\t{0} {1} {2} {3}", A, B, C, D));
            data.Add(string.Format("\t{0} {1} {2} {3}",
                orientations[index][0],
                orientations[index][1],
                orientations[index][2],
                orientations[index][3]
                ));

            data.Add("$EndComp");
        }

        public void SetOrientation(int A, int B, int C, int D)
        {
            Rotation = 0;
            Mirror = false;
            for (int index = 0; index < orientations.Count; index++)
            {
                if ((A == orientations[index][0]) &&
                    (B == orientations[index][1]) &&
                    (C == orientations[index][2]) &&
                    (D == orientations[index][3]))
                {
                    Rotation = (index % 4) * 90;
                    if (index > 3)
                        Mirror = true;
                }
            }
        }

    }
}
