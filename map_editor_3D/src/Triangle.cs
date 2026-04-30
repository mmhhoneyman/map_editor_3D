using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static map_editor_3D.src.Projection;

namespace map_editor_3D.src
{
    internal class Triangle
    {
        public Vector3 C1 { get; set; } // corner 1, 2, 3
        public Vector3 C2 { get; set; }
        public Vector3 C3 { get; set; }
        public bool Visible { get; set; }
        public Vector3 Normal { get; set; } // the vector perpendicular to the plane
        public SolidBrush RGB { get; set; }

        // NOTE: It is important that the triangle points are created in a counter-clockwise order in order to get the proper face for the projection plane
        // Otherwise, the plane will face the opposide direction
        public Triangle(Vector3 C1, Vector3 C2, Vector3 C3, Camera camera, Vector3 light)
        {
            this.C1 = C1;
            this.C2 = C2;
            this.C3 = C3;
            this.Normal = FindNormal();
            //this.Visible = SetVisibility(this.Normal, camera);
            this.Visible = SetVisibility();
        }

        public Vector3 FindNormal()
        {
            Vector3 line1 = new Vector3(C2.X - C1.X, C2.Y - C1.Y, C2.Z - C1.Z);
            Vector3 line2 = new Vector3(C3.X - C1.X, C3.Y - C1.Y, C3.Z - C1.Z);

            Vector3 normal = new Vector3(
                line1.Y * line2.Z - line1.Z * line2.Y,
                line1.Z * line2.X - line1.X * line2.Z,
                line1.X * line2.Y - line1.Y * line2.X
            );

            float length = (float)Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
            //System.Diagnostics.Debug.WriteLine($"Normal before normalization: {normal.X}, {normal.Y}, {normal.Z}");
            if (length != 0)
            {
                normal = new Vector3(normal.X / length, normal.Y / length, normal.Z / length);
            }

            return normal;
        }

        public bool SetVisibility()
        {
            Vector3 edge1 = C2 - C1;
            Vector3 edge2 = C3 - C1;
            Vector3 normal = Vector3.Cross(edge1, edge2);

            Vector3 center = (C1 + C2 + C3) / 3f;
            return Vector3.Dot(normal, center) > 0;
        }

        public SolidBrush SetShading(Vector3 normal, Vector3 light)
        {
            Vector3 n = Vector3.Normalize(normal);
            Vector3 l = Vector3.Normalize(light);

            float dp = Math.Max(0f, Vector3.Dot(n, l));

            dp = dp * Constants.SCALING_FACTOR + Constants.AMBIENT;
            dp = (float)Math.Pow(dp, Constants.SMOOTHING);

            int min = 0;
            int max = 255;
            int colorValue = (int)(min + dp * (max - min));
            colorValue = Math.Clamp(colorValue, min, max);

            return new SolidBrush(Color.FromArgb(colorValue, colorValue, colorValue));
        }

        /*public void Update(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 light)
        {
            this.C1 = c1;
            this.C2 = c2;
            this.C3 = c3;
            this.Brush = SetShading(FindNormal(), light);
        }*/

        public void Draw(Graphics g, Vector3 light)
        {
            if (this.Visible)
            {
                //draw lines
                //g.DrawLine(Pens.Red, C1.px, C1.py, C2.px, C2.py);
                //g.DrawLine(Pens.Red, C1.px, C1.py, C3.px, C3.py);
                //g.DrawLine(Pens.Red, C3.px, C3.py, C2.px, C2.py);

                List<Vector4[]> vector4s = 
                    ClipTriangleFrustum(new Vector4[] { 
                        ToClipSpace(Constants.PROJECTION_MATRIX, C1), 
                        ToClipSpace(Constants.PROJECTION_MATRIX, C2), 
                        ToClipSpace(Constants.PROJECTION_MATRIX, C3) 
                    });

                foreach (var tri in vector4s)
                {
                    Vector3[] ndc = new Vector3[3];

                    for (int i = 0; i < 3; i++)
                    {
                        float invW = 1f / tri[i].W;

                        ndc[i] = new Vector3(
                            tri[i].X * invW,
                            tri[i].Y * invW,
                            tri[i].Z * invW
                        );
                    }

                    PointF[] pts = new PointF[3];

                    for (int i = 0; i < 3; i++)
                    {
                        pts[i] = new PointF(
                            (ndc[i].X * 0.5f + 0.5f) * Constants.SCREEN_WIDTH,
                            (ndc[i].Y * 0.5f + 0.5f) * Constants.SCREEN_HEIGHT
                        );
                    }
                    SolidBrush brush = SetShading(FindNormal(), light);

                    g.FillPolygon(brush, pts);
                }


                //fill triangle
                /*PointF[] pts = new PointF[]
                {
                    new PointF(Project(Constants.PROJECTION_MATRIX,C1).X, Project(Constants.PROJECTION_MATRIX,C1).Y),
                    new PointF(Project(Constants.PROJECTION_MATRIX,C2).X, Project(Constants.PROJECTION_MATRIX,C2).Y),
                    new PointF(Project(Constants.PROJECTION_MATRIX,C3).X, Project(Constants.PROJECTION_MATRIX,C3).Y)
                };*/
                //SolidBrush brush = SetShading(FindNormal(), light);

                //g.FillPolygon(brush, pts);

                //g.DrawPolygon(Pens.White, pts);

            }
        }

