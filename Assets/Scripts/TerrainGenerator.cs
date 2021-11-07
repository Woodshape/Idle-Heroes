using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour {
    public static TerrainGenerator Instance;

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

    private Texture2D _caveNoiseTexture;
    private Texture2D _biomeNoiseTexture;

    private int _seed;
    private bool _validated;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }

    // private void Start() {
    //     Generate();
    //
    //     _validated = true;
    // }

    private void OnValidate() {
        if (!_validated)
            return;
        
        Generate();
    }

    private void Reset() {
        foreach (GameObject chunk in chunks) {
            DestroyImmediate(chunk);
        }

        chunks.Clear();

        // caveNoiseTexture = null;
        // oreNoiseTextures.Clear();
    }

    private void GenerateTerrain() {
        for (int x = 0; x < worldSize; x++) {
            int xMod = x + _seed;
            float height = Mathf.PerlinNoise(xMod * terrainFrequency, _seed * terrainFrequency) * worldHeightMultiplier + worldHeight;

            for (int y = 0; y < height; y++) {
                Biome biome = GetBiomeForPosition(x, y);
                if (biome == null) {
                    continue;
                }

                PlaceTileForBiome(biome, x, y, height);
                PlaceFloraForBiome(biome, x, y, height);
            }
        }
    }
    
    private void PlaceTileForBiome(Biome biome, int x, int y, float height) {
        TileData tileData = GetTileForHeight(biome, x, y, height);
        //  Place tiles only if noiseTexture did not "generate" a cave or we are at bedrock level
        if (y == 0 || !generateCaves || _caveNoiseTexture.GetPixel(x, y) == Color.white) {
            CreateAndPlaceTile(tileData, x, y);
        }
    }
    
    private void PlaceFloraForBiome(Biome biome, int x, int y, float height) { //  PLace trees and long grass
        if (y > height - 1) {
            Tile t = World.Instance.TileAtPosition(x, y);

            if (t == null) {
                Debug.LogWarning($"No tile at position: {x},{y}");
                return;
            }

            int treeChance = Random.Range(0, biome.treeChance);
            if (treeChance == 1) {
                //  Make sure that the tree is placed on solid ground (viz. grass)
                if (t != null && t == biome.tileAtlas.grass) {
                    GenerateAndPlaceTree(biome, Random.Range(biome.treeSizeMin, biome.treeSizeMax + 1), x, y);
                }
            }

            int grassChance = Random.Range(0, biome.longGrassChance);
            if (grassChance == 1) {
                Tile top = World.Instance.TileAtPosition(x, y + 1);

                //  Make sure that the long grass is placed on solid ground (viz. grass) and not on top of any other object
                if (t != null && t == biome.tileAtlas.grass && top == null) {
                    GenerateAndPlaceLongGrass(biome, x, y);
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

    private TileData GetTileForHeight(Biome biome, int x, int y, float height) {
        if (biome == null) {
            Debug.LogWarning($"::GetTileForHeight -> Could not find any biome for position: {x},{y}");
            return null;
        }
        if (biome.tileAtlas == null) {
            Debug.LogWarning("Could not find any tile atlas for biome: " + biome);
            return null;
        }
        
        //  Default stone
        TileData tileData = biome.tileAtlas.stone;

        if (y == 0) {
            //  Bedrock level
            tileData = biome.tileAtlas.bedrock;
        }
        else if (y < height - biome.dirtLayerHeight) {
            //  Stone level
            foreach (Ore ore in biome.ores) {
                // (height - y) determines the max distance from the top layer the ore can spawn
                //  i.e. 0 means ore can spawn at every level, 40 means ore can only spawn 40 "y-layers" deep or below
                if (ore.spreadTexture.GetPixel(x, y) == Color.white &&
                    height - y > ore.maxSpawnHeight) {
                    tileData = ore;
                }
            }
        }
        else if (y < height - 1) {
            //  Place dirt above stone layer and one tile below surface
            tileData = biome.tileAtlas.dirt;
        }
        else {
            //  Top layer
            tileData = biome.tileAtlas.grass;
        }

        return tileData;
    }

    private Biome GetBiomeForPosition(int x, int y) {
        foreach (Biome biome in biomes) {
            if (biome.Color.Equals(_biomeNoiseTexture.GetPixel(x, y))) {
                return biome;
            }
        }
        
        Debug.LogWarning($"::GenerateTerrain -> Could not find any biome for position: {x},{y}");
        
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

    private void CreateAndPlaceTile(TileData tileData, int x, int y) {
        GameObject newTile = new GameObject {name = tileData.tileName};

        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        newTile.transform.parent = chunks.ElementAt((int) chunkCoord).transform;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        Sprite sprite = tileData.sprites[Random.Range(0, tileData.sprites.Length)];
        
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = sprite;

        if (tileData.isSolid) {
            newTile.AddComponent<BoxCollider2D>();

            Rigidbody2D rb = newTile.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            // rb.simulated = true;

            newTile.tag = "Ground";
        }

        Tile tile = newTile.AddComponent<Tile>();
        tile.Create(tileData, x, y);

        // Debug.Log($"Spawning {tile.tileName} at: {x}, {y}");
        
        if (World.Instance != null) {
            World.Instance.tiles.Add(tile);
        }
    }

    private void GenerateNoiseTexture(Texture2D noiseTexture, float frequency, float threshold) {
        int seedMod = Random.Range(-1000000, 1000000);
        for (int x = 0; x < noiseTexture.width; x++) {
            for (int y = 0; y < noiseTexture.height; y++) {
                int xMod = x + _seed + seedMod;
                int yMod = y + _seed + seedMod;

                float value = Mathf.PerlinNoise(xMod * frequency, yMod * frequency);

                noiseTexture.SetPixel(x, y, value > threshold ? Color.white : Color.black);
            }
        }

        noiseTexture.Apply();
    }

    private void GenerateBiomeTexture() {
        int seedMod = Random.Range(-1000000, 1000000);
        for (int x = 0; x < _biomeNoiseTexture.width; x++) {
            for (int y = 0; y < _biomeNoiseTexture.height; y++) {
                int xMod = x + _seed + seedMod;
                int yMod = y + _seed + seedMod;

                // float value = Mathf.PerlinNoise(xMod * biomeFrequency, seed * biomeFrequency);
                float value = Mathf.PerlinNoise(xMod * biomeFrequency, yMod * biomeFrequency);
                
                //  Pick color according to the biome color gradient from the perlin noise value
                Color color = biomeGradient.Evaluate(value);
                
                //  Apply color to noise map
                _biomeNoiseTexture.SetPixel(x, y, color);
            }
        }

        _biomeNoiseTexture.Apply();
    }

    [Button(ButtonSizes.Large)]
    public void Generate() {
        if (!Application.isPlaying || World.Instance == null)
            return;

        _seed = Random.Range(-1000000, 1000000);

        Reset();

        GenerateNoiseTextures();

        GenerateChunks();
        GenerateTerrain();
    }

    private void GenerateNoiseTextures() {
        _caveNoiseTexture = new Texture2D(worldSize, worldSize);
        GenerateNoiseTexture(_caveNoiseTexture, caveFrequency, caveThreshold);

        //  Generate biomes
        _biomeNoiseTexture = new Texture2D(worldSize, worldSize);
        GenerateBiomeTexture();
        
        foreach (Biome biome in biomes) {
            //  Generate caves
            // biome.caveNoiseTexture = new Texture2D(worldSize, worldHeight);
            // GenerateNoiseTexture(biome.caveNoiseTexture, biome.caveFrequency, biome.caveThreshold);

            //  Generate ores
            foreach (Ore ore in biome.ores) {
                //  We don't need to generate ores "above ground level", thus we only use worldHeight here
                ore.spreadTexture = new Texture2D(worldSize, worldHeight);
                GenerateNoiseTexture(ore.spreadTexture, ore.rarity, ore.size);
            }
        }
    }
}
