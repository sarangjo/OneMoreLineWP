﻿using Microsoft.Xna.Framework;
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
        
        public static readonly int GEN_BUFFER = 10;
        public static readonly float VIEWFRAME_OFFSET_FROM_PLAYER = 200f;

        public static int VIEWPORT_HEIGHT;
        public static int VIEWPORT_WIDTH;
        public static int LEFT_BOUNDARY;
        public static int RIGHT_BOUNDARY;
        public static float L1, L2, R1, R2;

        public static Vector2 viewFrame;
        
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

            r = new Random();

            StartGame();
            base.Initialize();
        }

        private static float ZONE1;
        private static float ZONE2;

        /// <summary>
        /// Initializes the nodes.
        /// </summary>
        private void InitNodes()
        {
            nodes = new List<Node>();

            // "Constants"
            ZONE1 = VIEWPORT_HEIGHT + 250;
            ZONE2 = VIEWPORT_HEIGHT * 2 + 600;
            L1 = LEFT_BOUNDARY + 80;
            L2 = (int)((RIGHT_BOUNDARY - LEFT_BOUNDARY) / 2 - ((player.Texture == null) ? Player.BASE_SIZE : player.Texture.Width) / 2 - GEN_BUFFER * 1.5);
            R1 = (int)((RIGHT_BOUNDARY - LEFT_BOUNDARY) / 2 + ((player.Texture == null) ? Player.BASE_SIZE : player.Texture.Width) / 2 + GEN_BUFFER * 1.5);
            R2 = RIGHT_BOUNDARY - 80;

            addNextNodeAtY = ZONE1;
            /*nodes.Add(new Node(new Vector2(0, 660), 1f));
            nodes.Add(new Node(new Vector2(510, 860), 0.5f));
            nodes.Add(new Node(new Vector2(60, 1360), 6f));
            */
        }

        #region TimeSpans
        TimeSpan pastPlayTime = TimeSpan.Zero;
        TimeSpan currentPlayTime;
        TimeSpan totalPlayTime;
        TimeSpan currentPlayTimeStart;
        #endregion

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
                    // Update time
                    currentPlayTime = gameTime.TotalGameTime - currentPlayTimeStart;
                    totalPlayTime = pastPlayTime + currentPlayTime;

                    // Update ViewFrame
                    viewFrame = player.GlobalCenter - new Vector2(VIEWPORT_WIDTH / 2, VIEWFRAME_OFFSET_FROM_PLAYER);

                    // Updates the nodes
                    UpdateNodes();

                    // Object updates
                    player.Update(totalPlayTime);
                    foreach (Node n in nodes)
                        n.Update(viewFrame);

                    // Moving from LINKED to HOOKED
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
                        if (player.IsCircularColliding(n))
                            player.isAlive = false;

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

        private float currentAddingNodesAtY;
        private float addNextNodeAtY;

        /// <summary>
        /// Gets which zone nodes are being added at currently.
        /// </summary>
        /// <returns></returns>
        private int getUpdateNodeZone()
        {
            if (currentAddingNodesAtY <= ZONE1)
                return 0;
            else if (currentAddingNodesAtY < ZONE2)
                return 1;
            else return 2;
        }

        Random r;

        /// <summary>
        /// Updates the nodes, adding new ones.
        /// </summary>
        private void UpdateNodes()
        {
            // The y value at which new nodes are being added
            currentAddingNodesAtY = viewFrame.Y + VIEWPORT_HEIGHT;
            if (Math.Abs(currentAddingNodesAtY - addNextNodeAtY) < GEN_BUFFER)
            {
                if(AddNode(getUpdateNodeZone()))
                    UpdateAddNextNodeAtY();
            }
        }

        public static float ADD_NEXT_NODE_LOWER = 200;
        public static float ADD_NEXT_NODE_UPPER = 420;

        /// <summary>
        /// Updates where to add the next node.
        /// </summary>
        private void UpdateAddNextNodeAtY()
        {
            addNextNodeAtY += (float)(r.NextDouble() * (ADD_NEXT_NODE_UPPER - ADD_NEXT_NODE_LOWER) + ADD_NEXT_NODE_LOWER);
        }

        /// <summary>
        /// Adds a new node, based on the current zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>whether a new node was added or not</returns>
        private bool AddNode(int zone)
        {
            if (zone > 0)
            {
                double d = r.NextDouble();
                double x = 0;
                switch (zone)
                {
                    case 1:
                        // Zone 1 is where we don't want nodes in the direct path of the user
                        if (d < 0.5)
                            x = L1 + 2 * d * (L2 - L1);
                        else
                            x = R1 + 2 * (d - 0.5) * (R2 - R1);
                        break;
                    case 2:
                        // Zone 2 is the general zone.
                        x = L1 + d * (R2 - R1);
                        break;
                }
                float scale = (float)(r.NextDouble() * (NODE_SCALE_UPPER - NODE_SCALE_LOWER) + NODE_SCALE_LOWER);
                nodes.Add(new Node(new Vector2((float)x, addNextNodeAtY), scale));
                return true;
            }
            return false;
        }

        public static double NODE_SCALE_LOWER = 0.5;
        public static double NODE_SCALE_UPPER = 1.2;


        /// <summary>
        /// Sets up the playfield for the start of the game.
        /// </summary>
        private void StartGame()
        {
            viewFrame = new Vector2(50f, 0);

            // Objects
            player = new Player(new Vector2((VIEWPORT_WIDTH - Player.BASE_SIZE) / 2, 50f));
            InitNodes();

            // Setup motion
            player.initLinear(Player.BASE_VELOCITY, new TimeSpan(0));
        }

        /// <summary>
        /// Ends the game.
        /// </summary>
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
     
        #region Player-Node interaction
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
        /// Checks if the player is at the point of hooking.
        /// </summary>
        /// <returns></returns>
        private bool IsAtPointOfHooking()
        {
            //Rectangle recOfHooking = new Rectangle((int)regionOfHooking.X - buffer, (int)regionOfHooking.Y - buffer, buffer * 2, buffer * 2);
            //return recOfHooking.Contains(player.Center);
            float dot = nearestNode.GetDot(player);
            //return Math.Abs(dot) < BUFFER;
            return dot > -GEN_BUFFER;
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
                if (n.IsValid(player) /*&& IsPointOfHookingInBoundaries(n)*/)
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
            // First, gets the node that is nearest to the player, and within 
            // the max distance.
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
            else if (nodes.Count == 1 &&
                nodes[0].DistanceFromPlayer(player) < Player.MAX_DISTANCE)
                node = nodes[0];

            // Second, confirms that this can be hooked onto legally
            /*if (node != null)
            {
                if (!IsPointOfHookingInBoundaries(node))
                    node = null;
            }*/

            return node;
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
        #endregion
        
        /// <summary>
        /// Checks to see if the player has crossed the boundary.
        /// </summary>
        /// <returns></returns>
        private bool isOutsideBoundaries()
        {
            return player.GlobalRectangle.Right < LEFT_BOUNDARY
                || player.GlobalPosition.X > RIGHT_BOUNDARY;
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
            player.LoadContent(Content);
            Node.LoadNodeTexture(Content);
            /*foreach (Node n in nodes)
                n.LoadContent(Content);
            */font = Content.Load<SpriteFont>("MyFont");
        }

        #region Drawing
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
        #endregion
    }
}