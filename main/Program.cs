#pragma warning disable 0472, 8618

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Forms;
using BrickGame;
using static BrickGame.Constants;
using BrickGame.Games;

/// <summary>
/// Entry point for the execution of the main package.
/// </summary>
/// <para>Manages all the client and game mechanics and their interactions.</para>
/// <para>Read the <strong>Game Manuals</strong>.</para>
class BrickGameClient : InteractiveClient
{
    enum Env {Selector, Game}
    static Env environment = Env.Selector;
    static Game game;
    Selector selector;

    /// <summary>
    /// Initializing the GUI.
    /// </summary>
    BrickGameClient() : base()
    {
        window.Text = "Game Selection";
    }

    /// <summary>
    /// Generates the HighScores.json file, if it doesn't already exist.
    /// </summary>
    static void CreateHighScores()
    {
        if (File.Exists(HighScoresDir))
        {
            return;
        }
        try
        {
            // Create a zero-ed Scores object.
            Scores scores = new()
            {
                Snake = 0,
                Breakout = 0,
                Asteroids = 0,
                Tetris = 0
            };
            // Serialize that object to JSON.
            string jsonContent = JsonSerializer.Serialize(scores);
            // Write to the file.
            File.WriteAllText(HighScoresDir, jsonContent);
            Console.WriteLine("File HighScores.json successfully created at " + HighScoresDir);
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Failed to generate the HighScores.json file");
            return;  // This interrupts the scoring system without stopping the game.
        }
    }

    /// <summary>
    /// Manages all the console and game mechanics and their interactions.
    /// </summary>
    protected override void Setup()
    {
        base.Setup();

        CreateHighScores();
        // Render the environments.
        selector = new Selector();

        // Cover everything with the Background.
        Back?.BringToFront();
    }

    /// <summary>
    /// List of scheduled events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected override void SetLoop(object sender, EventArgs e)
    {
        base.SetLoop(sender, e);
        switch (environment)
        {
            case Env.Selector:
                selector.AnimateScreen();
                break;
            case Env.Game:
                // Implement the game mechanics and check for endgame.
                game.Manage(ticks);
                // Draw the game's objects to the screen.
                game.DrawEntities();
                // Update BlinkingBlock's mechanics.
                game.Update(ticks);
                break;
        }
    }

    protected override void SetKeyBindings(Keys key, bool pressed)
    {
        base.SetKeyBindings(key, pressed);
        switch (environment)
        {
            case Env.Selector:
                // Shifting to selector keybindings.
                selector.SetKeyBindings(key, pressed);
                break;
            case Env.Game:
                // Leaving the game instance if *Backspace* is pressed.
                if (pressed && key == Keys.Back)
                {
                    game.HideEntities();
                    environment = Env.Selector;
                    window.Text = "Game Selection";
                }
                else
                {   // Shift to the game keybindings otherwise.
                    game.SetKeyBindings(key, pressed);
                    
                }
                break;
        }
    }

    /// <summary>
    /// Mechanics for game selection, previews, and high scores.
    /// </summary>
    class Selector
    {
        Dictionary<int, Game> gameSelect;
        Dictionary<string, Panel?> previews;
        int numberOfGames;
        int stageID;
        string gameName;

        /// <summary>
        /// Instantiates all the game classes and loads previews.
        /// </summary>
        internal Selector()
        {
            // The game timers start only when selected.
            gameSelect = new()
            {
                {1, new Snake()},
                {2, new Breakout()},
                {3, new Asteroids()},
                {4, new Tetris()}
            };
            
            // Load preview screens.
            previews = new()
            {
                {"Snake1", LoadScreens("snake_1")},
                {"Snake2", LoadScreens("snake_2")},
                {"Snake3", LoadScreens("snake_3")},
                {"Breakout1", LoadScreens("breakout_1")},
                {"Breakout2", LoadScreens("breakout_2")},
                {"Breakout3", LoadScreens("breakout_3")},
                {"Asteroids1", LoadScreens("asteroids_1")},
                {"Asteroids2", LoadScreens("asteroids_2")},
                {"Asteroids3", LoadScreens("asteroids_3")},
                {"Tetris1", LoadScreens("tetris_1")},
                {"Tetris2", LoadScreens("tetris_2")},
                {"Tetris3", LoadScreens("tetris_3")}
            };

            // Set instance fields.
            numberOfGames = gameSelect.Count;
            stageID = 1;  // Show Snake first.
            gameName = gameSelect[stageID].Name;
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
                if (key == Keys.Return)
                {   // Enter a game.
                    if (stageID <= numberOfGames)
                    {
                        environment = Env.Game;
                        game = gameSelect[stageID];
                        // Update the window title and informing of the game change.
                        window.Text = game.Name;
                        Console.WriteLine("Now playing: " + game.Name);
                        // Avoid the need to press Enter twice after endgames.
                        if (!game.Running)
                        {
                            game.Reset();
                        }
                        else
                        {
                            Back?.BringToFront();
                        }
                        if (game.Paused)
                        {
                            game.DrawEntities(true);
                        }
                    }
                }
                else if (key == Keys.Left)  // Choosing a game.
                {
                    if (stageID > 1)
                    {
                        if (stageID > numberOfGames)
                        {
                            window.Text = "Game Selection";
                        }
                        stageID -= 1;
                    }
                    else
                    {
                        stageID = numberOfGames + 1;
                        new HighScoresScreen();
                        window.Text = "High Scores";
                    }
                }
                else if (key == Keys.Right)
                {
                    if (stageID <= numberOfGames)
                    {
                        stageID += 1;
                        if (stageID > numberOfGames)
                        {
                            new HighScoresScreen();
                            window.Text = "High Scores";
                        }
                    }
                    else
                    {
                        stageID = 1;
                        window.Text = "Game Selection";
                    }
                }
            }
        }

        internal void AnimateScreen()
        {
            if (stageID <= numberOfGames)
            {
                gameName = gameSelect[stageID].Name;
                switch (ticks % FPS) {
                    case 0:  // 0.000s
                        previews[gameName + 1]?.BringToFront();
                        break;
                    case FPS/3:  // 0.333s
                        previews[gameName + 2]?.BringToFront();
                        break;
                    case 2*FPS/3:  // 0.666s
                        previews[gameName + 3]?.BringToFront();
                        break;
                }
            }
        }
    }

    [STAThread]
    static void Main()
    {
        new BrickGameClient();
        Application.Run(window);
    }
}
