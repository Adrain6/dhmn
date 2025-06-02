// Terrain/OceanGenerator.cs
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class OceanGenerator : ITerrainGenerator
    {
        private const float FREQUENCY = 0.000005f;
        private const float THRESHOLD = 0.4f;
        private static readonly Color COLOR = new Color(0, 50, 200);

        public Color GetColor(float x, float y)
        {
            return Noise.Perlin(x * FREQUENCY, y * FREQUENCY) <= THRESHOLD
                ? COLOR
                : Color.Transparent; // indica “no es océano”
        }
    }
}