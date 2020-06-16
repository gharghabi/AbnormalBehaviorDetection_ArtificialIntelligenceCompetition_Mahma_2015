using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheEnd1
{
    class Particles
    {
        public int x;
        public int y;
        public float deltaX;
        public float deltaY;
        public float v;
        public double angle;
        public double DeltaAngle;
        public Particles(int xin, int yin)
        {
            x = xin;
            y = yin;
            angle = 0;
            v = 0;
            DeltaAngle = 0;
            deltaX = 0;
            deltaY = 0;
        }
    }
}
