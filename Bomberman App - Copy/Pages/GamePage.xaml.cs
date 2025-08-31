using System.Diagnostics;
using Bomberman_App.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using SkiaSharp.Views.Maui;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace Bomberman_App.Pages;

[QueryProperty(nameof(Level), "Level")]
public partial class GamePage : ContentPage
{
    public string Level { get; set; }

    private Player _player;
    private LevelManager _levelManager;
    private Camera _camera;

    private List<Enemy> _enemies = new();
    private List<Gem> _gems = new();
    private List<SpriteEffect> _effects = new();

    private List<Heart> _hearts = new();
    private List<Dynamite> _dynamiteProjectiles = new();

#if WINDOWS
    private readonly HashSet<Windows.System.VirtualKey> _pressedKeys = new();
#endif

    private IDispatcherTimer _gameLoopTimer;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private long _lastFrameTime = 0;

    private int _gemCount = 0;
    private GemUI _gemUI;

    private bool _isPaused = false;

    public GamePage()
    {
        InitializeComponent();


        _camera = new Camera(1024, 576);

        this.Focused += (s, e) => Debug.WriteLine("GamePage GOT FOCUS");
        this.Unfocused += (s, e) => Debug.WriteLine("GamePage LOST FOCUS");

        // Force focus when loaded
        this.Loaded += (s, e) =>
        {
            this.Focus();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Focus();

        canvasView.Touch += OnCanvasTouch;

        await LoadContentAsync();

        _gameLoopTimer = Dispatcher.CreateTimer();
        _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        _gameLoopTimer.Tick += OnGameLoopTick;
        _stopwatch.Start();
        _gameLoopTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _gameLoopTimer?.Stop();
        _stopwatch.Stop();

        canvasView.Touch -= OnCanvasTouch;
    }

    private async Task LoadContentAsync()
    {
        await AssetManager.LoadAssetsAsync();
        _levelManager = new LevelManager();
        await _levelManager.LoadAndPreRenderLevelAsync($"Level"+Level);

        _dynamiteProjectiles.Clear();
        _enemies = _levelManager.Enemies;
        _gems = _levelManager.Gems;
        _effects.Clear();

        _gemCount = 0;
        _gemUI = new GemUI(13, 36);

        _hearts.Clear();
        _hearts.Add(new Heart(10, 10));
        _hearts.Add(new Heart(33, 10));
        _hearts.Add(new Heart(56, 10));

        if (_levelManager.PreRenderedBackground != null)
        {
            _camera.SetLevelBounds(
                _levelManager.LevelWidth,
                _levelManager.LevelHeight
            );
        }

        SKBitmap playerSpriteSheet = null;
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("player.png");
            if (stream != null)
            {
                playerSpriteSheet = SKBitmap.Decode(stream);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading player sprite: {ex.Message}");
            return;
        }

        //var playerSpriteSheet = AssetManager.GetBitmap("player.png");
        _player = new Player(100, 100, 32, playerSpriteSheet);
        _player.IsOnGround = false;

    }

    // --- Game Loop ---
    private void OnGameLoopTick(object sender, EventArgs e)
    {
        if (_isPaused) return;

        long currentFrameTime = _stopwatch.ElapsedMilliseconds;
        float deltaTime = (currentFrameTime - _lastFrameTime) / 1000.0f; 
        _lastFrameTime = currentFrameTime;

        if(_player != null && _levelManager != null)
        {
#if WINDOWS
            float moveInput = 0f;
        
            if (_pressedKeys.Contains(Windows.System.VirtualKey.Left))
                moveInput -= 1f;
            if (_pressedKeys.Contains(Windows.System.VirtualKey.Right))
                moveInput += 1f;
            
            _player.HandleHorizontalInput(moveInput);
        
            if (_pressedKeys.Contains(Windows.System.VirtualKey.Up))
            {
                _player.Jump();
            }
#endif

            if (_player.WantsToThrowDynamite)
            {
                const int dynamiteWidth = 16;
                float spawnX = (_player.Facing == "right")
                    ? _player.Hitbox.Right + 1
                    : _player.Hitbox.Left - dynamiteWidth - 1;
                float spawnY = _player.Y + (_player.Height / 4);

                _dynamiteProjectiles.Add(new Dynamite(spawnX, spawnY, _player.Facing));
                _player.WantsToThrowDynamite = false;
            }

            _player.Update(deltaTime, _levelManager.CollisionBlocks);
            _enemies.ForEach(e => e.Update(deltaTime, _levelManager.CollisionBlocks));
            _gems.ForEach(g => g.Update(deltaTime));
            _effects.ForEach(e => e.Update(deltaTime));

            _dynamiteProjectiles.ForEach(d => d.Update(deltaTime, _levelManager.CollisionBlocks));


            HandleCollisions();

            _effects.RemoveAll(e => e.IsFinished);
            _camera.Update(_player.X, _player.Y);
        }

        canvasView.InvalidateSurface();
    }

    private void OnCanvasTouch(object sender, SKTouchEventArgs e)
    {
        if (e.ActionType == SKTouchAction.Pressed)
        {
            Focus();
        }
    }

    // MAUI Keyboard Windows handling
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        Debug.WriteLine($"Handler changed - Platform: {DeviceInfo.Platform}");

#if WINDOWS
        if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Panel previousPanel)
        {
            previousPanel.KeyDown -= OnPlatformKeyDown;
            previousPanel.KeyUp -= OnPlatformKeyUp;
        }

