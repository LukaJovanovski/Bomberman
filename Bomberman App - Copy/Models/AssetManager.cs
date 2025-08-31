using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Bomberman_App.Models
{
    public static class AssetManager
    {
        private static Dictionary<string, SKBitmap> _bitmaps = new();

        public static async Task LoadAssetsAsync()
        {
            _bitmaps.Clear();

            await LoadBitmap("gem.png");
            await LoadBitmap("item_feedback.png");
            await LoadBitmap("hearts.png");
            await LoadBitmap("enemy_death.png");

            await LoadBitmap("dynamite_pack.png");
            await LoadBitmap("explosion.png");
        }

        private static async Task LoadBitmap(string fileName)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                if (stream != null)
                {
                    _bitmaps[fileName] = SKBitmap.Decode(stream);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load asset: {fileName}. Exception: {ex.Message}");
            }
        }

        public static SKBitmap GetBitmap(string fileName)
        {
            _bitmaps.TryGetValue(fileName, out var bitmap);
            return bitmap; // Will return null if not found
        }
    }
}
