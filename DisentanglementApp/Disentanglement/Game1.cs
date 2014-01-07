using System;
using System.Collections.Generic;
using System.Linq;

#if NETFX_CORE
using System.Threading.Tasks;
#endif


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PuzzleSolver;

#if WINDOWS_PHONE || ANDROID || IOS
using Microsoft.Xna.Framework.Input.Touch;
#endif

using Point = PuzzleSolver.Point;
using System.Diagnostics;


namespace WindowsPuzzleVisualizer
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		KeyboardState prevKeyboardState = Keyboard.GetState();
        SpriteBatch _spriteBatch;
        // SpriteFont _spriteFont;

        int _buttonTop;

		BasicEffect effect;

		VertexBuffer boxVB;
		IndexBuffer boxIB;

		//Solver _solver;
		SolverFrame[] _moves;
		int _moveIndex;
		private bool _solved = false;
		private object _lockObject = new object();

        PuzzleState _initialState;
		PuzzleState _puzzleState;
        PuzzleState _solverState;
        bool _showSolverProgress = false;

		Point _puzzleAnimationDirection = Point.Zero;
		IEnumerable<PuzzlePiece> _puzzlePiecesMoving = Enumerable.Empty<PuzzlePiece>();

		bool isAnimating = false;
		private TimeSpan _animationLength = TimeSpan.FromMilliseconds(500);
		private TimeSpan _animationStart;
		private TimeSpan _animationEnd;
		private float _animationPercent;
		private Vector3 _animationEndModelPosition;
		private Vector3 _animationStartModelPosition;
		private int _animationEndMoveIndex;

        private Vector2 _rotationVelocity = Vector2.Zero;

		// Set the position of the model in world space, and set the rotation.
		Vector3 modelPosition = Vector3.Zero;
		Matrix rotationMatrix = Matrix.Identity;

		// Set the position of the camera in world space, for our view matrix.
		Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 30.0f);

        Dictionary<Keys, string> _pieceKeyMapping = new Dictionary<Keys, string>();
        private Dictionary<string, bool> _pieceVisibility = new Dictionary<string, bool>();

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			IsMouseVisible = true;

			// Allow users to resize the window, and handle the Projection Matrix on Resize
#if WINDOWS
			Window.Title = "Disentanglement";
#endif
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += OnClientSizeChanged;

			_puzzleState = PuzzleParser.GetGordionCubePuzzle();
            _initialState = _puzzleState;

            foreach (var piece in _puzzleState.Pieces)
            {
                _pieceVisibility[piece.Piece.Name] = true;
            }

            _pieceKeyMapping[Keys.D1] = "Orange";
            _pieceKeyMapping[Keys.D2] = "Blue";
            _pieceKeyMapping[Keys.D3] = "Yellow";
            _pieceKeyMapping[Keys.D4] = "Red";
            _pieceKeyMapping[Keys.D5] = "Green";
            _pieceKeyMapping[Keys.D6] = "Purple";
#if NETFX_CORE
            Windows.System.Threading.ThreadPool.RunAsync(delegate { Solve(); });
#else
			System.Threading.ThreadPool.QueueUserWorkItem(delegate { Solve(); });
