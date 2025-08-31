using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public abstract class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; protected set; }
        public float Height { get; protected set; }

        public SKRect Hitbox { get; protected set; }

        public Enemy(float startX, float startY)
        {
            X = startX;
            Y = startY;
        }

        public abstract void Update(float deltaTime, List<CollisionBlock> collisionBlocks);
        public abstract void Draw(SKCanvas canvas);
    }
}
