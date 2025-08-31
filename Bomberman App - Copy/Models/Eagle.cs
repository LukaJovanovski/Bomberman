using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class Eagle : Enemy
    {
        private const float X_VELOCITY = -50f;
        private const float TURNING_DISTANCE = 100f;

        private SKBitmap _spriteSheet;
        private SKPoint _velocity;
        private string _facing = "left";

        private Dictionary<string, SpriteAnimation> _animations;
        private SpriteAnimation _currentAnimation;
        private int _currentFrame = 0;
        private float _frameTime = 0f;
        private const float SECONDS_PER_FRAME = 0.1f;

        private float _distanceTraveled = 0f;

        public Eagle(float startX, float startY) : base(startX, startY) 
        {
            Width = 40;
            Height = 41;
            _velocity = new SKPoint(X_VELOCITY, 0);

            Task.Run(async () => {
                _spriteSheet = await LoadBitmapAsync("eagle.png");
            });

            InitializeAnimations();
            _currentAnimation = _animations["fly"];
            UpdateHitbox();
        }

        private void InitializeAnimations()
        {
            _animations = new Dictionary<string, SpriteAnimation>
            {
                { "fly", new SpriteAnimation { X = 0, Y = 0, Width = 40, Height = 41, Frames = 4 } }
            };
        }

        public override void Update(float deltaTime, List<CollisionBlock> collisionBlocks)
        {
            UpdatePatrol(deltaTime);
            UpdateAnimation(deltaTime);
            UpdateHitbox();
        }

        private void UpdatePatrol(float deltaTime)
        {
            if (System.Math.Abs(_distanceTraveled) >= TURNING_DISTANCE)
            {
                _velocity.X *= -1;
                _facing = (_velocity.X > 0) ? "right" : "left";
                _distanceTraveled = 0;
            }
            X += _velocity.X * deltaTime;
            _distanceTraveled += _velocity.X * deltaTime;
        }

        private void UpdateAnimation(float deltaTime)
        {
            _frameTime += deltaTime;
            if (_frameTime >= SECONDS_PER_FRAME)
            {
                _currentFrame = (_currentFrame + 1) % _currentAnimation.Frames;
                _frameTime -= SECONDS_PER_FRAME;
            }
        }

        private void UpdateHitbox()
        {
            Hitbox = new SKRect(X, Y, X + Width, Y + Height);
        }

        public override void Draw(SKCanvas canvas)
        {
            if (_spriteSheet == null)
            {
                using (var paint = new SKPaint { Color = SKColors.Purple })
                {
                    canvas.DrawRect(X, Y, Width, Height, paint);
                }
                return;
            }

            int frameX = _currentAnimation.X + (_currentAnimation.Width * _currentFrame);
            SKRect sourceRect = new SKRect(frameX, _currentAnimation.Y, frameX + _currentAnimation.Width, _currentAnimation.Y + _currentAnimation.Height);
            SKRect destRect = new SKRect(X, Y, X + Width, Y + Height);

            canvas.Save();
            if (_facing == "right")
            {
                float centerX = X + Width / 2;
                canvas.Scale(-1, 1, centerX, Y);
            }
            canvas.DrawBitmap(_spriteSheet, sourceRect, destRect);
            canvas.Restore();
        }

        private async Task<SKBitmap> LoadBitmapAsync(string fileName)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                return SKBitmap.Decode(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
