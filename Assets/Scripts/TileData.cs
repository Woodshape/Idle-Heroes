using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "tile", menuName = "Tiles/Tile")]
public class TileData : ScriptableObject {
    public string tileName;
    public TileType tileType;
    public Sprite[] sprites;

    public bool isBreakable;
    public bool isSolid;
    public bool inBackground;
    public bool dropTile;
    
    public enum TileType {
        None,
        Stone,
        Dirt,
        Sand,
        Grass,
        Ore
    }
}
