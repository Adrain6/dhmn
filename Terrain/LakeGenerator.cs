// Terrain/LakeGenerator.cs - Versión simple pero efectiva
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class LakeGenerator : ITerrainGenerator
    {
        private const float FREQUENCY = 0.00008f;
        private const float THRESHOLD = 0.3f;
        
        // Domain warping para formas irregulares
        private const float WARP_FREQ = 0.0001f;
        private const float WARP_AMP = 120f;
        
        // Ruido adicional para detalles
        private const float DETAIL_FREQ = 0.0004f;
        private const float DETAIL_AMP = 0.2f;

        public Color GetColor(float x, float y)
        {
            // Domain warping para romper circularidad
            float warpX = Noise.Perlin(x * WARP_FREQ, y * WARP_FREQ) * WARP_AMP;
            float warpY = Noise.Perlin((x + 2000) * WARP_FREQ, (y + 2000) * WARP_FREQ) * WARP_AMP;
            
            float warpedX = x + warpX;
            float warpedY = y + warpY;
            
            // Ruido base con FBM para más variación
            float baseNoise = Noise.Fbm(warpedX, warpedY, 3, FREQUENCY, 2.0f, 0.5f);
            
            // Detalles para bordes irregulares
            float detailNoise = Noise.Perlin(warpedX * DETAIL_FREQ, warpedY * DETAIL_FREQ) * DETAIL_AMP;
            
            // Combinamos
            float combined = baseNoise + detailNoise;
            
            // Threshold simple con suavizado
            return combined <= THRESHOLD ? Color.Blue : Color.Transparent;
        }
    }
}