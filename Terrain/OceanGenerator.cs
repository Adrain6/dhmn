// Terrain/OceanGenerator.cs
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class OceanGenerator : ITerrainGenerator
    {
        // Parámetros que funcionan - NO CAMBIAR EL UMBRAL
        private const float FREQUENCY = 0.000002f;  
        private const float THRESHOLD = 0.5f;       // EL QUE FUNCIONA
        
        // Domain warping MUY SUTIL para solo mejorar las costas
        private const float WARP_FREQ = 0.00002f;
        private const float WARP_AMP = 50f;        // MUCHO más bajo
        
        // Ruido adicional MUY SUTIL
        private const float COAST_FREQ = 0.0001f;
        private const float COAST_AMP = 0.03f;     // MUCHO más bajo

        public Color GetColor(float x, float y)
        {
            // Domain warping más fuerte para romper líneas rectas
            float warpX = Noise.Perlin(x * WARP_FREQ, y * WARP_FREQ) * WARP_AMP;
            float warpY = Noise.Perlin((x + 5000) * WARP_FREQ, (y + 5000) * WARP_FREQ) * WARP_AMP;
            
            float warpedX = x + warpX;
            float warpedY = y + warpY;
            
            // Ruido principal
            float baseNoise = Noise.Perlin(warpedX * FREQUENCY, warpedY * FREQUENCY);
            
            // Ruido adicional para irregularidades en las costas
            float coastNoise = Noise.Perlin(warpedX * COAST_FREQ, warpedY * COAST_FREQ) * COAST_AMP;
            
            // Combinamos ambos ruidos
            float finalNoise = baseNoise + coastNoise;
            
            // USAMOS EL UMBRAL QUE FUNCIONA (0.5f) Y COLOR AZUL OSCURO
            return finalNoise >= THRESHOLD ? new Color(0, 50, 150) : Color.Transparent;
        }
    }
}