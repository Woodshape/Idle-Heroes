using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour {
    public TileAtlas tileAtlas;
    
    [BoxGroup("World Generation")]
    public int worldSize = 100;
    [BoxGroup("World Generation")]
    public int worldHeight = 100;
    [BoxGroup("World Generation")]
    [MinValue(1f)]
    public float worldHeightMultiplier = 30f;

    [BoxGroup("Chunk Generation")]
    [MinValue(1)]
    public int chunkSize = 16;

    [BoxGroup("Biomes")]
    public Biome[] biomes;
    
    [BoxGroup("Terrain Generation")]
    public float terrainFrequency = 0.05f;
    
    public bool generateCaves = true;
    
    [BoxGroup("generateCaves/Cave Generation")]
    [ShowIfGroup("generateCaves")]
    [Range(0f, 1f)]
    public float caveThreshold = 0.25f;
    [BoxGroup("generateCaves/Cave Generation")]
    [ShowIfGroup("generateCaves")]
    public float caveFrequency = 0.05f;

    [BoxGroup("Biome Generation")]
    public float biomeFrequency = 0.05f;
    [BoxGroup("Biome Generation")]
    public Gradient biomeGradient;

    public List<GameObject> chunks = new List<GameObject>();
    public List<Tile> tiles = new List<Tile>();

    private Texture2D caveNoiseTexture;
    private Texture2D biomeNoiseTexture;

    private int seed;

    private void Reset() {
        foreach (GameObject chunk in chunks) {
            DestroyImmediate(chunk);
        }

        chunks.Clear();
        tiles.Clear();

        caveNoiseTexture = null;
        // oreNoiseTextures.Clear();
    }

    // Start is called before the first frame update
    void Start() {
        Generate();
    }

    private void GenerateTerrain() {
        for (int x = 0; x < worldSize; x++) {
            int xMod = x + seed;
            float height = Mathf.PerlinNoise(xMod * terrainFrequency, seed * terrainFrequency) * worldHeightMultiplier + worldHeight;

            for (int y = 0; y < height; y++) {
                Tile tile = GetTileForHeight(x, y, height);

                if (tile == null) {
                    Debug.LogWarning($"No tile for coordinates: {x}, {y}");
                    continue;
                }
                
                // CreateAndPlaceTile(tile, x, y);

                Biome biome = GetBiomeForPosition(x, y);
                if (biome == null) {
                    Debug.LogWarning($"::GenerateTerrain -> Could not find any biome for position: {x},{y}");
                }

                //  Place tiles only if noiseTexture did not "generate" a cave or we are at bedrock level
                if (y == 0 || !generateCaves || caveNoiseTexture.GetPixel(x, y) == Color.white) {
                    CreateAndPlaceTile(tile, x, y);
                }

                //  PLace trees and long grass
                if (y > height - 1) {
                    Tile t = TileAtPosition(x, y);
                    
                    int treeChance = Random.Range(0, biome.treeChance);
                    if (treeChance == 1) {
                        //  Make sure that the tree is placed on solid ground (viz. grass)
                        if (t != null && t == biome.tileAtlas.grass) {
                            GenerateAndPlaceTree(biome, Random.Range(biome.treeSizeMin, biome.treeSizeMax + 1), x, y);
                        }
                    }
                        
                    int grassChance = Random.Range(0, biome.longGrassChance);
                    if (grassChance == 1) {
                        Tile top = TileAtPosition(x, y + 1);
                        //  Make sure that the long grass is placed on solid ground (viz. grass) and not on top of any other object
                        if (t != null && t == biome.tileAtlas.grass && top == null) {
                            GenerateAndPlaceLongGrass(biome, x, y);
                        }
                    }
                }
            }
        }
    }

    private void GenerateChunks() {
        int numChunks = worldSize / chunkSize;

        for (int i = 0; i < numChunks; i++) {
            GameObject chunk = new GameObject{name = i.ToString()};
            chunk.transform.parent = transform;
            
            chunks.Add(chunk);
        }
    }

    private Tile TileAtPosition(int x, int y) {
        foreach (Tile tile in tiles) {
            if (tile.IsAtPosition(x, y)){
                // Debug.Log($"Tile {tile} at position: {x}, {y}");
                return tile;
            }
        }

        return null;
    }

    private Tile GetTileForHeight(int x, int y, float height) {
        Biome biome = GetBiomeForPosition(x, y);
        if (biome == null) {
            Debug.LogWarning($"::GetTileForHeight -> Could not find any biome for position: {x},{y}");
            return null;
        }
        if (biome.tileAtlas == null) {
            Debug.LogWarning("Could not find any tile atlas for biome: " + biome);
            return null;
        }
        
        //  Default stone
        Tile tile = biome.tileAtlas.stone;

        if (y == 0) {
            //  Bedrock level
            tile = biome.tileAtlas.bedrock;
        }
        else if (y < height - biome.dirtLayerHeight) {
            //  Stone level
            
            foreach (Ore ore in biome.ores) {
                // (height - y) determines the max distance from the top layer the ore can spawn
                //  i.e. 0 means ore can spawn at every level, 40 means ore can only spawn 40 "y-layers" deep
                if (ore.spreadTexture.GetPixel(x, y) == Color.white && 
                    height - y > ore.maxSpawnHeight) {
                    tile = ore;
                }
            }
        }
        else if (y < height - 1) {
            //  Place dirt above stone layer and one tile below surface
            tile = biome.tileAtlas.dirt;
        }
        else {
            //  Top layer
            tile = biome.tileAtlas.grass;
        }

        return tile;
    }

    private Biome GetBiomeForPosition(int x, int y) {
        foreach (Biome biome in biomes) {
            if (biome.Color.Equals(biomeNoiseTexture.GetPixel(x, y))) {
                return biome;
            }
        }
        
        return null;
    }

    private void GenerateAndPlaceTree(Biome biome, int treeHeight, int x, int y) {
        if (biome == null || biome.tileAtlas == null) {
            Debug.LogWarning("::GenerateAndPlaceTree -> No biome or tree atlas defined...");
            return;
        }

        if (biome.tileAtlas.log == null)
            return;

        //  Place logs one tile above ground
        for (int i = 1; i <= treeHeight; i++) {
            CreateAndPlaceTile(biome.tileAtlas.log, x, y + i);
        }

        if (biome.tileAtlas.leaf == null)
            return;

        // Place leaves above log height
        CreateAndPlaceTile(biome.tileAtlas.leaf, x, y + treeHeight + 1);
        CreateAndPlaceTile(biome.tileAtlas.leaf, x, y + treeHeight + 2);
        CreateAndPlaceTile(biome.tileAtlas.leaf, x, y + treeHeight + 3);
        
        // Place leaves to the left and right only if we are not at the world's outer bounds
        if (x > 0) {
            CreateAndPlaceTile(biome.tileAtlas.leaf, x - 1, y + treeHeight + 1);
            CreateAndPlaceTile(biome.tileAtlas.leaf, x - 1, y + treeHeight + 2);
        }
        if (x < worldSize - 1) {
            CreateAndPlaceTile(biome.tileAtlas.leaf, x + 1, y + treeHeight + 1);
            CreateAndPlaceTile(biome.tileAtlas.leaf, x + 1, y + treeHeight + 2);
        }
    }

    private void GenerateAndPlaceLongGrass(Biome biome, int x, int y) {
        if (biome == null || biome.tileAtlas == null) {
            Debug.LogWarning("::GenerateAndPlaceTree -> No biome or tree atlas defined...");
            return;
        }

        if (biome.tileAtlas.longGrass != null) {
            CreateAndPlaceTile(biome.tileAtlas.longGrass, x, y + 1);
        }
    }

    private void CreateAndPlaceTile(Tile tile, int x, int y) {
        GameObject newTile = new GameObject {name = tile.tileName};

        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        newTile.transform.parent = chunks.ElementAt((int) chunkCoord).transform;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        Sprite sprite = tile.sprites[Random.Range(0, tile.sprites.Length)];
        
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = sprite;
        
        tile.CreateAt(x, y);

        // Debug.Log($"Spawning {tile.tileName} at: {x}, {y}");

        tiles.Add(tile);
    }

    private void GenerateNoiseTexture(Texture2D noiseTexture, float frequency, float threshold) {
        int seedMod = Random.Range(-1000000, 1000000);
        for (int x = 0; x < noiseTexture.width; x++) {
            for (int y = 0; y < noiseTexture.height; y++) {
                int xMod = x + seed + seedMod;
                int yMod = y + seed + seedMod;

                float value = Mathf.PerlinNoise(xMod * frequency, yMod * frequency);

                noiseTexture.SetPixel(x, y, value > threshold ? Color.white : Color.black);
            }
        }

        noiseTexture.Apply();
    }
    
    private void GenerateBiomeTexture() {
        int seedMod = Random.Range(-1000000, 1000000);
        for (int x = 0; x < biomeNoiseTexture.width; x++) {
            for (int y = 0; y < biomeNoiseTexture.height; y++) {
                int xMod = x + seed + seedMod;
                int yMod = y + seed + seedMod;

                // float value = Mathf.PerlinNoise(xMod * biomeFrequency, seed * biomeFrequency);
                float value = Mathf.PerlinNoise(xMod * biomeFrequency, yMod * biomeFrequency);
                
                //  Pick color according to the biome color gradient from the perlin noise value
                Color color = biomeGradient.Evaluate(value);
                
                //  Apply color to noise map
                biomeNoiseTexture.SetPixel(x, y, color);
            }
        }

        biomeNoiseTexture.Apply();
    }

    [Button(ButtonSizes.Large)]
    private void Generate() {
        if (!Application.isPlaying)
            return;

        seed = Random.Range(-1000000, 1000000);

        Reset();

        GenerateNoiseTextures();

        GenerateChunks();
        GenerateTerrain();
    }

    private void GenerateNoiseTextures() {
        //  Generate biomes
        biomeNoiseTexture = new Texture2D(worldSize, worldSize);
        GenerateBiomeTexture();

        caveNoiseTexture = new Texture2D(worldSize, worldHeight);
        GenerateNoiseTexture(caveNoiseTexture, caveFrequency, caveThreshold);

        foreach (Biome biome in biomes) {
            //  Generate caves
            biome.caveNoiseTexture = new Texture2D(worldSize, worldHeight);
            GenerateNoiseTexture(biome.caveNoiseTexture, biome.caveFrequency, biome.caveThreshold);

            //  Generate ores
            foreach (Ore ore in biome.ores) {
                ore.spreadTexture = new Texture2D(worldSize, worldHeight);
                GenerateNoiseTexture(ore.spreadTexture, ore.rarity, ore.size);
            }
        }
    }
}
