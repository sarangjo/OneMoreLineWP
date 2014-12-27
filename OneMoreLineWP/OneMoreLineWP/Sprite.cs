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
            Position.X = GlobalPosition.X;
        }

        public void LoadContent(ContentManager manager)
        {
            Texture = manager.Load<Texture2D>(Asset);
        }

        public void Update(float viewFrameY)
        {
            Position.Y = viewFrameY - GlobalPosition.Y - Texture.Height;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color);
        }

        /// <summary>
        /// Gets the global rectangle for this Sprite.
        /// </summary>
        public Rectangle SpriteRectangle
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

        public Vector2 Center
        {
            get
            {
                return new Vector2(SpriteRectangle.Center.X, SpriteRectangle.Center.Y);
            }
        }
    }
}
