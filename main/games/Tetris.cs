#pragma warning disable 8602, 8604, 8618

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static BrickGame.Constants;

namespace BrickGame.Games
{
    /// <summary>
    /// A Tetris game.
    /// </summary>
    /// <para>Read the <strong>Game Manuals</strong>.</para>
    class Tetris : Game
    {
        internal static Random random = new();
        static double startSpeed = 1.0;
        static char storedShape;
        /// <summary>
        /// Allows movement when a piece touches the floor.
        /// </summary>
        double floatSpeed;
        Piece piece;
        FallenBlocks fallen;

        /// <summary>
        /// Constructor for <see cref="Tetris"/>.
        /// </summary>
        public Tetris() : base()
        {
            // Set containers for the entities.
            SetEntities("piece", "fallen");
        }

        /// <summary>
        /// Defines game objects.
        /// </summary>
        internal override void Start()
        {
            base.Start();
            floatSpeed = startSpeed;
            Speed = (int)startSpeed;
            // Spawn the entities.
            piece = new Piece();  // Tetrominoes
            fallen = new FallenBlocks();  // Fallen remains
            piece.PrintPreview();
        }

        /// <summary>
        /// Deals with user input during the game.
        /// </summary>
        /// <param name="key">A <see cref="Keys"/> identifier for a key.</param>
        /// <param name="pressed">Whether <c>key</c> was pressed or released.</param>
        internal override void SetKeyBindings(Keys key, bool pressed)
        {
            base.SetKeyBindings(key, pressed);
            if (Running)
            {
                if (pressed)
                {  // Key pressed.
                    // Set Piece's movement.
                    if (key == Keys.Up)
                    {
                        piece.Rotate();
                    }
                    else if (key == Keys.Down)
                    {
                        piece.direction = Direction.Down;
                    }
                    else if (key == Keys.Left)
                    {
                        piece.direction = Direction.Left;
                    }
                    else if (key == Keys.Right)
                    {
                        piece.direction = Direction.Right;
                    }
                    else if (key == Keys.Space)
                    {
                        piece.Drop();
                        SpawnNext();
                    }
                    else if (key == Keys.ShiftKey)
                    {
                        if (!piece.switchLocked)
                        {
                            piece.SwitchShapes();  // Only once for every new piece.
                        }
                    }
                }
                else
                {  // Key released.
                    if (key == Keys.Down)
                    {
                        piece.direction = Direction.Null;
                    }
                    else if (key == Keys.Left)
                    {
                        piece.direction = Direction.Null;
                    }
                    else if (key == Keys.Right)
                    {
                        piece.direction = Direction.Null;
                    }
                }
            }
        }

        /// <summary>
        /// Game logic implementation.
        /// </summary>
        /// <param name="t">A timer.</param>
        internal override void Manage(int t)
        {
            if (Running && !Paused)
            {
                // Set the action rate at speed Blocks per second.
                if (t % (FPS/Speed) == 0)
                {
                    piece.Move(Direction.Down);  // Slow fall
                    TrySpawnNext();
                }
                // speed scales over time, every 30 seconds.
                if (Speed <= 10)
                {
                    if (t % (30*FPS) == 0)
                    {
                        floatSpeed *= Math.Pow(10.0, 0.05);
                        if ((int)floatSpeed > Speed)
                        {
                            Speed = (int)floatSpeed;
                        }
                    }
                    
                }
                // Adjust for movement proportional to the scaling speed.
                if (t % (FPS/(7 + 3*Speed)) == 0)
                {
                    // Horizontal movement and downwards acceleration.
                    piece.Move();
                }
            }
            // Manage endgame.
            base.Manage(t);
        }

        /// <summary>
        /// Scoring mechanics.
        /// </summary>
        /// <param name="fullLines"></param>
        void UpdateScore(int fullLines)
        {
            // More points for more lines at once.
            switch (fullLines)
            {
                case 1:
                    Score += (2 + Speed*fallen.height)*15;
                    break;
                case 2:
                    Score += (6 + Speed*fallen.height)*15;
                    break;
                case 3:
                    Score += (12 + Speed*fallen.height)*15;
                    break;
                case 4:
                    Score += (20 + Speed*fallen.height)*15;
                    break;
            }
            base.UpdateScore();
        }