        if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Panel newPanel)
        {
            newPanel.KeyDown += OnPlatformKeyDown;
            newPanel.KeyUp += OnPlatformKeyUp;

            newPanel.IsTabStop = true;
            newPanel.KeyDown += (s, e) => Debug.WriteLine($"Panel KeyDown: {e.Key}");
        }
#endif
    }

#if WINDOWS
    private void OnPlatformKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        Debug.WriteLine($"[KEYDOWN] {e.Key} (Handled: {e.Handled})");
        HandleKeyPress(e.Key, true);
        e.Handled = true;
    }

    private void OnPlatformKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        Debug.WriteLine($"[KEYUP] {e.Key}");
        HandleKeyPress(e.Key, false);
        e.Handled = true;
    }
#endif

#if WINDOWS
    private void HandleKeyPress(Windows.System.VirtualKey key, bool isPressed)
    {
        if (key == Windows.System.VirtualKey.Escape && isPressed)
        {
            TogglePause();
            return; 
        }

        if (key == Windows.System.VirtualKey.Space && isPressed)
        {
            _player.WantsToThrowDynamite = true;
        }
        
        if (_player == null || _isPaused) return;

        if (isPressed)
        {
            _pressedKeys.Add(key);
        }
        else
        {
            _pressedKeys.Remove(key);
        }
    }
