using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneMoreLineWP
{
    public class Node : CircularSprite
    {
        public static readonly float BASE_SIZE = 50f;

        /// <summary>
        /// Creates a new Node with the given global center.
        /// </summary>
        /// <param name="newGlobalCenter">the global center</param>
        /// <param name="newSize">the size of the node</param>
        public Node(Vector2 newGlobalCenter, float newSize)
            : base("Graphics\\node", newGlobalCenter, newSize)
        {

        }

        public Node(float x, float y, float newSize)
            : this(new Vector2(x, y), newSize)
        { }

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
            return Vector2.Dot(player.LinearGlobalUnitVelocity, player.GlobalCenter - GlobalCenter);
        }

        /// <summary>
        /// Given a player, gets the point where the player would hook onto this node.
        /// </summary>
        public Vector2 GetPointOfHooking(Player player)
        {
            Vector2 n = GlobalCenter;
            Vector2 p = player.GlobalCenter;
            float x = 0, y = 0;
            if (player.LinearGlobalUnitVelocity.Y == 0 || player.LinearGlobalUnitVelocity.X == 0)
            {
                // Special Cases
                if (player.LinearGlobalUnitVelocity.Y == 0)
                { x = n.X; y = p.Y; }
                else { x = p.X; y = n.Y; }
            }
            else
            {
                float m = player.LinearGlobalUnitVelocity.Y / player.LinearGlobalUnitVelocity.X;
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