        /// <summary>
        /// pieces shall spawn once they stop falling.
        /// </summary>
        void TrySpawnNext()
        {
            if (piece.height == 0)
            {
                SpawnNext();
            }
        }

        void SpawnNext()
        {
            // Transfer the piece's Blocks to the fallen structure.
            fallen.Grow();

            // Account for a proper score according to the lines cleared.
            int fullLines = fallen.RemoveFullLines();
            UpdateScore(fullLines);

            // Spawn a new Piece object.
            piece = new Piece();
            piece.PrintPreview();
        }

        /// <summary>
        /// The game is endless, except for defeat.
        /// </summary>
        /// <returns>Whether the game has been beaten.</returns>
        protected override bool CheckVictory()
        {
            return false;
        }

        /// <summary>
        /// Defeat condition.
        /// </summary>
        /// <para> Occurs happens if the fallen structure reaches the top of the grid.</para>
        /// <returns>Whether the game was lost.</returns>
        protected override bool CheckDefeat()
        {
            return fallen.height > 20;
        }

        /// <summary>
        /// Organizes the four-tiled-piece's mechanics.
        /// </summary>
        private class Piece
        {
            internal int height;
            internal Direction direction = Direction.Null;
            internal Point Coordinates;
            internal bool switchLocked;
            internal char activeShape;
            int nextID;
            char[] shapes = {'T', 'J', 'L', 'S', 'Z', 'I', 'O'};
            Dictionary<int, (int, List<Block>)> rotationMap;
            
            /** Manages spawn and preview. */
            internal Piece()
            {
                height = 19;
                rotationMap = new();
                int randomIndex;
                // If storedShape has been initialized, activeShape shall acquire its value.
                if (storedShape != '\0')
                {
                    activeShape = storedShape;
                }
                else
                {
                    // Otherwise, the value of activeShape shall be chosen randomly.
                    randomIndex = random.Next(7);
                    activeShape = shapes[randomIndex];
                }
                // Store a new shape.
                randomIndex = random.Next(7);
                storedShape = shapes[randomIndex];
                // Spawn a Piece object with the activeShape.
                Spawn();
                // Permit one switch between activeShape and storedShape.
                switchLocked = false;
            }

            /// <summary>
            /// Draws the desired piece on the screen, at top center.
            /// </summary>
            void Spawn() {
                Place(activeShape, 4, 0);
                // Identify the next rotated position.
                nextID = 1;
                // Draw it.
                entities["piece"] = rotationMap[1].Item2;
                foreach (Block block in entities["piece"])
                {
                    block.Show();
                }
            }

            /** Showcases <see cref="storedShape"/>. */
            internal void PrintPreview()
            {
                string drawing;
                switch (storedShape)
                {
                    case 'T':
                        drawing = ""
                        + "============\n"
                        + "Next:\n"
                        + "    _\n"
                        + " _ |_| _\n"
                        + "|_||_||_|";
                        Console.WriteLine(drawing); 
                        break;
                    case 'J':
                        drawing = ""
                        + "============\n"
                        + "Next:\n"
                        + " _\n"
                        + "|_| _  _\n"
                        + "|_||_||_|";
                        Console.WriteLine(drawing); 
                        break;
                    case 'L':
                        drawing = ""
                        + "============\n"
                        + "Next:\n"
                        + "       _\n"
                        + " _  _ |_|\n"
                        + "|_||_||_|";
                        Console.WriteLine(drawing); 
                        break;
                    case 'S':
                        drawing = ""
                        + "============\n"
                        + "Next:\n"
                        + "    _  _\n"
                        + " _ |_||_|\n"
                        + "|_||_|";
                        Console.WriteLine(drawing); 
                        break;
                    case 'Z':
                        drawing = ""
                        + "============\n"
                        + "Next:\n"
                        + " _  _\n"
                        + "|_||_| _\n"
                        + "   |_||_|";
                        Console.WriteLine(drawing); 
                        break;
                    case 'I':
                        drawing = ""
                        + "============\n"
                        + "Next:\n"
                        + " _  _  _  _\n"
                        + "|_||_||_||_|";
                        Console.WriteLine(drawing); 
                        break;
                    case 'O':
                        drawing = ""
                        + "============\n"
                        + "Next:\n"
                        + " _  _\n"
                        + "|_||_|\n"
                        + "|_||_|";
                        Console.WriteLine(drawing); 
                        break;
                }
            }

