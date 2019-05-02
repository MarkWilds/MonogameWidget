using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rune.Monogame.Cameras;
using Rune.Monogame.Geometry;
using Rune.Monogame.Renderer;

namespace Rune.Monogame
{
    namespace game
    {
        public class GameApplication : Game
        {
            private GraphicsDeviceManager _graphics;
            private PrimitiveBatch _primitiveBatch;

            private Camera _camera;
            private Grid grid = new Grid(128);

            private Color boxColor = Color.White;
            private Aabb box;

            private List<Vector3> previewBoxes = new List<Vector3>();

            private Vector3 startRayhitPos;

            private MouseState previousMouseState;

            public GameApplication()
            {
                _graphics = new GraphicsDeviceManager(this);

                Content.RootDirectory = "Content";
                IsMouseVisible = true;
            }

            protected override void Initialize()
            {
                base.Initialize();
                _primitiveBatch = new PrimitiveBatch(GraphicsDevice, 2048);

                previousMouseState = Mouse.GetState();

                box = new Aabb();
                box.Grow(-32, 0, -32);
                box.Grow(32, 8, 32);
                _camera = new Camera(70, GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 8192.0f);
                _camera.Position = box.Center +
                                   Vector3.Backward * 30f +
                                   Vector3.Up * 20;
                _camera.RotateLocal(Vector3.Right, -45);
            }

            protected override void Update(GameTime gameTime)
            {
                Input(gameTime);

                var viewport = GraphicsDevice.Viewport;
                var mouseState = Mouse.GetState();
                var mousePos = new Vector2(mouseState.X, mouseState.Y);

                Vector2 tVector = new Vector2();
                Vector3 startPos = _camera.Unproject(viewport, mousePos);
                Vector3 endPos = _camera.Unproject(viewport, mousePos, false);
                var rayDir = Vector3.Normalize(endPos - startPos);

                previewBoxes.Clear();
                if (Maths.RayIntersectsBox(startPos, rayDir, box.Min, box.Max, -Maths.EpsilonBig, ref tVector))
                {
                    boxColor = Color.Orange;

                    startRayhitPos = startPos + rayDir * tVector.X;
                    var endRayhitPos = startPos + rayDir * tVector.Y;
                    Maths.RaycastImplicitGrid(startRayhitPos, endRayhitPos, 1, i =>
                    {
                        previewBoxes.Add(new Vector3(i.X, i.Y, i.Z) + Vector3.One * 0.5f);
                        return false;
                    });
                }
                else
                {
                    Maths.RayIntersectsPlane(Vector3.Up, 0, startPos, rayDir,
                        ref startRayhitPos);
                    boxColor = Color.White;
                }

                base.Update(gameTime);
            }

            private void Input(GameTime gameTime)
            {
                MouseState newMouseState = Mouse.GetState();
                if (newMouseState != previousMouseState
                    && newMouseState.RightButton == ButtonState.Pressed)
                {
                    float xDifference = previousMouseState.X - newMouseState.X;
                    float yDifference = previousMouseState.Y - newMouseState.Y;

                    _camera.Rotate(Vector3.Up, (float) (xDifference * gameTime.ElapsedGameTime.TotalSeconds * 10.0f));
                    _camera.RotateLocal(Vector3.Right,
                        (float) (yDifference * gameTime.ElapsedGameTime.TotalSeconds * 10.0f));
                }

                previousMouseState = newMouseState;

                KeyboardState keyboardState = Keyboard.GetState();
                Vector3 movement = Vector3.Zero;
                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Z = -1;
                else if (keyboardState.IsKeyDown(Keys.S))
                    movement.Z = 1;

                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X = -1;
                else if (keyboardState.IsKeyDown(Keys.D))
                    movement.X = 1;

                var speed = 50.0f;
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    speed = 100.0f;

                _camera.MoveLocal(movement * speed * (float) gameTime.ElapsedGameTime.TotalSeconds);
            }

            protected override void Draw(GameTime gameTime)
            {
                GraphicsDevice.Clear(new Color(32, 32, 32));

                grid.Draw(_primitiveBatch, _camera);

                _primitiveBatch.Begin(_camera.View, _camera.Projection);

                foreach (var b in previewBoxes)
                {
                    _primitiveBatch.DrawCube(b, 1f, Color.Yellow, PrimitiveBatch.DrawStyle.Wireframe);
                }

                _primitiveBatch.DrawAabb(box.Min, box.Max, boxColor, PrimitiveBatch.DrawStyle.Wireframe);

                _primitiveBatch.End();

                base.Draw(gameTime);
            }
        }
    }
}