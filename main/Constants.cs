using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

namespace BrickGame
{
    /// <summary>
    /// Contants used throughout the package.
    /// </summary>
    class Constants
    {
        // Colors.
        internal static Color BackgroundColor { get; } = Color.FromArgb(109, 120, 92);  // Light green
        internal static Color ShadeColor { get; } = Color.FromArgb(97, 112, 91);  // Dark green
        internal static Color LineColor { get; } = Color.FromArgb(0, 0, 0);  // Black
        
        // Graphic dimensions.
        internal const int PixelSize = 3;
        internal const int BorderSize = PixelSize;
        internal const int BlockSize = 10*PixelSize;
        internal const int DistanceBlocks = PixelSize + BlockSize;
        internal const int WindowWidth = 2*BorderSize + 10*DistanceBlocks - PixelSize;
        internal const int WindowHeight = 2*BorderSize + 20*DistanceBlocks - PixelSize;
        
        // Handling time.
        internal const int TickRate = 23;  // Minimum time interval, in milliseconds.
        internal const int FPS = 1000/TickRate;  // 50 when TickRate is 20.

        // Handling movement.
        internal enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            Null
        }
        internal static readonly Dictionary<Direction, Point> ConvertDirection = new()
        {
            {Direction.Up, new Point(0, -1)},
            {Direction.Down, new Point(0, 1)},
            {Direction.Left, new Point(-1, 0)},
            {Direction.Right, new Point(1, 0)},
            {Direction.Null, new Point(0, 0)}
        };

        // Tracking directories.
        static readonly string AssemblyDir = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string RootDir = AssemblyDir[..AssemblyDir.IndexOf(@"\bin")];
        internal static readonly string GameManualsDir = Path.Combine(RootDir, "docs", "GameManuals.md");
        internal static readonly string HighScoresDir = Path.Combine(RootDir, "HighScores.json");
        internal static readonly string ScreensDir = Path.Combine(RootDir, "screens");
    }

    class Scores
    {
        public int Snake { get; set; }
        public int Breakout { get; set; }
        public int Asteroids { get; set; }
        public int Tetris { get; set; }
    }
}