            /// <summary>
            /// Mechanics for switching <see cref="piece"/>s.
            /// </summary>
            /// <para>Change a <c>piece</c> with <see cref="activeShape"/> to one
            /// with <see cref="storedShape"/> (only once for each new spawn).</para>
            internal void SwitchShapes()
            {
                // Clear the current piece's drawing and references.
                foreach (Block block in entities["piece"])
                {
                    block.Hide();
                }
                entities["piece"].Clear();

                // Switch shapes and reset the height.
                char s = storedShape;
                char a = activeShape;
                storedShape = a;
                activeShape = s;
                height = 19;

                Spawn();
                PrintPreview();
                switchLocked = true;  // Only once for every new piece.
            }

            /// <summary>
            /// Positions of <see cref="Block"/>s to form each shape and track rotation.
            /// </summary>
            /// <param name="shape">The desired shape.</param>
            /// <param name="i">Horizontal coordinate reference.</param>
            /// <param name="j">Vertical coordinate reference.</param>
            void Place(char shape, int i, int j)
            {
                Coordinates = new Point(i, j);
                switch (shape) {
                    case 'T':
                        rotationMap = new()
                        {
                            {1, (2, new(){new(i-1, j), new(i, j), new(i+1, j), new(i, j-1)})},
                            {2, (3, new(){new(i-1, j), new(i, j), new(i, j-1), new(i, j+1)})},
                            {3, (4, new(){new(i-1, j), new(i, j), new(i+1, j), new(i, j+1)})},
                            {4, (1, new(){new(i, j-1), new(i, j), new(i+1, j), new(i, j+1)})}
                        };
                        break;
                    case 'J':
                        rotationMap = new()
                        {
                            {1, (2, new(){new(i-1, j-1), new(i-1, j), new(i, j), new(i+1, j)})},
                            {2, (3, new(){new(i, j-1), new(i, j), new(i, j+1), new(i-1, j+1)})},
                            {3, (4, new(){new(i-1, j-1), new(i, j-1), new(i+1, j-1), new(i+1, j)})},
                            {4, (1, new(){new(i-1, j-1), new(i, j-1), new(i-1, j), new(i-1, j+1)})}
                        };
                        break;
                    case 'L':
                        rotationMap = new()
                        {
                            {1, (2, new(){new(i-1, j), new(i, j), new(i+1, j), new(i+1, j-1)})},
                            {2, (3, new(){new(i-1, j-1), new(i, j-1), new(i, j), new(i, j+1)})},
                            {3, (4, new(){new(i-1, j), new(i-1, j-1), new(i, j-1), new(i+1, j-1)})},
                            {4, (1, new(){new(i-1, j-1), new(i-1, j), new(i-1, j+1), new(i, j+1)})}
                        };
                        break;
                    case 'S':
                        rotationMap = new()
                        {
                            {1, (2, new(){new(i-1, j), new(i, j), new(i, j-1), new(i+1, j-1)})},
                            {2, (1, new(){new(i, j+1), new(i, j), new(i-1, j), new(i-1, j-1)})}
                        };
                        break;
                    case 'Z':
                        rotationMap = new()
                        {
                            {1, (2, new(){new(i-1, j-1), new(i, j-1), new(i, j), new(i+1, j)})},
                            {2, (1, new(){new(i, j - 1), new(i, j), new(i-1, j), new(i-1, j+1)})}
                        };
                        break;
                    case 'I':
                        rotationMap = new()
                        {
                            {1, (2, new(){new(i-1, j), new(i, j), new(i+1, j), new(i+2, j)})},
                            {2, (1, new(){new(i, j-1), new(i, j), new(i, j+1), new(i, j+2)})}
                        };
                        break;
                    case 'O':
                        rotationMap = new()
                        {
                            {1, (1, new(){new(i, j), new(i, j+1), new(i+1, j), new(i+1, j+1)})}
                        };
                        break;
                    default:
                        rotationMap = new(){{1, (1, new List<Block>(){})}};
                        break;
                }
                foreach (KeyValuePair<int, (int, List<Block>)> entry in rotationMap)
                {
                    foreach (Block block in entry.Value.Item2)
                    {
                        block.Hide();
                    }
                }
            }

