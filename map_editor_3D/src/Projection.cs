using System;
using System.Diagnostics.Metrics;
using System.Drawing.Drawing2D;
using System.Numerics;
using static map_editor_3D.src.Camera;
using static System.Windows.Forms.DataFormats;

namespace map_editor_3D.src
{
    class Projection
    {
        private Camera camera; // camera location and direction

        private LightSource lightSource; // light location and direction
        private float length;
        private Vector3 light;

        private MeshObject mo;

        private Triangle[] triArr;

        public Projection()
        {
            camera = new Camera(new Vector3(0, 0, 0), -15, 45, 0.1f);

            lightSource = new LightSource(new Vector3(0.5f, 1f, 0.2f));
            length = (float)Math.Sqrt(lightSource.Position.X * lightSource.Position.X + lightSource.Position.Y * lightSource.Position.Y + lightSource.Position.Z * lightSource.Position.Z);
            light = new Vector3(lightSource.Position.X / length, lightSource.Position.Y / length, lightSource.Position.Z / length);

            mo = new MeshObject();

            triArr = new Triangle[0];
        }
        public void ApplyCameraTransformations(Shape[] shapes)
        { 
            for (int i = 0; i < shapes.Length; i++)
            {
                shapes[i].Translate(new Vector3(-camera.Position.X, -camera.Position.Y, -camera.Position.Z));
                shapes[i].RotateY(-camera.Yaw - 90);
                shapes[i].RotateX(-camera.Pitch);
            }
        }
        // extracts all triangles from Shape and adds it to one master list
        public Triangle[] AddAllTriangles(Shape[] shapes)
        {
            Triangle[][] temp = new Triangle[shapes.Length][];
            int totalCount = 0;

            for (int i = 0; i < shapes.Length; i++)
            {
                temp[i] = shapes[i].FindTriangles(camera, light);
                totalCount += temp[i].Length;
            }

            Triangle[] triangles = new Triangle[totalCount];
            int currentIndex = 0;

            for (int i = 0; i < temp.Length; i++)
            {
                Array.Copy(temp[i], 0, triangles, currentIndex, temp[i].Length);
                currentIndex += temp[i].Length;
            }

            return triangles;
        }

        private Triangle[] SubdivideAllLargeTriangles(Triangle[] triangles)
        {
            List<Triangle> result = new List<Triangle>();

            foreach (var tri in triangles)
            {
                SubdivideTriangle(tri, result, Constants.MAX_EDGE_LENGTH);
            }

            return result.ToArray();
        }

        // recursive helper
        private void SubdivideTriangle(Triangle tri, List<Triangle> output, float maxEdge)
        {
            // compute edge lengths in world space
            float d12 = Vector3.Distance(tri.C1, tri.C2);
            float d23 = Vector3.Distance(tri.C2, tri.C3);
            float d31 = Vector3.Distance(tri.C3, tri.C1);

            float maxEdgeLength = Math.Max(d12, Math.Max(d23, d31));

            // if triangle is small enough, add it and return
            if (maxEdgeLength <= maxEdge)
            {
                output.Add(tri);
                return;
            }

            // split along longest edge
            if (maxEdgeLength == d12)
            {
                Vector3 mid = (tri.C1 + tri.C2) / 2f;
                SubdivideTriangle(new Triangle(tri.C1, mid, tri.C3, camera, light), output, maxEdge);
                SubdivideTriangle(new Triangle(mid, tri.C2, tri.C3, camera, light), output, maxEdge);
            }
            else if (maxEdgeLength == d23)
            {
                Vector3 mid = (tri.C2 + tri.C3) / 2f;
                SubdivideTriangle(new Triangle(tri.C2, mid, tri.C1, camera, light), output, maxEdge);
                SubdivideTriangle(new Triangle(mid, tri.C3, tri.C1, camera, light), output, maxEdge);
            }
            else // d31
            {
                Vector3 mid = (tri.C3 + tri.C1) / 2f;
                SubdivideTriangle(new Triangle(tri.C3, mid, tri.C2, camera, light), output, maxEdge);
                SubdivideTriangle(new Triangle(mid, tri.C1, tri.C2, camera, light), output, maxEdge);
            }
        }

