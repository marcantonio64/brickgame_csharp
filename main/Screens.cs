#pragma warning disable 8600, 8622

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Drawing;
using System.Windows.Forms;
using static BrickGame.Constants;
using static BrickGame.Client;

namespace BrickGame
{
    /// <summary>
    /// Defines the appearance and the positioning of unit cells.
    /// </summary>
    internal class Sprite : Panel
    {
        Color Color { get; set; }
        Point Coordinates { get; }
        Panel Panel;
        bool active { get; set; }

        /// <summary>
        /// A constructor with all parameters.
        /// </summary>
        /// <param name="i">Horizontal position. Must be from 0 (inclusive) to 10 (exclusive)
        /// to show on the screen.</param>
        /// <param name="j">Vertical position. Must be from 0 (inclusive) to 20 (exclusive)
        /// to show on the screen.</param>
        /// <param name="color"><see cref="Color"/> of the <see cref="Sprite"/> lines.</param>
        /// <param name="panel">Where to draw the <see cref="Sprite"/> on.</param>
        internal Sprite(int i, int j, Color color, Panel panel) : base()
        {
            Color = color;
            Coordinates = new Point(i, j);
            Panel = panel;
            Place();

            // Subscribe to the Paint event.
            Paint += Sprite_Paint;

            // Draw.
            Raise(true);
        }

        /// <summary>
        /// A constructor without the <c>color</c> parameter, which is standardized to <see cref="LineColor"/>.
        /// </summary>
        /// <param name="i">Horizontal position. Must be from 0 (inclusive) to 10 (exclusive)
        /// to show on the screen.</param>
        /// <param name="j">Vertical position. Must be from 0 (inclusive) to 20 (exclusive)
        /// to show on the screen.</param>
        /// <param name="panel">Where to draw the <see cref="Sprite"/> on.</param>
        internal Sprite(int i, int j, Panel panel) : this(i, j, LineColor, panel) {}

        /// <summary>
        /// A constructor with only raw coordinates.
        /// </summary>
        /// <param name="i">Horizontal position. Must be from 0 (inclusive) to 10 (exclusive)
        /// to show on the screen.</param>
        /// <param name="j">Vertical position. Must be from 0 (inclusive) to 20 (exclusive)
        /// to show on the screen.</param>
        internal Sprite(int i, int j) : this(i, j, LineColor, canvas) {}

        /// <summary>
        /// Draws a stroked rectangle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pe"></param>
        void Sprite_Paint(object sender, PaintEventArgs pe)
        {
            int inSide = 7*PixelSize;
            int outSide = 9*PixelSize;
            // Create a Graphics object.
            Graphics g = pe.Graphics;
            // Set the tools.
            Brush brush = new SolidBrush(Color);
            Pen pen1 = new(BackgroundColor, PixelSize);
            Pen pen2 = new(Color, PixelSize);
            // Draw an inner square.
            g.FillRectangle(brush, 0, 0, BlockSize - 2*PixelSize, BlockSize - 2*PixelSize);
            // Draw a middle stroked rectangle of a different colokr.
            g.DrawRectangle(pen1, PixelSize + PixelSize/2, PixelSize + PixelSize/2, inSide, inSide);
            // Draw an outer stroked rectangle of the inner color.
            g.DrawRectangle(pen2, PixelSize/2, PixelSize/2, outSide, outSide);
            // Free the memory.
            g.Dispose();
        }

        /// <summary>
        /// <see cref="Sprite"/> display.
        /// </summary>
        /// <para> Adjusts and shows the <c>Sprite</c> in the 20x10 grid at
        /// a fixed position.</para>
        protected void Place()
        {
            // Set Sprite dimensions.
            SetBounds(Coordinates.X*DistanceBlocks,
                      Coordinates.Y*DistanceBlocks,
                      BlockSize,
                      BlockSize);
            // Add the Sprite to the Panel.
            Panel.Controls.Add(this);
        }