            /// <summary>
            /// Tracks the boundary dimensions of each <see cref="Piece"/> and updates its <see cref="height"/>.
            /// </summary>
            /// <returns>Horizontal and vertical limits.</returns>
            (int, int, int) CalculateDimensions()
            {
                int iMin, iMax, jMax;
                List<int> iList = new(4);
                List<int> jList = new(4);
                foreach (Block block in entities["piece"])
                {
                    iList.Add(block.Coordinates.X);
                    jList.Add(block.Coordinates.Y);
                }
                iMin = iList.Min();
                iMax = iList.Max();
                jMax = jList.Max();
                
                // Height calculation.
                List<int> heights = new(4);
                List<int> vPiece;
                List<int> vFall;
                for (int i = iMin; i <= iMax; i++)
                {
                    // For each horizontal coordinate,...
                    vPiece = new(){0};
                    vFall = new();
                    foreach (Block block in entities["piece"])
                    {
                        if (block.Coordinates.X == i)
                        {
                            vPiece.Add(block.Coordinates.Y);
                        }
                    }
                    // ... the piece's lowest vertical coordinate is compared to...
                    int j = vPiece.Max();
                    // ... the fallen structure's highest vertical coordinate.
                    foreach (Block block in entities["fallen"])
                    {
                        if (block.Coordinates.X == i && block.Coordinates.Y > j)
                        {
                            vFall.Add(block.Coordinates.Y);
                        }
                    }
                    // If there's anything below the piece in this column, ...
                    if (vFall.Any())
                    {
                        // the distance is added to the list of heights.
                        heights.Add(vFall.Min()-j-1);
                    }
                    else
                    {
                        // Otherwise, the distance to the bottom of the grid is added to the list instead. */
                        heights.Add(19-j);
                    }
                }
                // The height is the shortest distance from a piece to the fallen structure.
                height = heights.Min();

                return (iMin, iMax, jMax);
            }

            /// <summary>
            /// Checks if there are obstacles for movement, moves if not.
            /// </summary>
            /// <param name="direction">One of <c>Direction.Down</c>, <c>Direction.Left</c>,
            /// <c>Direction.Right</c>.</param>
            internal void Move(Direction direction)
            {
                // Gatter all the necessary dimensions to detect whether movement is possible.
                Point P = Coordinates;
                Point D = ConvertDirection[direction];
                (int iMin, int iMax, int jMax) = CalculateDimensions();
                // First, a list with the desired new positions for each Block in the piece.
                List<Point> X = new();
                foreach (Block block in entities["piece"])
                {
                    Point B = block.Coordinates;
                    X.Add(new Point(B.X + D.X, B.Y + D.Y));
                }
                // Second, another list with the current positions of the already formed structure.
                var Y = from Block block in entities["fallen"]
                        select block.Coordinates;
                // Movement can happen if these sets don't intersect.
                if (!X.Intersect(Y).Any())
                {
                    // Check also if the movement won't get any Block outside the grid.
                    if (0 <= iMin + D.X && iMax + D.X < 10 && jMax + D.Y < 20)
                    {
                        // Update rotationMap.
                        Place(activeShape, P.X + D.X, P.Y + D.Y);
                        // Make the movement.
                        foreach (Block block in entities["piece"])
                        {
                            Point B = block.Coordinates;
                            block.MoveTo(B.X + D.X, B.Y + D.Y);
                        }
                    }
                }
            }

            /// <summary>
            /// Checks if there are obstacles for movement, moves if not.
            /// </summary>
            internal void Move()
            {
                Move(direction);
            }

