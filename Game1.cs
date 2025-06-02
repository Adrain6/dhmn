using hm8.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace hm8
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch  _sb;

        private CameraController  _cam;
        private MapController     _map;
        private SoldierController _soldiers;
        private UiController      _ui;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen             = false,
                PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                PreferredBackBufferHeight= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
            };
            Window.IsBorderless = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);

            // Inicializar el pixel blanco para dibujar chunks
            MapController.Initialize(GraphicsDevice);

            // Crear controllers
            _cam      = new CameraController(GraphicsDevice.Viewport);
            _map      = new MapController(GraphicsDevice);
            _soldiers = new SoldierController(GraphicsDevice);

            // *** PRECARGAR TODOS LOS CHUNKS ***
            _map.PreloadAllChunks();

            // Fuente y UI
            var font = Content.Load<SpriteFont>("DefaultFont");
            _ui      = new UiController(GraphicsDevice, font);
        }

        protected override void Update(GameTime gt)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _cam.Update(gt);
            base.Update(gt);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // ----- Mundo (afectado por cámara) -----
            _sb.Begin(
                samplerState: SamplerState.LinearClamp,
                transformMatrix: _cam.GetTransformMatrix()
            );

            _map.Draw(_sb, _cam.Position, _cam.Zoom);
            _soldiers.Draw(_sb);

            _sb.End();

            // ----- UI -----
            _sb.Begin(samplerState: SamplerState.PointClamp);
            if (_ui.UpdateAndDraw(_sb))
            {
                _soldiers.Spawn(_cam.Position);
            }
            _sb.End();

            base.Draw(gameTime);
        }
    }
}