using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OneMoreLineWP
{
    public class Player : Sprite
    {
        public static readonly Vector2 BASE_VELOCITY = new Vector2(0, 1f);
        public static readonly Vector2 BASE_POSITION = new Vector2(0, 10f);

        public Vector2 Velocity;
        public bool IsCircling;
        public bool isAlive;
        public int CircleRadius;

        public Player(Vector2 newGPosition)
            : base("Graphics\\player", newGPosition)
        {
            Velocity = BASE_VELOCITY;
            isAlive = true;
            IsCircling = false;
        }

        public void Update(float viewFrameY)
        {
            // Apply Velocity
            GlobalPosition.X += Velocity.X;
            GlobalPosition.Y += Velocity.Y;

            Position.X = GlobalPosition.X;
            // Update for view frame
            base.Update(viewFrameY);
        }

        /// <summary>
        /// Checks to see if the given node is colliding with this Player.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool isCollided(Node n)
        {
            // Currently, Player is a square
            return (SpriteRectangle.Intersects(n.SpriteRectangle));
        }
    }
}