            /// <summary>
            /// Checks if there are obstacles for rotation, rotates if not.
            /// </summary>
            internal void Rotate()
            {
                // First list: desired new positions of the piece's Blocks if a rotation were to happen.
                List<Point> X = new();
                int nextIDCandidate = rotationMap[nextID].Item1;
                foreach (Block block in rotationMap[nextIDCandidate].Item2)
                {
                    X.Add(block.Coordinates);
                }
                // Second list: current positions of the Blocks in the fallen structure.
                var Y = from Block block in entities["fallen"]
                        select block.Coordinates;
                // Third list: Block positions outside the borders after the rotation.
                var Z = from Point p in X
                        where p.X < 0 || p.X >= 10 || p.Y >= 20
                        select p;

                // If the rotated piece doesn't collide with the structure and remains inside the grid,
                // then movement occurs.
                if (!X.Intersect(Y).Any() && !Z.Any())
                {
                    // Erase the current piece from the screen.
                    entities["piece"].Clear();
                    // Replace it with another with the next rotated state.
                    Place(activeShape, Coordinates.X, Coordinates.Y);
                    // Update rotationMap and the rotating id, and draw the piece's Blocks.
                    nextID = rotationMap[nextID].Item1;
                    foreach (Block block in rotationMap[nextID].Item2)
                    {
                        block.Show();
                        entities["piece"].Add(block);
                    }
                }
            }

            /// <summary>
            /// Hard drop. Moves down a piece by its full height.
            /// </summary>
            internal void Drop()
            {
                // Update the height.
                CalculateDimensions();
                // Drop.
                Place(activeShape, Coordinates.X, Coordinates.Y + height);
                // Make the movement.
                foreach (Block block in entities["piece"])
                {
                    Point B = block.Coordinates;
                    block.MoveTo(B.X, B.Y + height);
                }
                // Check if the piece actually hit the bottom.
                CalculateDimensions();
                if (height > 0)
                {
                    foreach (Block block in entities["piece"])
                    {
                        Point B = block.Coordinates;
                        block.MoveTo(B.X, B.Y + height);
                    }
                }
            }
        }

        /// <summary>
        /// The structure formed by the fallen <see cref="piece"/>'s <see cref="Block"/>s.
        /// </summary>
        class FallenBlocks
        {
            internal int height;  // Not the same as piece.height.
            
            internal FallenBlocks()
            {
                height = 0;
            }

            /// <summary>
            /// Makes <see cref="Piece"/> part of the structure.
            /// </summary>
            internal void Grow()
            {
                // Transfer the Blocks from the "piece" group to the "fallen" group.
                entities["fallen"].AddRange(entities["piece"]);
                entities["piece"].Clear();
                // Update the structure's height.
                List<int> vFallen = new(){20};
                foreach (Block block in entities["fallen"])
                {
                    vFallen.Add(block.Coordinates.Y);
                }
                height = 20 - vFallen.Min();
            }

            /// <returns>Number of lines removed.</returns>
            internal int RemoveFullLines()
            {
                Dictionary<int, List<Block>> fullLines = new();
                // Group all the completed lines (with 10 aligned Blocks).
                for (int b = 0; b < 20; b++)
                {
                    List<Block> line = new();
                    foreach (Block block in entities["fallen"])
                    {
                        if (block.Coordinates.Y == b)
                        {
                            line.Add(block);
                        }
                    }
                    if (line.Count == 10)
                    {
                        fullLines[b] = line;
                    }
                    Console.WriteLine(line.Count);
                }
                // Remove them from the structure and lower the Blocks above it.
                foreach (KeyValuePair<int, List<Block>> entry in fullLines)
                {
                    int b = entry.Key;
                    List<Block> line = entry.Value;
                    foreach (Block block in line)
                    {
                        block.Hide();
                    }
                    entities["fallen"].RemoveAll(line.Contains);
                    foreach (Block block in entities["fallen"])
                    {
                        if (block.Coordinates.Y < b)
                        {
                            block.MoveTo(block.Coordinates.X, block.Coordinates.Y + 1);
                        }
                    }
                }

                return fullLines.Count;
            }
        }
    }
}
