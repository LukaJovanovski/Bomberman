using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class SpriteEffect : Sprite
    {
        public static readonly AnimationData GemCollection = new()
        { X = 0, Y = 0, Width = 32, Height = 32, Frames = 5, SecondsPerFrame = 0.08f };
        public static readonly AnimationData EnemyDeath = new()
        { X = 0, Y = 0, Width = 40, Height = 41, Frames = 6, SecondsPerFrame = 0.08f };
        public static readonly AnimationData Explosion = new()
        { X = 0, Y = 0, Width = 64, Height = 64, Frames = 6, SecondsPerFrame = 0.08f };


        public SpriteEffect(float x, float y, string imageName, AnimationData animation) : base(
            x, y, animation.Width, animation.Height,
            AssetManager.GetBitmap(imageName), animation)
        {
            // Center the effect on the spawn point
            X = x - (animation.Width / 2f) + 8;
            Y = y - (animation.Height / 2f) + 8;
        }
    }
}
