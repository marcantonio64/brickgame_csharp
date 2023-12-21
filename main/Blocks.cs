using System.Collections.Generic;
using System.Drawing;
using static BrickGame.Constants;

namespace BrickGame
{
    /// <summary>
    /// Establishes the structure, appearance, and behavior of unit cells.
    /// </summary>
    internal class Block
    {
        internal Point Coordinates { get; set; }
        internal Direction direction { get; set; }

        /// <summary>
        /// A constructor with a <see cref="Point"/> of coordinates.
        /// </summary>
        /// <param name="coordinates">A pair of coordinates. Must range from <c>(0, 0)</c> (inclusive)
        /// to <c>(9, 19)</c> (inclusive) to show on the screen.</param>
        internal Block(Point coordinates)
        {
            Coordinates = coordinates;
            direction = Direction.Null;
            Show();
        }

        /// <summary>
        /// A constructor with raw coordinates.
        /// </summary>
        /// <param name="i">Horizontal position. Must be from 0 (inclusive) to 10 (exclusive)
        /// to show on the screen.</param>
        /// <param name="j">Vertical position. Must be from 0 (inclusive) to 20 (exclusive)
        /// to show on the screen.</param>
        internal Block(int i, int j) : this(new Point(i, j)) {}

        private bool IsOnScreen()
        {
        (int i, int j) = (Coordinates.X, Coordinates.Y);
        return 0 <= i && i < 10 && 0 <= j && j < 20;
        }
        
        /// <summary>
        /// Selects the <see cref="Sprite"/> at the current <see cref="Coordinates"/>.
        /// </summary>
        internal Sprite GetSprite()
        {
            return InteractiveClient.SpriteDict[Coordinates];
        }
        internal void Show(bool force)
        {
            if (IsOnScreen())
            {
                GetSprite().Raise(force);
            }
        }
            
        internal void Show()
        {
            Show(false);
        }

        internal void Hide()
        {
            if (IsOnScreen())
            {
                GetSprite().Lower();
            }
        }
        internal void Blink(int t)
        {
            if (IsOnScreen())
            {
                GetSprite().Blink(t);
            }
        }
        
        /// <summary>
        /// Changes the <see cref="Coordinates"/> and the <see cref="Sprite"/>
        /// according to the given <see cref="Point"/> position.
        /// </summary>
        internal void MoveTo(Point coordinates)
        {
            Hide();
            Coordinates = coordinates;
            Show();
        }

        /// <summary>
        /// Changes the <see cref="Coordinates"/> and the <see cref="Sprite"/>
        /// according to the given position.
        /// </summary>
        internal void MoveTo(int i, int j)
        {
            MoveTo(new Point(i, j));
        }

        /// <summary>
        /// Changes the <see cref="Coordinates"/> and the <see cref="Sprite"/>
        /// according to <see cref="direction"/>.
        /// </summary>
        internal virtual void Move()
        {
            Point p = Coordinates;
            p.Offset(ConvertDirection[direction]);
            MoveTo(p);
        }
    }

    /// <summary>
    /// A <see cref="Block"/> with a name identifier.
    /// </summary>    
    internal class BlinkingBlock : Block
    {
        internal BlinkingBlock(Point coordinates) : base(coordinates) {}
        internal BlinkingBlock(int i, int j) : base(i, j) {}
    }

    /// <summary>
    /// A <see cref="Block"/> with a name identifier.
    /// </summary>
    internal class HiddenBlock : Block
    {
        internal HiddenBlock(Point coordinates) : base(coordinates) {}
        internal HiddenBlock(int i, int j) : base(i, j){}
    }

    /// <summary>
    /// Used to destroy a target upon collision.
    /// </summary>
    /// <para>The bombs are built as an <c>List</c> of <see cref="Block"/>
    /// objects, forming an X shape, akin to that of a sea mine.</para>
    internal class Bomb
    {
        private static List<List<Block>> bombs = new();
        private List<Block>? group;

