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
using PuzzleSolver;
using Point = PuzzleSolver.Point;

namespace WindowsPuzzleVisualizer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        PuzzleState _puzzleState;

        Model _cubeModel;
        //Vector3 _modelPosition = Vector3.Zero;
        
        //Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 10.0f);

        // Set the position of the model in world space, and set the rotation.
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;

        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 5000.0f);

        float aspectRatio;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _puzzleState = PuzzleParser.GetGordionCubePuzzle();
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

            _cubeModel = Content.Load<Model>("cube");

            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
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

            modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                MathHelper.ToRadians(0.1f);

            base.Update(gameTime);
        }

        private Vector3 RGB(int r, int g, int b)
        {
            return new Vector3(r / 255f, g / 255f, b / 255f);
        }

        private Vector3 ToVector(Point point)
        {
            const int cubeSize = 200;
            return new Vector3(point.X, point.Y, point.Z) * cubeSize;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            var min = ToVector(_puzzleState.MinBounds);
            var max = ToVector(_puzzleState.MaxBounds) + ToVector(new Point(1,1,1));

            var center = (max - min) / 2;

            modelPosition = -center;

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[_cubeModel.Bones.Count];
            _cubeModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (var piece in _puzzleState.Pieces)
            {
                Vector3 pieceColor = new Vector3(0, 0, 0);
                if (piece.Piece.Name == "Orange")
                {
                    pieceColor = RGB(255, 140, 0);
                }
                else if (piece.Piece.Name == "Blue")
                {
                    pieceColor = RGB(0, 0, 255);
                }
                else if (piece.Piece.Name == "Yellow")
                {
                    pieceColor = RGB(255, 255, 0);
                }
                else if (piece.Piece.Name == "Red")
                {
                    pieceColor = RGB(255, 0, 0);
                }
                else if (piece.Piece.Name == "Green")
                {
                    pieceColor = RGB(0, 255, 0);
                }
                else if (piece.Piece.Name == "Purple")
                {
                    pieceColor = RGB(160, 32, 240);
                }
                
                foreach (var point in piece.CurrentPoints)
                {
                    Vector3 pointPosition = ToVector(point);

                    // Draw the model. A model can have multiple meshes, so loop.
                    foreach (ModelMesh mesh in _cubeModel.Meshes)
                    {
                        // This is where the mesh orientation is set, as well 
                        // as our camera and projection.
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            //  Lighting: http://gamedevelopedia.com/post/Tutorial-5-Intro-To-Lighting.aspx

                            //effect.EnableDefaultLighting();

                            effect.LightingEnabled = true;
                            effect.AmbientLightColor = new Vector3(.5f, .5f, .5f);
                            effect.DirectionalLight0.DiffuseColor = new Vector3(1, 1, 1);
                            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(0.5f, -0.5f, -0.5f));



                            //effect.EmissiveColor = pieceColor;
                            //effect.DiffuseColor = new Vector3(1, 0, 0);
                            //effect.SpecularColor = new Vector3(1, 0, 0);
                            effect.World = transforms[mesh.ParentBone.Index] *
                                Matrix.CreateTranslation(pointPosition) *
                                Matrix.CreateTranslation(modelPosition) *
                                Matrix.CreateRotationY(modelRotation);
                            effect.View = Matrix.CreateLookAt(cameraPosition,
                                Vector3.Zero, Vector3.Up);
                            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                                MathHelper.ToRadians(45.0f), aspectRatio,
                                1.0f, 10000.0f);
                        }

                        // Draw the mesh, using the effects set above.
                        mesh.Draw();
                    }
                }
            }

            //Matrix[] transforms = new Matrix[_cubeModel.Bones.Count];
            //_cubeModel.CopyAbsoluteBoneTransformsTo(transforms);

            //// Draw the model. A model can have multiple meshes, so loop.
            //foreach (ModelMesh mesh in _cubeModel.Meshes)
            //{
            //    // This is where the mesh orientation is set, as well as our camera and
            //    // projection.
            //    foreach (BasicEffect effect in mesh.Effects)
            //    {
            //        effect.EnableDefaultLighting();
            //        effect.World = transforms[mesh.ParentBone.Index];
            //        effect.View = Matrix.CreateLookAt(_cameraPosition, Vector3.Zero,
            //           Vector3.Up);
            //        effect.Projection = Matrix.CreatePerspectiveFieldOfView(
            //           MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            //    }

            //    // Draw the mesh, using the effects set above.
            //    mesh.Draw();
            //}

            base.Draw(gameTime);
        }
    }
}
