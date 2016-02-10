using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;

using VoxelGenerator.UI;
using VoxelGenerator.Dynamic;
using VoxelGenerator.Environment;

namespace VoxelGenerator {
    public enum GameState {
        MainMenu,
        Loading,
        InGame,
        Paused
    }
    public class Main : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        SpriteFont dbFont;
        Texture2D blankTexture;

        Player player;
        World world;

        GameState GameState;

        KeyboardState ks, lastks;
        MouseState ms, lastms;

        int fps;
        int fc; // frame count
        float ft; // frame time

        public Main() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (object sender, EventArgs e) => {
                if (player != null && player.Camera != null)
                    player.Camera.AspectRatio = (float)Window.ClientBounds.Width / Window.ClientBounds.Height;

                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();

                if (world != null) {
                    world.sunRenderTarget = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
                    world.postfxRenderTarget1 = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
                    world.postfxRenderTarget2 = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
                    world.sceneRenderTarget = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
                }
            };

            ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromChildHandle(Window.Handle)).WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }

        protected override void Initialize() {
            base.Initialize();

            #region generate UI
            Screen.Screens.Add("Main Menu", new Screen(true, "Main Menu", new Rectangle(0, 0, 0, 0), true));
            Screen.Screens["Main Menu"].Elements.Add("startbtn", new Button("startbtn", new UDim2(.5f, .5f, 0, 0), 1f, "Start Game", 0, Color.White, Color.Orange, loadGame));
            Screen.Screens["Main Menu"].Elements.Add("exitbtn", new Button("exitbtn", new UDim2(.5f, .5f, 0, 50), 1f, "Exit Game", 0, Color.White, Color.Orange, exitGame));

            Screen.Screens.Add("Loading", new Screen(false, "Loading", new Rectangle(0, 0, 0, 0), true));
            Screen.Screens["Loading"].Elements.Add("bigLabel", new Label("bigLabel", new UDim2(.5f, .5f, 0, 0), 1.25f, "Loading", 0, Color.White));
            Screen.Screens["Loading"].Elements.Add("smallLabel", new Label("smallLabel", new UDim2(.5f, .5f, 0, 60), 1f, "0%", 0, Color.White));
            Screen.Screens["Loading"].Elements.Add("bigBar", new ImageLabel("loadBar", new UDim2(.5f, .5f, -210, 115), 1f, blankTexture, new Vector2(410, 30), 0, Color.Gray));
            Screen.Screens["Loading"].Elements.Add("loadBar", new ImageLabel("loadBar", new UDim2(.5f, .5f, -200, 120), 1f, blankTexture, new Vector2(400, 20), 0, Color.IndianRed));
            #endregion
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            World.debugEffect = Content.Load<Effect>("fx/debug");
            World.terrainEffect = Content.Load<Effect>("fx/world");
            World.postfxEffect = Content.Load<Effect>("fx/postfx");
            World.skyEffect = Content.Load<Effect>("fx/sky");
            World.NightSkybox = Content.Load<TextureCube>("sky/night");
            Billboard.billboardEffect = Content.Load<Effect>("fx/billboard");
            World.SunTexture = Content.Load<Texture2D>("tex/sun");
            World.MoonTexture = Content.Load<Texture2D>("tex/moon");
            Block.BlockTexture = Content.Load<Texture2D>("tex/block");
            UIElement.Fonts = new SpriteFont[] {
                Content.Load<SpriteFont>("font/menufont")
            };
            dbFont = Content.Load<SpriteFont>("font/dbfont");
            blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            blankTexture.SetData<Color>(new Color[] { Color.White });
        }

        internal void exitGame() {
            Exit();
        }

        internal void loadGame() {
            world = new World(5);
            world.sunRenderTarget = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
            world.postfxRenderTarget1 = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
            world.postfxRenderTarget2 = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
            world.sceneRenderTarget = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
            //world.depthRenderTarget = new RenderTarget2D(GraphicsDevice, 4096, 4096, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            player = new Player(world);
            player.Position = new Vector3(0, Chunk.CHUNK_HEIGHT, 0);
            player.Camera = new Camera((float)graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight);
            
            Mouse.SetPosition(Window.ClientBounds.Center.X, Window.ClientBounds.Center.Y);
            IsMouseVisible = false;

            GameState = GameState.InGame;

            Screen.Screens["Main Menu"].Visible = false;
            Screen.Screens["Loading"].Visible = false;
        }

        protected override void UnloadContent() {

        }
        
        protected override void Update(GameTime gameTime) {
            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            Screen.Update(gameTime, new Point(ms.X, ms.Y), this.Window.ClientBounds, ms.LeftButton == ButtonState.Pressed && lastms.LeftButton == ButtonState.Released);

            switch (GameState) {
                case GameState.MainMenu:
                    {
                        IsMouseVisible = true;
                        break;
                    }
                case GameState.Loading:
                    {
                        world.LoadChunks(GraphicsDevice, player.Camera.Position);

                        bool done = true;
                        float dp = 0;
                        foreach (Chunk c in world.Chunks) {
                            if (!c.built)
                                done = false;
                            dp += c.percentDone / world.Chunks.Count;
                        }
                        (Screen.Screens["Loading"].Elements["loadBar"] as ImageLabel).Size = new Vector2(400 * dp, 20);
                        (Screen.Screens["Loading"].Elements["smallLabel"] as Label).Text = (int)(dp * 100) + "%";
                        if (done) {
                            GameState = GameState.InGame;
                            Screen.Screens["Loading"].Visible = false;
                        }

                        break;
                    }
                case GameState.InGame:
                    {
                        #region get mouse delta
                        Vector2 delta = Vector2.Zero;
                        if (!IsMouseVisible) {
                            delta = new Vector2(100 - ms.X, 100 - ms.Y);
                            Mouse.SetPosition(100, 100);
                        }
                        if (ks.IsKeyDown(Keys.LeftAlt) && lastks.IsKeyUp(Keys.LeftAlt)) {
                            IsMouseVisible = !IsMouseVisible;
                            if (IsMouseVisible)
                                Mouse.SetPosition(Window.ClientBounds.Center.X, Window.ClientBounds.Center.Y);
                            Mouse.SetPosition(100, 100);
                        }
                        #endregion

                        #region bind keys
                        if (ks.IsKeyDown(Keys.V) && lastks.IsKeyUp(Keys.V))
                            player.Gravity = !player.Gravity;
                        if (ks.IsKeyDown(Keys.G) && lastks.IsKeyUp(Keys.G))
                            world.DrawGrass = !world.DrawGrass;
                        if (ks.IsKeyDown(Keys.Add) && lastks.IsKeyUp(Keys.Add))
                            world.chunkLoadRadius++;
                        if (ks.IsKeyDown(Keys.Subtract) && lastks.IsKeyUp(Keys.Subtract))
                            world.chunkLoadRadius = Math.Max(world.chunkLoadRadius - 1, 1);
                        if (ks.IsKeyDown(Keys.X) && lastks.IsKeyUp(Keys.X)) {
                            Chunk c = world.ChunkAt(player.Position.X, player.Position.Y, player.Position.Z);
                            if (c != null) {
                                c.setDirty();
                                c.generateLight();
                            }
                        }
                        if (ks.IsKeyDown(Keys.E) && lastks.IsKeyUp(Keys.E))
                            world.SetBlock((int)player.Position.X, (int)player.Position.Y - 1, (int)player.Position.Z, BlockType.Glowy);

                        if (ks.IsKeyDown(Keys.L) && lastks.IsKeyUp(Keys.L))
                            world.Chunks.Clear();
                        #endregion

                        world.Update((float)gameTime.ElapsedGameTime.TotalSeconds * (ks.IsKeyDown(Keys.F2) ? 50f : 1f));

                        player.Update((float)gameTime.ElapsedGameTime.TotalSeconds, delta, ks);
                        
                        world.LoadChunks(GraphicsDevice, player.Camera.Position);
                        break;
                    }
                case GameState.Paused:
                    {
                        break;
                    }
            }

            lastks = ks;
            lastms = ms;
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime) {
            fc++;
            ft += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ft >= 1f) {
                fps = fc;
                fc = 1;
                ft = 0;
            }
            bool debug = false;

            GraphicsDevice.Clear(Color.Black);

            switch (GameState) {
                case GameState.MainMenu:
                    {

                        break;
                    }
                case GameState.Loading:
                    {

                        break;
                    }
                case GameState.InGame:
                    {
                        // debug wire-frame mode
                        GraphicsDevice.RasterizerState = new RasterizerState() { FillMode = ks.IsKeyDown(Keys.F) ? FillMode.WireFrame : FillMode.Solid };

                        debug = ks.IsKeyDown(Keys.F1);
                        world.Render(GraphicsDevice, spriteBatch, player.Camera, debug);
                        
                        break;
                    }
                case GameState.Paused:
                    {
                        break;
                    }
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // UI overlays
            Debug.DrawText(spriteBatch, dbFont);
            
            spriteBatch.DrawString(dbFont, fps + " FPS (" + gameTime.ElapsedGameTime.TotalSeconds + ")", new Vector2(20, Window.ClientBounds.Height - 20), fps < 20 ? Color.Red : Color.Green);
            if (GameState == GameState.InGame) {
                spriteBatch.DrawString(dbFont, (int)world.TimeOfDay + ":" + (int)((world.TimeOfDay - (int)world.TimeOfDay) * 60), new Vector2(300, Window.ClientBounds.Height - 50), Color.White);
                spriteBatch.DrawString(dbFont, "chunk load radius: " + world.chunkLoadRadius + " " + "(" + world.ChunksDrawnLastFrame + " drawn)", new Vector2(300, Window.ClientBounds.Height - 20), Color.White);
                spriteBatch.DrawString(dbFont,
                    "pos : " + (int)player.Position.X + ", " + (int)player.Position.Y + ", " + (int)player.Position.Z
                     + "  (" + (int)(player.Position.X / Chunk.CHUNK_SIZE) + ", " + (int)(player.Position.Y / Chunk.CHUNK_SIZE) + ", " + (int)(player.Position.Z / Chunk.CHUNK_SIZE) + ")"
                    , new Vector2(700, Window.ClientBounds.Height - 20), Color.White);
            }
            Screen.Draw(spriteBatch, Window.ClientBounds);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
