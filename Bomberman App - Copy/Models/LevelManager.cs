using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json;
using SkiaSharp;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Bomberman_App.Models
{
    public class LevelManager
    {
        public SKBitmap PreRenderedBackground { get; private set; }
        public List<CollisionBlock> CollisionBlocks { get; private set; }
        public List<Enemy> Enemies { get; private set; }

        public List<Gem> Gems { get; private set; }

        private const int TileSize = 16;
        public int LevelWidth { get; private set; }
        public int LevelHeight { get; private set; }

        public LevelManager()
        {
            CollisionBlocks = new List<CollisionBlock>();

            Enemies = new List<Enemy>();
            Gems = new List<Gem>();
        }

        public async Task LoadAndPreRenderLevelAsync(string levelDirectory)
        {
            CollisionBlocks.Clear();
            Enemies.Clear();
            Gems.Clear();

            var tileset = await LoadBitmapAsync("tileset.png");
            var decorations = await LoadBitmapAsync("decorations.png");

            if (tileset == null || decorations == null)
            {
                Debug.WriteLine("Failed to load one or more tilesets.");
                return;
            }

            var layerTilesets = new Dictionary<string, SKBitmap>
            {
                { "l_Sky_Ocean.json", decorations },
                { "l_Back_Tiles.json", tileset },
                { "l_Decorations.json", decorations },
                { "l_Gems.json", decorations },
                { "l_Collisions.json", decorations }, 
                { "l_Front_Tiles.json", tileset },
                { "l_Bramble.json", decorations }
            };

            var collisionLayout = await LoadLayoutAsync(Path.Combine(levelDirectory, "collisions.json"));
            if (collisionLayout == null)
            {
                Debug.WriteLine("Failed to load critical collision layout 'collisions.json'. Cannot create level.");
                return;
            }

            LevelWidth = collisionLayout[0].Length * TileSize;
            LevelHeight = collisionLayout.Length * TileSize;
            CreateCollisionBlocks(collisionLayout);

            PreRenderedBackground = new SKBitmap(LevelWidth, LevelHeight);
            using (var canvas = new SKCanvas(PreRenderedBackground))
            {
                canvas.Clear(SKColors.Transparent);

                var renderOrder = new List<string>
                {
                    "l_Sky_Ocean.json",
                    "l_Bramble.json",
                    "l_Back_Tiles.json",
                    //"l_Gems.json",
                    "l_Front_Tiles.json",
                    "l_Decorations.json",
                    "l_Collisions.json",

                };

                foreach (var layerFileName in renderOrder)
                {
                    if (layerTilesets.TryGetValue(layerFileName, out var layerTileset))
                    {
                        var layout = await LoadLayoutAsync(Path.Combine(levelDirectory, layerFileName));
                        if (layout != null)
                        {
                            DrawTileLayer(canvas, layerTileset, layout);
                        }
                    }
                }
            }

            LevelWidth = collisionLayout[0].Length * TileSize;
            LevelHeight = collisionLayout.Length * TileSize;

            var gemLayout = await LoadLayoutAsync(Path.Combine(levelDirectory, "l_Gems.json"));
            if (gemLayout != null)
            {
                for (int row = 0; row < gemLayout.Length; row++)
                {
                    for (int col = 0; col < gemLayout[row].Length; col++)
                    {
                        if (gemLayout[row][col] == 18)
                        {
                            Gems.Add(new Gem(col * TileSize, row * TileSize));
                        }
                    }
                }
            }

            var enemyDataPath = Path.Combine(levelDirectory, "enemies.json");
            var enemyDataList = await LoadEnemyDataAsync(enemyDataPath);

            if (enemyDataList != null)
            {
                foreach (var data in enemyDataList)
                {
                    switch (data.Type)
                    {
                        case "Eagle":
                            Enemies.Add(new Eagle(data.X, data.Y));
                            break;
                        case "Oposum":
                            Enemies.Add(new Oposum(data.X, data.Y));
                            break;
                    }

                    Debug.WriteLine($"Loaded enemy: {data.Type} at ({data.X}, {data.Y})");
                }
            }
        }

        private void DrawTileLayer(SKCanvas canvas, SKBitmap tileset, int[][] layout)
        {
            int tilesetCols = tileset.Width / TileSize;

            for (int row = 0; row < layout.Length; row++)
            {
                for (int col = 0; col < layout[row].Length; col++)
                {
                    int tileId = layout[row][col];
                    if (tileId <= 0) continue;

                    int tileIndex = tileId - 1;
                    int sourceX = (tileIndex % tilesetCols) * TileSize;
                    int sourceY = (tileIndex / tilesetCols) * TileSize;

                    SKRect sourceRect = new SKRect(sourceX, sourceY, sourceX + TileSize, sourceY + TileSize);
                    SKRect destRect = new SKRect(col * TileSize, row * TileSize, (col + 1) * TileSize, (row + 1) * TileSize);

                    canvas.DrawBitmap(tileset, sourceRect, destRect);
                }
            }
        }

        private void CreateCollisionBlocks(int[][] layout)
        {
            for (int row = 0; row < layout.Length; row++)
            {
                for (int col = 0; col < layout[row].Length; col++)
                {
                    if (layout[row][col] == 1 || layout[row][col] == 2)
                    {
                        CollisionBlocks.Add(new CollisionBlock(col * TileSize, row * TileSize, TileSize, TileSize));
                    }
                }
            }
        }

        private async Task<int[][]> LoadLayoutAsync(string filePath)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(filePath);
                return await JsonSerializer.DeserializeAsync<int[][]>(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load layout {filePath}: {ex.Message}");
                return null;
            }
        }

        private async Task<SKBitmap> LoadBitmapAsync(string fileName)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                return SKBitmap.Decode(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load bitmap {fileName}: {ex.Message}");
                return null;
            }
        }

        private async Task<List<EnemyData>> LoadEnemyDataAsync(string filePath)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(filePath);
                return await JsonSerializer.DeserializeAsync<List<EnemyData>>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load enemy data file {filePath}: {ex.Message}");
                return null;
            }
        }

    }
}
