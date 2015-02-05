using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneMoreLineWP
{
    public class CircularSprite : Sprite
    {
        public CircularSprite(String newAsset, Vector2 newGlobalPosition, float newScale)
            : base(newAsset, newGlobalPosition, newScale)
        {
        }

        /// <summary>
        /// The radius of the circular sprite
        /// </summary>
        public float Radius
        {
            get
            {
                try
                {
                    return Texture.Height * Scale / 2;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Checks if this circular sprite collides with the given circular sprite
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsCircularColliding(CircularSprite other)
        {
            float distance = (GlobalCenter - other.GlobalCenter).Length();
            if (distance < Radius + other.Radius)
                return true;
            return false;
        }
    }
}
