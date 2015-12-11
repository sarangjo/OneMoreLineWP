using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OneMoreLineWP
{
    public class Player : CircularSprite
    {
        public enum HookState
        {
            NOT_LINKED, LINKED, HOOKED
        }

        #region Constants
        public static readonly float BASE_SIZE = 40;
        public static readonly Vector2 BASE_VELOCITY = new Vector2(0, 1);
        public static float BUFFER_DISTANCE = 30f;
        public static float MAX_DISTANCE = 400f;
        public float SPEED = 0.49f;
        #endregion

        private TimeSpan initTime;
        //public Vector2 velocity;
        public bool isAlive;
        public HookState hookState;
        private float t;

        public List<Vector2> playerTail;
        
        public Player(Vector2 newGPosition)
            : base("Graphics\\player", newGPosition, 1f)
        {
            isAlive = true;
            hookState = HookState.NOT_LINKED;
            playerTail = new List<Vector2>();
        }

        /// <summary>
        /// Updates the player depending on the hook state.
        /// </summary>
        /// <param name="total">The total game time</param>
        public void Update(TimeSpan total)
        {
            // Apply Velocity
            //GlobalPosition.X += velocity.X * BASE_SPEED;
            //GlobalPosition.Y += velocity.Y * BASE_SPEED;

            if (hookState == HookState.HOOKED)
                updateCircular(total);
            else
                updateLinear(total);

            playerTail.Add(GlobalCenter);

            // Update for view frame
            base.Update(OMLGame.viewFrame);
        }

        #region Linear Movement
        private Vector2 linearStart;
        public Vector2 LinearGlobalUnitVelocity { get; set; }

        public void initLinear(Vector2 vel, TimeSpan initial)
        {
            linearStart = GlobalPosition;
            LinearGlobalUnitVelocity = vel;
            LinearGlobalUnitVelocity.Normalize();
            initTime = initial;
        }

        public void updateLinear(TimeSpan totalTime)
        {
            float t = (float)((totalTime - initTime).TotalMilliseconds);
            GlobalPosition = linearStart + LinearGlobalUnitVelocity * SPEED * t;
        }
        #endregion

        #region Circular Movement
        private Vector2 circularCenter;
        public float circularRadius;
        private float initialAngle;
        private int isClockwise;

        /// <summary>
        /// Sets up the circular motion.
        /// </summary>
        /// <param name="center">the global center of the node</param>
        /// <param name="initial">the initial time</param>
        public void initCircular(Vector2 center, TimeSpan initial)
        {
            circularCenter = center;
            Vector2 d = GlobalCenter - circularCenter;
            circularRadius = d.Length();
            // Set Initial Angle, Time, and clockwise
            initialAngle = (float)Math.Atan2((double)d.Y, (double)d.X);
            initTime = initial;
            isClockwise = (GetIsClockwise() ? -1 : 1);
        }

        /// <summary>
        /// Sees if the rotation is clockwise or not
        /// </summary>
        /// <returns></returns>
        private bool GetIsClockwiseOld()
        {
            Vector2 d = GlobalCenter - circularCenter;
            if (Math.Abs(d.X) <= OMLGame.GEN_BUFFER)
            {
                if (LinearGlobalUnitVelocity.X > 0)
                    return d.Y > 0;
                else
                    return d.Y < 0;
            }
            else if (Math.Abs(d.Y) <= OMLGame.GEN_BUFFER)
            {
                if (LinearGlobalUnitVelocity.Y > 0)
                    return d.X < 0;
                else
                    return d.X > 0;
            }
            else if (d.X > 0)
            {
                if (d.Y > 0)
                    return LinearGlobalUnitVelocity.X > 0;
                else
                    return LinearGlobalUnitVelocity.X < 0;
            }
            else
            {
                if (d.Y > 0)
                    return LinearGlobalUnitVelocity.X > 0;
                else
                    return LinearGlobalUnitVelocity.X < 0;
            }
        }

        /// <summary>
        /// Gets whether the rotation in the circular motion is clockwise.
        /// </summary>
        /// <returns></returns>
        private bool GetIsClockwise()
        {
            // Radius of the circle around which to rotate the player's velocity vector
            float fauxCircleRadius = Vector2.Distance(circularCenter, GlobalCenter);
            // Angles of adjustment
            // Theta is how much the velocity vector centered at the node needs to be adjusted
            float theta = -(float)Math.Atan2((double)LinearGlobalUnitVelocity.Y, (double)LinearGlobalUnitVelocity.X);
            // Phi is the current coordinate of the player in the circle centered at the node: (phi, fauxCircleRadius)
            float phi = (float)Math.Atan2((GlobalCenter-circularCenter).Y, (GlobalCenter-circularCenter).X);
            // The new adjusted position of the player
            Vector2 s = circularCenter + new Vector2(fauxCircleRadius * (float)Math.Cos(phi + theta), fauxCircleRadius * (float)Math.Sin(phi + theta));

            if (s.Y - circularCenter.Y <= 0)
                // "to the right of the node"
                return false;
            else
                // "to the left of the node"
                return true;
        }


        /// <summary>
        /// Updates this Player's position in circular motion.
        /// </summary>
        /// <param name="totalTime"></param>
        public void updateCircular(TimeSpan totalTime)
        {
            t = (float)((totalTime - initTime).TotalMilliseconds);
            float x = circularCenter.X + circularRadius * (float)Math.Cos(isClockwise * SPEED / circularRadius * t + initialAngle);
            float y = circularCenter.Y + circularRadius * (float)Math.Sin(isClockwise * SPEED / circularRadius * t + initialAngle);
            GlobalCenter = new Vector2(x, y);
        }
        #endregion

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

        /// <summary>
        /// Unlinks the player.
        /// </summary>
        /// <param name="total">the total time elapsed</param>
        public void unLink(TimeSpan total)
        {
            hookState = HookState.NOT_LINKED;
            // Calculated using the derivative (s/o to Ms. Iliescu)
            float x = -(float)Math.Sin(isClockwise * SPEED / circularRadius * t + initialAngle) * isClockwise;
            float y = (float)Math.Cos(isClockwise * SPEED / circularRadius * t + initialAngle) * isClockwise;
            initLinear(new Vector2(x, y), total);
        }

        /// <summary>
        /// Draws the player.
        /// </summary>
        /// <param name="spriteBatch">the current spritebatch</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            List<Vector2> playerTailDraw = GetPlayerDrawTail();
            Primitives2D.DrawPoints(spriteBatch, Vector2.Zero, playerTailDraw, Color.White, 2f);
        }

        /// <summary>
        /// Converts the player tail to actual positions to be drawn on the screen.
        /// </summary>
        /// <returns></returns>
        private List<Vector2> GetPlayerDrawTail()
        {
            List<Vector2> drawTail = new List<Vector2>();
            foreach (Vector2 v in playerTail)
            {
                drawTail.Add(new Vector2(v.X - OMLGame.viewFrame.X, OMLGame.VIEWPORT_HEIGHT - (v.Y - OMLGame.viewFrame.Y)));
            }
            return drawTail;
        }

        /// <summary>
        /// Gets the Position and Global Position of this player.
        /// </summary>
        public override string ToString()
        {
            string s = "Position: " + Position;
            s += "\nGlobal Position: " + GlobalPosition;
            return s;
        }
    }
}
