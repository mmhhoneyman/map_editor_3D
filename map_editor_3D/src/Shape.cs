using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static map_editor_3D.src.Projection;

namespace map_editor_3D.src
{
    internal class Shape
    {
        public Vector3[] vertices;
        public int[] indices;

        public Vector3 translation;
        public Vector3 scale;
        public Vector3 rotation;

        public Shape(Vector3[] vertices, int[] indices)
        {
            this.vertices = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                this.vertices[i] = new Vector3(vertices[i].X, vertices[i].Y, vertices[i].Z);

            this.indices = indices;
            this.translation = Vector3.Zero;
            this.scale = Vector3.One;
            this.rotation = Vector3.Zero;
        }
        public void Translate(Vector3 v3)
        {
            translation += v3;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].X + v3.X, vertices[i].Y + v3.Y, vertices[i].Z + v3.Z);
            }
        }
        public void normTranslate() // normalize translation to origin
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].X - translation.X, vertices[i].Y - translation.Y, vertices[i].Z - translation.Z);
            }
            translation = new Vector3(0, 0, 0);
        }
        public void Scale(Vector3 v3)
        {
            scale *= v3;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].X * v3.X, vertices[i].Y * v3.Y, vertices[i].Z * v3.Z);
            }
        }
        public void RotateX(float angle)
        {
            rotation.X += angle;
            //translate degrees to radians
            float radAngle = (float)(Math.PI / 180f * angle);
            for (int i = 0; i < vertices.Length; i++)
            {
                float newY = vertices[i].Y * (float)Math.Cos(radAngle) - vertices[i].Z * (float)Math.Sin(radAngle);
                float newZ = vertices[i].Y * (float)Math.Sin(radAngle) + vertices[i].Z * (float)Math.Cos(radAngle);
                vertices[i] = new Vector3(vertices[i].X, newY, newZ);
            }
        }
        public void RotateY(float angle)
        {
            rotation.Y += angle;
            //translate degrees to radians
            float radAngle = (float)(Math.PI / 180f * angle);
            for (int i = 0; i < vertices.Length; i++)
            {
                float newX = vertices[i].X * (float)Math.Cos(radAngle) + vertices[i].Z * (float)Math.Sin(radAngle);
                float newZ = -vertices[i].X * (float)Math.Sin(radAngle) + vertices[i].Z * (float)Math.Cos(radAngle);
                vertices[i] = new Vector3(newX, vertices[i].Y, newZ);
            }
        }
        public void RotateZ(float angle)
        {
            rotation.Z += angle;
            //translate degrees to radians
            float radAngle = (float)(Math.PI / 180f * angle);
            for (int i = 0; i < vertices.Length; i++)
            {
                float newX = vertices[i].X * (float)Math.Cos(radAngle) - vertices[i].Y * (float)Math.Sin(radAngle);
                float newY = vertices[i].X * (float)Math.Sin(radAngle) + vertices[i].Y * (float)Math.Cos(radAngle);
                vertices[i] = new Vector3(newX, newY, vertices[i].Z);
            }
        }
        public Triangle[] FindTriangles(Camera camera, Vector3 light)
        {
            Triangle[] triangles = new Triangle[indices.Length / 3];
            for (int i = 0; i < indices.Length; i += 3)
            {
                Triangle t = new Triangle(vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], camera, light);
                triangles[i / 3] = t;
            }
            return triangles;
        }
    }
}
