#pragma warning disable 8618
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using static BrickGame.Constants;

namespace BrickGame.Games
{
    /// <summary>
    /// A Breakout game.
    /// </summary>
    /// <para>Read the <strong>Game Manuals</strong>.</para>
    class Breakout : Game
    {
        internal static int total = 0;
        /// <summary>
        /// Used to count the target <see cref="Block"/>s at the start of a loop.
        /// </summary>
        internal static int number = 0;
        internal static int startSpeed = 15;
        internal static int[] speedValues = new int[2];
        int level;
        Target target;
        Ball ball;
        Paddle paddle;

        /// <summary>
        /// Constructor for <see cref="Breakout"/>.
        /// </summary>
        internal Breakout() : base()
        {
            // Set containers for the entities.
            SetEntities("target", "ball", "paddle");
        }

        /// <summary>
        /// Defines game objects.
        /// </summary>
        internal override void Start()
        {
            base.Start();
            speedValues = new int[]
            {
                startSpeed,
                2*startSpeed
            };
            level = 1;
            Speed = startSpeed;
            // Spawn the entities.
            target = new Target(level);
            ball = new Ball(4, 18);
            paddle = new Paddle(ball);
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
                if (pressed)  // Key pressed.
                {   // Set the Paddle's movement.
                    if (key == Keys.Space)
                    {
                        Speed = speedValues[1];
                    }
                    else if (key == Keys.Left)
                    {
                        paddle.direction = Direction.Left;
                    }
                    else if (key == Keys.Right)
                    {
                        paddle.direction = Direction.Right;
                    }
                }
                else  // Key released.
                {
                    if (key == Keys.Left)
                    {
                        paddle.direction = Direction.Null;
                    }
                    else if (key == Keys.Right)
                    {
                        paddle.direction = Direction.Null;
                    }
                    else if (key == Keys.Space)
                    {
                        Speed = speedValues[0];
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
            {   // Dealing with speed values greater than FPS.
                for (int i = 0; i <= Speed/FPS; i++)
                {
                    // Setting the action rate at speed Blocks per second.
                    int q = (Speed % FPS > 0) ? FPS/(Speed % FPS) : 1;
                    if (t % q == 0)
                    {
                        // Move the ball.
                        ball.Move();

                        // Check if the ball hit the target.
                        target.CheckHit(ball);
                        UpdateScore();

                        // Check if there still are Blocks left in target,
                        // calling the next stage if not.
                        ManageLevels();

                        ball.CheckBorderReflect();

                        // Deal with immediate collision after hitting a border.
                        target.CheckHit(ball);
                        UpdateScore();
                        ManageLevels();

                        // paddle dragging ball upon contact.
                        paddle.CheckPaddleDrag();
                        paddle.CheckPaddleReflect();

                        // Avoid the ball from leaving the screen by the lateral borders.
                        ball.CheckBorderReflect();

                        // Making the paddle move.
                        paddle.Move(Speed);
                    }
                }
            }
            // Manage endgame.
            base.Manage(t);
        }

        /// <summary>
        /// Scoring mechanics.
        /// </summary>
        internal override void UpdateScore()
        {
            // target Blocks left. */
            int n = entities["target"].Count;
            for (int i = number; i > n; i--)
            {
                switch (level)
                {
                    case 1:
                        Score += 15;
                        break;
                    case 2:
                        Score += 20;
                        break;
                    case 3:
                        Score += 30;
                        break;
                }
            }
            number = n;  // Update the number of target Blocks.
            base.UpdateScore();
        }

        /// <summary>
        /// Turns to the next stage upon clearing the current one.
        /// </summary>
        void ManageLevels()
        {
            if (level <= 3 && !entities["target"].Any())
            {
                // Toggle the next stage.
                Console.WriteLine($"Stage {level} cleared");
                level += 1;
                // Add a bonus score from phase completion.
                Score += 3000 + 3000*(level - 1);
                // Construct the next target.
                target = new Target(level);
                // DeletE and respawn the ball.
                ball.Hide();
                entities["ball"].Clear();
                ball = new Ball(4, 18);
                // Respawn the paddle.
                foreach (Block block in entities["paddle"]) {
                    block.Hide();
                }
                entities["paddle"].Clear();
                paddle = new Paddle(ball);
            }
        }

        /// <summary>
        /// Victory occurs when all 3 stages are cleared.
        /// </summary>
        /// <returns>Whether the game was beaten.</returns>
        protected override bool CheckVictory()
        {
            return level == 4;
        }

        /// <summary>
        /// Defeat happens if the {@link #ball} falls past the bottom border.
        /// </summary>
        /// <returns>Whether the game was lost.</returns>
        protected override bool CheckDefeat()
        {
            return ball.Coordinates.Y > 19;
        }

        /// <summary>
        /// Manages the <see cref="Block"/>s to be destroyed.
        /// </summary>
        /// Organizes the drawing and breaking of the target <c>Block</c>s at the top of the grid.
        class Target
        {
            /// <summary>
            /// Map coordinates to <see cref="Block"/>.
            /// </summary>
            Dictionary<Point, Block> dictBlock = new();

            /// <summary>
            /// Builds the <see cref="Target"/>'s structure.
            /// </summary>
            /// <param name="level">The current stage.</param>
            internal Target(int level) {
                // Cleaning the target's drawing and references.
                entities["target"].Clear();
                Dictionary<int, int[]> sketch;
                switch (level)
                {
                    case 1:
                        sketch = new()//10)
                        {
                            {0, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {1, new int[] {0,                         9}},
                            {2, new int[] {0,                         9}},
                            {3, new int[] {0,       3, 4, 5, 6,       9}},
                            {4, new int[] {0,       3, 4, 5, 6,       9}},
                            {5, new int[] {0,       3, 4, 5, 6,       9}},
                            {6, new int[] {0,       3, 4, 5, 6,       9}},
                            {7, new int[] {0,                         9}},
                            {8, new int[] {0,                         9}},
                            {9, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}}
                        };

                        dictBlock = new();
                        foreach (KeyValuePair<int, int[]> entry in sketch)
                        {
                            int j = entry.Key;
                            foreach (int i in entry.Value)
                            {
                                Block block = new Block(i, j);
                                dictBlock[block.Coordinates] = block;
                                entities["target"].Add(block);
                            }
                        }
                        break;
                    case 2:
                        sketch = new(6)
                        {
                            {0, new int[] {0, 1,                   8, 9}},
                            {1, new int[] {0, 1, 2,             7, 8, 9}},
                            {2, new int[] {   1, 2, 3,       6, 7, 8   }},
                            {3, new int[] {      2, 3, 4, 5, 6, 7      }},
                            {4, new int[] {   1, 2, 3,       6, 7, 8   }},
                            {5, new int[] {0, 1, 2,             7, 8, 9}},
                            {6, new int[] {0, 1,                   8, 9}}
                        };

                        dictBlock = new();
                        foreach (KeyValuePair<int, int[]> entry in sketch)
                        {
                            int j = entry.Key;
                            foreach (int i in entry.Value)
                            {
                                Block block = new Block(i, j);
                                dictBlock[block.Coordinates] = block;
                                entities["target"].Add(block);
                            }
                        }
                        break;
                    case 3:
                        sketch = new(6)
                        {
                            {0, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {1, new int[] {0,          4, 5,          9}},
                            {2, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {3, new int[] {0,          4, 5,          9}},
                            {4, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {5, new int[] {0,          4, 5,          9}},
                            {6, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}}
                        };

                        dictBlock = new();
                        foreach (KeyValuePair<int, int[]> entry in sketch)
                        {
                            int j = entry.Key;
                            foreach (int i in entry.Value)
                            {
                                Block block = new Block(i, j);
                                dictBlock[block.Coordinates] = block;
                                entities["target"].Add(block);
                            }
                        }
                        break;
                    
                    /* Template for more stages:

                    case X:
                        sketch = new(10)
                        {
                            {0, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {1, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {2, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {3, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {4, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {5, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {6, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {7, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {8, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
                            {9, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}}
                        };
                        dictBlock = new();
                        foreach (KeyValuePair<int, int[]> entry in sketch)
                        {
                            int j = entry.Key;
                            foreach (int i in entry.Value)
                            {
                                Block block = new Block(i, j);
                                dictBlock[block.Coordinates] = block;
                                entities["target"].Add(block);
                            }
                        }
                        break;
                    */
                }

                total = dictBlock.Count;
                number = total;
            }

            /// <summary>
            /// Collisions of <c>ball</c> and <c>target</c>.
            /// </summary>
            /// <param name="ball">The active <c>Ball()</c> instance.</param>
            internal void CheckHit(Ball ball)
            {
                Point B = ball.Coordinates;
                Point V = ball.Velocity;
                Point Px = new(B.X + V.X, B.Y);
                Point Py = new(B.X, B.Y + V.Y);
                Point Pxy = new(B.X + V.X, B.Y + V.Y);
                // When the ball hits a corner between two target Blocks...
                if (dictBlock.ContainsKey(Px) && dictBlock.ContainsKey(Py))
                {   // ... reverse both directions,...
                    ball.Velocity = new Point(-V.X, -V.Y);
                    // ... and destroy both Blocks that make the corner,...
                    dictBlock[Px].Hide();
                    entities["target"].Remove(dictBlock[Px]);
                    dictBlock.Remove(Px);
                    dictBlock[Py].Hide();
                    entities["target"].Remove(dictBlock[Py]);
                    dictBlock.Remove(Py);
                    // ... including the vertex between the targets, if it exists.
                    if (dictBlock.ContainsKey(Pxy))
                    {
                        dictBlock[Pxy].Hide();
                        entities["target"].Remove(dictBlock[Pxy]);
                        dictBlock.Remove(Pxy);
                    }
                }
                // When the ball hits the target Blocks horizontally only...
                else if (dictBlock.ContainsKey(Px) && !dictBlock.ContainsKey(Py))
                {   // ... reverse only its first coordinate, ...
                    ball.Velocity.X = -V.X;
                    // ... and destroy only the Block it hit.
                    dictBlock[Px].Hide();
                    entities["target"].Remove(dictBlock[Px]);
                    dictBlock.Remove(Px);
                }
                // When the ball hits the target Blocks vertically only...
                else if (!dictBlock.ContainsKey(Px) && dictBlock.ContainsKey(Py))
                {   // ... reverse only its second coordinate, ...
                    ball.Velocity.Y = -V.Y;
                    // ... and destroy only the Block it hit.
                    dictBlock[Py].Hide();
                    entities["target"].Remove(dictBlock[Py]);
                    dictBlock.Remove(Py);
                }
                // When the ball hits the target Blocks at exactly a vertex...
                else if (dictBlock.ContainsKey(Pxy))
                {   // ... reverse both directions, ...
                    ball.Velocity = new Point(-V.X, -V.Y);
                    // ... and destroy the Block at said vertex.
                    dictBlock[Pxy].Hide();
                    entities["target"].Remove(dictBlock[Pxy]);
                    dictBlock.Remove(Pxy);
                }
            }
        }

        /// <summary>
        /// The player-controlled paddle.
        /// </summary>
        /// <para>Organizes the drawing, movement, dragging, and reflection off of the paddle.</para>
        class Paddle
        {
            internal Direction direction = Direction.Null;
            bool dragging;
            Ball ball;
            List<Block> blocks;
            List<Point> Coordinates;
            int size;
            
            /// <summary>
            /// Builds the <see cref="Paddle"/>.
            /// </summary>
            /// <para>It must drag the ball at the start of each phase.</para>
            /// <param name="ball">The <c>Ball()</c> instance.</param>
            internal Paddle(Ball ball)
            {
                this.ball = ball;
                
                // Set the paddle's initial position.
                blocks = new()
                {
                    new Block(3, 19),
                    new Block(4, 19),
                    new Block(5, 19)
                };
                size = blocks.Count;
                
                // Add the ball to the paddle initially to allow for a launching choice.
                blocks.Add(ball);
                
                // Track coordinates and add to the corresponding group for drawing.
                Coordinates = new();
                foreach (Block block in blocks)
                {
                    Coordinates.Add(block.Coordinates);
                    entities["paddle"].Add(block);
                }
            }

            /// <summary>
            /// Mechanics for the <see cref="Paddle"/>'s movement and the <see cref="Ball"/>'s launch.
            /// </summary>
            /// <param name="speed">The <c>ball</c> speed in <see cref="Block"/>s per second.</param>
            internal void Move(int speed)
            {
                int x = ConvertDirection[direction].X;
                if (direction != Direction.Null)
                {
                    foreach (Block block in blocks)
                    {
                        Point B = block.Coordinates;
                        // Ensure the paddle will remain within the screen.
                        if (0 <= Coordinates[0].X + x && Coordinates[0].X + x <= 10 - size)
                        {
                            block.MoveTo(B.X + x, B.Y);
                        }
                    }
                    // Update the paddle's coordinates.
                    Coordinates = new();
                    foreach (Block block in blocks)
                    {
                        Coordinates.Add(block.Coordinates);
                    }
                }

                // Launch mechanics at the start of the stage (the ball is
                // released from the paddle if *Space* is pressed).
                if (speed > startSpeed)
                {
                    // Stage start conditions.
                    if (Coordinates.Count > size && number == total)
                    {
                        entities["paddle"].Remove(ball);
                        blocks.RemoveAt(size);
                        Coordinates.RemoveAt(size);
                        // Update ball data.
                        ball.isMoving = true;
                        ball.Velocity = new Point(1, -1);  // First direction.
                    }
                }
            }

            /// <summary>
            /// Manages <see cref="ball"/> drag and release.
            /// </summary>
            internal void CheckPaddleDrag()
            {
                Point B = ball.Coordinates;
                Point V = ball.Velocity;
                Point Py = new(B.X, B.Y + V.Y);
                // Allow ball drag when it hits the paddle from the top.
                if (Coordinates.GetRange(0, size).Contains(Py))
                {
                    // The ball will become part of the paddle for exactly one iteration.
                    dragging = !dragging;
                    if (dragging)
                    {
                        ball.isMoving = false;
                        blocks.Add(ball);
                        Coordinates.Add(ball.Coordinates);
                        entities["paddle"].Add(ball);
                    }
                    else
                    {
                        blocks.RemoveAt(size);
                        Coordinates.RemoveAt(size);
                        entities["paddle"].Remove(ball);
                        ball.isMoving = true;
                    }
                }
            }

            /// <summary>
            /// Hypothesis for when the <see cref="ball"/> reflects from the paddle.
            /// </summary>
            internal void CheckPaddleReflect() {
                if (!dragging) {
                    Point B = ball.Coordinates;
                    Point V = ball.Velocity;
                    Point Py = new(B.X, B.Y + V.Y);
                    Point Pxy = new(B.X + V.X, B.Y + V.Y);
                    // Vertical reflection occurs if the ball hits the paddle directly from above
                    // or at a vertex.
                    if (Coordinates.GetRange(0, size).Intersect(new Point[] {Py, Pxy}).Any())
                    {
                        ball.Velocity.Y = -1;
                        // Horizontal reflection happens only when the paddle is hit at a vertex.
                        if (new Point[] {Py, Pxy}.Contains(Coordinates[0]))
                        {
                            // The ball moves left after hitting the left corner of the paddle.
                            ball.Velocity.X = -1;
                        }
                        else if (new Point[] {Py, Pxy}.Contains(Coordinates[size-1]))
                        {
                            // The ball moves right if it hit the right corner of the paddle.
                            ball.Velocity.X = 1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deals with ball's movement, spawning, and border reflection.
        /// </summary>
        class Ball : Block
        {
            internal bool isMoving;
            internal Point Velocity;
            internal Ball(int i, int j) : base(i, j)
            {
                isMoving = false;
                Velocity = new(0, 0);
                // Enable drawing.
                entities["ball"].Add(this);
            }

            /// <summary>
            /// Sets the diagonal movement.
            /// </summary>
            internal override void Move()
            {
                if (isMoving)
                {
                    MoveTo(Coordinates.X + Velocity.X, Coordinates.Y + Velocity.Y);
                }
            }

            /// <summary>
            /// Hypothesis for when the ball reflects from the border.
            /// </summary>
            internal void CheckBorderReflect()
            {
                Point B = Coordinates;
                Point V = Velocity;
                // Reversing the horizontal coordinate if the ball hits a vertical border.
                if ((B.X == 0 && V.X == -1) || (B.X == 9 && V.X == 1))
                {
                    Velocity.X = -V.X;
                }
                // Reversing the vertical coordinate if the ball hits the top border.
                if (B.Y == 0 && V.Y == -1)
                {
                    Velocity.Y = 1;
                }
            }
        }
    }
}
