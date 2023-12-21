#pragma warning disable 8605, 8618

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Forms;
using static BrickGame.Constants;
using static BrickGame.InteractiveClient;

namespace BrickGame.Games
{
    /// <summary>
    /// Engine for pixel games in a 20x10 grid.
    /// </summary>
    abstract class Game
    {
        internal static Scores? scores;
        internal string Name { get; }
        internal bool Running { get; set; }
        internal bool Paused { get; set; }
        internal int Score { get; set; }
        internal int Speed { get; set; }
        int HighestScore { get; set; }
        protected static Dictionary<string, List<Block>> entities = new();
        protected string[] entityNames;

        /// <summary>
        /// Loads the game screens.
        /// </summary>
        internal Game()
        {
            Name = GetType().Name;
        }

        protected void SetEntities(params string[] entitiesList)
        {
            entityNames = entitiesList;
            foreach (string entity in entitiesList)
            {
                entities[entity] = new List<Block>();
            }
        }

        internal void HideEntities()
        {
            if (entities == null)
            {
                return;
            }
            foreach (string name in entityNames)
            {
                foreach (Block entity in entities[name])
                {
                    entity.Hide();
                }
            }
        }

        /// <summary>
        /// Initializes the instance variables and shows the <see cref="Background"/>.
        /// </summary>
        internal virtual void Start()
        {
            Score = 0;
            Speed = 1;       // Unit cells per second.
            Paused = false;
            Running = true;  // Where this game is being played.
            Back?.BringToFront();
            if (entities == null)
            {
                return;
            }
            foreach (string name in entityNames)
            {
                foreach (Block entity in entities[name])
                {
                    if (entity is not BlinkingBlock && entity is not HiddenBlock)
                    {
                        entity.Show(true);
                    }
                }
            }
            
            Console.WriteLine("Check docs\\GameManuals.md for instructions.");
        }

        /// <summary>
        /// Removes all elements from the screen and starts again.
        /// </summary>
        internal void Reset()
        {
            Running = false;
            // Clear the groups.
            foreach (string name in entityNames)
            {
                entities[name].Clear();
            }
            Start();
        }

        /// <summary>
        /// Deals with user input.
        /// </summary>
        /// <param name="key">A <see cref="Keys"/> identifier for a key.</param>
        /// <param name="pressed">Whether <c>key</c> was pressed or released.</param>
        internal virtual void SetKeyBindings(Keys key, bool pressed)
        {
            if (pressed)  // Key press.
            {
                if (key == Keys.P)
                {
                    // P pauses/unpauses the game.
                    Paused = !Paused;
                    if (Paused)
                    {
                        Console.WriteLine("Game paused");
                    }
                    else
                    {
                        Console.WriteLine("Game unpaused");
                    }
                }
                else if (key == Keys.Return)
                {
                    // Return resets the game.
                    Reset();
                }
            }
        }

        /// <summary>
        /// Game logic implementation for endgame.
        /// </summary>
        /// <param name="t"></param>
        internal virtual void Manage(int t)
        {
            if (Running && !Paused)
            {
                if (CheckVictory())
                {
                    ToggleVictory();
                    Console.WriteLine("Congratulations!");
                    Console.WriteLine($"Your score on {Name}: {Score}");
                }
                if (CheckDefeat())
                {
                    ToggleDefeat();
                    Console.WriteLine("Better luck next time...");
                    Console.WriteLine($"Your score on {Name}: {Score}");
                }
            }
        }

        /// <summary>
        /// Communicates with the <c>HighScores.json</c> file.
        /// </summary>
        internal virtual void UpdateScore()
        {
            // Reading the highest scores.
            try
            {
                // Read the file.
                string jsonContent = File.ReadAllText(HighScoresDir);
                // Feed the Scores instance.
                scores = JsonSerializer.Deserialize<Scores>(jsonContent);
                // Access fields dynamically using their names
                PropertyInfo? propertyInfo = typeof(Scores).GetProperty(Name);
                if (propertyInfo != null)
                {
                    HighestScore = (int)propertyInfo.GetValue(scores);
                }
                else
                {
                    HighestScore = 0;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Failed to read HighScores.json.");
                return;  // This interrupts the scoring system without stopping the game.
            }
            catch (AmbiguousMatchException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Failed to read HighScores.json.");
                return;  // This interrupts the scoring system without stopping the game.
            }
            // Updating the highest score to the HighScores.json file.
            if (Score > HighestScore)
            {
                if (Score >= 1e8)
                {
                    Score = (int)1e8 - 1;
                }
                else
                {
                    HighestScore = Score;
                }
                try
                {
                    // Access fields dynamically using their names
                    PropertyInfo? propertyInfo = typeof(Scores).GetProperty(Name);
                    propertyInfo?.SetValue(scores, HighestScore);
                    // Serialize object to JSON.
                    string jsonContent = JsonSerializer.Serialize(scores);
                    // Write to the file.
                    File.WriteAllText(HighScoresDir, jsonContent);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Failed to update HighScores.json.");
                    return;  // This interrupts the scoring system without stopping the game.
                }
                catch (AmbiguousMatchException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Failed to update HighScores.json.");
                    return;  // This interrupts the scoring system without stopping the game.
                }
            }
        }

        protected abstract bool CheckVictory();
        protected abstract bool CheckDefeat();

        /// <summary>
        /// Removes all elements from the screen and shows the <see cref="VictoryScreen"/>.
        /// </summary>
        internal void ToggleVictory()
        {
            Running = false;
            // Clean the groups.
            foreach (string name in entityNames)
            {
                entities[name].Clear();
            }
            // Show the victory message.
            VS?.BringToFront();
        }

        /// <summary>
        /// Removes all elements from the screen and shows the <see cref="DefeatScreen"/>.
        /// </summary>
        internal void ToggleDefeat()
        {
            Running = false;
            // Clean the groups.
            foreach (string name in entityNames)
            {
                entities[name].Clear();
            }
            // Show the defeat message.
            DS?.BringToFront();
        }

        /// <summary>
        /// Handles the blinking of <see cref="BlinkingBlock"/> objects.
        /// </summary>
        /// <param name="t">A timer.</param>
        internal void Update(int t)
        {
            if (Paused || entities == null)
            {
                return;
            }
            foreach (string name in entityNames)
            {
                foreach (Block entity in entities[name])
                {
                    if (entity is BlinkingBlock)
                    {
                        entity.Blink(t);
                    }
                }
            }
        }

        /** Draws all the sprites to the screen. */
        internal void DrawEntities(bool forceDraw)
        {
            if (Running && forceDraw)
            {
                // Draw the current objects to the screen.
                if (entities == null)
                {
                    return;
                }
                foreach (string name in entityNames)
                {
                    foreach (Block entity in entities[name])
                    {
                        if (entity is HiddenBlock)
                        {
                            entity.Hide();
                        }
                        else if (entity is not BlinkingBlock && entity.direction == Direction.Null)
                        {
                            entity.Show();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws all the sprites to the screen if the game isn't paused.
        /// </summary>
        internal void DrawEntities()
        {
            DrawEntities(!Paused);
        }
    }
}
