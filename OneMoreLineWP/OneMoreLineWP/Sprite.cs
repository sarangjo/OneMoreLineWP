using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OneMoreLineWP
{
    public class Sprite
    {
        public Texture2D Texture;
        public String Asset;
        public Vector2 Position;
        public Color Color;
        public Vector2 GlobalPosition;

        public Sprite(String newAsset, Vector2 newGlobalPosition) {
            Asset = newAsset;
            Color = Color.White;
            GlobalPosition = newGlobalPosition;
            Position = Vector2.Zero;
        }

        public void LoadContent(ContentManager manager)
        {
            Texture = manager.Load<Texture2D>(Asset);
        }

        public void Update(Vector2 viewFrame)
        {
            Position.X = (GlobalPosition.X - viewFrame.X);
            Position.Y = Game1.VIEWPORT_HEIGHT - (GlobalPosition.Y - viewFrame.Y + Texture.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color);
        }

        /// <summary>
        /// Gets the global rectangle for this Sprite.
        /// </summary>
        public Rectangle GlobalRectangle
        {
            get
            {
                try
                {
                    return new Rectangle((int)GlobalPosition.X, (int)GlobalPosition.Y, (int)Texture.Width, (int)Texture.Height);
                }
                catch (Exception e)
                {
                    return Rectangle.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the coordinates of this Sprite's center on the global axes.
        /// </summary>
        public Vector2 GlobalCenter
        {
            get
            {
                return new Vector2(GlobalRectangle.Center.X, GlobalRectangle.Center.Y);
            }
            set
            {
                GlobalPosition = value - new Vector2(Texture.Width / 2, Texture.Height / 2);
            }
        }

        /// <summary>
        /// Gets the coordinates of this Sprite's center on the screen's axes.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return Position + new Vector2(Texture.Width / 2, Texture.Height / 2);
            }
        }
    }
}