        public Triangle[] ClipTriangles(Triangle[] triangles)
        {
            List<Triangle> result = new List<Triangle>();
            float near = camera.NearPlane;
            const float EPS = 0.0001f;

            foreach (var t in triangles)
            {
                Vector3[] verts = { t.C1, t.C2, t.C3 };

                List<Vector3> inside = new();
                List<Vector3> outside = new();

                foreach (var v in verts)
                {
                    if (v.Z >= near - EPS)
                        inside.Add(v);
                    else
                        outside.Add(v);
                }

                if (inside.Count == 0)
                    continue;

                if (true)//(inside.Count == 3)
                {
                    result.Add(t);
                    continue;
                }

                if (inside.Count == 1)
                {
                    Vector3 A = inside[0];
                    Vector3 B = IntersectNearPlane(A, outside[0], near);
                    Vector3 C = IntersectNearPlane(A, outside[1], near);

                    result.Add(new Triangle(A, B, C, camera, light));
                }
                else if (inside.Count == 2)
                {
                    Vector3 A = inside[0];
                    Vector3 B = inside[1];

                    Vector3 C = IntersectNearPlane(A, outside[0], near);
                    Vector3 D = IntersectNearPlane(B, outside[0], near);

                    result.Add(new Triangle(A, B, C, camera, light));
                    result.Add(new Triangle(B, D, C, camera, light));
                }
            }

            return result.ToArray();
        }

        // helper to interpolate a vertex along the line to the near plane
        private Vector3 IntersectNearPlane(Vector3 inside, Vector3 outside, float near)
        {
            float t = (near - inside.Z) / (outside.Z - inside.Z);
            Vector3 v = inside + t * (outside - inside);

            if (v.Z >= near - Constants.EPSILON)
            {
                v.Z = near;
            }

            return v;
        }

        public Triangle[] SortTriangles(Triangle[] triangles)
        {
            Array.Sort(triangles, (t1, t2) =>
            {
                // find farthest Z for each triangle
                float z1 = Math.Max(t1.C1.Z, Math.Max(t1.C2.Z, t1.C3.Z));
                float z2 = Math.Max(t2.C1.Z, Math.Max(t2.C2.Z, t2.C3.Z));

                return z2.CompareTo(z1); // farthest first
            });
            return triangles;
        }

