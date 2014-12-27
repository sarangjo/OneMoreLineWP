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
        public static readonly Vector2 BASE_VELOCITY = new Vector2(-.5f, 0.866f);
        public static readonly Vector2 BASE_POSITION = new Vector2(0, 10f);
        public static readonly float BASE_SPEED = 4f;

        public Vector2 velocity;
        public bool isCircling;
        public bool isAlive;
        public int CircleRadius;

        public Player(Vector2 newGPosition)
            : base("Graphics\\player", newGPosition)
        {
            velocity = BASE_VELOCITY;
            velocity.Normalize();
            isAlive = true;
            isCircling = false;
        }

        public void Update(float viewFrameY)
        {
            // Apply Velocity
            GlobalPosition.X += velocity.X * BASE_SPEED;
            GlobalPosition.Y += velocity.Y * BASE_SPEED;

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
            return (GlobalRectangle.Intersects(n.GlobalRectangle));
        }

        public override string ToString()
        {
            string s = "Position: " + Position;
            s += "\nGlobal Position: " + GlobalPosition;
            s += "\nVelocity: " + velocity;
            return s;
        }
    }
}
