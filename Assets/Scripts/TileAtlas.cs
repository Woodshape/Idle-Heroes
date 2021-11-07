using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Atlas", menuName = "Tiles/Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [BoxGroup("Tiles")]
    public TileData stone;
    [BoxGroup("Tiles")]
    public TileData stoneWall;
    [BoxGroup("Tiles")]
    public TileData bedrock;
    [BoxGroup("Tiles")]
    public TileData dirt;
    [BoxGroup("Tiles")]
    public TileData dirtWall;
    [BoxGroup("Tiles")]
    public TileData grass;
    [BoxGroup("Tiles")]
    public TileData log;
    [BoxGroup("Tiles")]
    public TileData leaf;
    [BoxGroup("Tiles")]
    public TileData longGrass;
}
