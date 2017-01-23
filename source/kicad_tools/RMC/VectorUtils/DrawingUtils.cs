using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Surface;

namespace RMC.VectorUtils
{
    public enum WindingOrder { Clockwise, AntiClockwise };

    public class DrawingUtils
    {
        #region Public methods


        public static bool LineIntersectsPolyLine(FigPolyline Line1, FigPolyline Line2)
        {
            PointF iPt;

            for (int j = 0; j < Line1.Points.Count - 1; j++)
            {
                for (int k = 0; k < Line1.Points.Count - 1; k++)
                {
                    if (Intersect(Line1.Points[j], Line1.Points[j + 1], Line2.Points[k], Line2.Points[k + 1], out iPt))
                        return true;
                }
            }

            return false;
        }

        public static bool LineIntersectsPolyLines(FigPolyline Line1, List <FigPolyline> Lines)
        {
            foreach (FigPolyline line in Lines)
                if (LineIntersectsPolyLine(Line1, line))
                    return true;
            return false;
        }


        /// <summary>
        /// Replace the selected lines or polylines with a single polyline.
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        public static bool MakePath(GraphicsSurface surface)
        {
            bool result = false;
            FigPolyline new_fig = new FigPolyline();

            if (surface.selected.Count == 0)
                return result; // nothing selected

            foreach (Figure fig in surface.selected)
            {
                if (!((fig is Line) || (fig is FigPolyline)))
                    return false;
            }

            foreach (Figure fig in surface.selected)
            {
                if (new_fig.Points.Count == 0)
                {
                    if (fig is Line)
                    {
                        Line line = fig as Line;
                        new_fig.Points.Add(new PointF(line.p1.X, line.p1.Y));
                        new_fig.Points.Add(new PointF(line.p2.X, line.p2.Y));
                        new_fig.LineProperties = line.LineProperties;
                    }
                    else
                    {
                        FigPolyline poly = fig as FigPolyline;
                        foreach (PointF p in poly.Points)
                        {
                            new_fig.Points.Add(new PointF(p.X, p.Y));
                        }
                        new_fig.LineProperties = poly.LineProperties;
                    }
                }
                else
                {
                    // check contiguous
                    if (fig is Line)
                    {
                        Line line = fig as Line;

                        if (new_fig.Points[new_fig.Points.Count - 1] == line.p1)
                        {
                            new_fig.Points.Add(new PointF(line.p2.X, line.p2.Y));
                        }
                        else if (new_fig.Points[new_fig.Points.Count - 1] == line.p2)
                        {
                            new_fig.Points.Add(new PointF(line.p1.X, line.p1.Y));
                        }
                        else if (new_fig.Points[0] == line.p1)
                        {
                            new_fig.Points.Insert(0, new PointF(line.p2.X, line.p2.Y));
                        }
                        else if (new_fig.Points[0] == line.p2)
                        {
                            new_fig.Points.Insert(0, new PointF(line.p1.X, line.p1.Y));
                        }
                        else
                            return false;
                    }
                    else
                    {
                        //polyline
                        FigPolyline poly = fig as FigPolyline;
                        if (new_fig.Points[new_fig.Points.Count - 1] == poly.Points[0])
                        {
                            AddPoints(ref new_fig.Points, poly.Points, 1);
                        }
                        else if (new_fig.Points[new_fig.Points.Count - 1] == poly.Points[poly.Points.Count - 1])
                        {
                            AddPointsReverse(ref new_fig.Points, poly.Points, 1);
                        }
                    }
                }

            }

            surface.Drawing.RemoveFigures(surface.selected);
            surface.Drawing.AddFigure(new_fig);

            // set new selection
            //surface.ClearSelected();
            surface.selected.Clear();
            //surface.AddToSelected(new_fig);
            surface.selected.Add(new_fig);

            return result;
        }

