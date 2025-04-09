using System;
using System.Drawing.Drawing2D;

namespace map_editor_3D.src
{
    class Projection
    {
        private static float h = Screen.PrimaryScreen.Bounds.Height;
        private static float w = Screen.PrimaryScreen.Bounds.Width;
        private static float ar = h / w;

        private static float zFar = 1000f;
        private static float zNear = 0.1f;

        private static float FOV = 90f;
        private static float radFOV = (float)Math.PI / 180f * FOV;
        private static float pFOV = (float)(1 / Math.Tan(radFOV / 2f));


        private static float[,] projectionMat = { 
            { ar * pFOV, 0, 0, 0 },
            { 0, pFOV, 0, 0 },
            { 0, 0, zFar / (zFar - zNear), 1 },
            { 0, 0, (-1f * zFar * zNear) / (zFar - zNear), 0 }
        };

        private static Coordinates camera = new Coordinates(0, 0, 0);

        public struct Coordinates
        {
            public float x;
            public float y;
            public float z;

            public float px;
            public float py;
            public float pr;

            public Coordinates(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;

                float[] projected = Project(projectionMat);
                this.px = projected[0];
                this.py = projected[1];
                this.pr = projected[2];
            }

            public float[] Project(float[,] m)
            {
                float[] vector = { this.x, this.y, this.z, 1 };

                if (m.GetLength(0) == vector.Length)
                {
                    float fpx = x * m[0, 0] + y * m[1, 0] + z * m[2, 0] + 1 * m[3, 0];
                    float fpy = x * m[0, 1] + y * m[1, 1] + z * m[2, 1] + 1 * m[3, 1];
                    float fpr = x * m[0, 2] + y * m[1, 2] + z * m[2, 2] + 1 * m[3, 2];
                    float fpz = x * m[0, 3] + y * m[1, 3] + z * m[2, 3] + 1 * m[3, 3];

                    float px = ((fpx / fpz) + 1f) * 0.5f * w;
                    float py = ((fpy / fpz) + 1f) * 0.5f * h;
                    float pr = fpr / fpz;

                    return new float[] { px, py, pr };
                }
                else
                {
                    throw new Exception("Matrix row number must be equal to vector size (4)");
                }
            }
        }

        public struct Line
        {
            public Coordinates c1;
            public Coordinates c2;

            public Line(Coordinates c1, Coordinates c2)
            {
                this.c1 = c1;
                this.c2 = c2;
            }
        }

        public struct Triangle
        {
            public Coordinates c1;
            public Coordinates c2;
            public Coordinates c3;
            public bool visible;

            // NOTE: It is important that the triangle points are created in a clockwise order in order to get the proper face for the projection plane
            // Otherwise, the plane will face the opposide direction
            public Triangle(Coordinates c1, Coordinates c2, Coordinates c3)
            {
                this.c1 = c1;
                this.c2 = c2;
                this.c3 = c3;
                this.visible = SetVisibility(CrossProduct());
            }

            public Coordinates CrossProduct()
            {
                Coordinates line1 = new Coordinates(c2.px - c1.px, c2.py - c1.py, c2.z - c1.z);
                Coordinates line2 = new Coordinates(c3.px - c2.px, c3.py - c2.py, c3.z - c2.z);
                Coordinates normal = new Coordinates(
                    line1.y * line2.z - line1.z * line2.y,
                    line1.z * line2.x - line1.x * line2.z,
                    line1.x * line2.y - line1.y * line2.x
                );

                float length = (float)(Math.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z*normal.z));
                normal = new Coordinates(normal.x / length, normal.y / length, normal.z / length);

                return normal;
            }

            public bool SetVisibility(Coordinates c)
            {
                if (c.x * (c1.px - camera.x) +
                    c.y * (c1.py - camera.y) +
                    c.z * (c1.z - camera.z) >= 0.0f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Draw(Graphics g)
            {
                if (this.visible)
                {
                    g.DrawLine(Pens.Red, c1.px, c1.py, c2.px, c2.py);
                    g.DrawLine(Pens.Red, c1.px, c1.py, c3.px, c3.py);
                    g.DrawLine(Pens.Red, c3.px, c3.py, c2.px, c2.py);
                }
                
                //g.DrawLine(Pens.Red, 1, 1, 1, 1);
                //g.DrawLine(Pens.Red, 1, 1, 1, 1);
                //g.DrawLine(Pens.Red, 1, 1, 1, 1);
            }
        }

        public struct Matrix
        {
            public required int[,] m;
            public int rLength;
            public int cLength;

            public Matrix(int[,] m)
            {
                Array.Copy(m, this.m, m.Length);
                this.rLength = m.GetLength(0);
                this.cLength = m.GetLength(1);
            }
        }

        public struct Cube
        {
            Triangle[] triArr;

            public Cube(Coordinates c, float l) // l = length
            {
                triArr = new Triangle[12];

                // naming convention: A = above, U = under, L = left, R = right, F = front, B = back
                // front face coordinates
                Coordinates pointALF = new Coordinates(c.x, c.y, c.z);
                Coordinates pointARF = new Coordinates(c.x + l, c.y, c.z);
                Coordinates pointULF = new Coordinates(c.x, c.y + l, c.z);
                Coordinates pointURF = new Coordinates(c.x + l, c.y + l, c.z);

                // back face coordinates
                Coordinates pointALB = new Coordinates(c.x, c.y, c.z + l);
                Coordinates pointARB = new Coordinates(c.x + l, c.y, c.z + l);
                Coordinates pointULB = new Coordinates(c.x, c.y + l, c.z + l);
                Coordinates pointURB = new Coordinates(c.x + l, c.y + l, c.z + l);

                // front face triangles
                triArr[0] = new Triangle(pointALF, pointARF, pointULF);
                triArr[1] = new Triangle(pointARF, pointURF, pointULF);

                // back face triangles
                triArr[2] = new Triangle(pointARB, pointALB, pointULB);
                triArr[3] = new Triangle(pointURB, pointARB, pointULB);

                // left face triangles
                triArr[4] = new Triangle(pointALB, pointALF, pointULF);
                triArr[5] = new Triangle(pointULB, pointALB, pointULF);

                // right face triangles
                triArr[6] = new Triangle(pointARF, pointARB, pointURF);
                triArr[7] = new Triangle(pointARB, pointURB, pointURF);

                // top face triangles
                triArr[8] = new Triangle(pointARB, pointARF, pointALB);
                triArr[9] = new Triangle(pointARF, pointALF, pointALB);

                // bottom face triangles
                triArr[10] = new Triangle(pointURF, pointURB, pointULB);
                triArr[11] = new Triangle(pointULF, pointURF, pointULB);
            }

            public void Draw(Graphics g)
            {
                foreach(Triangle i in triArr)
                {
                    i.Draw(g);
                }
            }
        }
    }
}
