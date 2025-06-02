// Terrain/ITerrainGenerator.cs
using Microsoft.Xna.Framework;

namespace hm8.Terrain
{
    public interface ITerrainGenerator
    {
        /// <summary>
        /// Devuelve el color de terreno para estas coordenadas.
        /// </summary>
        Color GetColor(float worldX, float worldY);
    }
}