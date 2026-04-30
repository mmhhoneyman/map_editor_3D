using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace map_editor_3D.src
{
    internal class Camera
    {
        private Vector3 speed; 

        private int forwardAxis;   // -1, 0, 1
        private int rightAxis;     // -1, 0, 1
        private int verticalAxis;     // -1, 0, 1

        public Vector3 Position { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float NearPlane { get; set; } = Constants.Z_NEAR;
        public float FarPlane { get; set; } = Constants.Z_FAR;

        public Camera(Vector3 pos, float pitch, float yaw, float nearPlane)
        {
            this.Position = pos;
            this.Pitch = pitch;
            this.Yaw = yaw;
            this.NearPlane = nearPlane;

            this.speed = Vector3.Zero;

            this.forwardAxis = 0;
            this.rightAxis = 0;
        }
        public void Update(float delta)
        {
            this.CheckInputs();
            this.UpdateCameraRotationFromMouse();

            float cosYaw = (float)Math.Cos((float)Math.PI * Yaw / 180);
            float sinYaw = (float)Math.Sin((float)Math.PI * Yaw / 180);
            float cosPitch = (float)Math.Cos((float)Math.PI * Pitch / 180);
            float sinPitch = (float)Math.Sin((float)Math.PI * Pitch / 180);

            Vector3 forward = new Vector3(
                cosYaw * cosPitch,
                -sinPitch,
                -sinYaw * cosPitch
            );
            forward = Vector3.Normalize(forward);

            Vector3 worldUp = new Vector3(0, -1, 0);

            Vector3 right = new Vector3(
                sinYaw,
                0,
                cosYaw
            );
            right = Vector3.Normalize(right);

            Vector3 up = Vector3.Cross(forward, right);

            Vector3 inputDir = (forward * forwardAxis) + (-right * rightAxis) + (worldUp * verticalAxis);

            Vector3 acceleration = Vector3.Zero;

            if (speed.LengthSquared() > 0.0001f) // avoid division by zero
            {
                Vector3 velDir = Vector3.Normalize(speed);
                float dot = Vector3.Dot(inputDir, velDir);
                if (dot < 0f)
                {
                    inputDir *= Constants.BREAKING; // double acceleration when braking
                }
            }

            acceleration += inputDir * Constants.THRUST;

            speed += acceleration * delta;

            if (!(Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.D)))
            {
                speed *= 1f - Constants.SPEED_DECAY * delta;
            }

            // clamp velocity
            speed.X = Math.Clamp(speed.X, -Constants.MAX_SPEED, Constants.MAX_SPEED);
            speed.Y = Math.Clamp(speed.Y, -Constants.MAX_SPEED, Constants.MAX_SPEED);
            speed.Z = Math.Clamp(speed.Z, -Constants.MAX_SPEED, Constants.MAX_SPEED);

            Position += speed * delta;
        }

        public void UpdateCameraRotationFromMouse()
        {
            System.Drawing.Point mousePos = Cursor.Position;

            // compute delta from screen center
            int deltaX = mousePos.X - (int)(Constants.SCREEN_WIDTH / 2);
            int deltaY = (int)(Constants.SCREEN_HEIGHT / 2) - mousePos.Y; // must be reversed because Y axis is flipped in screen space

            Yaw += deltaX * Constants.MOUSE_SENSITIVITY;
            Pitch += deltaY * Constants.MOUSE_SENSITIVITY;

            // clamp pitch to avoid flipping
            Pitch = Math.Clamp(Pitch, Constants.MIN_PITCH, Constants.MAX_PITCH);
        }

        public void CheckInputs()
        {
            //NOTE: W and S are reversed because my logic has a coordinate system mismatch
            forwardAxis = 0;
            if (Input.IsKeyDown(Keys.W)) forwardAxis += 1; 
            if (Input.IsKeyDown(Keys.S)) forwardAxis -= 1;

            rightAxis = 0;
            if (Input.IsKeyDown(Keys.D)) rightAxis += 1;
            if (Input.IsKeyDown(Keys.A)) rightAxis -= 1;

            verticalAxis = 0;
            if (Input.IsKeyDown(Keys.Space)) verticalAxis += 1;
            if (Input.IsCtrlDown()) verticalAxis -= 1;
        }
    }
}
