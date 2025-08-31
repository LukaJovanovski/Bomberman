using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class CollisionBlock
    {
        public SKRect Hitbox { get; private set; }

        public CollisionBlock(float x, float y, float width, float height)
        {
            Hitbox = new SKRect(x, y, x + width, y + height);
        }

        // Debugging boxes
        public void Draw(SKCanvas canvas)
        {
            using (var paint = new SKPaint { Color = new SKColor(255, 0, 0, 100) })
            {
                canvas.DrawRect(Hitbox, paint);
            }
        }
    }
}
