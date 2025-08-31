using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using System.Drawing;

namespace Bomberman_App.Models
{
    public class Camera
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public const float FixedZoom = 2.5f;

        // Thresholds before the camera starts moving
        private const float ScrollRightThreshold = 300f;
        private const float ScrollLeftThreshold = 200f; 
        private const float ScrollTopThreshold = 120f;  
        private const float ScrollBottomThreshold = 100f; 

        private readonly float _viewportWidth;
        private readonly float _viewportHeight;

        private float _levelWidth;
        private float _levelHeight;

        private float _verticalOffset = 20f;

        public Camera(float viewportWidth, float viewportHeight)
        {
            _viewportWidth = viewportWidth / FixedZoom;
            _viewportHeight = viewportHeight / FixedZoom;
        }

        public void SetLevelBounds(float width, float height)
        {
            _levelWidth = width;
            _levelHeight = height;

            Debug.Print("Map Width: " + _levelWidth);
        }

        public void Update(float playerX, float playerY)
        {
            // If player is off-screen to the left, don't update camera
            if (playerX < 2f)
            {
                playerX = 2f;
            }

            float effectiveRightThreshold = ScrollRightThreshold / FixedZoom;
            float effectiveLeftThreshold = ScrollLeftThreshold / FixedZoom;
            float effectiveTopThreshold = ScrollTopThreshold / FixedZoom;
            float effectiveBottomThreshold = ScrollBottomThreshold / FixedZoom;

            float maxX = Math.Max(0, _levelWidth - _viewportWidth);
            float maxY = Math.Max(0, _levelHeight - _viewportHeight);

            float currentX = X / FixedZoom;
            float currentY = -Y / FixedZoom; 

            float rightBoundaryX = currentX + _viewportWidth - effectiveRightThreshold;
            float newX = currentX;

            if (playerX > rightBoundaryX)
            {
                newX = playerX - _viewportWidth + effectiveRightThreshold;
            }
            else if (playerX < currentX + effectiveLeftThreshold)
            {
                if (playerX >= 0)
                {
                    newX = playerX - effectiveLeftThreshold;
                }
            }

            newX = Math.Clamp(newX, 0, maxX);

            // --- Vertical Movement ---
            float newY = currentY;

            float topBoundaryY = currentY + effectiveTopThreshold;
            float bottomBoundaryY = currentY + _viewportHeight - effectiveBottomThreshold - _verticalOffset;

            if (playerY < topBoundaryY)
            {
                newY = playerY - effectiveTopThreshold;
            }
            else if (playerY > bottomBoundaryY)
            {
                newY = playerY - _viewportHeight + effectiveBottomThreshold + _verticalOffset;
            }

            newY = Math.Clamp(newY, 0, maxY);

            X = newX * FixedZoom;
            Y = -newY * FixedZoom;
        }
    }
}

