using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

///
/// Inspired by Sidd Gorti
///
namespace OneMoreLineWP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public enum GameState
        {
            MENU, PLAYING, GAMEOVER
        }
        GameState state = GameState.MENU;
        
        public static readonly int BUFFER = 10;
        public static int VIEWPORT_HEIGHT;
        public static int VIEWPORT_WIDTH;
        public static int LEFT_BOUNDARY;
        public static int RIGHT_BOUNDARY;

        public static Vector2 viewFrame;
        //public static int viewFrameY;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player;
        List<Node> nodes;
        Node nearestNode = null;

        SpriteFont font;

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
            // Set up viewport details
            VIEWPORT_HEIGHT = GraphicsDevice.Viewport.TitleSafeArea.Height;
            VIEWPORT_WIDTH = GraphicsDevice.Viewport.TitleSafeArea.Width;
            LEFT_BOUNDARY = 0;
            RIGHT_BOUNDARY = VIEWPORT_WIDTH;
            int x = 10;
            
            // Objects
            player = new Player(new Vector2(VIEWPORT_WIDTH/2, 50f));
            InitNodes();
            viewFrame = new Vector2(50, 0);// player.GlobalCenter - new Vector2(VIEWPORT_WIDTH / 2, 50f);
            
            //nearestNode = GetNearestNode();
            player.initLinear(Player.BASE_VELOCITY, new TimeSpan(0));

            base.Initialize();
        }

        /// <summary>
        /// Initializes the nodes.
        /// </summary>
        private void InitNodes() 
        {
            nodes = new List<Node>();
            nodes.Add(new Node(new Vector2(0, 660), 1f));
            nodes.Add(new Node(new Vector2(510, 860), 0.5f));
            nodes.Add(new Node(new Vector2(60, 1360), 6f));
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

        TimeSpan pastPlayTime = TimeSpan.Zero;
        TimeSpan currentPlayTime;
        TimeSpan totalPlayTime;
        TimeSpan currentPlayTimeStart;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            switch (state)
            {
                case GameState.PLAYING:
                    // Time
                    currentPlayTime = gameTime.TotalGameTime - currentPlayTimeStart;
                    totalPlayTime = pastPlayTime + currentPlayTime;

                    // Update ViewFrame
                    //viewFrameY += player.Velocity.Y * Player.BASE_SPEED;
                    viewFrame = player.GlobalCenter - new Vector2(VIEWPORT_WIDTH / 2, 100f);

                    // Object updates
                    player.Update(totalPlayTime);
                    foreach (Node n in nodes)
                        n.Update(viewFrame);

                    if (player.hookState == Player.HookState.LINKED)
                    {
                        if (IsAtPointOfHooking())
                            Hook();
                    }

                    #region old
                    /*if (!isStarted)
                {
                    isStarted = true;
                    Start(gameTime.ElapsedGameTime);
                }

                if (isStarted)
                {
                    Rotate(gameTime.TotalGameTime);
                }*/

                    /*switch (player.hookState)
                    {
                        case Player.HookState.LINKED:
                            if (IsAtPointOfHooking())
                                Hook();
                            break;
                        case Player.HookState.HOOKED:
                            Rotate(gameTime.ElapsedGameTime);
                            break;
                    }*/
                    #endregion

                    // Check collisions
                    // 1 - nodes

                    foreach (Node n in nodes)
                        if (player.isCollided(n))
                        {
                            player.isAlive = false;
                        }
                    // 2 - boundaries
                    if (isOutsideBoundaries() && player.hookState == Player.HookState.NOT_LINKED)
                        player.isAlive = false;

                    if (!player.isAlive)
                        EndGame();

                    if (BackPressed())
                        state = GameState.MENU;
                    break;
                case GameState.GAMEOVER:
                case GameState.MENU:
                    if (BackPressed())
                        Exit();
                    break;
            }

            ProcessInput(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Processes touch input.
        /// </summary>
        private void ProcessInput(GameTime gameTime)
        {
            TouchCollection touchCollection = TouchPanel.GetState();
            if (touchCollection.Count > 0)
            {
                TouchLocation tl = touchCollection[0];
                if (tl.State == TouchLocationState.Released)
                {
                    switch (state)
                    {
                        case GameState.PLAYING:
                            // Process touch events
                            switch (player.hookState)
                            {
                                case Player.HookState.NOT_LINKED:
                                    Link();
                                    break;
                                case Player.HookState.HOOKED:
                                    UnLink();
                                    break;
                            }
                            break;
                        case GameState.GAMEOVER:
                            break;
                        case GameState.MENU:
                            state = GameState.PLAYING;
                            currentPlayTimeStart = gameTime.TotalGameTime;
                            break;
                    }
                }
            }
        }

        private void EndGame()
        {
            pastPlayTime = totalPlayTime;
            currentPlayTime = TimeSpan.Zero;
            state = GameState.GAMEOVER;
        }

        /// <summary>
        /// Checks if the back button is pressed.
        /// </summary>
        private bool BackPressed()
        {
            return GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;
        }
     
        /// <summary>
        /// Checks if the player is at the point of hooking.
        /// </summary>
        /// <returns></returns>
        private bool IsAtPointOfHooking()
        {
            //Rectangle recOfHooking = new Rectangle((int)regionOfHooking.X - buffer, (int)regionOfHooking.Y - buffer, buffer * 2, buffer * 2);
            //return recOfHooking.Contains(player.Center);
            float dot = nearestNode.GetDot(player);
            //return Math.Abs(dot) < BUFFER;
            return dot > - BUFFER;
        }

        /// <summary>
        /// Gets the nearest node.
        /// </summary>
        /// <returns></returns>
        private Node GetNearestNode()
        {
            // 2) Get all nodes with dot < 0 and distance < MAX_DISTANCE and hooking in boundaries
            List<Node> possibleNodes = new List<Node>();
            foreach (Node n in nodes)
                if (n.IsValid(player) && IsPointOfHookingInBoundaries(n))
                    possibleNodes.Add(n);

            if (possibleNodes.Count != 0)
                return selectNodeInFront(possibleNodes);
            else
                return selectNearestNode();
        }

        /// <summary>
        /// Checks whether the point of hooking of the given node.
        /// </summary>
        private bool IsPointOfHookingInBoundaries(Node n)
        {
            Vector2 poh = n.GetPointOfHooking(player);
            return !(poh.X > VIEWPORT_WIDTH ||
                 poh.X < 0);
        }

        /// <summary>
        /// Selects a node in front of the player.
        /// </summary>
        private Node selectNodeInFront(List<Node> possibleNodes)
        {
            Node node = null;
            // Get nearest of these
            if (possibleNodes.Count >= 1)
            {
                node = possibleNodes[0];
                float distance = node.DistanceFromPlayer(player);
                for (int i = 1; i < possibleNodes.Count; i++)
                {
                    if (possibleNodes[i].DistanceFromPlayer(player) < distance)
                    {
                        node = possibleNodes[i];
                        distance = node.DistanceFromPlayer(player);
                    }
                }
            }
            // TODO: See if this has a valid point of hooking
            return node;
        }

        /// <summary>
        /// Selects the nearest node, regardless of whether it's in front of or behind the player.
        /// </summary>
        private Node selectNearestNode()
        {
            Node node = null;
            // First, gets the node that is nearest to the player, and within the max distance.
            if (nodes.Count > 1)
            {
                Node n = nodes[0];
                float distance = n.DistanceFromPlayer(player);
                for (int i = 1; i < nodes.Count; i++)
                {
                    if (nodes[i].DistanceFromPlayer(player) < distance)
                    {
                        // This is the new minimum
                        n = nodes[i];
                        distance = n.DistanceFromPlayer(player);
                    }
                }
                if (distance < Player.MAX_DISTANCE)
                    node = n;
            }
            else if (nodes.Count == 1
                && nodes[0].DistanceFromPlayer(player) < Player.MAX_DISTANCE)
                node = nodes[0];

            // Second, confirms that this can be hooked onto legally
            if (node != null)
            {
                if (!IsPointOfHookingInBoundaries(node))
                    node = null;
            }

            return node;
        }

        /// <summary>
        /// Links the player to the nearest node.
        /// </summary>
        private void Link()
        {
            //Vector2 pointOfHooking = CalculatePointOfHooking(nodes[1].Center);
            //regionOfHooking = new Rectangle((int)pointOfHooking.X - buffer, (int)pointOfHooking.Y - buffer, buffer * 2, buffer * 2);

            // 1) Check if it's linking back to the same node
            if (nearestNode != null)
            {
                float newRadius = nearestNode.DistanceFromPlayer(player);
                if (newRadius - player.circularRadius < Player.BUFFER_DISTANCE)
                {
                    player.hookState = Player.HookState.LINKED;
                }
                else
                {
                    // 2) Find nearest node
                    nearestNode = GetNearestNode();
                }
            }
            else
            {
                // 2) Find nearest node
                nearestNode = GetNearestNode();
            }
            if (nearestNode != null)
                player.hookState = Player.HookState.LINKED;
        }

        /// <summary>
        /// Hooks the player to the nearest node.
        /// </summary>
        private void Hook()
        {
            //regionOfHooking = Rectangle.Empty;
            player.hookState = Player.HookState.HOOKED;
            //clockwise = GetIsClockwise();
            //r = nearestNode.DistanceFromPlayer(player);
            player.initCircular(nearestNode.GlobalCenter, totalPlayTime);
        }

        /// <summary>
        /// Unlinks the player.
        /// </summary>
        private void UnLink()
        {
            player.unLink(totalPlayTime);
        }
                
        /*
        bool clockwise;
        float r;
        

        //Rectangle regionOfHooking = Rectangle.Empty;

        bool isAtPoints = false;
        TimeSpan startRotate;

        private void Rotate()
        {
            // 2) Calculate radius, start calculating initial velocity
            // 3) Continuously calculate velocity
            Vector2 d = player.GlobalCenter - nearestNode.GlobalCenter;
            if(Math.Abs(d.X) < buffer)
            {
                if (!isAtPoints)
                {
                    player.GlobalCenter = nearestNode.GlobalCenter + new Vector2(0, ((d.Y > 0) ? r : -r));
                    isAtPoints = true;
                }
            }
            else if (Math.Abs(d.Y) < buffer)
            {
                if (!isAtPoints)
                {
                    player.GlobalCenter = nearestNode.GlobalCenter + new Vector2(((d.X > 0) ? r : -r), 0);
                    isAtPoints = true;
                }
            }
            else
            {
                isAtPoints = false;
            }
            SetPlayerVelocity(nearestNode);
        }

        private void ScreenReleased()
        {
            player.hookState = Player.HookState.NOT_LINKED;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">the center of the node</param>
        /// <returns></returns>
        private Vector2 CalculatePointOfHooking(Vector2 n)
        {
            Vector2 p = player.GlobalCenter;
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

        /// <summary>
        /// Calculates the velocity of the player around the given node.
        /// </summary>
        /// <param name="n">the node to rotate around</param>
        /// <param name="isClockwise">whether the player is going clockwise or not</param>
        private void SetPlayerVelocity(Node n)
        {
            if (player.hookState == Player.HookState.HOOKED)
            {
                Vector2 d = player.GlobalCenter - n.GlobalCenter;
                Vector2 newVelocity = new Vector2(Math.Abs(d.Y / r), Math.Abs(d.X / r));
                if (clockwise)
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
                    else if (d.Y < 0)
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
        }

        /// <summary>
        /// Sees if the rotation is clockwise or not
        /// </summary>
        /// <returns></returns>
        private bool GetIsClockwise()
        {
            Vector2 d = player.GlobalCenter - nearestNode.GlobalCenter;
            if (Math.Abs(d.X) <= buffer) {
                if (player.velocity.X > 0)
                    return d.Y > 0;
                else
                    return d.Y < 0;
            }
            else if (Math.Abs(d.Y) <= buffer)
            {
                if (player.velocity.Y > 0)
                    return d.X < 0;
                else
                    return d.X > 0;
            }
            else if (d.X > 0)
            {
                if (d.Y > 0)
                    return player.velocity.X > 0;
                else
                    return player.velocity.X < 0;
            }
            else
            {
                if (d.Y > 0)
                    return player.velocity.X > 0;
                else
                    return player.velocity.X < 0;
            }
        }

        private void Start(TimeSpan initial)
        {
            // Set Initial Angle
            Vector2 d = player.GlobalCenter - nearestNode.GlobalCenter;
            r = d.Length();
            initialAngle = (float)Math.Atan2((double)d.Y, (double)d.X);
            initialTime = initial;
        }

        private void Rotate(TimeSpan elapsed)
        {
            // Elapsed time
            float speed = 1/10000f;
            float t = (float)((elapsed - initialTime).TotalMilliseconds);
            float x = nearestNode.GlobalCenter.X + r * (float)Math.Cos(speed*t + initialAngle);
            float y = nearestNode.GlobalCenter.Y + r * (float)Math.Sin(speed*t + initialAngle);
            player.GlobalCenter = new Vector2(x, y);
        }
        */

        /// <summary>
        /// Checks to see if the player has crossed the boundary.
        /// </summary>
        /// <returns></returns>
        private bool isOutsideBoundaries()
        {
            return player.GlobalRectangle.Right < LEFT_BOUNDARY
                || player.GlobalPosition.X > RIGHT_BOUNDARY;
        }

        //float initialAngle;
        //TimeSpan initialTime;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            DrawState(spriteBatch);

            if (state == GameState.PLAYING)
            {
                // Objects
                player.Draw(spriteBatch);
                foreach (Node n in nodes)
                    n.Draw(spriteBatch);

                // Boundaries
                Primitives2D.DrawLine(spriteBatch, new Vector2(LEFT_BOUNDARY - viewFrame.X, 0), new Vector2(LEFT_BOUNDARY - viewFrame.X, VIEWPORT_HEIGHT), Color.White, 5f);
                Primitives2D.DrawLine(spriteBatch, new Vector2(RIGHT_BOUNDARY - viewFrame.X, 0), new Vector2(RIGHT_BOUNDARY - viewFrame.X, VIEWPORT_HEIGHT), Color.White, 5f);

                // HookState-specific drawing
                switch (player.hookState)
                {
                    case Player.HookState.HOOKED:
                        Primitives2D.DrawCircle(spriteBatch, nearestNode.Center,
                            nearestNode.DistanceFromPlayer(player), 360, Color.White);
                        Primitives2D.DrawLine(spriteBatch, player.Center, nearestNode.Center, Color.White, 2f);
                        break;
                    case Player.HookState.LINKED:
                        Primitives2D.DrawLine(spriteBatch, player.Center, nearestNode.Center, Color.White, 2f);
                        break;
                }
            }

            DrawDebugText();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the current state of the game.
        /// </summary>
        /// <param name="spriteBatch"></param>
        private void DrawState(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, "State: " + state, Vector2.Zero, Color.White);
        }

        /// <summary>
        /// Draws all the debug text.
        /// </summary>
        private void DrawDebugText()
        {
            string str = "";
            str += "Player Global: " + player.GlobalPosition;
            str += "\nPlayer Position: " + player.Position;
            str += "\nViewframe: " + viewFrame;
            str += "\nPlay time: " + totalPlayTime;
            
            spriteBatch.DrawString(font, str, new Vector2(0, 20), Color.White);
        }
    }
}