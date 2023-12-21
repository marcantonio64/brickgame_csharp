#pragma warning disable 8618

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static BrickGame.Constants;

namespace BrickGame.Games
{
    /// <summary>
    /// A Snake game.
    /// </summary>
    /// <para>Read the <strong>Game Manuals</strong>.</para>
    class Snake : Game
    {
        internal static Random random = new();
        internal static Direction direction = Direction.Null;
        internal static bool growing = false;
        static int startSpeed = 10;
        static int[] speedValues = new int[2];
        /// <summary>
        /// Used to allow only one directional movement at a time.
        /// </summary>
        bool keyEnabled;
        Body snake;
        Food food;

        /// <summary>
        /// Constructor for <see cref="Snake"/>.
        /// </summary>
        internal Snake() : base()
        {
            // Set containers for the entities.
            SetEntities("body", "food");
        }

        /// <summary>
        /// Defines game objects.
        /// </summary>
        internal override void Start()
        {
            base.Start();
            
            direction = Direction.Down;
            growing = false;
            speedValues[0] = startSpeed;
            speedValues[1] = 2*speedValues[0];
            Speed = speedValues[0];
            keyEnabled = false;

            // Spawn the entities.
            snake = new Body();
            food = new Food();
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
                {
                    if (key == Keys.Space)
                    {
                        Speed = speedValues[1];
                    }
                    else if (keyEnabled)
                    {
                        // Lock direction changes after the first one,
                        // until the next iteration.
                        keyEnabled = false;
                        // Direction changes, making sure the snake's head
                        // will not collide with itself.
                        if (key == Keys.Up && direction != Direction.Down)
                        {
                            direction = Direction.Up;
                        }
                        else if (key == Keys.Down && direction != Direction.Up)
                        {
                            direction = Direction.Down;
                        }
                        else if (key == Keys.Left && direction != Direction.Right)
                        {
                            direction = Direction.Left;
                        }
                        else if (key == Keys.Right && direction != Direction.Left)
                        {
                            direction = Direction.Right;
                        }
                    }
                }
                else  // Key released.
                {
                    if (key == Keys.Space)
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
            {
                CheckEat();
                // Set the action rate at speed Blocks per second.
                if (t % (FPS/(Speed % FPS)) == 0)
                {
                    UpdateScore();
                    snake.Move();
                    keyEnabled = true;

                    // Deal with speed values greater than FPS.
                    for (int i = 0; i < Speed/FPS; i++)
                    {
                        Manage(t);
                    }
                }
            }
            // Manage endgame.
            base.Manage(t);
        }

        /// <summary>
        /// The snake grows when it reaches the <see cref="food"/>.
        /// </summary>
        void CheckEat()
        {
            if (snake.head.Coordinates.Equals(food.Coordinates))
            {
                growing = true;
                food.Respawn();
                // Avoid the food spawning inside the snake.
                while (snake.Coordinates.GetRange(0, snake.Coordinates.Count - 1).Contains(food.Coordinates))
                {
                    food.Respawn();
                }
            }
        }

        /// <summary>
        /// Scoring mechanics.
        /// </summary>
        internal override void UpdateScore()
        {
            int n = entities["body"].Count;
            if (growing)
            {
                if (3 < n && n <= 25)
                {
                    Score += 15;
                }
                else if (25 < n && n <= 50)
                {
                    Score += 45;
                }
                else if (50 < n && n <= 100)
                {
                    Score += 100;
                }
                else if (100 < n && n < 200)
                {
                    Score += 250;
                }
            }
            base.UpdateScore();
        }

        /// <summary>
        /// Victory occurs when the snake's size becomes that of the whole grid.
        /// </summary>
        /// <returns>Whether the game was beaten.</returns>
        protected override bool CheckVictory()
        {
            return entities["body"].Count == 200;
        }

        /// <summary>
        /// Defeat happens if the snake's <see cref="Body.head">head</see> hits the
        /// rest of its own body or the borders.
        /// </summary>
        /// <returns>Whether the game was lost.</returns>
        protected override bool CheckDefeat()
        {
            int i = snake.head.Coordinates.X;
            int j = snake.head.Coordinates.Y;
            int n = snake.Coordinates.Count;
            if (i < 0 || i >= 10 || j < 0 || j >= 20)
            {
                return true;
            }
            if (snake.Coordinates.GetRange(0, snake.Coordinates.Count - 1).Contains(snake.head.Coordinates))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Organizes the snake's drawing, movement, and growth.
        /// </summary>
        class Body
        {
            internal List<Point> Coordinates;
            internal Block head;
            Block tail;
            internal Body() {
                // Set the snake's initial position.
                Coordinates = new()
                {
                    new Point(4, 3),
                    new Point(4, 4),
                    new Point(4, 5)
                };
                // Add Blocks to the container for drawing.
                foreach (Point p in Coordinates)
                {
                    entities["body"].Add(new Block(p));
                }
                // Identify the head and the tail.
                head = entities["body"][Coordinates.Count - 1];
                tail = entities["body"][0];
            }

            /// <summary>
            /// Handles the snake's movement and growth mechanics.
            /// </summary>
            internal void Move()
            {
                int i = ConvertDirection[direction].X;
                int j = ConvertDirection[direction].Y;
                int a = head.Coordinates.X;
                int b = head.Coordinates.Y;
                // Movement is achieved by creating a new Block object in
                // the head's next position, ...
                head = new Block(i+a, j+b);
                entities["body"].Add(head);
                Coordinates.Add(new Point(i+a, j+b));
                // ...keeping the tail in the same place if the head does
                // not hit the food, ...
                if (growing)
                {
                    growing = !growing;
                }
                else
                {
                    // ... or deleting it (references and drawing)
                    // otherwise.
                    tail.Hide();
                    entities["body"].RemoveAt(0);
                    Coordinates.RemoveAt(0);
                    tail = entities["body"][0];
                }
            }
        }

        /// <summary>
        /// Organizes the food's spawn randomly.
        /// </summary>
        class Food : BlinkingBlock
        {
            /// <summary>
            /// Generates random coordinates to spawn a <see cref="BlinkingBlock"/> at.
            /// </summary>
            internal Food() : base(random.Next(10), random.Next(20))
            {
                // Add the instance to the container for drawing.
                entities["food"].Clear();
                entities["food"].Add(this);
            }

            /// <summary>
            /// Draws a <see cref="BlinkingBlock"/> at a new random position in the grid.
            /// </summary>
            internal void Respawn()
            {
                Sprite sprite = GetSprite();
                MoveTo(random.Next(10), random.Next(20));
                sprite.Show();
            }
        }
    }
}
