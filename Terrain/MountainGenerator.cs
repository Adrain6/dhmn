// Terrain/MountainGenerator.cs
using Microsoft.Xna.Framework;
using hm8.Utils;
using System;

namespace hm8.Terrain
{
    public class MountainGenerator : ITerrainGenerator
    {
        private const float MAP_SIZE = 101 * 32 * 32f;
        private static readonly Vector2 MAP_CENTER = new(MAP_SIZE * 0.5f, MAP_SIZE * 0.5f);
        
        // Parámetros para cordilleras más definidas
        private const float RIDGE_SHARPNESS = 1.8f;  // Reducido para bordes más suaves
        private const float NOISE_AMPLITUDE = 0.25f;  // Más ruido para naturalidad
        private const float EDGE_WARP_FREQ = 0.0003f;  // Frecuencia para deformar bordes
        private const float EDGE_WARP_AMP = 0.4f;     // Amplitud de deformación aumentada

        public Color GetColor(float x, float y)
        {
            // Simplemente no generar montañas muy lejos del centro
            // Las islas están todas más allá de cierta distancia
            float distFromCenter = Vector2.Distance(new Vector2(x, y), MAP_CENTER);
            
            // Fade out irregular para las montañas cerca del borde del continente
            float continentFade = 1f;
            
            // Solo aplicar fade muy cerca del borde
            float fadeNoise = Noise.Perlin(x * 0.00003f, y * 0.00003f) * 0.08f;
            float fadeRadius = MAP_SIZE * (0.33f + fadeNoise);
            
            if (distFromCenter > fadeRadius)
            {
                // Fade más suave
                float fadeDist = distFromCenter - fadeRadius;
                float fadeWidth = MAP_SIZE * 0.05f;
                continentFade = 1f - (fadeDist / fadeWidth);
                continentFade = MathHelper.Clamp(continentFade, 0, 1);
                
                // Transición suave
                continentFade = continentFade * continentFade * (3f - 2f * continentFade);
            }
            
            // No montañas fuera del continente principal
            if (continentFade <= 0)
            {
                return Color.Green;
            }
            
            float mountain = 0f;
            
            // 1. GRAN CORDILLERA CENTRAL (tipo Andes/Rocosas)
            // Línea base con una curva suave
            float t = (x - MAP_SIZE * 0.15f) / (MAP_SIZE * 0.7f);
            t = MathHelper.Clamp(t, 0, 1);
            
            // Fade suave en los extremos de la cordillera
            float ridgeFade = 1f;
            if (t < 0.1f) ridgeFade = t / 0.1f;
            else if (t > 0.9f) ridgeFade = (1f - t) / 0.1f;
            ridgeFade = (float)Math.Sqrt(ridgeFade); // Fade menos agresivo
            
            // Curva en S para la cordillera principal con más variación
            float centerY = MAP_SIZE * 0.3f + 
                           (float)Math.Sin(t * Math.PI * 1.5f) * MAP_SIZE * 0.2f +
                           t * MAP_SIZE * 0.3f;
            
            // Añadir múltiples escalas de variación
            centerY += Noise.Perlin(x * 0.00001f, 0) * MAP_SIZE * 0.08f;     // Variación grande
            centerY += Noise.Perlin(x * 0.00003f, 100) * MAP_SIZE * 0.04f;   // Variación media
            centerY += Noise.Perlin(x * 0.00008f, 200) * MAP_SIZE * 0.02f;   // Variación pequeña
            
            // Distancia a la línea central
            float distToMain = Math.Abs(y - centerY);
            
            // Ancho variable de la cordillera con más variación
            float width = 15000f +   // Aumentado de 12000f
                         Noise.Perlin(x * 0.00002f, 1000) * 6000f +
                         Noise.Perlin(x * 0.00005f, 2000) * 3000f;
            
            // Valor de la cordillera con falloff pronunciado pero irregular
            float mainValue = Math.Max(0, 1f - distToMain / width);
            
            // Añadir variación en los bordes (menos agresiva)
            float edgeVar = Noise.Perlin(x * 0.0002f, y * 0.0002f) * 0.2f + 
                           Noise.Perlin(x * 0.0005f, y * 0.0005f) * 0.1f;
            mainValue *= (1f + edgeVar);
            
            // Aplicar sharpness
            mainValue = (float)Math.Pow(mainValue, RIDGE_SHARPNESS);
            
            // Picos y valles a lo largo de la cordillera con más variación
            float peaks = Noise.RidgedMultifractal(x, y, 3, 0.00008f, 2.0f, 0.6f);
            float valleys = Noise.Perlin(x * 0.00015f, y * 0.00015f);
            mainValue *= (0.6f + peaks * 0.4f) * (0.8f + valleys * 0.2f);
            
            // Aplicar el fade de los extremos
            mainValue *= ridgeFade;
            
            mountain = Math.Max(mountain, mainValue);
            
            // 2. CORDILLERA NORTE (más pequeña)
            if (y < MAP_SIZE * 0.4f && x > MAP_SIZE * 0.25f && x < MAP_SIZE * 0.75f)  // Más amplia
            {
                float northY = MAP_SIZE * 0.22f + 
                              Noise.Perlin(x * 0.00002f, 2000) * MAP_SIZE * 0.04f +
                              Noise.Perlin(x * 0.00006f, 3000) * MAP_SIZE * 0.02f;
                float distToNorth = Math.Abs(y - northY);
                float northWidth = 10000f + Noise.Perlin(x * 0.00003f, 4000) * 4000f;  // Más ancha
                float northValue = Math.Max(0, 1f - distToNorth / northWidth);
                northValue = (float)Math.Pow(northValue, RIDGE_SHARPNESS);
                
                // Conectar con la principal
                float connectionFactor = 1f - Math.Abs(x - MAP_SIZE * 0.5f) / (MAP_SIZE * 0.2f);
                connectionFactor = MathHelper.Clamp(connectionFactor, 0, 1);
                northValue *= connectionFactor;
                
                mountain = Math.Max(mountain, northValue * 0.8f);
            }
            
            // 3. RAMA OCCIDENTAL (bifurcación de la principal)
            if (x < MAP_SIZE * 0.4f && y > MAP_SIZE * 0.4f && y < MAP_SIZE * 0.7f)
            {
                // Calcular el punto de bifurcación
                float branchT = 0.3f; // 30% a lo largo de la cordillera principal
                float branchX = MAP_SIZE * 0.15f + branchT * MAP_SIZE * 0.7f;
                float branchY = MAP_SIZE * 0.3f + 
                               (float)Math.Sin(branchT * Math.PI * 1.5f) * MAP_SIZE * 0.2f +
                               branchT * MAP_SIZE * 0.3f;
                
                // Línea desde el punto de bifurcación hacia el suroeste
                float westT = (y - branchY) / (MAP_SIZE * 0.3f);
                westT = MathHelper.Clamp(westT, 0, 1);
                
                // Fade suave en los extremos
                float westFade = 1f;
                if (westT < 0.15f) westFade = westT / 0.15f;
                else if (westT > 0.85f) westFade = (1f - westT) / 0.15f;
                westFade = (float)Math.Sqrt(westFade);
                
                float westX = branchX - westT * MAP_SIZE * 0.15f;
                float westY = branchY + westT * MAP_SIZE * 0.3f;
                
                // Añadir variación
                westX += Noise.Perlin(y * 0.00003f, 5000) * MAP_SIZE * 0.03f;
                westY += Noise.Perlin(x * 0.00003f, 6000) * MAP_SIZE * 0.02f;
                
                float distToWest = Vector2.Distance(new Vector2(x, y), new Vector2(westX, westY));
                float westValue = Math.Max(0, 1f - distToWest / 12000f);  // Más ancha
                
                // Bordes irregulares
                float westEdge = Noise.Perlin(x * 0.0003f, y * 0.0003f) * 0.2f + 0.8f;  // Menos reducción
                westValue *= westEdge;
                
                westValue = (float)Math.Pow(westValue, RIDGE_SHARPNESS * 0.9f) * westFade;
                
                mountain = Math.Max(mountain, westValue * 0.75f);
            }
            
            // 4. SISTEMA SUR (extensión de la principal)
            if (y > MAP_SIZE * 0.65f)
            {
                // Continuación de la cordillera principal que se divide
                float southT = (y - MAP_SIZE * 0.65f) / (MAP_SIZE * 0.2f);
                southT = MathHelper.Clamp(southT, 0, 1);
                
                // Rama sureste
                float se_X = MAP_SIZE * 0.65f + southT * MAP_SIZE * 0.1f;
                float se_Y = MAP_SIZE * 0.65f + southT * MAP_SIZE * 0.15f;
                
                // Añadir ruido para hacerla más irregular
                se_X += Noise.Perlin(y * 0.00004f, 7000) * MAP_SIZE * 0.04f;
                se_Y += Noise.Perlin(x * 0.00004f, 8000) * MAP_SIZE * 0.03f;
                
                float distToSE = Vector2.Distance(new Vector2(x, y), new Vector2(se_X, se_Y));
                float seValue = Math.Max(0, 1f - distToSE / 8000f);
                
                // Bordes irregulares
                float seEdge = Noise.Perlin(x * 0.0004f, y * 0.0004f) * 0.4f + 0.6f;
                seValue *= seEdge;
                
                seValue = (float)Math.Pow(seValue, RIDGE_SHARPNESS * 0.85f) * (1f - southT * 0.3f);
                
                // Rama suroeste
                float sw_X = MAP_SIZE * 0.45f - southT * MAP_SIZE * 0.1f;
                float sw_Y = MAP_SIZE * 0.65f + southT * MAP_SIZE * 0.15f;
                
                // Añadir ruido
                sw_X += Noise.Perlin(y * 0.00005f, 9000) * MAP_SIZE * 0.035f;
                sw_Y += Noise.Perlin(x * 0.00005f, 10000) * MAP_SIZE * 0.025f;
                
                float distToSW = Vector2.Distance(new Vector2(x, y), new Vector2(sw_X, sw_Y));
                float swValue = Math.Max(0, 1f - distToSW / 7000f);
                
                // Bordes irregulares
                float swEdge = Noise.Perlin(x * 0.00035f, y * 0.00035f) * 0.35f + 0.65f;
                swValue *= swEdge;
                
                swValue = (float)Math.Pow(swValue, RIDGE_SHARPNESS * 0.9f) * (1f - southT * 0.4f);
                
                mountain = Math.Max(mountain, seValue * 0.7f);
                mountain = Math.Max(mountain, swValue * 0.65f);
            }
            
            // 5. Añadir algo de ruido para naturalidad
            float noise = Noise.Perlin(x * 0.0001f, y * 0.0001f);
            float detailNoise = Noise.Perlin(x * 0.0003f, y * 0.0003f) * 0.3f;
            mountain *= (0.85f + (noise + detailNoise) * NOISE_AMPLITUDE * 0.5f);  // Menos reducción
            
            // 6. Montañas secundarias muy ocasionales
            float secondary = Noise.RidgedMultifractal(x, y, 2, 0.00012f, 2.0f, 0.5f);
            if (secondary > 0.65f && mountain < 0.5f)  // Umbral más bajo
            {
                float secondaryValue = (secondary - 0.65f) * 2f;  // Más intensidad
                
                // Aplicar variación para que no sean uniformes
                float secVar = Noise.Perlin(x * 0.0002f, y * 0.0002f) * 0.5f + 0.5f;
                secondaryValue *= secVar;
                
                // Hacer que también respeten el fade del continente
                float localFade = 1f;
                if (distFromCenter > MAP_SIZE * 0.3f)
                {
                    localFade = 1f - (distFromCenter - MAP_SIZE * 0.3f) / (MAP_SIZE * 0.05f);
                    localFade = MathHelper.Clamp(localFade, 0, 1);
                }
                
                mountain = Math.Max(mountain, secondaryValue * 0.7f * localFade);
            }
            
            mountain = MathHelper.Clamp(mountain, 0, 1);
            
            // Aplicar el fade del continente
            mountain *= continentFade;
            
            // Deformar los bordes para hacerlos más orgánicos
            float edgeWarp = Noise.Perlin(x * EDGE_WARP_FREQ, y * EDGE_WARP_FREQ) * EDGE_WARP_AMP * 0.5f;
            float edgeWarp2 = Noise.Perlin(x * EDGE_WARP_FREQ * 2.5f, y * EDGE_WARP_FREQ * 2.5f) * EDGE_WARP_AMP * 0.25f;
            float threshold = 0.12f + edgeWarp + edgeWarp2;  // Umbral más bajo
            
            // Transición suave en el umbral con más variación
            float transitionWidth = 0.08f + Noise.Perlin(x * 0.0001f, y * 0.0001f) * 0.04f;
            if (mountain > threshold - transitionWidth && mountain < threshold + transitionWidth)
            {
                float transitionT = (mountain - (threshold - transitionWidth)) / (transitionWidth * 2f);
                float smooth = transitionT * transitionT * (3f - 2f * transitionT);
                
                // Añadir algo de ruido a la transición
                smooth += (Noise.Perlin(x * 0.0008f, y * 0.0008f) - 0.5f) * 0.1f;
                smooth = MathHelper.Clamp(smooth, 0, 1);
                
                return smooth > 0.5f ? Color.Gray : Color.Green;
            }
            
            // Umbral para determinar qué es montaña
            return mountain > threshold ? Color.Gray : Color.Green;
        }
    }
}