#endif

    private void TogglePause()
    {
        _isPaused = !_isPaused;

        if (_isPaused)
        {
            _stopwatch.Stop();
            PauseMenuOverlay.IsVisible = true;
        }
        else
        {
            _stopwatch.Start();
            PauseMenuOverlay.IsVisible = false;
        }
    }

    private void HandleCollisions()
    {
        // --- Gem Collection ---
        for (int i = _gems.Count - 1; i >= 0; i--)
        {
            var gem = _gems[i];
            if (_player.Hitbox.IntersectsWith(gem.Hitbox))
            {
                _effects.Add(new SpriteEffect(gem.X, gem.Y, "item_feedback.png", SpriteEffect.GemCollection));
                _gems.RemoveAt(i);

                _gemCount++;
                if (_gems.Count == 0)
                {
                    TriggerYouWin();
                }
            }
        }

        // --- Enemy Collisions ---
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];

            if (_player.Hitbox.IntersectsWith(enemy.Hitbox))
            {
                if (_player.Velocity.Y > 0 && _player.Hitbox.Bottom <= enemy.Hitbox.Top + 10)
                {
                    _player.Bounce();

                    _effects.Add(new SpriteEffect(enemy.X, enemy.Y, "enemy_death.png", SpriteEffect.EnemyDeath));
                    _enemies.RemoveAt(i);
                }
                else
                {
                    PlayerTakesDamage();
                }
            }
        }

        for (int i = _dynamiteProjectiles.Count - 1; i >= 0; i--)
        {
            var dynamite = _dynamiteProjectiles[i];

            if (dynamite.State == DynamiteState.Flying)
            {
                // Check against player
                if (dynamite.Hitbox.IntersectsWith(_player.Hitbox))
                {
                    dynamite.State = DynamiteState.Exploded;
                }

                // Check against enemies
                foreach (var enemy in _enemies)
                {
                    if (dynamite.Hitbox.IntersectsWith(enemy.Hitbox))
                    {
                        dynamite.State = DynamiteState.Exploded;
                        break;
                    }
                }

                // Check against walls/ground
                foreach (var block in _levelManager.CollisionBlocks)
                {
                    if (dynamite.Hitbox.IntersectsWith(block.Hitbox))
                    {
                        dynamite.State = DynamiteState.Exploded;
                        break;
                    }
                }
            }

            // --- Handle Explosion ---
            if (dynamite.State == DynamiteState.Exploded)
            {
                _effects.Add(new SpriteEffect(dynamite.X, dynamite.Y, "explosion.png", SpriteEffect.Explosion));
                var explosionRadius = 64;
                var explosionHitbox = new SKRect(
                    dynamite.X - explosionRadius / 2,
                    dynamite.Y - explosionRadius / 2,
                    dynamite.X + explosionRadius / 2,
                    dynamite.Y + explosionRadius / 2);

                // Damage enemies
                for (int j = _enemies.Count - 1; j >= 0; j--)
                {
                    if (explosionHitbox.IntersectsWith(_enemies[j].Hitbox))
                    {
                        _effects.Add(new SpriteEffect(_enemies[j].X, _enemies[j].Y, "enemy_death.png", SpriteEffect.EnemyDeath));
                        _enemies.RemoveAt(j);
                    }
                }

                // Damage player
                if (explosionHitbox.IntersectsWith(_player.Hitbox))
                {
                    PlayerTakesDamage();
                }

                _dynamiteProjectiles.RemoveAt(i);
            }
        }
    }

    private void TriggerYouWin()
    {
        _gameLoopTimer?.Stop();
        YouWinOverlay.IsVisible = true;
    }

    private void PlayerTakesDamage()
    {
        if (_player.IsInvincible) return;

        var fullHeart = _hearts.LastOrDefault(h => !h.IsDepleted);
        if (fullHeart != null)
        {
            fullHeart.IsDepleted = true;
            // Make the player invincible for 1.5 seconds
            _player.TakeDamage(1.5f);

            if (!_hearts.Any(h => !h.IsDepleted))
            {
                TriggerGameOver();
            }
        }
    }

    private void TriggerGameOver()
    {
        _gameLoopTimer?.Stop();
        GameOverOverlay.IsVisible = true;
    }

    private async void OnReturnToMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    void OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        SKCanvas canvas = args.Surface.Canvas;
        canvas.Clear(SKColors.Black);

        canvas.Save();
        canvas.Scale(Camera.FixedZoom, Camera.FixedZoom);
        canvas.Translate(-_camera.X / Camera.FixedZoom, _camera.Y / Camera.FixedZoom);


        if (_levelManager?.PreRenderedBackground != null)
        {
            canvas.DrawBitmap(_levelManager.PreRenderedBackground, 0, 0);
        }

        _dynamiteProjectiles.ForEach(d => d.Draw(canvas));
        _gems.ForEach(g => g.Draw(canvas));
        _enemies.ForEach(e => e.Draw(canvas));
        _player?.Draw(canvas);
        _effects.ForEach(e => e.Draw(canvas));

        canvas.Restore();

        canvas.Save();

        const float uiScale = 2.0f;
        canvas.Scale(uiScale);

        _hearts.ForEach(h => h.Draw(canvas));
        _gemUI?.Draw(canvas, _gemCount);

        if (_player != null)
        {
            var paint = new SKPaint { Color = SKColors.White, TextSize = 20 };
            canvas.DrawText($"Position: {_player.X:0}, {_player.Y:0}", 20, 30, paint);
            canvas.DrawText($"Camera: {_camera.X/2.5f:0}, {_camera.Y/2.5f:0}", 20, 60, paint);
        }

        canvas.Restore();
    }
}
