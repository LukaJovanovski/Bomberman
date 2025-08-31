using SkiaSharp;
using System.Collections.Generic;

namespace Bomberman_App.Models
{
    public enum DynamiteState { Flying, Ticking, Exploded }

    public class Dynamite
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public SKRect Hitbox { get; private set; }
        public DynamiteState State { get; set; } = DynamiteState.Flying;

        private SKPoint _velocity;
        private float _explosionTimer = 2.0f; 
        private const float GRAVITY = 600f;
        private SKBitmap _sprite;
        private const int WIDTH = 16;
        private const int HEIGHT = 16;

        private float _gracePeriodTimer = 0.2f;
        public bool IsInGracePeriod => _gracePeriodTimer > 0;

        public Dynamite(float startX, float startY, string facingDirection)
        {
            X = startX;
            Y = startY;
            _sprite = AssetManager.GetBitmap("dynamite_pack.png");

            float horizontalVelocity = (facingDirection == "right") ? 150f : -150f;
            _velocity = new SKPoint(horizontalVelocity, -200f);
        }

        public void Update(float deltaTime, List<CollisionBlock> collisionBlocks)
        {
            if (_gracePeriodTimer > 0)
            {
                _gracePeriodTimer -= deltaTime;
            }

            if (State == DynamiteState.Flying)
            {
                _velocity.Y += GRAVITY * deltaTime;
                X += _velocity.X * deltaTime;
                Y += _velocity.Y * deltaTime;
                Hitbox = new SKRect(X, Y, X + WIDTH, Y + HEIGHT);

                // Check for collision with the ground
                foreach (var block in collisionBlocks)
                {
                    if (Hitbox.IntersectsWith(block.Hitbox))
                    {
                        State = DynamiteState.Ticking;
                        _velocity = SKPoint.Empty;
                        Y = block.Hitbox.Top - HEIGHT;
                        break;
                    }
                }
            }
            else if (State == DynamiteState.Ticking)
            {
                _explosionTimer -= deltaTime;
                if (_explosionTimer <= 0)
                {
                    State = DynamiteState.Exploded;
                }
            }
        }

        public void Draw(SKCanvas canvas)
        {
            if (_sprite != null && State != DynamiteState.Exploded)
            {
                SKRect sourceRect = new SKRect(0, 0, 32, 32);
                SKRect destRect = new SKRect(X, Y, X + WIDTH, Y + HEIGHT);
                canvas.DrawBitmap(_sprite, sourceRect, destRect);
            }
        }
    }
}