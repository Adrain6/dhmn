using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using hm8.Terrain;
using hm8.Utils;

namespace hm8.Controllers
{
    public class MapController
    {
        public const int TILE_SIZE = 32;
        public const int CHUNK_TILES = 32;
        public const int CHUNK_PIXELS = TILE_SIZE * CHUNK_TILES;
        public const float CHUNK_SIZE = CHUNK_PIXELS;

        // *** LÍMITES DEL MAPA (en coordenadas de chunk) ***
        private const int MIN_CHUNK_X = 0;      // ← ajusta esto
        private const int MAX_CHUNK_X = 100;      // ← ajusta esto (por ejemplo 4 chunks de ancho)
        private const int MIN_CHUNK_Y = 0;      // ← ajusta esto
        private const int MAX_CHUNK_Y = 100;      // ← ajusta esto (por ejemplo 3 chunks de alto)

        // Generadores de terreno
        private readonly OceanGenerator _ocean = new();
        private readonly LakeGenerator _lake = new();
        private readonly BiomeGenerator _biome = new();
        private readonly MountainGenerator _mountain = new();

        private static Texture2D _pixel;
        private readonly GraphicsDevice _graphics;
        private readonly Dictionary<Point, Texture2D> _cache =
            new Dictionary<Point, Texture2D>();

        public MapController(GraphicsDevice graphicsDevice)
        {
            _graphics = graphicsDevice;
        }

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch sb, Vector2 camPos, float zoom)
        {
            int cx = (int)Math.Floor(camPos.X / CHUNK_SIZE),
                cy = (int)Math.Floor(camPos.Y / CHUNK_SIZE);

            var vp = sb.GraphicsDevice.Viewport;
            float halfW = vp.Width * 0.5f / zoom;
            float halfH = vp.Height * 0.5f / zoom;
            int rX = (int)Math.Ceiling(halfW / CHUNK_SIZE) + 1;
            int rY = (int)Math.Ceiling(halfH / CHUNK_SIZE) + 1;

            for (int dy = -rY; dy <= rY; dy++)
                for (int dx = -rX; dx <= rX; dx++)
                {
                    var coord = new Point(cx + dx, cy + dy);

                    // --- saltarnos chunks fuera de los límites ---
                    if (coord.X < MIN_CHUNK_X || coord.X > MAX_CHUNK_X ||
                        coord.Y < MIN_CHUNK_Y || coord.Y > MAX_CHUNK_Y)
                    {
                        continue;
                    }

                    var tex = GetOrCreateChunkTexture(coord);
                    float bx = coord.X * CHUNK_SIZE;
                    float by = coord.Y * CHUNK_SIZE;

                    sb.Draw(tex,
                        new Rectangle((int)bx, (int)by, CHUNK_PIXELS, CHUNK_PIXELS),
                        Color.White);

                    // rejilla de chunks
                    sb.Draw(_pixel, new Rectangle((int)bx, (int)by, CHUNK_PIXELS, 1), Color.Black);
                    sb.Draw(_pixel, new Rectangle((int)bx, (int)(by + CHUNK_SIZE) - 1, CHUNK_PIXELS, 1), Color.Black);
                    sb.Draw(_pixel, new Rectangle((int)bx, (int)by, 1, CHUNK_PIXELS), Color.Black);
                    sb.Draw(_pixel, new Rectangle((int)(bx + CHUNK_SIZE) - 1, (int)by, 1, CHUNK_PIXELS), Color.Black);
                }
        }

        public Texture2D GetOrCreateChunkTexture(Point coord)
        {
            // chequeo de límites para evitar generación
            if (coord.X < MIN_CHUNK_X || coord.X > MAX_CHUNK_X ||
                coord.Y < MIN_CHUNK_Y || coord.Y > MAX_CHUNK_Y)
            {
                // devolvemos un chunk “vacío” (todo blanco o transparente si prefieres)
                var empty = new Texture2D(_graphics, CHUNK_TILES, CHUNK_TILES);
                var blank = new Color[CHUNK_TILES * CHUNK_TILES];
                for (int i = 0; i < blank.Length; i++) blank[i] = Color.CornflowerBlue;
                empty.SetData(blank);
                return empty;
            }

            if (_cache.TryGetValue(coord, out var existing))
                return existing;

            var tex = new Texture2D(_graphics, CHUNK_TILES, CHUNK_TILES);
            var data = new Color[CHUNK_TILES * CHUNK_TILES];

            float baseX = coord.X * CHUNK_SIZE;
            float baseY = coord.Y * CHUNK_SIZE;

            for (int y = 0, i = 0; y < CHUNK_TILES; y++)
                for (int x = 0; x < CHUNK_TILES; x++, i++)
                {
                    float wx = baseX + x * TILE_SIZE;
                    float wy = baseY + y * TILE_SIZE;

                    // 0) océano?
                    var col = _ocean.GetColor(wx, wy);
                    if (col != Color.Transparent) { data[i] = col; continue; }

                    // 1) lago?
                    col = _lake.GetColor(wx, wy);
                    if (col != Color.Transparent) { data[i] = col; continue; }

                    // 2) pradera vs cordillera
                    col = _biome.GetColor(wx, wy);
                    if (col == Color.Green) { data[i] = Color.Green; continue; }

                    // 3) detalle de montaña
                    data[i] = _mountain.GetColor(wx, wy);
                }

            tex.SetData(data);
            _cache[coord] = tex;
            return tex;
        }

        /// <summary>
        /// Fuerza la generación y cacheo de todos los chunks del mapa 
        /// usando los límites MIN_CHUNK_*/MAX_CHUNK_*.
        /// </summary>
        public void PreloadAllChunks()
        {
            for (int cy = MIN_CHUNK_Y; cy <= MAX_CHUNK_Y; cy++)
            {
                for (int cx = MIN_CHUNK_X; cx <= MAX_CHUNK_X; cx++)
                {
                    // Llama a GetOrCreateChunkTexture para que se genere y se almacene en _cache
                    GetOrCreateChunkTexture(new Point(cx, cy));
                }
            }
        }
    }
}