using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman_App.Models
{
    public class AnimationData
    {
        public int X { get; set; } 
        public int Y { get; set; }
        public int Width { get; set; } 
        public int Height { get; set; } 
        public int Frames { get; set; }
        public float SecondsPerFrame { get; set; } = 0.1f;
    }
}
