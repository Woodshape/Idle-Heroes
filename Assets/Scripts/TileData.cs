using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "tile", menuName = "Tiles/Tile")]
public class TileData : ScriptableObject {
    public string tileName;
    public Sprite[] sprites;

    public bool isSolid;
}