        public static bool ClipPolyToLine(GraphicsSurface surface, Line ClipLine)
        {
            bool result = false;
            FigPolyline new_fig = new FigPolyline();

            if (surface.selected.Count == 0 || surface.selected.Count > 1)
                return result; // nothing selected

            if (surface.selected[0] is FigPolyline)
            {
                PointF s, p, i;
                int j;
                FigPolyline inPoly = surface.selected[0] as FigPolyline;

                s = inPoly.Points[inPoly.Points.Count - 1];
                for (j = 0; j < inPoly.Points.Count; j++)
                {
                    p = inPoly.Points[j];
                    if (Inside (p, ClipLine))  //  {Cases 1 and 4}
                    {
                        if (Inside(s, ClipLine))
                        {
                            new_fig.Points.Add (p); // {Case 1}
                        }
                        else
                        {
                            Intersect(s, p, ClipLine, out i);
                            new_fig.Points.Add (i);
                            new_fig.Points.Add (p);
                        }
                    }
                    else  // {Cases 2 and 3}
                    {
                        if (Inside(s, ClipLine)) // {Cases 2}
                        {
                            Intersect(s, p, ClipLine, out i);
                            new_fig.Points.Add (i);
                        } // {No action for case 3}
                    }
                    s = p; // {Advance to next pair of vertices}
                }
                       
                // replace selected
                surface.Drawing.RemoveFigures(surface.selected);
                surface.Drawing.AddFigure(new_fig);

                // set new selection
                surface.selected.Clear();
                //surface.AddToSelected(new_fig);
                surface.selected.Add(new_fig);

                return result;
            }
            else
                return result;
        }

        // {Checks whether the test point lies "inside" the clip line or not}
        // returns true if point is on or to left of clip line
        // for anti-clockwise poly
        public static bool Inside (PointF test, Line clip)
        {
            //float dp1, dp2;

            //dp1 = DotProduct (Vector (clipBoundary.p1, clipBoundary.p2),
            //                  Vector (clipBoundary.p1, testVertex));

            //dp2 = DotProduct (Vector (clipBoundary.p1, clipBoundary.p2),
            //                  Vector (clipBoundary.p1, clipBoundary.p2));
             
            //if (dp1 >=0 && dp1 <= dp2)
            //    return true;
            //else
            //    return false;

            float A = -(clip.p2.Y - clip.p1.Y);
            float B = clip.p2.X - clip.p1.X;
            float C = -(A * clip.p1.X + B * clip.p1.Y);

            float D = A * test.X + B * test.Y + C;

            return D>=0;
        }

        public static PointF Vector(PointF A, PointF B)
        {
            return new PointF(B.X - A.X, B.Y - A.Y);
        }