        internal void Raise(bool force)
        {
            BringToFront();
            active = !force;
        }

        internal void Raise()
        {
            if (!active)
            {
                active = true;
                BringToFront();
            }
        }

        internal void Lower()
        {
            if (active)
            {
                active = false;
              SendToBack();
            }
        }

        internal void Blink(int t)
        {
            if (t % FPS == 0)
            {
                Raise();
            }
            else if (t % FPS == FPS/2)
            {
                Lower();
            }
        }
    }

    /// <summary>
    /// The background image for the Brick Game's client.
    /// </summary>
    class Background : Panel
    {
        internal Background() : base()
        {
            ClientSize = canvas.ClientSize;
            BackColor = BackgroundColor;
            Draw();
            // Add the background panel to the main canvas.
            canvas.Controls.Add(this);
        }

        /// <summary>
        /// Draws a 20x10 grid of <see cref="Sprite"/>s colored <see cref="ShadeColor"/>.
        /// </summary>
        protected virtual void Draw()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    new Sprite(i, j, ShadeColor, this);
                }
            }
        }
    }

    /// <summary>
    /// A 'You Win' message.
    /// </summary>
    internal class VictoryScreen : Background
    {
        /// <summary>
        /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
        /// </summary>
        protected override void Draw()
        {
            base.Draw();
            Dictionary<int, int[]> sketch = new()
            {
                [1]  = new int[] {0,    2,       5,          9},
                [2]  = new int[] {0,    2,       5,          9},
                [3]  = new int[] {0, 1, 2,       5,    7,    9},
                [4]  = new int[] {      2,       5, 6, 7, 8, 9},
                [5]  = new int[] {0, 1, 2,       5, 6,    8, 9},
                
                [7]  = new int[] {0, 1, 2,          6, 7, 8   },
                [8]  = new int[] {0,    2,             7      },
                [9]  = new int[] {0,    2,             7      },
                [10] = new int[] {0, 1, 2,          6, 7, 8   },

                [12] = new int[] {0,    2,       5, 6,       9},
                [13] = new int[] {0,    2,       5, 6, 7,    9},
                [14] = new int[] {0,    2,       5,    7, 8, 9},
                [15] = new int[] {0, 1, 2,       5,       8, 9}
            };

            foreach (var entry in sketch)
            {
                int j = entry.Key;
                foreach (int i in entry.Value)
                {
                    new Sprite(i, j, this);
                }
            }
        }
    }
    
    /// <summary>
    /// A 'Game Over' message.
    /// </summary>
    internal class DefeatScreen : Background
    {
        /// <summary>
        /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
        /// </summary>
        protected override void Draw()
        {
            base.Draw();
            Dictionary<int, int[]> sketch = new()
            {
                [0]  = new int[] {   1, 2, 3, 4,       7, 8, 9},
                [1]  = new int[] {0,                   7,    9},
                [2]  = new int[] {0,    2, 3, 4,       7,    9},
                [3]  = new int[] {0,          4,       7, 8, 9},
                [4]  = new int[] {   1, 2, 3                  },
                [5]  = new int[] {                     7,    9},
                [6]  = new int[] {   1, 2, 3,          7,    9},
                [7]  = new int[] {0,          4,       7,    9},
                [8]  = new int[] {0, 1, 2, 3, 4,          8   },
                [9]  = new int[] {0,          4               },
                [10] = new int[] {                     7, 8, 9},
                [11] = new int[] {0,          4,       7      },
                [12] = new int[] {0, 1,    3, 4,       7, 8, 9},
                [13] = new int[] {0,    2,    4,       7      },
                [14] = new int[] {                     7, 8, 9},
                [15] = new int[] {   1, 2, 3                  },
                [16] = new int[] {   1,                7, 8   },
                [17] = new int[] {   1, 2, 3,          7,    9},
                [18] = new int[] {   1,                7, 8   },
                [19] = new int[] {   1, 2, 3,          7,    9}
            };
            
            foreach (var entry in sketch)
            {
                int j = entry.Key;
                foreach (int i in entry.Value)
                {
                    new Sprite(i, j, this);
                }
            }
        }
    }

    /// <summary>
    /// Organizes the game preview screens.
    /// </summary>
    class GamePreviews
    {
        /// <summary>
        /// First preview for <strong>Snake</strong>.
        /// </summary>
        internal class Snake1 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {                            },
                    [1]  = new int[] {                            },
                    [2]  = new int[] {                            },
                    [3]  = new int[] {      2, 3, 4, 5, 6, 7,     },
                    [4]  = new int[] {                            },
                    [5]  = new int[] {                            },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                     7,     },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {                            },
                    [11] = new int[] {                            },
                    [12] = new int[] {                            },
                    [13] = new int[] {                            },
                    [14] = new int[] {                            },
                    [15] = new int[] {            4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3, 4, 5, 6,        },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3,       6,        }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }

        /// <summary>
        /// Second preview for <strong>Snake</strong>.
        /// </summary>
        internal class Snake2 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {                            },
                    [1]  = new int[] {                            },
                    [2]  = new int[] {                            },
                    [3]  = new int[] {         3, 4, 5, 6, 7,     },
                    [4]  = new int[] {                     7,     },
                    [5]  = new int[] {                            },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                     7,     },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {                            },
                    [11] = new int[] {                            },
                    [12] = new int[] {                            },
                    [13] = new int[] {                            },
                    [14] = new int[] {                            },
                    [15] = new int[] {            4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3, 4, 5, 6,        },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3,       6,        }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// Third preview for <strong>Snake</strong>.
        /// </summary>
        internal class Snake3 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {                            },
                    [1]  = new int[] {                            },
                    [2]  = new int[] {                            },
                    [3]  = new int[] {            4, 5, 6, 7,     },
                    [4]  = new int[] {                     7,     },
                    [5]  = new int[] {                     7,     },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                     7,     },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {                            },
                    [11] = new int[] {                            },
                    [12] = new int[] {                            },
                    [13] = new int[] {                            },
                    [14] = new int[] {                            },
                    [15] = new int[] {            4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3, 4, 5, 6,        },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3,       6,        }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// First preview for <strong>Breakout</strong>.
        /// </summary>
        internal class Breakout1 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
                    [1]  = new int[] {0,                         9},
                    [2]  = new int[] {0,                         9},
                    [3]  = new int[] {0,       3, 4, 5, 6,       9},
                    [4]  = new int[] {0,       3, 4, 5, 6,       9},
                    [5]  = new int[] {0,       3, 4, 5, 6,       9},
                    [6]  = new int[] {0,       3, 4, 5, 6,       9},
                    [7]  = new int[] {0,                         9},
                    [8]  = new int[] {0,                         9},
                    [9]  = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
                    [10] = new int[] {                            },
                    [11] = new int[] {               5,           },
                    [12] = new int[] {                            },
                    [13] = new int[] {         3, 4, 5,           },
                    [14] = new int[] {                            },
                    [15] = new int[] {         3, 4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3, 4, 5,           },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3, 4, 5,           }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// Second preview for <strong>Breakout</strong>.
        /// </summary>
        internal class Breakout2 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
                    [1]  = new int[] {0,                         9},
                    [2]  = new int[] {0,                         9},
                    [3]  = new int[] {0,       3, 4, 5, 6,       9},
                    [4]  = new int[] {0,       3, 4, 5, 6,       9},
                    [5]  = new int[] {0,       3, 4, 5, 6,       9},
                    [6]  = new int[] {0,       3, 4, 5, 6,       9},
                    [7]  = new int[] {0,                         9},
                    [8]  = new int[] {0,                         9},
                    [9]  = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
                    [10] = new int[] {                  6,        },
                    [11] = new int[] {                            },
                    [12] = new int[] {                            },
                    [13] = new int[] {         3, 4, 5,           },
                    [14] = new int[] {                            },
                    [15] = new int[] {         3, 4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3, 4, 5,           },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3, 4, 5,           }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// Third preview for <strong>Breakout</strong>.
        /// </summary>
        internal class Breakout3 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
                    [1]  = new int[] {0,                         9},
                    [2]  = new int[] {0,                         9},
                    [3]  = new int[] {0,       3, 4, 5, 6,       9},
                    [4]  = new int[] {0,       3, 4, 5, 6,       9},
                    [5]  = new int[] {0,       3, 4, 5, 6,       9},
                    [6]  = new int[] {0,       3, 4, 5, 6,       9},
                    [7]  = new int[] {0,                         9},
                    [8]  = new int[] {0,                         9},
                    [9]  = new int[] {0, 1, 2, 3, 4, 5,       8, 9},
                    [10] = new int[] {                            },
                    [11] = new int[] {                     7,     },
                    [12] = new int[] {                            },
                    [13] = new int[] {            4, 5, 6,        },
                    [14] = new int[] {                            },
                    [15] = new int[] {         3, 4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3, 4, 5,           },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3, 4, 5,           }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// First preview for <strong>Asteroids</strong>.
        /// </summary>
        internal class Asteroids1 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {0, 1, 2, 3,             8, 9},
                    [1]  = new int[] {0,    2,    4, 5, 6, 7,    9},
                    [2]  = new int[] {0,    2,                   9},
                    [3]  = new int[] {0,       3, 4, 5,           },
                    [4]  = new int[] {   1,                       },
                    [5]  = new int[] {            4,              },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                            },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {                            },
                    [11] = new int[] {                            },
                    [12] = new int[] {            4,              },
                    [13] = new int[] {            4,              },
                    [14] = new int[] {                            },
                    [15] = new int[] {            4, 5, 6,        },
                    [16] = new int[] {         3,                 },
                    [17] = new int[] {         3,                 },
                    [18] = new int[] {         3,                 },
                    [19] = new int[] {            4, 5, 6,        }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// Second preview for <strong>Asteroids</strong>.
        /// </summary>
        internal class Asteroids2 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {0, 1, 2, 3,             8, 9},
                    [1]  = new int[] {0,    2,    4, 5, 6, 7,    9},
                    [2]  = new int[] {0,    2,                   9},
                    [3]  = new int[] {0,       3, 4, 5,           },
                    [4]  = new int[] {   1,       4,              },
                    [5]  = new int[] {                            },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                            },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {                            },
                    [11] = new int[] {            4,              },
                    [12] = new int[] {                            },
                    [13] = new int[] {            4,              },
                    [14] = new int[] {                            },
                    [15] = new int[] {            4, 5, 6,        },
                    [16] = new int[] {         3,                 },
                    [17] = new int[] {         3,                 },
                    [18] = new int[] {         3,                 },
                    [19] = new int[] {            4, 5, 6,        }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
        
        /// <summary>
        /// Third preview for <strong>Asteroids</strong>.
        /// </summary>
        internal class Asteroids3 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {0, 1, 2, 3,             8, 9},
                    [1]  = new int[] {0,    2,    4, 5, 6, 7,    9},
                    [2]  = new int[] {0,    2,                   9},
                    [3]  = new int[] {0,       3,    5,           },
                    [4]  = new int[] {   1,                       },
                    [5]  = new int[] {                            },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                            },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {            4,              },
                    [11] = new int[] {                            },
                    [12] = new int[] {                            },
                    [13] = new int[] {            4,              },
                    [14] = new int[] {                            },
                    [15] = new int[] {            4, 5, 6,        },
                    [16] = new int[] {         3,                 },
                    [17] = new int[] {         3,                 },
                    [18] = new int[] {         3,                 },
                    [19] = new int[] {            4, 5, 6,        }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }

        /// <summary>
        /// First preview for <strong>Tetris</strong>.
        /// </summary>
        internal class Tetris1 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {                            },
                    [1]  = new int[] {         3, 4, 5,           },
                    [2]  = new int[] {            4,              },
                    [3]  = new int[] {                            },
                    [4]  = new int[] {                            },
                    [5]  = new int[] {                            },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                            },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {0,                          },
                    [11] = new int[] {0,                          },
                    [12] = new int[] {0, 1, 2,          6, 7,     },
                    [13] = new int[] {0, 1, 2, 3,    5, 6, 7, 8, 9},
                    [14] = new int[] {                            },
                    [15] = new int[] {         3, 4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3,       6,        },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3, 4, 5,           }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }

        /// <summary>
        /// Second preview for <strong>Tetris</strong>.
        /// </summary>
        internal class Tetris2 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {                            },
                    [1]  = new int[] {                            },
                    [2]  = new int[] {         3, 4, 5,           },
                    [3]  = new int[] {            4,              },
                    [4]  = new int[] {                            },
                    [5]  = new int[] {                            },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                            },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {0,                          },
                    [11] = new int[] {0,                          },
                    [12] = new int[] {0, 1, 2,          6, 7,     },
                    [13] = new int[] {0, 1, 2, 3,    5, 6, 7, 8, 9},
                    [14] = new int[] {                            },
                    [15] = new int[] {         3, 4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3,       6,        },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3, 4, 5,           }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }

        /// <summary>
        /// Third preview for <strong>Tetris</strong>.
        /// </summary>
        internal class Tetris3 : Background
        {
            /// <summary>
            /// Draws the pixel message with <see cref="Sprite"/>s in a 20x10 grid.
            /// </summary>
            protected override void Draw()
            {
                base.Draw();
                Dictionary<int, int[]> sketch = new()
                {
                    [0]  = new int[] {                            },
                    [1]  = new int[] {                            },
                    [2]  = new int[] {                            },
                    [3]  = new int[] {         3, 4, 5,           },
                    [4]  = new int[] {            4,              },
                    [5]  = new int[] {                            },
                    [6]  = new int[] {                            },
                    [7]  = new int[] {                            },
                    [8]  = new int[] {                            },
                    [9]  = new int[] {                            },
                    [10] = new int[] {0,                          },
                    [11] = new int[] {0,                          },
                    [12] = new int[] {0, 1, 2,          6, 7,     },
                    [13] = new int[] {0, 1, 2, 3,    5, 6, 7, 8, 9},
                    [14] = new int[] {                            },
                    [15] = new int[] {         3, 4, 5,           },
                    [16] = new int[] {         3,       6,        },
                    [17] = new int[] {         3,       6,        },
                    [18] = new int[] {         3,       6,        },
                    [19] = new int[] {         3, 4, 5,           }
                };
                
                foreach (var entry in sketch)
                {
                    int j = entry.Key;
                    foreach (int i in entry.Value)
                    {
                        new Sprite(i, j, this);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reads, formats, and shows all entries from <c>HighScores.json</c>.
    /// </summary>
    class HighScoresScreen : Panel
    {
        Scores? highScores;

        /// <summary>
        /// A constructor for a <see cref="Panel"/> with all the necessary information,
        /// which is then shown at the main <see cref="canvas"/>.
        /// </summary>
        internal HighScoresScreen() : base()
        {
            ClientSize = new Size(WindowWidth - 2*BorderSize, WindowHeight - 2*BorderSize);
            BackColor = BackgroundColor;

            ReadHighScores();

            DrawText();

            // Add to the canvas.
            canvas.Controls.Add(this);
            BringToFront();
        }

        private void ReadHighScores()
        {
            try
            {
                // Read the file.
                string jsonContent = File.ReadAllText(HighScoresDir);
                // Feed the Scores instance.
                highScores = JsonSerializer.Deserialize<Scores>(jsonContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Failed to read HighScores.json.");
            }
        }

        private void DrawText()
        {
            // Write a title.
            Label title = new()
            {
                Text = "HIGH SCORES",
                Font = new Font("Impact", 9*PixelSize, FontStyle.Regular),
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleCenter

            };
            // Place it at top center.
            title.SetBounds(0, DistanceBlocks, WindowWidth, DistanceBlocks);
            Controls.Add(title);

            // Write the data.
            Font font1 = new Font("Impact", 7*PixelSize, FontStyle.Regular);
            Font font2 = new Font("Inconsolata", 7*PixelSize, FontStyle.Bold);
            Label Snake = new()
            {
                Text = "Snake",
                Font = font1,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label SnakeScore = new()
            {
                Text = string.Format("{0:D7}", highScores?.Snake),
                Font = font2,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleRight
            };
            Label Breakout = new()
            {
                Text = "Breakout",
                Font = font1,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label BreakoutScore = new()
            {
                Text = string.Format("{0:D7}", highScores?.Breakout),
                Font = font2,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleRight
            };
            Label Asteroids = new()
            {
                Text = "Asteroids",
                Font = font1,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label AsteroidsScore = new()
            {
                Text = string.Format("{0:D7}", highScores?.Asteroids),
                Font = font2,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleRight
            };
            Label Tetris = new()
            {
                Text = "Tetris",
                Font = font1,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label TetrisScore = new()
            {
                Text = string.Format("{0:D7}", highScores?.Tetris),
                Font = font2,
                ForeColor = LineColor,
                BackColor = BackgroundColor,
                TextAlign = ContentAlignment.MiddleRight
            };

            // Place the game names at the left size.
            Snake.SetBounds(BorderSize + DistanceBlocks/2 - 2*PixelSize,
                            BorderSize + 4*DistanceBlocks,
                            WindowWidth/2 - DistanceBlocks,
                            DistanceBlocks);
            Breakout.SetBounds(BorderSize + DistanceBlocks/2 - 2*PixelSize,
                               BorderSize + 6*DistanceBlocks,
                               WindowWidth/2 - DistanceBlocks,
                               DistanceBlocks);
            Asteroids.SetBounds(BorderSize + DistanceBlocks/2 - 2*PixelSize,
                                BorderSize + 8*DistanceBlocks,
                                WindowWidth/2 - DistanceBlocks,
                                DistanceBlocks);
            Tetris.SetBounds(BorderSize + DistanceBlocks/2 - 2*PixelSize,
                             BorderSize + 10*DistanceBlocks,
                             WindowWidth/2 - DistanceBlocks,
                             DistanceBlocks);
            
            // Place the scores at the right side.
            SnakeScore.SetBounds(WindowWidth/2 + DistanceBlocks/2 - BorderSize,
                                 BorderSize + 4*DistanceBlocks,
                                 WindowWidth/2 - DistanceBlocks,
                                 DistanceBlocks);
            BreakoutScore.SetBounds(WindowWidth/2 + DistanceBlocks/2 - BorderSize,
                                    BorderSize + 6*DistanceBlocks,
                                    WindowWidth/2 - DistanceBlocks,
                                    DistanceBlocks);
            AsteroidsScore.SetBounds(WindowWidth/2 + DistanceBlocks/2 - BorderSize,
                                     BorderSize + 8*DistanceBlocks,
                                     WindowWidth/2 - DistanceBlocks,
                                     DistanceBlocks);
            TetrisScore.SetBounds(WindowWidth/2 + DistanceBlocks/2 - BorderSize,
                                  BorderSize + 10*DistanceBlocks,
                                  WindowWidth/2 - DistanceBlocks,
                                  DistanceBlocks);
            
            // Add to the canvas.
            Controls.Add(Snake);
            Controls.Add(SnakeScore);
            Controls.Add(Breakout);
            Controls.Add(BreakoutScore);
            Controls.Add(Asteroids);
            Controls.Add(AsteroidsScore);
            Controls.Add(Tetris);
            Controls.Add(TetrisScore);
        }
    }
}
