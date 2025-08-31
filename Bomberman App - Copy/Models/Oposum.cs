using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class Oposum : Enemy
    {
        private const float X_VELOCITY = -20f;
        private const float GRAVITY = 580f;
        private const float TURNING_DISTANCE = 100f;
        private SKPoint _velocity;
        private bool _isOnGround = false;
        private float _distanceTraveled = 0f;

        // Visuals & Animation
        private SKBitmap _spriteSheet;
        private string _facing = "left";
        private Dictionary<string, SpriteAnimation> _animations;
        private SpriteAnimation _currentAnimation;
        private int _currentFrame = 0;
        private float _frameTime = 0f;
        private const float SECONDS_PER_FRAME = 0.1f;

        public Oposum(float startX, float startY) : base(startX, startY)
        {
            Width = 36;
            Height = 28;
            _velocity = new SKPoint(X_VELOCITY, 0);

            Task.Run(async () => {
                _spriteSheet = await LoadBitmapAsync("oposum.png");
            });

            InitializeAnimations();
            _currentAnimation = _animations["run"];
            UpdateHitbox();
        }

        private void InitializeAnimations()
        {
            _animations = new Dictionary<string, SpriteAnimation>
            {
                { "run", new SpriteAnimation { X = 0, Y = 0, Width = 36, Height = 28, Frames = 6 } }
            };
        }

        public override void Update(float deltaTime, List<CollisionBlock> collisionBlocks)
        {
            UpdatePatrol(deltaTime);
            UpdateAnimation(deltaTime);

            ApplyGravity(deltaTime);

            X += _velocity.X * deltaTime;
            UpdateHitbox();
            CheckHorizontalCollisions(collisionBlocks);

            Y += _velocity.Y * deltaTime;
            UpdateHitbox();
            CheckVerticalCollisions(collisionBlocks);
        }

        private void UpdatePatrol(float deltaTime)
        {
            if (_isOnGround && System.Math.Abs(_distanceTraveled) >= TURNING_DISTANCE)
            {
                _velocity.X *= -1;
                _facing = (_velocity.X > 0) ? "right" : "left";
                _distanceTraveled = 0;
            }
            _distanceTraveled += _velocity.X * deltaTime;
        }

        private void ApplyGravity(float deltaTime)
        {
            _velocity.Y += GRAVITY * deltaTime;
        }

        private void CheckHorizontalCollisions(List<CollisionBlock> collisionBlocks)
        {
            foreach (var block in collisionBlocks)
            {
                if (Hitbox.IntersectsWith(block.Hitbox))
                {
                    if (_velocity.X < 0) // Moving left
                    {
                        X = block.Hitbox.Right;
                    }
                    else if (_velocity.X > 0) // Moving right
                    {
                        X = block.Hitbox.Left - Width;
                    }
                    _velocity.X *= -1; // Turn around
                    _facing = (_velocity.X > 0) ? "right" : "left";
                    UpdateHitbox();
                    break;
                }
            }
        }

        private void CheckVerticalCollisions(List<CollisionBlock> collisionBlocks)
        {
            _isOnGround = false;
            foreach (var block in collisionBlocks)
            {
                if (Hitbox.IntersectsWith(block.Hitbox))
                {
                    if (_velocity.Y > 0) // Moving down
                    {
                        Y = block.Hitbox.Top - Height;
                        _velocity.Y = 0;
                        _isOnGround = true;
                        UpdateHitbox();
                        break;
                    }
                    if (_velocity.Y < 0) // Moving up
                    {
                        Y = block.Hitbox.Bottom;
                        _velocity.Y = 0;
                        UpdateHitbox();
                        break;
                    }
                }
            }
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
            // A tighter hitbox than the full sprite
            Hitbox = new SKRect(X, Y + 9, X + 30, Y + 9 + 19);
        }

        public override void Draw(SKCanvas canvas)
        {
            if (_spriteSheet == null) { /* ... fallback drawing ... */ return; }

            int frameX = _currentAnimation.X + (_currentAnimation.Width * _currentFrame);
            SKRect sourceRect = new SKRect(frameX, _currentAnimation.Y, frameX + _currentAnimation.Width, _currentAnimation.Y + _currentAnimation.Height);
            SKRect destRect = new SKRect(X, Y, X + Width, Y + Height);

            canvas.Save();
            if (_facing == "right")
            {
                canvas.Scale(-1, 1, X + Width / 2, Y);
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
            catch { return null; }
        }
    }
}