        private static float DotProduct(PointF v1, PointF v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        // calculate the intersection point of lines (p1-p2) and line2
        public static bool Intersect(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersectPt)
        {
            Line line1 = new Line(p1, p2);
            Line line2 = new Line(p3, p4);

            return Intersect(line1, line2, out intersectPt);
        }

        // calculate the intersection point of lines (p1-p2) and line2
        public static bool Intersect(PointF p1, PointF p2, Line line2, out PointF intersectPt)
        {
            Line line1 = new Line(p1, p2);

            return Intersect(line1, line2, out intersectPt);
        }

        // calculate the intersection point of lines (p1-p2) and line2
        public static bool Intersect(Line line1, Line line2, out PointF intersectPt)
        {
            intersectPt = new PointF (0,0);
//            Line line1 = new Line(p1, p2);

            /* Denominator for ua and ub are the same so store this calculation */
            float d = (line2.p2.Y - line2.p1.Y) * (line1.p2.X - line1.p1.X) -
                        (line2.p2.X - line2.p1.X) * (line1.p2.Y - line1.p1.Y);

            /* n_a and n_b are calculated as seperate values for readability */
            float n_a = (line2.p2.X - line2.p1.X) * (line1.p1.Y - line2.p1.Y) -
                        (line2.p2.Y - line2.p1.Y) * (line1.p1.X - line2.p1.X);

            float n_b = (line1.p2.X - line1.p1.X) * (line1.p1.Y - line2.p1.Y) -
                        (line1.p2.Y - line1.p1.Y) * (line1.p1.X - line2.p1.X);

            /* Make sure there is not a division by zero - this also indicates that
             * the lines are parallel.  
             *
             * If n_a and n_b were both equal to zero the lines would be on top of each 
             * other (coincidental).  This check is not done because it is not 
             * necessary for this implementation (the parallel check accounts for this).
             */
            if (Math.Abs(d) < 1e-6)
                return false;

            /* Calculate the intermediate fractional point that the lines potentially
             *  intersect.
             */
            double ua = (n_a) / d;
            double ub = (n_b) / d;

            /* The fractional point will be between 0 and 1 inclusive if the lines
             * intersect.  If the fractional calculation is larger than 1 or smaller
             * than 0 the lines would need to be longer to intersect.
             */
            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                intersectPt.X = (float)(line1.p1.X + ua * (line1.p2.X - line1.p1.X));
                intersectPt.Y = (float)(line1.p1.Y + ua * (line1.p2.Y - line1.p1.Y));
                return true;
            }

            return false;
        }
    

        
        /// <summary>
        /// Test if a polygon is closed (last point = first point)
        /// </summary>
        /// <param name="Polygon"></param>
        /// <returns></returns>
        public static bool IsClosed(FigPolyline Polygon)
        {
            if (Polygon.Points[0] == Polygon.Points[Polygon.Points.Count - 1])
                return true;
            else
                return false;
        }

        /// <summary>
        /// Return winding order
        /// </summary>
        /// <param name="Polygon"></param>
        /// <returns></returns>
        public static WindingOrder GetWindingOrder(FigPolyline Polygon)
        {
            double Area = 0;

            for (int j = 0; j < Polygon.Points.Count - 1; j++ )
            {
                PointF Start = Polygon.Points[j];
                PointF End = Polygon.Points[j + 1];

                Area = Area + (Start.X * End.Y) - (End.X * Start.Y);
            }

            if (!IsClosed (Polygon))
            {
                PointF Start = Polygon.Points[Polygon.Points.Count-1];
                PointF End = Polygon.Points[0];
                Area = Area + (Start.X * End.Y) - (End.X * Start.Y);
            }

            if (Area < 0)
                return WindingOrder.Clockwise;
            else
                return WindingOrder.AntiClockwise;
        }

        /// <summary>
        /// Calculate the area of a polygon
        /// </summary>
        /// <param name="Polygon"></param>
        /// <returns></returns>
        public static double CalcPolygonArea (FigPolyline Polygon)
        {
            double Area = 0;

            for (int j = 0; j < Polygon.Points.Count - 1; j++ )
            {
                PointF Start = Polygon.Points[j];
                PointF End = Polygon.Points[j + 1];
                Area = Area + (Start.X * End.Y) - (End.X * Start.Y);
            }

            if (!IsClosed (Polygon))
            {
                PointF Start = Polygon.Points[Polygon.Points.Count-1];
                PointF End = Polygon.Points[0];
                Area = Area + (Start.X * End.Y) - (End.X * Start.Y);
            }
            return Math.Abs(Area) / 2.0;
        }

        #endregion

        #region Private methods
        private static void AddPoints(ref List<PointF> Points, List<PointF> NewPoints, int offset)
        {
            // offfset
            for (int j = offset; j < NewPoints.Count; j++)
                Points.Add(new PointF(NewPoints[j].X, NewPoints[j].Y));
        }

        private static void AddPointsReverse(ref List<PointF> Points, List<PointF> NewPoints, int offset)
        {
            // offfset
            for (int j = NewPoints.Count - offset - 1; j >= 0; j--)
                Points.Add(new PointF(NewPoints[j].X, NewPoints[j].Y));
        }

        #endregion
    }
}
