using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public class GemUI
    {
        private float _x, _y;
        private SKBitmap _gemIcon;
        private SKPaint _textPaint;

        public GemUI(float x, float y)
        {
            _x = x;
            _y = y;

            _gemIcon = AssetManager.GetBitmap("gem.png");
            _textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 18,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };
        }

        public void Draw(SKCanvas canvas, int gemCount)
        {
            // Draw the small gem icon.
            if (_gemIcon != null)
            {
                SKRect sourceRect = new SKRect(0, 0, 15, 13);
                SKRect destRect = new SKRect(_x, _y, _x + 15, _y + 13);
                canvas.DrawBitmap(_gemIcon, sourceRect, destRect);
            }

            canvas.DrawText(gemCount.ToString(), _x + 20, _y + 11, _textPaint);
        }
    }
}
