// Terrain/BiomeGenerator.cs
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class BiomeGenerator : ITerrainGenerator
    {
        private const float FREQUENCY = 0.00004f;
        private const int OCTAVES = 3;
        private const float LACUNARITY = 2f;
        private const float GAIN = 0.5f;
        private const float THRESHOLD = 0.6f;
        private const float WARP_FREQ = 0.0002f;
        private const float WARP_AMP = 100f;
        private const float BLEND_BAND = 0.1f;

        public Color GetColor(float x, float y)
        {
            // domain warp
            float wx = x + Noise.Perlin(x * WARP_FREQ, y * WARP_FREQ) * WARP_AMP;
            float wy = y + Noise.Perlin((x + 1234) * WARP_FREQ, (y + 1234) * WARP_FREQ) * WARP_AMP;

            // FBM
            float v = Noise.Fbm(wx, wy, OCTAVES, FREQUENCY, LACUNARITY, GAIN);
            float t = MathHelper.Clamp((v - (THRESHOLD - BLEND_BAND)) / (2 * BLEND_BAND), 0, 1);
            float b = t * t * (3 - 2 * t);
            return b > 0.5f ? Color.Gray /*marca que es cordillera*/ : Color.Green;
        }
    }
}