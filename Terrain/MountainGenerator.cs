// Terrain/MountainGenerator.cs
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class MountainGenerator : ITerrainGenerator
    {
        // Frecuencia base más alta para muchas crestas
        private const float BASE_FREQ    = 0.0005f;  
        private const int   OCTAVES      = 5;        
        private const float LACUNARITY   = 2f;       
        private const float GAIN         = 0.8f;     
        // Umbral más bajo → más pixeles marcados como montaña
        private const float THRESHOLD    = 0.15f;    
        // Banda de transición amplia para suavizar bordes
        private const float BAND         = 0.25f;    

        public Color GetColor(float x, float y)
        {
            // Genera ruido ridged multifractal
            float m = Noise.RidgedMultifractal(x, y,
                                              OCTAVES,
                                              BASE_FREQ,
                                              LACUNARITY,
                                              GAIN);

            // Normalizamos respecto a [THRESHOLD–BAND … THRESHOLD+BAND]
            float t = MathHelper.Clamp(
                (m - (THRESHOLD - BAND)) 
                / (2f * BAND),
                0f, 1f
            );
            // smoothstep clásico
            float b = t * t * (3f - 2f * t);

            return (b > 0.5f)
                ? Color.Gray   // montaña
                : Color.Green; // pradera
        }
    }
}