using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class Heart
    {
        public float X { get; }
        public float Y { get; }
        public bool IsDepleted { get; set; } = false;

        private const int SPRITE_WIDTH = 21;
        private const int SPRITE_HEIGHT = 18;

        public Heart(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Draw(SKCanvas canvas)
        {
            var spriteSheet = AssetManager.GetBitmap("hearts.png");
            if (spriteSheet == null) return;

            // Frame 0 is the full heart.
            // Frame 1 is the depleted (empty) heart.
            int frameX = IsDepleted ? SPRITE_WIDTH * 1 : SPRITE_WIDTH * 0;

            SKRect sourceRect = new SKRect(frameX, 0, frameX + SPRITE_WIDTH, SPRITE_HEIGHT);
            SKRect destRect = new SKRect(X, Y, X + SPRITE_WIDTH, Y + SPRITE_HEIGHT);

            canvas.DrawBitmap(spriteSheet, sourceRect, destRect);
        }
    }
}
