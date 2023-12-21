#pragma warning disable 8602, 8618, 8622

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static BrickGame.Constants;

namespace BrickGame
{
    /// <summary>
    /// Establishes the basic structure of the client.
    /// </summary>
    class Client
    {
        internal static Form window;
        internal static Panel canvas;
        static Timer timer;
        static protected int ticks = 0;
        int delay = TickRate;
        long currTime;
        long lastTime;

        /// <summary>
        /// A constructor that creates a GUI, reads user input, and sets a loop.
        /// </summary>
        internal Client()
        {
            // Create the main window.
            window = new()
            {
                StartPosition = FormStartPosition.CenterScreen,
                ClientSize = new Size(WindowWidth, WindowHeight),
                // Set the form's border style to FixedDialog (non-resizable)
                FormBorderStyle = FormBorderStyle.FixedDialog,
                BackColor = LineColor,
                Visible = true
            };
            // Close the application when the form is closed.
            window.FormClosed += (sender, e) => Application.Exit();
            // Hide the cursor.
            Cursor.Hide();
            
            // Create a canvas to draw on.
            canvas = new()
            {
                ClientSize = window.ClientSize,
                BackColor = BackgroundColor
            };

            // Add the canvas to the window.
            window.Controls.Add(canvas);
            window.Focus();
            window.Show();
            
            Setup();

            HandleKeyEvents();

            // Setting the main loop.
            timer = new()
            {
                Interval = delay
            };
            timer.Tick += SetLoop;
            currTime = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
            timer.Start();
        }

        /// <summary>
        /// Sets the drawing of sprites.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> from the current <see cref="Control"/> object.</param>
        /// <param name="c">The desired <see cref="Color"/>.</param>
        /// <param name="x">The desired <c>x</c> position.</param>
        /// <param name="y">The desired <c>y</c> position.</param>
        internal static void Paint(Graphics g, Color c, int x, int y)
        {
            int inSide = 7*PixelSize;
            int outSide = 9*PixelSize;
            // Set the tools.
            Brush brush = new SolidBrush(c);
            Pen pen1 = new(BackgroundColor, PixelSize);
            Pen pen2 = new(c, PixelSize);
            // Draw an inner square.
            g.FillRectangle(brush, x, y, BlockSize - 2*PixelSize, BlockSize - 2*PixelSize);
            // Draw a middle stroked rectangle of a different colokr.
            g.DrawRectangle(pen1, x + PixelSize + PixelSize/2, y + PixelSize + PixelSize/2, inSide, inSide);
            // Draw an outer stroked rectangle of the inner color.
            g.DrawRectangle(pen2, x + PixelSize/2, y + PixelSize/2, outSide, outSide);
            // Free the memory.
            g.Dispose();
        }

        /// <summary>
        /// Sets the drawing of sprites.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> from the current <see cref="Control"/> object.</param>
        /// <param name="c">The desired <see cref="Color"/>.</param>
        internal static void Paint(Graphics g, Color c)
        {
            Paint(g, c, 0, 0);
        }

        /// <summary>
        /// Adding functionalities to <see cref="canvas"/>.
        /// </summary>
        protected virtual void Setup() {}

        /// <summary>
        /// Deals with user input.
        /// </summary>
        /// <param name="key">A <see cref="Keys"/> identifier for a key.</param>
        /// <param name="pressed">Whether <c>key</c> was pressed or released.</param>
        protected virtual void SetKeyBindings(Keys key, bool pressed) {}

        /// <summary>
        /// Schedules looping events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void SetLoop(object sender, EventArgs e)
        {
            ticks++;
            lastTime = currTime;
            currTime = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
            // Removing the extra time since the last loop from the reference.
            int delayError = (int)(currTime - lastTime - TickRate);
            //Console.WriteLine(100*delayError/TickRate);
            delay = (0 <= delayError && delayError < delay) ? (delay - delayError) : (TickRate - 15);
            timer.Interval = delay;
            //if (ticks % FPS == 0)
            //{
            //    Console.WriteLine(DateTime.Now);
            //}
        }
        
        private void HandleKeyEvents()
        {
            window.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    // Close the form when the *Escape* key is pressed.
                    window.Close();
                }
                else
                {
                    SetKeyBindings(e.KeyCode, true);
                }
            };
            window.KeyUp += (sender, e) => SetKeyBindings(e.KeyCode, false);
        }
    }

    /// <summary>
    /// Complete client with layered <see cref="Sprite"/>s at fixed positions.
    /// </summary>
    class InteractiveClient : Client
    {
        internal static Dictionary<Point, Sprite> SpriteDict = new();
        internal static Panel? Back, VS, DS;
        protected override void Setup()
        {
            // Layering all the Sprite objects to be used.
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    SpriteDict[new Point(i, j)] = new Sprite(i, j);
                }
            }

            // Load the necessary screens and cover with the Background.
            VS = LoadScreens("victory_screen");
            DS = LoadScreens("defeat_screen");
            Back = LoadScreens("background");
        }

        /// <summary>
        /// Accessing images from <strong>screens\</strong>.
        /// </summary>
        /// <param name="imagePrefix">Name prefix (without the <c>-wxh.png</c> part) identifying the image.</param>
        /// <returns>A <see cref="Panel"/> with the image.</returns>
        internal static Panel? LoadScreens(string imagePrefix)
        {
            string imagePath = Path.Combine(ScreensDir, $"{imagePrefix}-{WindowWidth}x{WindowHeight}.png");
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("File " + imagePath + " doesn't exist");
                return null;
            }
            Panel panel = new()
            {
                ClientSize = canvas.ClientSize
            };
            try
            {
                Image image = Image.FromFile(imagePath);
                
                // Subscribe to the Paint event of the panel.
                panel.Paint += (sender, e) =>
                {
                    // Use the e.Graphics from the PaintEventArgs to draw the image.
                    e.Graphics.DrawImage(image, 0, 0, panel.Width, panel.Height);
                };

                // Add the panel to the canvas.
                canvas.Controls.Add(panel);

                // Force the panel to repaint.
                panel.Invalidate();

                return panel;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
