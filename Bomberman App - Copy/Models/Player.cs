using SkiaSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bomberman_App.Models
{
    public class SpriteAnimation
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Frames { get; set; }
    }

    public class Player
    {
        private const float X_VELOCITY = 200f;
        private const float JUMP_POWER = 250f;
        private const float BOUNCE_POWER = 200f;
        private const float GRAVITY = 700f;

        public SKPoint Position
        {
            get => new SKPoint(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
                UpdateHitbox();
            }
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public SKPoint Velocity { get; private set; }
        public bool IsOnGround { get; set; }

        private SKBitmap _spriteSheet;
        private Dictionary<string, SpriteAnimation> _animations;
        private SpriteAnimation _currentAnimation;
        private int _currentFrame = 0;
        private float _elapsedTime = 0;
        private string _facing = "right";

        public SKRect Hitbox { get; private set; }
        public bool IsInvincible { get; private set; } = false;
        private float _invincibilityTimer = 0f;
        public bool IsRolling { get; private set; }
        private bool _isInAirAfterRolling = false;

        public bool WantsToThrowDynamite { get; set; } = false;
        public string Facing => _facing;

        public Player(float startX, float startY, float size, SKBitmap spriteSheet)
        {
            X = startX;
            Y = startY;
            Width = size;
            Height = size;
            Velocity = new SKPoint(0, 0);
            _spriteSheet = spriteSheet;

            InitializeAnimations();
            _currentAnimation = _animations["fall"];
            UpdateHitbox();
        }

        private void InitializeAnimations()
        {
            _animations = new Dictionary<string, SpriteAnimation>
            {
                { "idle", new SpriteAnimation { X = 0, Y = 0, Width = 33, Height = 32, Frames = 4 } },
                { "run", new SpriteAnimation { X = 0, Y = 32, Width = 33, Height = 32, Frames = 6 } },
                { "jump", new SpriteAnimation { X = 0, Y = 32 * 5, Width = 33, Height = 32, Frames = 1 } },
                { "fall", new SpriteAnimation { X = 33, Y = 32 * 5, Width = 33, Height = 32, Frames = 1 } },
                { "roll", new SpriteAnimation { X = 0, Y = 32 * 9, Width = 33, Height = 32, Frames = 4 } }
            };
        }

        public void Update(float deltaTime, List<CollisionBlock> collisionBlocks)
        {
            if (IsInvincible)
            {
                _invincibilityTimer -= deltaTime;
                if (_invincibilityTimer <= 0)
                {
                    IsInvincible = false;
                }
            }

            _elapsedTime += deltaTime;
            const float secondsInterval = 0.1f;
            if (_elapsedTime > secondsInterval)
            {
                _currentFrame = (_currentFrame + 1) % _currentAnimation.Frames;
                _elapsedTime -= secondsInterval;
            }

            UpdateHorizontalPosition(deltaTime);
            CheckForHorizontalCollisions(collisionBlocks);

            ApplyGravity(deltaTime);
            UpdateVerticalPosition(deltaTime);
            CheckForVerticalCollisions(collisionBlocks);

            DetermineDirection();
            SwitchSprites();
        }

        public void Draw(SKCanvas canvas)
        {
            if (_spriteSheet == null)
            {
                using (var paint = new SKPaint { Color = SKColors.Red })
                {
                    canvas.DrawRect(X, Y, Width, Height, paint);
                }
                return;
            }

            int frameX = _currentAnimation.X + (_currentAnimation.Width * _currentFrame);
            SKRect sourceRect = new SKRect(frameX, _currentAnimation.Y, frameX + _currentAnimation.Width, _currentAnimation.Y + _currentAnimation.Height);
            SKRect destRect = new SKRect(X, Y, X + Width, Y + Height);

            using (var paint = new SKPaint())
            {
                // If the player is invincible, set the paint to be semi-transparent
                if (IsInvincible)
                {
                    paint.Color = SKColors.White.WithAlpha(128); // 50% opacity
                }

                canvas.Save();
                if (_facing == "left")
                {
                    float centerX = X + Width / 2;
                    canvas.Scale(-1, 1, centerX, Y);
                }

                canvas.DrawBitmap(_spriteSheet, sourceRect, destRect, paint);
                canvas.Restore();
            }
        }

        private void CheckForHorizontalCollisions(List<CollisionBlock> collisionBlocks)
        {
            foreach (var block in collisionBlocks)
            {
                if (Hitbox.IntersectsWith(block.Hitbox))
                {
                    // Moving left and hit a block
                    if (Velocity.X < 0)
                    {
                        Velocity = new SKPoint(0, Velocity.Y);
                        X = block.Hitbox.Right - (Hitbox.Left - X); // Reposition player
                        break;
                    }
                    // Moving right and hit a block
                    if (Velocity.X > 0)
                    {
                        Velocity = new SKPoint(0, Velocity.Y);
                        X = block.Hitbox.Left - Hitbox.Width - (Hitbox.Left - X); // Reposition player
                        break;
                    }
                }
            }
        }

        private void CheckForVerticalCollisions(List<CollisionBlock> collisionBlocks)
        {
            // Assume we are not on the ground until we prove otherwise
            IsOnGround = false;

            foreach (var block in collisionBlocks)
            {
                if (Hitbox.IntersectsWith(block.Hitbox))
                {
                    // Falling down and hit the top of a block
                    if (Velocity.Y > 0)
                    {
                        Velocity = new SKPoint(Velocity.X, 0);
                        IsOnGround = true;
                        Y = block.Hitbox.Top - Height;
                        break;
                    }
                    // Jumping up and hit the bottom of a block
                    if (Velocity.Y < 0)
                    {
                        Velocity = new SKPoint(Velocity.X, 0);
                        Y = block.Hitbox.Bottom - (Hitbox.Top - Y);
                        break;
                    }
                }
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            // Only apply gravity if the player is not on the ground
            if (!IsOnGround)
            {
                Velocity = new SKPoint(Velocity.X, Velocity.Y + GRAVITY * deltaTime);
            }
        }

        private void UpdateHorizontalPosition(float deltaTime)
        {
            X += Velocity.X * deltaTime;

            if (X < 2f)
            {
                X = 2f;
                Velocity = new SKPoint(0, Velocity.Y);
            }

            UpdateHitbox();
        }

        private void UpdateVerticalPosition(float deltaTime)
        {
            Y += Velocity.Y * deltaTime;
            UpdateHitbox();
        }

        private void UpdateHitbox()
        {
            Hitbox = new SKRect(X + 4, Y + 9, X + 4 + 20, Y + 9 + 23);
        }

        public void Jump()
        {
            Velocity = new SKPoint(Velocity.X, -JUMP_POWER);
            IsOnGround = false;
            //if (IsOnGround)
            //{
            //    Velocity = new SKPoint(Velocity.X, -JUMP_POWER);
            //    IsOnGround = false;
            //}
        }

        public void Bounce()
        {
            // Give the player an upward boost, but not as strong as a full jump
            Velocity = new SKPoint(Velocity.X, -BOUNCE_POWER);
            IsOnGround = false;
        }

        public void TakeDamage(float duration)
        {
            if (!IsInvincible)
            {
                IsInvincible = true;
                _invincibilityTimer = duration;
            }
        }

        private void DetermineDirection()
        {
            if (Velocity.X > 0) _facing = "right";
            else if (Velocity.X < 0) _facing = "left";
        }

        private void SwitchSprites()
        {
            SpriteAnimation newSprite = _currentAnimation;
            if (!IsOnGround && Velocity.Y < 0) newSprite = _animations["jump"];
            else if (!IsOnGround && Velocity.Y > 0) newSprite = _animations["fall"];
            else if (IsOnGround && Velocity.X != 0) newSprite = _animations["run"];
            else if (IsOnGround) newSprite = _animations["idle"];

            if (_currentAnimation != newSprite)
            {
                _currentAnimation = newSprite;
                _currentFrame = 0;
            }
        }

        public void HandleHorizontalInput(float direction)
        {
            Velocity = new SKPoint(direction * X_VELOCITY, Velocity.Y);
        }
    }
}
