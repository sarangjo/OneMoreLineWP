using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneMoreLineWP
{
    public class Node : Sprite
    {
        public Node(Vector2 newPosition)
            : base("Graphics\\node", newPosition)
        {

        }

        /// <summary>
        /// Gets the distance from this node to the player.;
        /// </summary>
        public float DistanceFromPlayer(Player p)
        {
            return Vector2.Distance(p.GlobalCenter, GlobalCenter);
        }

        /// <summary>
        /// Gets the dot product of the player's velocity and the vector from this node's center to the player's center.
        /// </summary>
        public float GetDot(Player player)
        {
            return Vector2.Dot(player.LinearUnitVelocity, player.GlobalCenter - GlobalCenter);
        }

        public Vector2 GetPointOfHooking(Player player)
        {
            Vector2 n = GlobalCenter;
            Vector2 p = player.GlobalCenter;
            float x = 0, y = 0;
            if (player.LinearUnitVelocity.Y == 0 || player.LinearUnitVelocity.X == 0)
            {
                // Special Cases
                if (player.LinearUnitVelocity.Y == 0)
                { x = n.X; y = p.Y; }
                else { x = p.X; y = n.Y; }
            }
            else
            {
                float m = player.LinearUnitVelocity.Y / player.LinearUnitVelocity.X;
                x = (n.X / m + m * p.X + n.Y - p.Y) / (m + 1 / m);
                y = m * (x - p.X) + p.Y;
            }
            return new Vector2(x, y);
        }

        /// <summary>
        /// Gets if this node is a valid linkable node for the given player.
        /// </summary>
        public bool IsValid(Player player)
        {
            return (GetDot(player) < Game1.BUFFER) &&
                (DistanceFromPlayer(player) < Player.MAX_DISTANCE);
        }
    }
}