        /// <summary>
        /// A constructor with raw coordinates where the required <see cref="Block"/>,
        /// <see cref="BlinkingBlock"/>, and <see cref="HiddenBlock"/> objects are created
        /// and organized in the given group.
        /// </summary>
        /// <param name="i">Horizontal position. Must be from 0 (inclusive) to 10 (exclusive)
        /// to show on the screen.</param>
        /// <param name="j">Vertical position. Must be from 0 (inclusive) to 20 (exclusive)
        /// to show on the screen.</param>
        /// <param name="group">A <c>List</c> containing the <see cref="Bomb"/>s elements.</param>
        internal Bomb(int i, int j, List<Block> group)
        {
            this.group = group;
            List<Block> bomb = new()
            {
                new BlinkingBlock(i, j),    // 4 outer corners.
                new BlinkingBlock(i, j+3),
                new BlinkingBlock(i+3, j),
                new BlinkingBlock(i+3, j+3),
                new Block(i+1, j+1),        // 4 core Blocks.
                new Block(i+1, j+2),
                new Block(i+2, j+1),
                new Block(i+2, j+2),
                new HiddenBlock(i, j+1),  // Filling spaces.
                new HiddenBlock(i, j+2),
                new HiddenBlock(i+3, j+1),
                new HiddenBlock(i+3, j+2),
                new HiddenBlock(i+1, j),
                new HiddenBlock(i+2, j),
                new HiddenBlock(i+1, j+3),
                new HiddenBlock(i+2, j+3)
            };
            foreach (Block block in bomb)
            {
                this.group.Add(block);  // Add bomb's components to group for drawing.
                if (block is HiddenBlock)
                {
                    block.Hide();
                }
            }
            bombs.Add(bomb);  // Add bomb itself to bombs.
        }

        /// <summary>
        /// A constructor with a <see cref="Point"/> of coordinates, where the required
        /// <see cref="Block"/>, <see cref="BlinkingBlock"/>, and <see cref="HiddenBlock"/>
        /// objects are created and organized in the given group.
        /// </summary>
        /// <param name="coordinates">A pair of coordinates. Must range from <c>(0, 0)</c> (inclusive)
        /// to <c>(9, 19)</c> (inclusive) to show on the screen.</param>
        /// <param name="group">A <c>List</c> containing the <see cref="Bomb"/>s elements.</param>
        internal Bomb(Point coordinates, List<Block> group) : this(coordinates.X, coordinates.Y, group) {}

        /// <summary>
        /// An empty <see cref="Bomb"/> object.
        /// </summary>
        internal Bomb() {}

        internal void Move(Direction direction)
        {
            Point position;
            for (int i = bombs.Count - 1; i >= 0; i--)
            {
                List<Block> bomb = bombs[i];
                // Move each component using its MoveTo() method.
                foreach (Block block in bomb)
                {
                    position = ConvertDirection[direction];
                    position.Offset(block.Coordinates);
                    block.MoveTo(position);
                    if (block is HiddenBlock)
                    {
                        block.Hide();
                    }
                }
                foreach (Block block in bomb)
                {
                    if (block is not BlinkingBlock && block is not HiddenBlock)
                    {
                        block.Show();
                    }
                }

                // Delete a Bomb when it exists the grid.
                int k = bomb[0].Coordinates.Y;
                if (direction == Direction.Up && k < 0 || direction == Direction.Down && k >= 17)
                {
                    foreach (Block block in bomb)
                    {
                        // Erase the drawings.
                        block.Hide();
                        group?.Remove(block);
                    }
                    // Remove references.
                    bombs.RemoveAt(i);
                }
            }
        }

        internal bool CheckExplosion(List<Block> target)
        {
            bool anyErased = false;
            // Iterate through bombs.
            for (int index = bombs.Count - 1; index >= 0; index--)
            {
                List<Block> bomb = bombs[index];
                Point bo = bomb[0].Coordinates;
                bool erase = false;
                // Detect explosion.
                foreach (Block block in target)
                {
                    Point bk = block.Coordinates;
                    if ((bo.X <= bk.X && bk.X <= bo.X + 3) && (bo.Y <= bk.Y && bk.Y <= bo.Y + 3))
                    {
                        // Stop if Bomb hits any component of target.
                        erase = true;
                        anyErased = true;
                        break;
                    }
                }
                if (erase)
                {
                    Explode(bomb, target);
                    bombs.RemoveAt(index);
                }
            }
            return anyErased;
        }

        internal void Explode(List<Block> bomb, List<Block> target)
        {
            Point bo = bomb[0].Coordinates;
            // Destroy the target.
            for (int i = target.Count - 1; i >= 0; i--)
            {
                Block block = target[i];
                Point bk = block.Coordinates;
                // Blast range of 2 cells from the Bomb's edges.
                if ((bo.X - 2 <= bk.X && bk.X <= bo.X + 5) && (bo.Y - 2 <= bk.Y && bk.Y <= bo.Y + 5))
                {
                    block.Hide();
                    target.RemoveAt(i);
                }
            }
            // Destroy the Bomb.
            foreach (Block block in bomb)
            {
                block.Hide();
                group?.Remove(block);
            }
        }

        /// <summary>
        /// Destroys the targets hit by the last <see cref="Bomb"/> in the <see cref="bombs"/> static list.
        /// </summary>
        /// <param name="target">List of <see cref="Block"/>s and <see cref="BlinkingBlock"/>s to be destroyed.</param>
        internal void Explode(List<Block> target)
        {
            Explode(bombs[^1], target);
            bombs.Remove(bombs[^1]);
        }
    }
}
