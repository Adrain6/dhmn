// Terrain/LakeGenerator.cs
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class LakeGenerator : ITerrainGenerator
    {
        private const float FREQUENCY = 0.0001f;
        private const float THRESHOLD = 0.2f;

        public Color GetColor(float x, float y)
        {
            return Noise.Perlin(x * FREQUENCY, y * FREQUENCY) <= THRESHOLD
                ? Color.Blue
                : Color.Transparent;
        }
    }
}