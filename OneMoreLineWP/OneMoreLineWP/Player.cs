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
        public enum HookState
        {
            NOT_LINKED, LINKED, HOOKED
        }

        public static readonly Vector2 BASE_VELOCITY = new Vector2(0, 1);
        public static readonly Vector2 BASE_POSITION = new Vector2(0, 10f);
        public static readonly float BASE_SPEED = 8f;

        //public Vector2 velocity;
        public bool isAlive;
        public HookState hookState;
        
        public Player(Vector2 newGPosition)
            : base("Graphics\\player", newGPosition)
        {
            isAlive = true;
            hookState = HookState.NOT_LINKED;
            Speed = 1 / 10f;
        }

        public void Update(float viewFrameY, TimeSpan total)
        {
            // Apply Velocity
            //GlobalPosition.X += velocity.X * BASE_SPEED;
            //GlobalPosition.Y += velocity.Y * BASE_SPEED;

            if (hookState == HookState.HOOKED)
                updateCircular(total);
            else
                updateLinear(total);

            Position.X = GlobalPosition.X;
            // Update for view frame
            base.Update(viewFrameY);
        }

        public float Speed { get; set; }
        private TimeSpan initTime;

        private Vector2 linearStart;
        public Vector2 LinearUnitVelocity { get; set; }

        public void initLinear(Vector2 vel, TimeSpan initial)
        {
            linearStart = GlobalPosition;
            LinearUnitVelocity = vel;
            LinearUnitVelocity.Normalize();
            initTime = initial;
        }

        public void updateLinear(TimeSpan totalTime)
        {
            float t = (float)((totalTime - initTime).TotalMilliseconds);
            GlobalPosition = linearStart + LinearUnitVelocity * Speed * t;
        }

        private Vector2 circularCenter;
        private float circularRadius;
        private float initialAngle;
        private int isClockwise;

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
        private bool GetIsClockwise()
        {
            Vector2 d = GlobalCenter - circularCenter;
            if (Math.Abs(d.X) <= Game1.BUFFER)
            {
                if (LinearUnitVelocity.X > 0)
                    return d.Y > 0;
                else
                    return d.Y < 0;
            }
            else if (Math.Abs(d.Y) <= Game1.BUFFER)
            {
                if (LinearUnitVelocity.Y > 0)
                    return d.X < 0;
                else
                    return d.X > 0;
            }
            else if (d.X > 0)
            {
                if (d.Y > 0)
                    return LinearUnitVelocity.X > 0;
                else
                    return LinearUnitVelocity.X < 0;
            }
            else
            {
                if (d.Y > 0)
                    return LinearUnitVelocity.X > 0;
                else
                    return LinearUnitVelocity.X < 0;
            }
        }

        float t;

        public void updateCircular(TimeSpan totalTime)
        {
            t = (float)((totalTime - initTime).TotalMilliseconds);
            float x = circularCenter.X + circularRadius * (float)Math.Cos(isClockwise * Speed / circularRadius * t + initialAngle);
            float y = circularCenter.Y + circularRadius * (float)Math.Sin(isClockwise * Speed / circularRadius * t + initialAngle);
            GlobalCenter = new Vector2(x, y);
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

        /// <summary>
        /// Unlinks the player.
        /// </summary>
        /// <param name="total">the total time elapsed</param>
        public void unLink(TimeSpan total)
        {
            hookState = HookState.NOT_LINKED;

            float x = -(float)Math.Sin(isClockwise * Speed / circularRadius * t + initialAngle) * isClockwise;
            float y = (float)Math.Cos(isClockwise * Speed / circularRadius * t + initialAngle) * isClockwise;
            initLinear(new Vector2(x, y), total);
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