        public void Update(float delta, long currentTime)
        {
            //System.Diagnostics.Debug.WriteLine(s1.vertices[0]);
            float frameRot = (float)(25 * currentTime / 1000.0);
            Shape[] shapes = new Shape[] {
                new Shape((Vector3[])mo.CubeVertices.Clone(), (int[])mo.CubeIndices.Clone()),
                new Shape((Vector3[])mo.SphereVertexArray.Clone(), (int[])mo.SphereIndexArray.Clone()),
                new Shape((Vector3[])mo.PyramidVertices.Clone(), (int[])mo.PyramidIndices.Clone()),
                new Shape((Vector3[])mo.CubeVertices.Clone(), (int[])mo.CubeIndices.Clone()),
                new Shape((Vector3[])mo.SphereVertexArray.Clone(), (int[])mo.SphereIndexArray.Clone()),
                new Shape((Vector3[])mo.PyramidVertices.Clone(), (int[])mo.PyramidIndices.Clone()),
                new Shape((Vector3[])mo.PlaneVertices.Clone(), (int[])mo.PlaneIndices.Clone()),
                new Shape((Vector3[])mo.PlaneVertices.Clone(), (int[])mo.PlaneIndices.Clone())
            };
            shapes[0].RotateX(-frameRot);
            //shapes[0].RotateY(frameRot);
            shapes[0].RotateZ(-frameRot);
            float rotSpeed = (float)(2.0 / Math.PI * 3.0);
            float radius = 2;
            shapes[0].Scale(new Vector3(1.25f + 1f / 4f * (float) Math.Cos(3.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.1 * rotSpeed * currentTime / 1000.0)));
            shapes[0].Translate(new Vector3(0f, 2f, 5f));
            shapes[0].Translate(new Vector3(radius * (float)Math.Cos(rotSpeed * currentTime / 1000.0), radius * (float)Math.Sin(rotSpeed * currentTime / 1000.0), 0));

            //shapes[1].Scale(new Vector3(1.25f + 1f / 4f * (float)Math.Cos(3.2 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.2 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.2 * rotSpeed * currentTime / 1000.0)));
            //shapes[1].RotateX(1.1f * frameRot);
            shapes[1].RotateY(1.2f * frameRot);
            //shapes[1].RotateZ(1.3f * frameRot);
            shapes[1].Translate(new Vector3(0f, 0f, 4f));

            shapes[2].Scale(new Vector3(1.25f + 1f / 4f * (float)Math.Cos(1.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(1.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(1.1 * rotSpeed * currentTime / 1000.0)));
            shapes[2].RotateX(180f);
            shapes[2].RotateZ(3f * frameRot);
            shapes[2].Translate(new Vector3(0f, -4f, 5f));
            shapes[2].Translate(new Vector3(-4f * radius * (float)Math.Cos(rotSpeed * currentTime / 1000.0 / 2), 0, 0));

            shapes[5].RotateX(-frameRot);
            //shapes[0].RotateY(frameRot);

            shapes[5].Scale(new Vector3(1.25f + 1f / 4f * (float)Math.Cos(3.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.1 * rotSpeed * currentTime / 1000.0)));
            shapes[5].Translate(new Vector3(0f, 2f, -5f));
            shapes[5].Translate(new Vector3(radius * (float)Math.Cos(rotSpeed * currentTime / 1000.0), radius * (float)Math.Sin(rotSpeed * currentTime / 1000.0), 0));

            //shapes[1].Scale(new Vector3(1.25f + 1f / 4f * (float)Math.Cos(3.2 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.2 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(3.2 * rotSpeed * currentTime / 1000.0)));
            //shapes[1].RotateX(1.1f * frameRot);
            shapes[3].RotateY(1.2f * frameRot);
            //shapes[1].RotateZ(1.3f * frameRot);
            shapes[3].Translate(new Vector3(0f, 0f, -5f));

            shapes[4].Scale(new Vector3(1.25f + 1f / 4f * (float)Math.Cos(1.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(1.1 * rotSpeed * currentTime / 1000.0), 1.25f + 1f / 4f * (float)Math.Cos(1.1 * rotSpeed * currentTime / 1000.0)));
            shapes[4].RotateX(180f);
            shapes[4].RotateZ(3f * frameRot);
            shapes[4].Translate(new Vector3(0f, -4f, -5f));
            shapes[4].Translate(new Vector3(-4f * radius * (float)Math.Cos(rotSpeed * currentTime / 1000.0 / 2), 0, 0));

            shapes[6].Scale(new Vector3(25, 0, 2));
            shapes[6].RotateX(-90f);
            shapes[6].RotateZ((float)(10f * Math.Cos(frameRot / 3)));
            shapes[6].Translate(new Vector3(0f, 0f, 7f));
            shapes[6].RotateY(-90f);

            shapes[7].Scale(new Vector3(25, 0, 2));
            shapes[7].RotateX(-90f);
            shapes[7].RotateZ(5f * frameRot);
            shapes[7].Translate(new Vector3(0f, 0f, 7f));
            shapes[7].RotateY(90f);

            shapes[0].RotateY(90);
            shapes[1].RotateY(90);
            shapes[2].RotateY(90);
            shapes[3].RotateY(90);
            shapes[4].RotateY(90);
            shapes[5].RotateY(90);
            shapes[6].RotateY(90);
            shapes[7].RotateY(90);

            camera.Update(delta);
            this.ApplyCameraTransformations(shapes);
            triArr = this.AddAllTriangles(shapes);
            //triArr = this.SubdivideAllLargeTriangles(triArr);
        }

        public void Draw(Graphics g)
        { 
            //triArr = this.ClipTriangles(triArr);
            triArr = this.SortTriangles(triArr);
            for (int i = 0; i < triArr.Length; i++)
            {
                triArr[i].Draw(g, light);
            }
        }
    }
}
