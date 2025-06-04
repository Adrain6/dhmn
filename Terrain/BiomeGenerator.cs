// Terrain/BiomeGenerator.cs
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class BiomeGenerator : ITerrainGenerator
    {
        // Configuración para zonas montañosas amplias
        private const float FREQUENCY = 0.00003f;     // Frecuencia baja para zonas grandes
        private const int OCTAVES = 4;                // Más octavas para variación
        private const float LACUNARITY = 2.2f;        
        private const float GAIN = 0.45f;             
        private const float THRESHOLD = 0.58f;        // Umbral para determinar montañas
        
        // Domain warping para formas más naturales
        private const float WARP_FREQ = 0.00001f;     
        private const float WARP_AMP = 2000f;         // Amplitud alta para deformación notable
        
        // Banda de transición suave
        private const float BLEND_BAND = 0.15f;

        public Color GetColor(float x, float y)
        {
            // No generar zonas montañosas en islas
            float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(101 * 32 * 32f * 0.5f, 101 * 32 * 32f * 0.5f));
            if (distFromCenter > 101 * 32 * 32f * 0.32f)
            {
                return Color.Green;
            }
            
            // Domain warping para romper patrones regulares
            float warpX = Noise.Perlin(x * WARP_FREQ, y * WARP_FREQ) * WARP_AMP;
            float warpY = Noise.Perlin((x + 5678) * WARP_FREQ, (y + 5678) * WARP_FREQ) * WARP_AMP;
            
            // Coordenadas deformadas
            float wx = x + warpX;
            float wy = y + warpY;

            // FBM para generar las zonas base
            float value = Noise.Fbm(wx, wy, OCTAVES, FREQUENCY, LACUNARITY, GAIN);
            
            // Aplicar umbral con transición suave
            float t = MathHelper.Clamp((value - (THRESHOLD - BLEND_BAND)) / (2 * BLEND_BAND), 0, 1);
            float smooth = t * t * (3 - 2 * t);
            
            // Si es zona montañosa, marcar como gris (el MountainGenerator añadirá los detalles)
            return smooth > 0.5f ? Color.Gray : Color.Green;
        }
    }
}