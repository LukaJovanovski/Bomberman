using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class Gem : Sprite
    {
        private static readonly AnimationData _gemAnimation = new()
        { X = 0, Y = 0, Width = 15, Height = 13, Frames = 5, SecondsPerFrame = 0.1f };

        public Gem(float x, float y) : base(x, y, 15, 13, AssetManager.GetBitmap("gem.png"), _gemAnimation)
        {
            // The constructor now gets the pre-loaded image from the AssetManager.
        }
    }
}
