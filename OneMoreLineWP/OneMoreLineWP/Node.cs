using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        // Graphics\node
        public static Texture2D NODE_TEXTURE;

        /// <summary>
        /// Creates a new Node with the given global center.
        /// </summary>
        /// <param name="newGlobalCenter">the global center</param>
        /// <param name="newSize">the size of the node</param>
        public Node(Vector2 newGlobalCenter, float newSize)
            : base("", newGlobalCenter, newSize)
        {
            Texture = NODE_TEXTURE;
        }

        public Node(float x, float y, float newSize)
            : this(new Vector2(x, y), newSize)
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
        /// Distance, dot, non-colliding.
        /// </summary>
        public bool IsValid(Player player)
        {
            return (DistanceFromPlayer(player) < Player.MAX_DISTANCE)
                && (GetDot(player) < OMLGame.GEN_BUFFER)
                && (NonColliding(player));
        }

        /// <summary>
        /// Checks if the player is colliding with this node.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool NonColliding(Player player)
        {
            // Linear velocity
            Vector2 d = (GlobalCenter - player.GlobalCenter);
            d.Normalize();
            Vector2 unitPerpendicular = new Vector2(-d.Y, d.X);
            //float slopeFromAtoB = -d.X / d.Y;
            Vector2 A = GlobalCenter + (Radius + player.Radius + OMLGame.GEN_BUFFER) * unitPerpendicular;
            Vector2 B = GlobalCenter - (Radius + player.Radius + OMLGame.GEN_BUFFER) * unitPerpendicular;
            float alpha = (float)Math.Atan2(A.Y - player.GlobalCenter.Y, A.X - player.GlobalCenter.X);
            float beta = (float)Math.Atan2(B.Y - player.GlobalCenter.Y, B.X - player.GlobalCenter.X);
            float playerAngle = (float)Math.Atan2(player.LinearGlobalUnitVelocity.Y, player.LinearGlobalUnitVelocity.X);
            if (alpha > beta)
            {
                return (playerAngle >= beta && playerAngle <= alpha);
            }
            else
            {
                return (playerAngle >= alpha && playerAngle <= beta);
            }
        }

        public static void LoadNodeTexture(ContentManager manager)
        {
            NODE_TEXTURE = manager.Load<Texture2D>("Graphics\\node");
        }
    }
}
