using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ProjectBaliskner;

namespace ProjectBaliskner
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //instance variables 
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //skybox vars
        Skybox skybox;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
        //Vector3 cameraPosition;
        float angle = 0;
        float distance = 20;
        
        //Ship vars
        // Set the 3D model to draw.
        Model baliskner;
        // The aspect ratio determines how to scale 3d to 2d projection.
        float aspectRatio;
        // Set the position of the model in world space, and set the rotation.
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;
        Quaternion rotation = Quaternion.Identity;
        Vector3 modelVelocity = Vector3.Zero;
        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 5000.0f);

        //media variables
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

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
            // TODO: Add your initialization logic here
            audioEngine = new AudioEngine("Content\\Audio\\MyGameAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //load skybox content
            skybox = new Skybox("Skyboxes/EmptySpace", Content);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            baliskner = Content.Load<Model>("Models\\baliskner3");
            aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
            (float)graphics.GraphicsDevice.Viewport.Height;

            // TODO: use this.Content to load your game content here
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
# if !XBOX
            if(Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape).Equals(true)){

                this.Exit();
            }
#endif
            // Get some input.
            UpdateInput();
            // Add velocity to the current position.
            modelPosition += modelVelocity;
            // Bleed off velocity over time.
            modelVelocity *= 0.95f;

            //spinning model
            modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.1f);

            //skybox update
             angle += 0.002f;
             cameraPosition = distance * new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle));
             view = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 0, 0), Vector3.UnitY);

            base.Update(gameTime);
        }
        //Update method that updates the input
        protected void UpdateInput()
        {
            // Get the game pad state.
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);
            // Getting the keyboard state
            KeyboardState keysState = Keyboard.GetState(PlayerIndex.One);
            
            //check if the game pad is connected and then proceed with the functionality 
            
                //floating points for the yaw, pitch and roll of the ship
                float yaw = currentState.ThumbSticks.Left.X * MathHelper.ToRadians(-1.5f);
                float pitch = currentState.ThumbSticks.Left.Y * MathHelper.ToRadians(-1.5f);
                float roll = currentState.ThumbSticks.Right.X * MathHelper.ToRadians(-1.5f);

                Quaternion rot = Quaternion.CreateFromAxisAngle(Vector3.Right, pitch) * Quaternion.CreateFromAxisAngle(Vector3.Up, yaw) * Quaternion.CreateFromAxisAngle(Vector3.Backward, roll);
                rotation *= rot;
                modelPosition += Vector3.Transform(new Vector3(0.0f, 0.0f, currentState.Triggers.Right * -20.0f), Matrix.CreateFromQuaternion(rotation));

                GamePad.SetVibration(PlayerIndex.One, currentState.Triggers.Right,
                    currentState.Triggers.Right);
            
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //graphics.GraphicsDevice.Clear(Color.DarkGoldenrod);
            
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            skybox.Draw(view, projection, cameraPosition);
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;

            /*Matrix[] transforms = new Matrix[baliskner.Bones.Count];
            baliskner.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in baliskner.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateRotationY(modelRotation)
                        * Matrix.CreateTranslation(modelPosition);
                    effect.View = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50.0f),
                        aspectRatio, 1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }*/
            base.Draw(gameTime);
        }
    }
}
