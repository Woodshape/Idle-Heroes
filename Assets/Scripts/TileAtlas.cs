using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Atlas", menuName = "Tiles/Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [BoxGroup("Tiles")]
    public Tile stone;
    [BoxGroup("Tiles")]
    public Tile bedrock;
    [BoxGroup("Tiles")]
    public Tile dirt;
    [BoxGroup("Tiles")]
    public Tile grass;
    [BoxGroup("Tiles")]
    public Tile log;
    [BoxGroup("Tiles")]
    public Tile leaf;
    [BoxGroup("Tiles")]
    public Tile longGrass;
}
