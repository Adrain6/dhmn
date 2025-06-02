using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace hm8.Controllers
{
    /// <summary>
    /// Controla posición y zoom de cámara.
    /// Maneja teclado (movimiento) y mouse wheel/trackpad (zoom).
    /// </summary>
    public class CameraController
    {
        // Velocidad de movimiento (unidades por segundo)
        private readonly float _speed;
        // Vista de la ventana
        private readonly Viewport _viewport;

        // Estado previo de scroll para detectar delta
        private int _prevScroll;

        /// <summary>
        /// Centro de la cámara en coordenadas de mundo.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Factor de zoom de la cámara (1 = 100%).
        /// </summary>
        public float Zoom { get; private set; } = 1f;

        /// <summary>
        /// Límite mínimo y máximo de zoom.
        /// </summary>
        private const float MinZoom = 0.01f;
        private const float MaxZoom = 3f;

        public CameraController(Viewport viewport, float speed = 500f)
        {
            _viewport = viewport;
            _speed = speed;
            Position = Vector2.Zero;
            // Inicializamos scroll previo
            _prevScroll = Mouse.GetState().ScrollWheelValue;
        }

        /// <summary>
        /// Actualiza posición y zoom. Llamar en Game1.Update.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Movimiento con teclado
            var kb = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var pos = Position;

            // Si mantiene Shift, velocidad xveces
            float currentSpeed = _speed * (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift) ? 40f : 1f);

            if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A)) pos.X -= currentSpeed * dt;
            if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D)) pos.X += currentSpeed * dt;
            if (kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.W)) pos.Y -= currentSpeed * dt;
            if (kb.IsKeyDown(Keys.Down) || kb.IsKeyDown(Keys.S)) pos.Y += currentSpeed * dt;

            Position = pos;

            // Resto de Update sigue igual...
            // Zoom con rueda o trackpad
            var ms = Mouse.GetState();
            int delta = ms.ScrollWheelValue - _prevScroll;
            _prevScroll = ms.ScrollWheelValue;
            if (delta != 0)
            {
                float zoomChange = delta / 6000f;
                Zoom = MathHelper.Clamp(Zoom + zoomChange, MinZoom, MaxZoom);
            }
        }

        /// <summary>
        /// Matriz de transformación que aplica zoom y centrar la cámara.
        /// Pasar a SpriteBatch.Begin(transformMatrix: ...).
        /// </summary>
        public Matrix GetTransformMatrix()
        {
            // Primero escalado, luego traslación con compensación de viewport
            return
                Matrix.CreateTranslation(-Position.X, -Position.Y, 0f) *
                Matrix.CreateScale(Zoom, Zoom, 1f) *
                Matrix.CreateTranslation(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0f);
        }
    }
}
