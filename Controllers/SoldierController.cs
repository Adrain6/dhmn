using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace hm8.Controllers
{
    public class SoldierController
    {
        private readonly List<Vector2> _soldiers = new();
        private readonly Texture2D _circle;
        private readonly int _radius = 8;      // 24 px diámetro aprox.

        public SoldierController(GraphicsDevice gd)
        {
            // Genera una textura-círculo en RAM (ARGB)
            int d = _radius * 2 + 1;
            _circle = new Texture2D(gd, d, d);
            var data = new Color[d * d];
            Vector2 c = new(_radius, _radius);
            float r2 = _radius * _radius;
            for (int y = 0; y < d; y++)
            for (int x = 0; x < d; x++)
            {
                if (Vector2.DistanceSquared(new(x, y), c) <= r2)
                    data[y * d + x] = Color.Blue;
                else
                    data[y * d + x] = Color.Transparent;
            }
            _circle.SetData(data);
        }

        public void Spawn(Vector2 worldPos) => _soldiers.Add(worldPos);

        public void Draw(SpriteBatch sb)
        {
            foreach (var pos in _soldiers)
            {
                sb.Draw(
                    _circle,
                    pos - new Vector2(_radius), // centra la textura
                    Color.White);
            }
        }
    }
}