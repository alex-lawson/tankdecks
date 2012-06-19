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

namespace TankDecks
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameLogic : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        SpriteFont debugFont;
        KeyboardState oldKS;
        MouseState oldMS;
        int frameCounter, fps, colcount;
        TimeSpan elapsedTime;
        Random random;
        public static HitEffects hiteffects;
        public static TickEffects tickeffects;
        public static bool edgebounce;

        public static Rectangle boundbox, cullbox;

        public static List<DrawingObject> drawshots;
        public static List<Projectile> projectiles, projectilequeue, particles;

        public static Player player;

        public static List<Enemy> enemies;

        public static List<Collidable> collidables;

        public static Attack blankattack, testatk1, testatk2;

        public GameLogic()
        {
            graphics = new GraphicsDeviceManager(this);

            this.Window.AllowUserResizing = false;
            this.Window.ClientSizeChanged += new System.EventHandler<System.EventArgs>(Window_ClientSizeChanged);
            InitGraphicsMode(1200, 900, false);

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
            oldKS = Keyboard.GetState();
            oldMS = Mouse.GetState();

            hiteffects = new HitEffects();
            tickeffects = new TickEffects();

            boundbox = GraphicsDevice.Viewport.Bounds;
            boundbox.Inflate(-30, -30);

            cullbox = GraphicsDevice.Viewport.Bounds;
            cullbox.Inflate(100, 100);

            drawshots = new List<DrawingObject>();
            particles = new List<Projectile>();
            projectiles = new List<Projectile>();
            projectilequeue = new List<Projectile>();
            enemies = new List<Enemy>();
            collidables = new List<Collidable>();

            blankattack = new Attack();

            testatk1 = new Attack();
            testatk1.cooldown = 20;
            testatk1.speed = 15;
            testatk1.drawshot = 6;
            testatk1.inaccuracy = 0.05f;
            testatk1.duration = 100;
            testatk1.projcount = 1;
            testatk1.spread = 1f;

            //testatk1.handletick += tickeffects.seek;
            //testatk1.handleexpire += hiteffects.burst;
            testatk1.handlecollide += hiteffects.bounce;
            testatk1.handlecollide += hiteffects.burst;

            testatk2 = new Attack();
            testatk2.drawshot = 0;
            testatk2.cooldown = 10;
            testatk2.projcount = 3;
            testatk2.spread = 0.2f;
            testatk2.speed = 15;
            testatk2.inaccuracy = 0.2f;
            testatk2.duration = 60;

            testatk2.handleexpire += hiteffects.burst;
            testatk2.handletick += tickeffects.slow;
            testatk2.handlecollide += hiteffects.fizzle;
            testatk2.handlecollide += hiteffects.burst;

            edgebounce = false;
            
            base.IsMouseVisible = true;
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

            player = new Player(new TankBase(new Vector2(600, 450)), new DrawingObject(Content.Load<Texture2D>("tankbase")), new DrawingObject(Content.Load<Texture2D>("tankturret")));

            enemies.Add(new Enemy(new PhysicsObject(new Vector2(200, 200)), new DrawingObject(Content.Load<Texture2D>("tankbase"))));
            enemies.Add(new Enemy(new PhysicsObject(new Vector2(400, 300)), new DrawingObject(Content.Load<Texture2D>("tankbase"))));
            enemies.Add(new Enemy(new PhysicsObject(new Vector2(300, 400)), new DrawingObject(Content.Load<Texture2D>("tankbase"))));
            enemies.Add(new Enemy(new PhysicsObject(new Vector2(100, 500)), new DrawingObject(Content.Load<Texture2D>("tankbase"))));

            collidables.Add(new Collidable(new PhysicsObject(new Vector2(1000, 300)), new DrawingObject(Content.Load<Texture2D>("coll_round1")), CollideMode.circle));
            for (int i = 300; i < 1200; i += 300)
            {
                collidables.Add(new Collidable(new PhysicsObject(new Vector2(i, 700)), new DrawingObject(Content.Load<Texture2D>("coll_square1")), CollideMode.box));
            }
            
            drawshots.Add(new DrawingObject(Content.Load<Texture2D>("shot1")));
            drawshots.Add(new DrawingObject(Content.Load<Texture2D>("shot2")));
            drawshots.Add(new DrawingObject(Content.Load<Texture2D>("spinner1")));
            drawshots.Add(new DrawingObject(Content.Load<Texture2D>("beam1")));
            drawshots.Add(new DrawingObject(Content.Load<Texture2D>("flame1")));
            drawshots[4].col = new Color(1, 1, 1, 0.1f);
            drawshots.Add(new DrawingObject(Content.Load<Texture2D>("lilwave1")));
            drawshots.Add(new DrawingObject(Content.Load<Texture2D>("lilmissile1")));

            debugFont = Content.Load<SpriteFont>("font_atari10s");
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
            //handle keyboard and mouse input
            UpdateInput(gameTime);

            //update mobs
            player.tick();
            foreach (Enemy enemy in enemies)
            {
                enemy.tick();
            }

            //update attacks (temporary)
            testatk1.tick();
            testatk2.tick();

            //add projectiles from the queue
            projectiles.AddRange(projectilequeue);
            projectilequeue.Clear();

            //update projectiles
            foreach (Projectile pro in projectiles)
            {
                pro.tick();
            }
            foreach (Projectile pro in particles)
            {
                pro.tick();
            }

            //do collisions
            colcount = DoCollisions();

            //clamp player
            if (!boundbox.Contains(player.physobj.ppos()))
            {
                //player.physobj.speed = 0;
                player.physobj.pos = Vector2.Clamp(player.physobj.pos, new Vector2(boundbox.Left, boundbox.Top), new Vector2(boundbox.Right, boundbox.Bottom));
                player.physobj.stop();
            }

            //cull projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (projectiles[i].duration <= 0)
                {
                    projectiles.RemoveAt(i);
                }
                else if (projectiles[i].physobj.pos.X < 0 || projectiles[i].physobj.pos.X > GraphicsDevice.Viewport.Bounds.Width)
                {
                    if (edgebounce) {
                        projectiles[i].physobj.vel = Vector2.Reflect(projectiles[i].physobj.vel, new Vector2(1, 0));
                        projectiles[i].angle = Physics.VectorToAngle(projectiles[i].physobj.vel);
                    } else {
                        projectiles.RemoveAt(i);
                    }
                }
                else if (projectiles[i].physobj.pos.Y < 0 || projectiles[i].physobj.pos.Y > GraphicsDevice.Viewport.Bounds.Height)
                {
                    if (edgebounce) {
                        projectiles[i].physobj.vel = Vector2.Reflect(projectiles[i].physobj.vel, new Vector2(0, 1));
                        projectiles[i].angle = Physics.VectorToAngle(projectiles[i].physobj.vel);
                    } else {
                        projectiles.RemoveAt(i);
                    }
                }
                //else if (!cullbox.Contains(new Point((int)projectiles[i].physobj.pos.X, (int)projectiles[i].physobj.pos.Y)))
                //{

                //}
            }

            //cull particles
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                if (particles[i].duration <= 0)
                {
                    particles.RemoveAt(i);
                }
            }

            //fps calculations
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                fps = frameCounter;
                frameCounter = 0;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Handles keyboard and mouse input.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void UpdateInput(GameTime gameTime)
        {
            KeyboardState newKS = Keyboard.GetState();
            MouseState newMS = Mouse.GetState();

            Point mPoint = new Point(newMS.X, newMS.Y);
            Vector2 mVec = new Vector2(newMS.X, newMS.Y);

            // Allows the game to exit (but does it do anything on PC?)
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (GraphicsDevice.Viewport.Bounds.Contains(mPoint))
            {
                Vector2 aimvec = Vector2.Subtract(mVec, player.physobj.pos);
                player.turangle = Physics.VectorToAngle(aimvec);
                if (newMS.LeftButton == ButtonState.Pressed)
                {
                    testatk1.fire(projectiles, Vector2.Add(player.physobj.pos, player.barrel()), mVec);
                    if (oldMS.LeftButton == ButtonState.Released)
                    {
                        
                    }
                }
                if (newMS.RightButton == ButtonState.Pressed)
                {
                    testatk2.fire(projectiles, Vector2.Add(player.physobj.pos, player.barrel()), mVec);
                    //player.physobj.moveto(new Vector2(newMS.X, newMS.Y));
                }
            }

            int movex = 0;
            int movey = 0;

            if (newKS.IsKeyDown(Keys.W))
            {
                movey-=100;
            }
            if (newKS.IsKeyDown(Keys.A))
            {
                movex-=100;
            }
            if (newKS.IsKeyDown(Keys.S))
            {
                movey+=100;
            }
            if (newKS.IsKeyDown(Keys.D))
            {
                movex+=100;
            }

            if (movex != 0 || movey != 0) player.physobj.moveto(new Vector2(player.physobj.pos.X + movex, player.physobj.pos.Y + movey));

            // Update keyboard and mouse state for comparison
            oldKS = newKS;
            oldMS = newMS;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGreen);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            
            //draw player
            player.draw();

            //draw enemies
            foreach (Enemy enemy in enemies)
            {
                enemy.draw();
            }

            //draw collidables
            foreach (Collidable col in collidables)
            {
                col.draw();
            }

            //draw projectiles
            foreach (Projectile pro in projectiles)
            {
                spriteBatch.Draw(drawshots[pro.drawshot].tex, pro.physobj.pos, null, drawshots[pro.drawshot].col, pro.angle, drawshots[pro.drawshot].cent, Drawing.scale, SpriteEffects.None, 0);
            }
            foreach (Projectile pro in particles)
            {
                spriteBatch.Draw(drawshots[pro.drawshot].tex, pro.physobj.pos, null, drawshots[pro.drawshot].col, pro.angle, drawshots[pro.drawshot].cent, Drawing.scale, SpriteEffects.None, 0);
            }

            //draw FPS
            string output = string.Format("FPS: {0}  Projectiles: {1}  Collision Tests: {2}  Particles: {3}", fps, projectiles.Count, colcount, particles.Count);
            Vector2 FontPos = new Vector2(20, 15);
            spriteBatch.DrawString(debugFont, output, FontPos, Color.White);

            spriteBatch.End();

            //update counter for FPS
            frameCounter++;

            base.Draw(gameTime);
        }

        public int DoCollisions()
        {
            int colcount = 0;

            //collide player with enemies
            foreach (Enemy mob in enemies)
            {
                colcount++;
                if (player.physobj.collide(mob.physobj))
                {
                    player.physobj.stop();
                    mob.oncollide(player.physobj);
                }
            }

            //collide player with stationary objects
            foreach (Mob mob in collidables)
            {
                colcount++;
                if (player.physobj.collide(mob.physobj))
                {
                    player.physobj.stop();
                }
            }

            //collide mobs with each other
            foreach (Enemy mob in enemies)
            {
                mob.colchecks++;
                foreach (Enemy mob2 in enemies)
                {
                    if (mob2.colchecks == 0)
                    {
                        colcount++;
                        if (mob != mob2 && mob.physobj.collide(mob2.physobj))
                        {
                            mob.oncollide(mob2.physobj);
                            mob2.oncollide(mob.physobj);
                        }
                    }
                }
            }

            //collide mobs with stationary objects
            foreach (Enemy mob in enemies)
            {
                foreach (Mob mob2 in collidables)
                {
                    colcount++;
                    if (mob != mob2 && mob.physobj.collide(mob2.physobj))
                    {
                        mob.oncollide(mob2.physobj);
                    }
                }
            }

            foreach (Projectile pro in projectiles)
            {
                //collide projectiles with enemies
                foreach (Mob mob in enemies)
                {
                    colcount++;
                    if (mob.physobj.collide(pro.physobj))
                    {
                        pro.source.oncollide(pro, mob);
                    }
                }

                //collide projectiles with stationary objects
                foreach (Collidable mob in collidables)
                {
                    colcount++;
                    if (mob.physobj.collide(pro.physobj))
                    {
                        pro.source.oncollide(pro, mob);
                    }
                }
            }
            return colcount;
        }

        protected void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            boundbox = GraphicsDevice.Viewport.Bounds;
            boundbox.Inflate(-30, -30);

            cullbox = GraphicsDevice.Viewport.Bounds;
            cullbox.Inflate(100, 100);
        }

        /// <summary>
        /// Attempt to set the display mode to the desired resolution.  Iterates through the display
        /// capabilities of the default graphics adapter to determine if the graphics adapter supports the
        /// requested resolution.  If so, the resolution is set and the function returns true.  If not,
        /// no change is made and the function returns false.
        /// </summary>
        /// <param name="iWidth">Desired screen width.</param>
        /// <param name="iHeight">Desired screen height.</param>
        /// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
        private bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphics.PreferredBackBufferWidth = iWidth;
                    graphics.PreferredBackBufferHeight = iHeight;
                    graphics.IsFullScreen = bFullScreen;
                    graphics.ApplyChanges();
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphics.PreferredBackBufferWidth = iWidth;
                        graphics.PreferredBackBufferHeight = iHeight;
                        graphics.IsFullScreen = bFullScreen;
                        graphics.ApplyChanges();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
