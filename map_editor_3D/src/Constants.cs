using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace map_editor_3D.src
{
    internal static class Constants
    {
        //=========================================== Screen/Plane Information ===========================================
        public static float SCREEN_HEIGHT = Screen.PrimaryScreen.Bounds.Height;
        public static float SCREEN_WIDTH = Screen.PrimaryScreen.Bounds.Width;
        public static float SCREEN_AREA = SCREEN_HEIGHT / SCREEN_WIDTH;

        public static float Z_FAR = 1000f; // how far you can see
        public static float Z_NEAR = 0.1f; // max closeness objects can to be to render

        public static float FOV = 90f;
        public static float RAD_FOV = (float)Math.PI / 180f * FOV;
        public static float P_FOV = (float)(1 / Math.Tan(RAD_FOV / 2f));

        public static float[,] PROJECTION_MATRIX = {
            { Constants.SCREEN_AREA * Constants.P_FOV, 0, 0, 0 },
            { 0, Constants.P_FOV, 0, 0 },
            { 0, 0, Constants.Z_FAR / (Constants.Z_FAR - Constants.Z_NEAR), 1 },
            { 0, 0, (-1f * Constants.Z_FAR * Constants.Z_NEAR) / (Constants.Z_FAR - Constants.Z_NEAR), 0 }
        };


        //=========================================== Camera/Mouse ===========================================
        public static float EPSILON = 0.1f;
        public static float MOUSE_SENSITIVITY = 0.1f;
        public static float MAX_SPEED = 10f;
        public static float THRUST = 1.5f;
        public static float BREAKING = 2f;
        public static float SPEED_DECAY = 0.5f;
        public static float MAX_PITCH = 80f;
        public static float MIN_PITCH = -80f;


        //=========================================== World ===========================================
        public static float MAX_EDGE_LENGTH = 5f;

        //=========================================== Lighting ===========================================
        public static float AMBIENT = 0.05f;
        public static float SCALING_FACTOR = 1f - AMBIENT;
        public static float SMOOTHING = 0.8f;
    }
}
