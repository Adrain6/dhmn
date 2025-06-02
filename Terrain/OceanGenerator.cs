// Terrain/OceanGenerator.cs
using System;
using Microsoft.Xna.Framework;
using hm8.Utils;

namespace hm8.Terrain
{
    public class OceanGenerator : ITerrainGenerator
    {
        // Dimensiones del mapa (en coordenadas de mundo)
        private const float MAP_WIDTH = 101 * 32 * 32f;   // 101 chunks * 32 tiles * 32 pixels
        private const float MAP_HEIGHT = 101 * 32 * 32f;  // 101 chunks * 32 tiles * 32 pixels
        
        // Centro del mapa
        private static readonly Vector2 MAP_CENTER = new(MAP_WIDTH * 0.5f, MAP_HEIGHT * 0.5f);
        
        // NUEVO: Usar múltiples formas base para crear el continente
        // Cada forma tiene: centro, ancho, alto, rotación
        private static readonly ContinentShape[] CONTINENT_SHAPES = new[]
        {
            // Forma principal - más pequeña y desplazada
            new ContinentShape(
                MAP_CENTER + new Vector2(-MAP_WIDTH * 0.08f, -MAP_WIDTH * 0.05f),
                MAP_WIDTH * 0.28f,  // ancho reducido
                MAP_WIDTH * 0.22f,  // alto reducido
                0.4f                // rotación
            ),
            // Extensión noreste - península
            new ContinentShape(
                MAP_CENTER + new Vector2(MAP_WIDTH * 0.12f, -MAP_WIDTH * 0.08f),
                MAP_WIDTH * 0.15f,
                MAP_WIDTH * 0.25f,
                -0.7f
            ),
            // Masa sur - irregular
            new ContinentShape(
                MAP_CENTER + new Vector2(-MAP_WIDTH * 0.05f, MAP_WIDTH * 0.12f),
                MAP_WIDTH * 0.2f,
                MAP_WIDTH * 0.15f,
                2.3f
            ),
            // Extensión oeste
            new ContinentShape(
                MAP_CENTER + new Vector2(-MAP_WIDTH * 0.18f, MAP_WIDTH * 0.02f),
                MAP_WIDTH * 0.12f,
                MAP_WIDTH * 0.18f,
                1.2f
            ),
            // Conexión central
            new ContinentShape(
                MAP_CENTER + new Vector2(MAP_WIDTH * 0.03f, MAP_WIDTH * 0.03f),
                MAP_WIDTH * 0.18f,
                MAP_WIDTH * 0.16f,
                -0.3f
            )
        };
        
        // Radio donde empiezan a aparecer islas
        private const float MIN_ISLAND_DISTANCE = MAP_WIDTH * 0.32f;
        
        // Radio donde empieza el océano puro
        private const float OCEAN_START_RADIUS = MAP_WIDTH * 0.65f;
        
        // Domain warping fuerte para deformación
        private const float WARP_FREQ = 0.00002f;
        private const float WARP_AMP = 600f;
        
        // Segunda capa de warping
        private const float WARP2_FREQ = 0.00008f;
        private const float WARP2_AMP = 250f;
        
        // Tercera capa para variación macro
        private const float MACRO_WARP_FREQ = 0.000005f;
        private const float MACRO_WARP_AMP = 1200f;
        
        // Ruido para bordes muy irregulares
        private const float EDGE_NOISE_FREQ = 0.0002f;
        private const float EDGE_NOISE_AMP = 0.5f;
        
        // Ruido para islas
        private const float ISLAND_FREQ = 0.00015f;
        private const float ISLAND_THRESHOLD = 0.62f;

        private struct ContinentShape
        {
            public Vector2 Center;
            public float Width;
            public float Height;
            public float Rotation;

            public ContinentShape(Vector2 center, float width, float height, float rotation)
            {
                Center = center;
                Width = width;
                Height = height;
                Rotation = rotation;
            }
        }