#endif
		}

		private void Solve()
		{
			var initialState = _puzzleState;
			var solver = new Solver(initialState);

            while (!solver.Done)
            {
                solver.Step();
                lock (_lockObject)
                {
                    _solverState = solver.CurrentState;
                }
            }


			lock (_lockObject)
			{
				_moves = solver.GetMoveSequence();

#if NETFX_CORE
                Debug.WriteLine("Moves: " + _moves.Length);
#else
                Console.WriteLine("Moves: " + _moves.Length);
#endif
				_moveIndex = 0;

				_puzzleState = _moves[_moveIndex].PuzzleState;
				_solved = true;
			}
		}

		protected bool IsFullScreen
		{
			get { return graphics.IsFullScreen; }
			set
			{
				if (value != graphics.IsFullScreen)
				{
					// Toggle FullScreen, and Mouse Display, then apply the changes
					// on the DeviceManager
					graphics.IsFullScreen = !graphics.IsFullScreen;
					IsMouseVisible = !IsMouseVisible;
					graphics.ApplyChanges();
				}
			}
		}

		protected void OnClientSizeChanged(object sender, EventArgs e)
		{
			ResetProjection();
		}

		protected void ResetProjection()
		{
			Viewport viewport = graphics.GraphicsDevice.Viewport;

			// Set the Projection Matrix
			effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
				(float)viewport.Width / viewport.Height,
				0.1f,
				100.0f);
            
            _buttonTop = GraphicsDevice.Viewport.Width - 80;

		}

		protected override void Initialize()
		{

			Vector2 topLeft = new Vector2(0.0f, 0.0f);
			Vector2 topRight = new Vector2(1.0f, 0.0f);
			Vector2 bottomLeft = new Vector2(0.0f, 1.0f);
			Vector2 bottomRight = new Vector2(1.0f, 1.0f);

			VertexPositionNormalTexture[] boxData = new VertexPositionNormalTexture[]
	        {
		        // Back Surface
		        new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, 1.0f), Vector3.Backward, bottomLeft),
		        new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, 1.0f), Vector3.Backward, topLeft), 
		        new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, 1.0f), Vector3.Backward, bottomRight),
		        new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, 1.0f), Vector3.Backward,topRight),  

		        // Front Surface
		        new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, -1.0f), Vector3.Forward, bottomLeft),
		        new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, -1.0f), Vector3.Forward, topLeft), 
		        new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, -1.0f), Vector3.Forward, bottomRight),
		        new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, -1.0f), Vector3.Forward, topRight), 

		        // Left Surface
		        new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, -1.0f), Vector3.Left, bottomLeft),
		        new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, -1.0f), Vector3.Left, topLeft),
		        new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, 1.0f), Vector3.Left, bottomRight),
		        new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, 1.0f), Vector3.Left, topRight),

		        // Right Surface
		        new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, 1.0f), Vector3.Right, bottomLeft),
		        new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, 1.0f), Vector3.Right, topLeft),
		        new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, -1.0f), Vector3.Right, bottomRight),
		        new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, -1.0f), Vector3.Right, topRight),

		        // Top Surface
		        new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, 1.0f), Vector3.Up, bottomLeft),
		        new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, -1.0f), Vector3.Up, topLeft),
		        new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, 1.0f), Vector3.Up, bottomRight),
		        new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, -1.0f), Vector3.Up, topRight),

		        // Bottom Surface
		        new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, -1.0f), Vector3.Down, bottomLeft),
		        new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, 1.0f), Vector3.Down, topLeft),
		        new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, -1.0f), Vector3.Down, bottomRight),
		        new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, 1.0f), Vector3.Down, topRight),
	        };

			short[] boxIndices = new short[] { 
				0, 1, 2, 2, 1, 3,   
				4, 5, 6, 6, 5, 7,
				8, 9, 10, 10, 9, 11, 
				12, 13, 14, 14, 13, 15, 
				16, 17, 18, 18, 17, 19,
				20, 21, 22, 22, 21, 23
			};


			boxVB = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, 24, BufferUsage.WriteOnly);
			boxIB = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);

			boxVB.SetData(boxData);
			boxIB.SetData(boxIndices);
			//boxData = null;
			//boxIndices = null;
