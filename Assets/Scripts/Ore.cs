using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ore", menuName = "Tiles/Ore")]
public class Ore : TileData
{
    [Range(0f, 1f)]
    public float rarity;
    [Range(0f, 1f)]
    public float size;
    public int maxSpawnHeight;
    
    public Sprite oreSprite;

    public Texture2D spreadTexture;
}
