using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace hm8.Controllers
{
    public class UiController
    {
        private readonly Rectangle _btn;
        private readonly Texture2D _pixel;
        private readonly SpriteFont _font;

        public UiController(GraphicsDevice gd, SpriteFont font)
        {
            _btn   = new Rectangle(20, 20, 160, 40); // posición y tamaño pantalla
            _font  = font;
            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public bool UpdateAndDraw(SpriteBatch sb)
        {
            var ms   = Mouse.GetState();
            bool over = _btn.Contains(ms.Position);
            bool click = over && ms.LeftButton == ButtonState.Pressed;

            // Dibujo
            sb.Draw(_pixel, _btn, over ? Color.LightGray : Color.Gray);
            sb.Draw(_pixel, new Rectangle(_btn.X, _btn.Y, _btn.Width, 1), Color.Black);
            sb.Draw(_pixel, new Rectangle(_btn.X, _btn.Bottom - 1, _btn.Width, 1), Color.Black);
            sb.Draw(_pixel, new Rectangle(_btn.X, _btn.Y, 1, _btn.Height), Color.Black);
            sb.Draw(_pixel, new Rectangle(_btn.Right - 1, _btn.Y, 1, _btn.Height), Color.Black);

            var txt = "Spawn soldier";
            var size = _font.MeasureString(txt);
            sb.DrawString(_font, txt,
                new Vector2(_btn.Center.X, _btn.Center.Y) - size * 0.5f,
                Color.Black);

            return click;
        }
    }
}