#if WINDOWS_PHONE || ANDROID || IOS
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Flick | GestureType.Tap | GestureType.DoubleTap;
#endif

			base.Initialize();

		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			effect = new BasicEffect(GraphicsDevice);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // _spriteFont = Content.Load<SpriteFont>("font");

			ResetProjection();
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		private void SetupAnimation(PuzzleMove move, TimeSpan totalGameTime, int endMoveIndex)
		{
			isAnimating = true;
			_puzzleAnimationDirection = move.Direction;
			_puzzlePiecesMoving = move.MovingPieces;
			
			_animationPercent = 0;
			_animationStart = totalGameTime;
			_animationEnd = totalGameTime.Add(_animationLength);

			_animationStartModelPosition = GetCenteringVector(move.StartingState);
			var nonNormalizedEndPosition = move.GetEndingState();
			_animationEndModelPosition = GetCenteringVector(nonNormalizedEndPosition);

			_animationEndMoveIndex = endMoveIndex;

		}

		private void EndAnimation()
		{
			isAnimating = false;
			_animationPercent = 0;
			_puzzleAnimationDirection = Point.Zero;
			_puzzlePiecesMoving = Enumerable.Empty<PuzzlePiece>();

			_moveIndex = _animationEndMoveIndex;

			_puzzleState = _moves[_moveIndex].PuzzleState;
			modelPosition = GetCenteringVector(_puzzleState);
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			KeyboardState keyboard = Keyboard.GetState();

			if (keyboard.IsKeyDown(Keys.Escape))
				Exit();

			if (keyboard.IsKeyDown(Keys.F11) && prevKeyboardState.IsKeyUp(Keys.F11))
				IsFullScreen = !IsFullScreen;

			//if (keyboard.IsKeyDown(Keys.A) && prevKeyboardState.IsKeyUp(Keys.A))
			//    isAnimating = !isAnimating;

			

			float movement = (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.02f;
			

			if (keyboard.IsKeyDown(Keys.PageUp))
			{
				cameraPosition = new Vector3(cameraPosition.X, cameraPosition.Y, cameraPosition.Z - movement);
			}

			if (keyboard.IsKeyDown(Keys.PageDown))
			{
				cameraPosition = new Vector3(cameraPosition.X, cameraPosition.Y, cameraPosition.Z + movement);
			}

			float rotation = (float)gameTime.ElapsedGameTime.TotalMilliseconds *
					MathHelper.ToRadians(0.1f);

			if (keyboard.IsKeyDown(Keys.Left))
			{
				rotationMatrix = rotationMatrix * Matrix.CreateRotationY(rotation);
			}
			if (keyboard.IsKeyDown(Keys.Right))
			{
				rotationMatrix = rotationMatrix * Matrix.CreateRotationY(-rotation);
			}
			if (keyboard.IsKeyDown(Keys.Up))
			{
				rotationMatrix = rotationMatrix * Matrix.CreateRotationX(rotation);
			}
			if (keyboard.IsKeyDown(Keys.Down))
			{
				rotationMatrix = rotationMatrix * Matrix.CreateRotationX(-rotation);
			}

            bool commandForward = false;
            bool commandBack = false;
            bool commandResetView = false;

#if WINDOWS_PHONE  || ANDROID || IOS
            TouchCollection touches = TouchPanel.GetState();

            if (touches.Count > 0)
            {
                foreach (TouchLocation touch in touches)
                {
                    if (touch.Position.X <= _buttonTop)
                    {
                        _rotationVelocity = Vector2.Zero;
                    }
                }
            }

            while (TouchPanel.IsGestureAvailable)
            {
                float factor = 0.01f;

                GestureSample gesture = TouchPanel.ReadGesture();
                if (gesture.GestureType == GestureType.FreeDrag)
                {
                    rotationMatrix = rotationMatrix *
                        Matrix.CreateRotationX(gesture.Delta.X * factor) *
                        Matrix.CreateRotationY(-gesture.Delta.Y * factor);
                }
                else if (gesture.GestureType == GestureType.Flick)
                {
                    float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    _rotationVelocity = new Vector2(gesture.Delta.X * factor, -gesture.Delta.Y * factor);
                }
                else if (gesture.GestureType == GestureType.Tap)
                {
                    if (gesture.Position.X <= GraphicsDevice.Viewport.Width / 2)
                       {
                            commandForward = true;   
                        }
                        else if (gesture.Position.X > GraphicsDevice.Viewport.Width / 2 )
                        {
                            commandBack = true;
                        }                    
                }
                else if (gesture.GestureType == GestureType.DoubleTap)
                {
                    commandResetView = true;
                }
            }
#endif

            foreach (var kvp in _pieceKeyMapping)
            {
                if (keyboard.IsKeyDown(kvp.Key) && prevKeyboardState.IsKeyUp(kvp.Key))
                {
                    _pieceVisibility[kvp.Value] = !_pieceVisibility[kvp.Value];
                }
            }

            if (keyboard.IsKeyDown(Keys.R) && prevKeyboardState.IsKeyUp(Keys.R))
            {
                commandResetView = true;
            }

            if (keyboard.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
            {
                commandForward = true;
            }

            if (keyboard.IsKeyDown(Keys.B) && prevKeyboardState.IsKeyUp(Keys.B))
            {
                commandBack = true;
            }

            if (keyboard.IsKeyDown(Keys.U) && prevKeyboardState.IsKeyUp(Keys.U))
            {
                lock (_lockObject)
                {
                    if (_solved)
                    {
                        _puzzleState = _moves[_moveIndex].PuzzleState;
                    }
                    else
                    {
                        _puzzleState = _solverState;
                    }
                }
            }

            if (commandResetView)
            {
                rotationMatrix = Matrix.Identity;
                cameraPosition = new Vector3(0.0f, 0.0f, 30.0f);
                _rotationVelocity = Vector2.Zero;
            }

			lock (_lockObject)
			{
                if (_solved)
                {
                    if (commandForward)
                    {
                        if (isAnimating)
                        {
                            EndAnimation();
                        }
                        if (_moveIndex < _moves.Length - 1)
                        {
                            var move = _moves[_moveIndex + 1].BestSolutionPrevMove;

                            if (move.IsRemoval)
                            {
                                _moveIndex++;
                                _puzzleState = _moves[_moveIndex].PuzzleState;
                            }
                            else
                            {
                                SetupAnimation(move, gameTime.TotalGameTime, _moveIndex + 1);
                            }
                        }
                    }

                    if (commandBack)
                    {
                        if (isAnimating)
                        {
                            EndAnimation();
                        }

                        if (_moveIndex > 0)
                        {
                            int newMoveIndex = _moveIndex - 1;
                            var move = _moves[_moveIndex].BestSolutionPrevMove;
                            if (move.IsRemoval)
                            {
                                _moveIndex--;
                                _puzzleState = _moves[_moveIndex].PuzzleState;
                            }
                            else
                            {
                                var moveToReverse = move;
                                PuzzleMove reverseMove = new PuzzleMove(moveToReverse.GetEndingState().Normalize(), moveToReverse.MovingPieces, moveToReverse.Direction.Negate());
                                SetupAnimation(reverseMove, gameTime.TotalGameTime, newMoveIndex);
                            }
                        }
                    }
                }
                else
                {
                    if (commandForward)
                    {
                        _showSolverProgress = !_showSolverProgress;
                    }
                    if (commandBack)
                    {
                        _showSolverProgress = false;
                        _puzzleState = _initialState;
                    }
                }
			}

            if (_rotationVelocity != Vector2.Zero)
            {
                rotationMatrix = rotationMatrix *
                    Matrix.CreateRotationX(_rotationVelocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds) *
                    Matrix.CreateRotationY(_rotationVelocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds);

                float friction = 0.03f;
                _rotationVelocity *= 1f - (friction - (float)gameTime.ElapsedGameTime.TotalSeconds);
            }



			if (isAnimating)
			{
				if (gameTime.TotalGameTime >= _animationEnd)
				{
					EndAnimation();
				}
				else
				{
					TimeSpan animationElapsed = gameTime.TotalGameTime - _animationStart;
					_animationPercent = (float)(animationElapsed.TotalMilliseconds / _animationLength.TotalMilliseconds);

					modelPosition = _animationStartModelPosition + (_animationEndModelPosition - _animationStartModelPosition) * _animationPercent;
				}
			}
			else
			{
				lock (_lockObject)
				{
					modelPosition = GetCenteringVector(_puzzleState);
				}
			}


			prevKeyboardState = keyboard;			

			base.Update(gameTime);
		}

		private Vector3 RGB(int r, int g, int b)
		{
			return new Vector3(r / 255f, g / 255f, b / 255f);
		}

		private Vector3 ToVector(Point point)
		{
			//const int cubeSize = 200;
			const int cubeSize = 2;
			return new Vector3(point.X, point.Y, point.Z) * cubeSize;
		}

		private Vector3 GetCenteringVector(PuzzleState state)
		{
			var min = ToVector(state.MinBounds);
			var max = ToVector(state.MaxBounds);

			var center = (max + min) / 2;
			return -center;

		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			PuzzleState currentState;
			bool solved;
			lock (_lockObject)
			{
                if (_showSolverProgress && !_solved && _solverState != null)
                {
                    _puzzleState = _solverState;
                }
                currentState = _puzzleState;
				solved = _solved;
			}

			if (solved)
			{
				graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			}
			else
			{
				graphics.GraphicsDevice.Clear(Color.Black);
			}

            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

            //int buttonTop = GraphicsDevice.Viewport.Width - 80;
            //_spriteBatch.DrawString(_spriteFont, "Prev", new Vector2(buttonTop, 0), Color.Black, -MathHelper.PiOver2, Vector2.Zero, 1f, SpriteEffects.None, 0);

            //_spriteBatch.End();

			GraphicsDevice.SetVertexBuffer(boxVB);
			GraphicsDevice.Indices = boxIB;


			

			//var min = ToVector(currentState.MinBounds);
			//var max = ToVector(currentState.MaxBounds);

			//var center = (max + min) / 2;
			//modelPosition = -center;

			foreach (var piece in currentState.Pieces)
			{
                if (!_pieceVisibility[piece.Piece.Name])
                {
                    continue;
                }

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
					if (_puzzlePiecesMoving.Contains(piece.Piece))
					{
						pointPosition += ToVector(_puzzleAnimationDirection) * _animationPercent;
					}

					//effect.World = Matrix.CreateFromYawPitchRoll(rectangleAngle,
					//			rectangleAngle, rectangleAngle) * rectangleTransform;
                    effect.World = Matrix.CreateTranslation(pointPosition) *
                                    Matrix.CreateTranslation(modelPosition) *
                                    rotationMatrix;
                                

#if WINDOWS_PHONE || ANDROID || IOS
                    Vector3 upViewVector = Vector3.Right;
#else
                    Vector3 upViewVector = Vector3.Up;
#endif

                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, upViewVector);

                    //effect.View = Matrix.CreateLookAt(cameraPosition,
                    //    modelPosition, Vector3.Up);
					
                    
                    //effect.Projection = Matrix.CreatePerspectiveFieldOfView(
					//    MathHelper.ToRadians(45.0f), aspectRatio,
					//    1.0f, 10000.0f);

					effect.AmbientLightColor = pieceColor * 0.2f;
					//effect.DiffuseColor = new Vector3(.5f, 0, 0);
					effect.LightingEnabled = true;
					//if (solved)
					{
						effect.DirectionalLight0.Enabled = true;
						effect.DirectionalLight1.Enabled = true;
						effect.DirectionalLight2.Enabled = true;
					}
                    //effect.DirectionalLight0.DiffuseColor = pieceColor * 0.8f;
                    //effect.DirectionalLight1.DiffuseColor = pieceColor * new Vector3(0.8f, 0.6f, 0.6f);
                    //effect.DirectionalLight2.DiffuseColor = pieceColor * new Vector3(0.6f, 0.8f, 0.8f);
                    effect.DirectionalLight0.DiffuseColor = pieceColor * 0.8f;
                    effect.DirectionalLight1.DiffuseColor = pieceColor * 0.8f;
                    effect.DirectionalLight2.DiffuseColor = pieceColor * 0.8f;
                    //effect.DirectionalLight0.Direction = Vector3.Down;
                    //effect.DirectionalLight1.Direction = Vector3.Normalize(Vector3.Right + Vector3.Up + Vector3.Forward);
                    //effect.DirectionalLight2.Direction = Vector3.Normalize(Vector3.Left + Vector3.Up + Vector3.Forward);
                    effect.DirectionalLight0.Direction = Vector3.Normalize(Vector3.Forward + Vector3.Left + Vector3.Down);
                    effect.DirectionalLight1.Direction = Vector3.Normalize(Vector3.Right + Vector3.Up);
                    effect.DirectionalLight2.Direction = Vector3.Normalize(Vector3.Left + Vector3.Down);


					effect.CurrentTechnique.Passes[0].Apply();

					GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
					
				}


               
			}

			base.Draw(gameTime);
		}
	}
}
