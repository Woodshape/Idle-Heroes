using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "biome", menuName = "Biomes/Biome")]
public class Biome : ScriptableObject {
    public Color Color;
    
    [BoxGroup("Tile Settings")]
    public TileAtlas tileAtlas;

    public bool generateCaves = true;

    [BoxGroup("generateCaves/Cave Generation")]
    [ShowIfGroup("generateCaves")]
    public Texture2D caveNoiseTexture;
    [BoxGroup("generateCaves/Cave Generation")]
    [ShowIfGroup("generateCaves")]
    [Range(0f, 1f)]
    public float caveThreshold = 0.25f;
    [BoxGroup("generateCaves/Cave Generation")]
    [ShowIfGroup("generateCaves")]
    public float caveFrequency = 0.05f;

    [BoxGroup("Biome Generation")]
    public int dirtLayerHeight = 5;
    [BoxGroup("Biome Generation")]
    public int longGrassChance = 6;
    [BoxGroup("Biome Generation")]
    public int treeChance = 10;
    [BoxGroup("Biome Generation")]
    public int treeSizeMin = 3;
    [BoxGroup("Biome Generation")]
    public int treeSizeMax = 6;

    [BoxGroup("Ore Settings")]
    public Ore[] ores;

    private void OnValidate() {
        if (!Application.isPlaying)
            return;
        
        TerrainGenerator.Instance.Generate();
    }
}
