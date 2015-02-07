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
        public float Scale;

        /// <summary>
        /// Constructs a new Sprite with the given details.
        /// </summary>
        /// <param name="newAsset">the asset filename</param>
        /// <param name="newGlobalPosition">the global position</param>
        /// <param name="newScale">the scale for the sprite</param>
        public Sprite(String newAsset, Vector2 newGlobalPosition, float newScale) {
            Asset = newAsset;
            Color = Color.White;
            GlobalPosition = newGlobalPosition;
            Position = Vector2.Zero;
            Scale = newScale;
        }

        /// <summary>
        /// Loads the given asset into a Texture.
        /// </summary>
        /// <param name="manager"></param>
        public void LoadContent(ContentManager manager)
        {
            Texture = manager.Load<Texture2D>(Asset);
        }

        /// <summary>
        /// Updates the Sprite's position every tick.
        /// </summary>
        /// <param name="viewFrame"></param>
        public void Update(Vector2 viewFrame)
        {
            Position.X = (GlobalPosition.X - viewFrame.X);
            Position.Y = Game1.VIEWPORT_HEIGHT - (GlobalPosition.Y - viewFrame.Y + Texture.Height * Scale);
        }

        /// <summary>
        /// Draws the Sprite.
        /// </summary>
        /// <param name="spriteBatch">the SpriteBatch</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
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
                    return new Rectangle((int)(GlobalPosition.X + 0.5), (int)(GlobalPosition.Y + 0.5),
                        (int)(Texture.Width * Scale + 0.5), (int)(Texture.Height * Scale + 0.5));
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
                GlobalPosition = value - new Vector2(Scale * (Texture.Width / 2), Scale * (Texture.Height / 2));
            }
        }

        /// <summary>
        /// Gets the coordinates of this Sprite's center on the screen's axes.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return Position + new Vector2(Scale * Texture.Width / 2, Scale * Texture.Height / 2);
            }
        }
    }
}