        /*public static Vector3 Project(float[,] m, Vector3 v3)
        {
            float[] vector = { v3.X, v3.Y, v3.Z, 1 };

            if (m.GetLength(0) == vector.Length)
            {
                float fpx = v3.X * m[0, 0] + v3.Y * m[1, 0] + v3.Z * m[2, 0] + 1 * m[3, 0];
                float fpy = v3.X * m[0, 1] + v3.Y * m[1, 1] + v3.Z * m[2, 1] + 1 * m[3, 1];
                float fpr = v3.X * m[0, 2] + v3.Y * m[1, 2] + v3.Z * m[2, 2] + 1 * m[3, 2];
                float fpz = v3.X * m[0, 3] + v3.Y * m[1, 3] + v3.Z * m[2, 3] + 1 * m[3, 3];

                float px = ((fpx / fpz) + 1f) * 0.5f * Constants.SCREEN_WIDTH;
                float py = ((fpy / fpz) + 1f) * 0.5f * Constants.SCREEN_HEIGHT;
                float pr = fpr / fpz;

                return new Vector3(px, py, pr);
            }
            else
            {
                throw new Exception("Matrix row number must be equal to vector size (4)");
            }
        }*/
        public static Vector4 ToClipSpace(float[,] m, Vector3 v)
        {
            return new Vector4(
                v.X * m[0, 0] + v.Y * m[1, 0] + v.Z * m[2, 0] + 1 * m[3, 0],
                v.X * m[0, 1] + v.Y * m[1, 1] + v.Z * m[2, 1] + 1 * m[3, 1],
                v.X * m[0, 2] + v.Y * m[1, 2] + v.Z * m[2, 2] + 1 * m[3, 2],
                v.X * m[0, 3] + v.Y * m[1, 3] + v.Z * m[2, 3] + 1 * m[3, 3]
            );
        }
        public List<Vector4[]> ClipTriangleFrustum(Vector4[] triangle)
        {
            List<Vector4> poly = new List<Vector4>(3)
            {
                triangle[0],
                triangle[1],
                triangle[2]
            };

            poly = ClipAgainstPlane(poly, v => v.X >= -v.W, IntersectLeft);
            if (poly.Count == 0) return new List<Vector4[]>();

            poly = ClipAgainstPlane(poly, v => v.X <= v.W, IntersectRight);
            if (poly.Count == 0) return new List<Vector4[]>();

            poly = ClipAgainstPlane(poly, v => v.Y >= -v.W, IntersectBottom);
            if (poly.Count == 0) return new List<Vector4[]>();

            poly = ClipAgainstPlane(poly, v => v.Y <= v.W, IntersectTop);
            if (poly.Count == 0) return new List<Vector4[]>();

            poly = ClipAgainstPlane(poly, v => v.Z >= 0, IntersectNear);
            if (poly.Count == 0) return new List<Vector4[]>();

            poly = ClipAgainstPlane(poly, v => v.Z <= v.W, IntersectFar);
            if (poly.Count == 0) return new List<Vector4[]>();

            return Triangulate(poly);
        }
        private List<Vector4> ClipAgainstPlane(
        List<Vector4> input,
        Func<Vector4, bool> inside,
        Func<Vector4, Vector4, Vector4> intersect)
        {
            List<Vector4> output = new List<Vector4>();

            for (int i = 0; i < input.Count; i++)
            {
                Vector4 current = input[i];
                Vector4 prev = input[(i - 1 + input.Count) % input.Count];

                bool currIn = inside(current);
                bool prevIn = inside(prev);

                if (currIn)
                {
                    if (!prevIn)
                        output.Add(intersect(prev, current));

                    output.Add(current);
                }
                else if (prevIn)
                {
                    output.Add(intersect(prev, current));
                }
            }

            return output;
        }
        private Vector4 IntersectLeft(Vector4 a, Vector4 b)
        {
            float denom = (a.X + a.W) - (b.X + b.W);
            if (MathF.Abs(denom) < 1e-6f) return a;

            float t = (a.X + a.W) / denom;
            return a + t * (b - a);
        }
        private Vector4 IntersectRight(Vector4 a, Vector4 b)
        {
            float denom = (a.W - a.X) - (b.W - b.X);
            if (MathF.Abs(denom) < 1e-6f) return a;

            float t = (a.W - a.X) / denom;
            return a + t * (b - a);
        }
        private Vector4 IntersectBottom(Vector4 a, Vector4 b)
        {
            float denom = (a.Y + a.W) - (b.Y + b.W);
            if (MathF.Abs(denom) < 1e-6f) return a;

            float t = (a.Y + a.W) / denom;
            return a + t * (b - a);
        }
        private Vector4 IntersectTop(Vector4 a, Vector4 b)
        {
            float denom = (a.W - a.Y) - (b.W - b.Y);
            if (MathF.Abs(denom) < 1e-6f) return a;

            float t = (a.W - a.Y) / denom;
            return a + t * (b - a);
        }
        private Vector4 IntersectNear(Vector4 a, Vector4 b)
        {
            float denom = a.Z - b.Z;
            if (MathF.Abs(denom) < 1e-6f) return a;

            float t = a.Z / denom;
            return a + t * (b - a);
        }
        private Vector4 IntersectFar(Vector4 a, Vector4 b)
        {
            float denom = (a.W - a.Z) - (b.W - b.Z);
            if (MathF.Abs(denom) < 1e-6f) return a;

            float t = (a.W - a.Z) / denom;
            return a + t * (b - a);
        }
        private List<Vector4[]> Triangulate(List<Vector4> poly)
        {
            List<Vector4[]> tris = new List<Vector4[]>();

            if (poly.Count < 3)
                return tris;

            for (int i = 1; i < poly.Count - 1; i++)
            {
                tris.Add(new Vector4[]
                {
                    poly[0],
                    poly[i],
                    poly[i + 1]
                });
            }

            return tris;
        }
    }
}
