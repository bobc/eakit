using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Cad2D;

namespace CadCommon
{
    public class Functions3D
    {
        public static void Bend(float angle, float radius, Cad2D.Polygon poly2d, List<Path3DNode> Nodes)
        {
            float c = (float)(Math.PI * 2 * radius);
            c = c * Math.Abs(angle) / 360;

            //float ang = 0;
            for (int step = 0; step < 9; step++)
            //for (float ang = 0; ang < angle; ang += 10)
            {
                //poly2d = Square(diam);
                Nodes.Add(new Path3DNode(poly2d, new Vector3(0, 0, c / (Math.Abs(angle) / 10)), new Vector3(angle / 9, 0, 0)));
            }
        }

        public static MeshIndexed StitchPolygons2D(List<Path3DNode> NodeList)
        {
            MeshIndexed result = new MeshIndexed();
            Path3DNode Current;

            int index = 0;

            Current = new Path3DNode(null, new Vector3(0, 0, 0), new Vector3(0, 0, 0));

            CadCommon.Polygon poly0 = null;
            CadCommon.Polygon poly1 = null;

            for (index = 0; index < NodeList.Count; index++)
            {
                Path3DNode CurNode = NodeList[index];

                // update current
                Matrix4 rot = Matrix4.CreateRotationX(MathUtil.DegToRad(Current.Direction.X));

                //Quaternion q = Quaternion.FromAxisAngle(Vector3.UnitZ, Current.Direction.X);

                Vector3 vec = Vector3.Transform(CurNode.Position, rot);

                Current.Position.Add(vec);

                Current.Direction.Add(CurNode.Direction);

                // triangulate first and last
                if ((index == 0) || (index == NodeList.Count - 1))
                {
                    //Cad2D.Polygon poly = CurNode.Poly2d;
                    //poly.Triangulate();

                    Cad2D.Polygon poly = PolygonTesselator.Tesselate(CurNode.Polygons);

                    foreach (Cad2D.Triangle triangle in poly.Triangles)
                    {
                        TriangleExt tri3d = new TriangleExt(
                            new Vector3(triangle.A.X, triangle.A.Y, 0),
                            new Vector3(triangle.B.X, triangle.B.Y, 0),
                            new Vector3(triangle.C.X, triangle.C.Y, 0)
                            );

                        if (index == 0)
                            tri3d.ReverseOrder();

                        tri3d.RotateX(Current.Direction.X);
                        tri3d.RotateX(Current.Direction.Y);

                        tri3d.Translate(Current.Position);

                        result.AddTriangle(tri3d);
                    }
                }


                poly1 = new CadCommon.Polygon(CurNode.Poly2d);

                poly1.RotateX(Current.Direction.X);
                poly1.RotateX(Current.Direction.Y);
                poly1.Translate(Current.Position);

                // intermediates
                if (index > 0)
                {
                    //
                    for (int k = 0; k < poly0.vertices.Length; k++)
                    {
                        int kp1 = (k + 1) % poly0.vertices.Length;

                        TriangleExt triangle = new TriangleExt(
                            new Vector3(poly1.vertices[kp1].Position.X, poly1.vertices[kp1].Position.Y, poly1.vertices[kp1].Position.Z),
                            new Vector3(poly0.vertices[kp1].Position.X, poly0.vertices[kp1].Position.Y, poly0.vertices[kp1].Position.Z),
                            new Vector3(poly0.vertices[k].Position.X, poly0.vertices[k].Position.Y, poly0.vertices[k].Position.Z)
                            );
                        result.AddTriangle(triangle);

                        triangle = new TriangleExt(
                            new Vector3(poly0.vertices[k].Position.X, poly0.vertices[k].Position.Y, poly0.vertices[k].Position.Z),
                            new Vector3(poly1.vertices[k].Position.X, poly1.vertices[k].Position.Y, poly1.vertices[k].Position.Z),
                            new Vector3(poly1.vertices[kp1].Position.X, poly1.vertices[kp1].Position.Y, poly1.vertices[kp1].Position.Z)
                            );
                        result.AddTriangle(triangle);
                    }
                }

                poly0 = poly1;
            }

            return result;
        }



        public static void StitchPolygons3D(MeshIndexed mesh, List<CadCommon.Polygon> PolyList)
        {
            // add first
            CadCommon.Polygon poly3d;

            //poly3d = PolyList[0];
            //result.AddPolygon (poly3d);

            // join
            for (int j = 0; j < PolyList.Count - 1; j++)
            {
                CadCommon.Polygon poly0 = PolyList[j];
                CadCommon.Polygon poly1 = PolyList[j + 1];

                // --
                for (int k = 0; k < poly0.vertices.Length; k++)
                {
                    int kp1 = (k + 1) % poly0.vertices.Length;

                    TriangleExt triangle = new TriangleExt(poly1.vertices[kp1].Position, poly0.vertices[kp1].Position, poly0.vertices[k].Position);
                    mesh.AddTriangle(triangle);

                    triangle = new TriangleExt(poly0.vertices[k].Position, poly1.vertices[k].Position, poly1.vertices[kp1].Position);
                    mesh.AddTriangle(triangle);
                }
            }

            // add last
            //poly3d = PolyList[PolyList.Count - 1];
            //result.AddPolygon(poly3d);

        }

    }
}
