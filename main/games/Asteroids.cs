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
    /// An Asteroids game.
    /// </summary>
    /// <para>Read the <strong>Game Manuals</strong>.</para>
    class Asteroids : Game
    {
        const bool UseBombs = true;
        static readonly Random random = new();
        static readonly int shooterSpeed = 10;
        int asteroidsSpeed;  // Falling speed.
        int gameTimer;  // To scale the difficulty.
        Shooter shooter;
        Bomb bomb;


        /// <summary>
        /// Constructor for <see cref="Asteroids"/>.
        /// </summary>
        internal Asteroids() : base()
        {
            // Set containers for the entities.
            SetEntities("asteroids", "bullet", "shooter", "bomb");
        }

        /// <summary>
        /// Defines game objects.
        /// </summary>
        internal override void Start()
        {
            base.Start();
            asteroidsSpeed = 2;
            Speed = asteroidsSpeed;
            gameTimer = 0;
            // Spawn the entities.
            shooter = new Shooter();
            bomb = new Bomb();
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
                {   // Set Shooter movement.
                    if (key == Keys.Left)
                    {
                        shooter.direction = Direction.Left;
                    }
                    else if (key == Keys.Right)
                    {
                        shooter.direction = Direction.Right;
                    }
                }
                else  // Key released.
                {
                    if (key == Keys.Left)
                    {
                        shooter.direction = Direction.Null;
                    }
                    else if (key == Keys.Right)
                    {
                        shooter.direction = Direction.Null;
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
                // Use a custom timer to allow for proper scaling of difficulty.
                gameTimer += 1;

                // Set an action rate of FPS Blocks per second.
                for (int i = 0; i <= 60/FPS; i++)
                {
                    shooter.MoveBullets();
                }

                // Manage multiple simultaneous hits and scoring.
                int numberOfCollisions = CheckHit();
                UpdateScore(numberOfCollisions);

                // Set an action rate of asteroidsSpeed Blocks per second.
                if (t % (FPS/asteroidsSpeed) == 0)
                {
                    MoveAsteroids(gameTimer);
                    bomb.Move(Direction.Up);
                    bomb.CheckExplosion(entities["asteroids"]);
                }

                // Set an action rate of shooterSpeed Blocks per second.
                if (t % (FPS/shooterSpeed) == 0)
                {
                    // The Bullet's movement is handled by its update method.
                    shooter.Shoot();
                    shooter.Move();
                    TrySpawnBomb(gameTimer);
                }
            }
            // Manage endgame.
            base.Manage(t);
        }

        /// <summary>
        /// Bullets disappear and destroy asteroids upon collision.
        /// </summary>
        /// <returns>Number of asteroid Blocks destroyed.</returns>
        int CheckHit()
        {
            List<Block> asteroids = entities["asteroids"];
            List<Block> collisions = new();
            List<Block> newCollisions;
            for (int i = entities["bullet"].Count - 1; i >= 0; i--)
            {
                Bullet bullet = (Bullet)entities["bullet"][i];
                Point Pbul = bullet.Coordinates;
                newCollisions = new();
                foreach (Block block in asteroids)
                {
                    Point Pblo = block.Coordinates;
                    // A collision happens if the coordinates coincide.
                    if (Pbul.X == Pblo.X && (Pblo.Y == Pbul.Y || Pblo.Y == Pbul.Y + 1))
                    {
                        newCollisions.Add(block);
                    }
                }
                if (newCollisions.Any())
                {
                    // Destroy the bullet.
                    bullet.Hide();
                    shooter.bullets.Remove(bullet);
                    entities["bullet"].RemoveAt(i);
                }
                collisions.AddRange(newCollisions);
            }
            foreach (Block block in collisions)
            {
                // Destroy the asteroids.
                block.Hide();
                entities["asteroids"].Remove(block);
            }
            return collisions.Count;
        }

        /// <summary>
        /// Scoring mechanics.
        /// </summary>
        /// <param name="blocksHit"></param>
        void UpdateScore(int blocksHit)
        {
            Score += 5*blocksHit;
            base.UpdateScore();
        }

        /// <summary>
        /// Handles the <see cref="Bomb"/>'s spawn over time according to its spawn rate.
        /// </summary>
        /// <param name="t">A timer.</param>
        void TrySpawnBomb(int t)
        {
            // The chance of a Bomb spawning increases from 0.1% up to 0.15% after 3 minutes.
            double spawnRate = 1.0/3000;
            if (t <= 180*FPS && t % (60*FPS) == 0)
            {
                spawnRate += 1.0/6000;
            }
            bool spawning = random.NextDouble() < spawnRate;
            if (UseBombs && spawning)
            {
                int i = random.Next(7);
                bomb = new Bomb(i, 19, entities["bomb"]);
            }
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
        /// <para>Occurs when an asteroid {@link Block} reaches either the
        /// <see cref="shooter"/> or the lower border of the grid.</para>
        /// <returns>Whether the game was lost.</returns>
        protected override bool CheckDefeat()
        {
            Block shooter = entities["shooter"][0];
            List<Block> asteroids = entities["asteroids"];
            List<int> heights = new(asteroids.Count);
            heights.Add(1);
            // Check for collisions with the shooter.
            List<Block> collision = new();
            foreach (Block asteroid in asteroids)
            {
                if (shooter.Coordinates.Equals(asteroid.Coordinates))
                {
                    collision.Add(asteroid);
                }
                heights.Add(asteroid.Coordinates.Y);
            }
            // Track the asteroids' height.
            int height = heights.Max();

            return collision.Any() || height >= 20;  //  Hitting the bottom.
        }

        /// <summary>
        /// Organizes the asteroids' display and movement.
        /// </summary>
        /// <param name="t">A timer.</param>
        void MoveAsteroids(int t)
        {
            if (entities["asteroids"].Any())
            {
                foreach (Block asteroid in entities["asteroids"])
                {
                    asteroid.MoveTo(asteroid.Coordinates.X, asteroid.Coordinates.Y + 1);
                }
            }
            // Spawn rate starts at 0.3 per tick, increasing linearly up to
            // 0.45 per tick after 3 minutes. (>=0.5 is unbeatable.) */
            double r;
            if (t < 180*FPS)
            {
                r = 0.3 + t*0.15/(180*FPS);
            }
            else
            {
                r = 0.45;
            }
            for (int i = 0; i < 10; i++)
            {
                if (random.NextDouble() < r)
                {
                    entities["asteroids"].Add(new Block(i, 0));
                }
            }
        }

        class Bullet : Block
        {
            /// <summary>
            /// A <see cref="Block"/> moving up.
            /// </summary>
            /// <param name="coords"></param>
            internal Bullet(Point coords) : base(coords)
            {
                direction = Direction.Up;
                entities["bullet"].Add(this);
            }
        }

        /// <summary>
        /// Manages the player-controlled shooter.
        /// </summary>
        /// <para>A <see cref="Block"/> moving horizontally at the bottom of the grid
        /// that can shoot <see cref="Bullet"/>s.</para>
        class Shooter : Block
        {
            internal List<Bullet> bullets;

            /// <summary>
            /// Sets <see cref="Shooter"/>'s initial position.
            /// </summary>
            internal Shooter() : base(4, 19)
            {
                entities["shooter"] = new List<Block>(){this};
                bullets = new();
            }

            /// <summary>
            /// Avoids the <see cref="Shooter"/> from leaving the grid.
            /// </summary>
            internal override void Move()
            {
                int x = ConvertDirection[direction].X;
                if (0 <= Coordinates.X + x && Coordinates.X + x < 10)
                {
                    base.Move();
                }
            }

            /// <summary>
            /// Spawns a <see cref="Bullet"/>.
            /// </summary>
            internal void Shoot()
            {
                bullets.Add(new Bullet(new Point(Coordinates.X, Coordinates.Y - 1)));
            }

            internal void MoveBullets()
            {
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    Bullet bullet = bullets[i];
                    bullet.Move();
                    // Make the bullet disappear if it hits the top border.
                    if (bullet.Coordinates.Y < 0)
                    {
                        bullet.Hide();
                        entities["bullet"].Remove(bullet);
                        bullets.RemoveAt(i);
                    }
                }
            }
        }
    }
}