        public Color GetColor(float x, float y)
        {
            // Domain warping macro para variación a gran escala
            float macroWarpX = Noise.Perlin(x * MACRO_WARP_FREQ, y * MACRO_WARP_FREQ) * MACRO_WARP_AMP;
            float macroWarpY = Noise.Perlin((x + 7000) * MACRO_WARP_FREQ, (y + 7000) * MACRO_WARP_FREQ) * MACRO_WARP_AMP;
            
            // Domain warping medio
            float warp1X = Noise.Perlin(x * WARP_FREQ, y * WARP_FREQ) * WARP_AMP;
            float warp1Y = Noise.Perlin((x + 5000) * WARP_FREQ, (y + 5000) * WARP_FREQ) * WARP_AMP;
            
            // Domain warping fino
            float warp2X = Noise.Perlin(x * WARP2_FREQ, y * WARP2_FREQ) * WARP2_AMP;
            float warp2Y = Noise.Perlin((x + 3000) * WARP2_FREQ, (y + 3000) * WARP2_FREQ) * WARP2_AMP;
            
            float warpedX = x + macroWarpX + warp1X + warp2X;
            float warpedY = y + macroWarpY + warp1Y + warp2Y;
            
            // Verificar si está dentro del continente
            float continentValue = 0f;
            
            foreach (var shape in CONTINENT_SHAPES)
            {
                // Transformar el punto al espacio local de la forma
                float dx = warpedX - shape.Center.X;
                float dy = warpedY - shape.Center.Y;
                
                // Rotar el punto
                float cos = (float)Math.Cos(-shape.Rotation);
                float sin = (float)Math.Sin(-shape.Rotation);
                float localX = dx * cos - dy * sin;
                float localY = dx * sin + dy * cos;
                
                // Calcular distancia elíptica normalizada
                float distX = localX / shape.Width;
                float distY = localY / shape.Height;
                float ellipseDist = distX * distX + distY * distY;
                
                // Aplicar ruido a los bordes para hacerlos irregulares
                float edgeNoise = Noise.Fbm(
                    warpedX * EDGE_NOISE_FREQ, 
                    warpedY * EDGE_NOISE_FREQ, 
                    3, 
                    1.0f, 
                    2.0f, 
                    0.5f
                );
                
                // Valor de la forma con bordes irregulares
                float shapeValue = 1.0f - (ellipseDist * (1.0f + (edgeNoise - 0.5f) * EDGE_NOISE_AMP));
                
                // Suavizar los bordes
                shapeValue = MathHelper.Clamp(shapeValue * 3.0f, 0, 1);
                
                // Combinar con el valor existente (unión de formas)
                continentValue = Math.Max(continentValue, shapeValue);
            }
            
            // Si el valor del continente es alto, no es océano
            if (continentValue > 0.5f)
            {
                return Color.Transparent;
            }
            
            // Distancia al centro para océano exterior
            float distanceFromCenter = Vector2.Distance(new Vector2(warpedX, warpedY), MAP_CENTER);
            
            // Océano puro en los bordes
            if (distanceFromCenter > OCEAN_START_RADIUS)
            {
                return new Color(0, 50, 150);
            }
            
            // Zona de islas - basada en el valor del continente
            if (continentValue < 0.1f && distanceFromCenter > MIN_ISLAND_DISTANCE)
            {
                // Ruido para islas
                float islandNoise = Noise.Fbm(x, y, 4, ISLAND_FREQ, 2.0f, 0.5f);
                
                // Variación adicional para romper patrones
                float variation = Noise.Perlin(x * 0.00005f, y * 0.00005f) * 0.2f;
                
                // Factor de distancia para reducir islas cerca del continente
                float distanceFactor = (distanceFromCenter - MIN_ISLAND_DISTANCE) / 
                                     (OCEAN_START_RADIUS - MIN_ISLAND_DISTANCE);
                distanceFactor = MathHelper.Clamp(distanceFactor, 0, 1);
                
                // Umbral ajustado
                float adjustedThreshold = ISLAND_THRESHOLD - (distanceFactor * 0.15f) + variation;
                
                if (islandNoise > adjustedThreshold)
                {
                    return Color.Transparent; // Es una isla
                }
            }
            
            // Por defecto: océano
            return new Color(0, 50, 150);
        }
    }
}