using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace OneMoreLineWP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public enum GameState
        {
            PLAYING, GAMEOVER
        }
        GameState state = GameState.PLAYING;

        public static int VIEWPORT_HEIGHT;
        public static int VIEWPORT_WIDTH;
        public static int LEFT_BOUNDARY;
        public static int RIGHT_BOUNDARY;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player;
        List<Node> nodes;
        SpriteFont font;

        float viewFrameY;
        bool coll = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
           
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            VIEWPORT_HEIGHT = GraphicsDevice.Viewport.TitleSafeArea.Height;
            VIEWPORT_WIDTH = GraphicsDevice.Viewport.TitleSafeArea.Width;
            LEFT_BOUNDARY = VIEWPORT_WIDTH / 6;
            RIGHT_BOUNDARY = VIEWPORT_WIDTH * 5 / 6;
            // Objects
            player = new Player(new Vector2(VIEWPORT_WIDTH/2 + 200, 50));
            nodes = new List<Node>();
            nodes.Add(new Node(new Vector2(30, 360)));
            nodes.Add(new Node(new Vector2(460, 760)));
            viewFrameY = VIEWPORT_HEIGHT;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            player.LoadContent(this.Content);
            foreach (Node n in nodes)
                n.LoadContent(this.Content);
            font = Content.Load<SpriteFont>("MyFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            if (state == GameState.PLAYING)
            {
                // Update ViewFrameY
                //viewFrameY += player.Velocity.Y * Player.BASE_SPEED;

                // Object updates
                player.Update(viewFrameY);
                foreach (Node n in nodes)
                    n.Update(viewFrameY);

                // User input
                ProcessInput();
                
                if (regionOfHooking != Rectangle.Empty)
                {
                    if (IsAtPointOfHooking())
                    {
                        ScreenHeld1();
                    }
                }
                
                if (player.isCircling)
                {
                    ScreenHeld2();
                }

                // Check collisions
                // 1 - nodes
                /*
                foreach (Node n in nodes)
                    if (player.isCollided(n))
                    {
                        player.isAlive = false;
                    }
                // 2 - boundaries
                if (isCollidedWithBoundaries())
                    player.isAlive = false;*/
            }

            base.Update(gameTime);
        }

        int buffer = 5;

        private bool IsAtPointOfHooking()
        {
            Rectangle recOfHooking = new Rectangle((int)regionOfHooking.X - buffer, (int)regionOfHooking.Y - buffer, buffer * 2, buffer * 2);
            return recOfHooking.Contains(player.Center);
        }

        Node nearestNode = null;
        bool isClockwise = false;
        
        private void ProcessInput()
        {
            // Process touch events
            TouchCollection touchCollection = TouchPanel.GetState();
            if (touchCollection.Count > 0)
            {
                TouchLocation tl = touchCollection[0];
                if (tl.State == TouchLocationState.Pressed)
                {
                }
                else if (tl.State == TouchLocationState.Released)
                {
                    ScreenPressed();
                }
            }
        }

        Rectangle regionOfHooking = Rectangle.Empty;
        private bool prehooked;

        private void ScreenPressed()
        {
            Vector2 pointOfHooking = CalculatePointOfHooking(nodes[1].Center);
            regionOfHooking = new Rectangle((int)pointOfHooking.X - buffer, (int)pointOfHooking.Y - buffer, buffer * 2, buffer * 2);
            prehooked = true;
        }

        private void ScreenHeld1()
        {
            regionOfHooking = Rectangle.Empty;

            // 1) Find nearest node
            nearestNode = nodes[1];// GetNearestNode();

            // Set clockwise
            isClockwise = player.Center.X < nearestNode.Center.X;

            player.isCircling = true;
        }

        private void ScreenHeld2()
        {
            // 2) Calculate radius, start calculating initial velocity
            // 3) Continuously calculate velocity
            SetPlayerVelocity(nearestNode, isClockwise);
        }

        private void ScreenReleased()
        {
            player.isCircling = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">the center of the node</param>
        /// <returns></returns>
        private Vector2 CalculatePointOfHooking(Vector2 n)
        {
            Vector2 p = player.Center;
            float x = 0, y = 0;
            if (player.velocity.X != 0 && player.velocity.Y != 0)
            {
                // General Case
                float m = player.velocity.Y / player.velocity.X;
                x = (n.X / m + m * p.X + n.Y - p.Y) / (m + 1 / m);
                y = m * (x - p.X) + p.Y;
            }
            else
            {
                // Special Cases
                if (player.velocity.Y == 0)
                { x = n.X; y = p.Y; }
                else { x = p.X; y = n.Y; }
            }
            return new Vector2(x, y);
        }

        private void SetPlayerVelocity(Node n, bool isClockwise)
        {
            float distance = n.DistanceFromPlayer(player);
            Vector2 d = player.Center - n.Center;
            Vector2 newVelocity = new Vector2(Math.Abs(d.Y / distance), Math.Abs(d.X / distance));
            if (isClockwise)
            {
                #region Clockwise
                if (d.X > 0)
                {
                    if (d.Y > 0)
                        // player in I
                        newVelocity.Y *= -1;
                    else
                    {    // player in IV
                        newVelocity.X *= -1;
                        newVelocity.Y *= -1;
                    }
                }
                else if(d.Y < 0)
                {
                    // player in III
                    newVelocity.X *= -1;
                }
                #endregion
            }
            else
            {
                #region Counter Clockwise
                if (d.X > 0)
                {
                    if (d.Y > 0)
                        // player in I
                        newVelocity.X *= -1;
                    // player in IV, do nothing
                }
                else if (d.Y > 0)
                {
                    // player in II
                    newVelocity.X *= -1;
                    newVelocity.Y *= -1;
                }
                else
                {
                    // player in III
                    newVelocity.Y *= -1;
                }
                #endregion
            }
            player.velocity = newVelocity;
        }

        /// <summary>
        /// Gets the nearest node.
        /// </summary>
        /// <returns></returns>
        private Node GetNearestNode()
        {
            if (nodes.Count > 1)
            {
                Node n = nodes[0];
                float distance = n.DistanceFromPlayer(player);
                for (int i = 1; i < nodes.Count; i++)
                {
                    if (nodes[i].DistanceFromPlayer(player) < distance)
                    {
                        n = nodes[i];
                        distance = n.DistanceFromPlayer(player);
                    }
                }
                return n;
            }
            else if (nodes.Count == 1)
                return nodes[0];
            else return null;
        }

        /// <summary>
        /// Checks to see if the player has crossed the boundary.
        /// </summary>
        /// <returns></returns>
        private bool isCollidedWithBoundaries()
        {
            return player.GlobalPosition.X < LEFT_BOUNDARY || player.GlobalRectangle.Right > RIGHT_BOUNDARY;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            if (state == GameState.PLAYING)
            {
                // Objects
                player.Draw(spriteBatch);
                foreach (Node n in nodes)
                    n.Draw(spriteBatch);

                Primitives2D.DrawLine(spriteBatch, new Vector2(LEFT_BOUNDARY, 0), new Vector2(LEFT_BOUNDARY, VIEWPORT_HEIGHT), Color.White, 5f);
                Primitives2D.DrawLine(spriteBatch, new Vector2(RIGHT_BOUNDARY, 0), new Vector2(RIGHT_BOUNDARY, VIEWPORT_HEIGHT), Color.White, 5f);

                if (player.isCircling)
                {
                    Primitives2D.DrawCircle(spriteBatch, nearestNode.Position.X + nearestNode.Texture.Width/2,
                        nearestNode.Position.Y + nearestNode.Texture.Height/2,
                        nearestNode.DistanceFromPlayer(player), 360, Color.White);
                }

                if(prehooked)
                {
                    Primitives2D.DrawRectangle(spriteBatch, regionOfHooking, Color.White);
                }
            }
            DrawText();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawText()
        {
            string str = "Player: " + player.ToString();
            str += "\nPOH: " + regionOfHooking;

            spriteBatch.DrawString(font, str, Vector2.Zero, Color.White);
        }
    }
}
