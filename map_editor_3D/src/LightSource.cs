using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace map_editor_3D.src
{
    internal class LightSource
    {
        // similar to camera
        public Vector3 Position { get; set; }

        public LightSource(Vector3 pos)
        {
            this.Position = pos;
        }
    }
}
