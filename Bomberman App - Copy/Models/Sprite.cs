using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class Sprite
    {
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public float Width { get; protected set; }
        public float Height { get; protected set; }
        public SKRect Hitbox { get; protected set; }

        // --- Animation State ---
        protected SKBitmap SpriteSheet;
        protected AnimationData CurrentAnimation;
        protected int CurrentFrame = 0;
        private float _frameTime = 0f;

        public int Iteration { get; private set; } = 0;
        public bool IsFinished => Iteration > 0;

        public Sprite(float x, float y, float width, float height, SKBitmap spriteSheet, AnimationData animation)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            SpriteSheet = spriteSheet;
            CurrentAnimation = animation;
            Hitbox = new SKRect(x, y, x + width, y + height);
        }

        public virtual void Update(float deltaTime)
        {
            if (CurrentAnimation == null || CurrentAnimation.Frames <= 1) return;

            _frameTime += deltaTime;

            if (_frameTime >= CurrentAnimation.SecondsPerFrame)
            {
                CurrentFrame++;
                _frameTime -= CurrentAnimation.SecondsPerFrame;

                if (CurrentFrame >= CurrentAnimation.Frames)
                {
                    CurrentFrame = 0;
                    Iteration++;
                }
            }
        }

        public virtual void Draw(SKCanvas canvas)
        {
            if (SpriteSheet == null || CurrentAnimation == null)
            {
                using (var paint = new SKPaint { Color = SKColors.Magenta })
                {
                    canvas.DrawRect(X, Y, Width, Height, paint);
                }
                return;
            }

            int frameX = CurrentAnimation.X + (CurrentAnimation.Width * CurrentFrame);
            SKRect sourceRect = new SKRect(frameX, CurrentAnimation.Y, frameX + CurrentAnimation.Width, CurrentAnimation.Y + CurrentAnimation.Height);
            SKRect destRect = new SKRect(X, Y, X + Width, Y + Height);

            canvas.DrawBitmap(SpriteSheet, sourceRect, destRect);
        }
    }
}
