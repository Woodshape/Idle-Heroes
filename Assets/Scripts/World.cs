using System.Collections.Generic;
using System.Linq;
using Controller;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour {
    public static World Instance;

    public CameraController cameraController;

    public GameObject player;
    
    public GameObject playerPrefab;
    public GameObject tileDropPrefab;
    
    public Vector2 mousePosition;
    
    public Text coordinatesText;
    public Text placeText;

    public List<Tile> tiles = new List<Tile>();
    
    private TerrainGenerator _terrainGenerator;
    private Camera _camera;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        _terrainGenerator = GetComponent<TerrainGenerator>();
        _camera = Camera.main;
    }

    private void Start() {
        _terrainGenerator.Generate();
        SpawnPlayer();

        if (player != null && cameraController != null) {
            cameraController.FollowTarget(player);
        }
    }

    private void Update() {
        UpdateMousePosition();
    }

    public Tile TileAtPosition(int x, int y) {
        foreach (Tile tile in tiles) {
            if (tile.IsAtPosition(x, y)){
                // Debug.Log($"Tile {tile} at position: {x}, {y}");
                return tile;
            }
        }

        return null;
    }
    
    public void PlaceTile(TileData selectedTile, int x, int y, bool replace) {
        if (replace) {
            Tile tileForPosition = TileAtPosition(x, y);
            //  Only replace tile if it's not the same type (i.e. stone with dirt)
            if (tileForPosition != null && tileForPosition.data.tileType != selectedTile.tileType) {
                Debug.LogWarning($"Tile position {x},{y} not empty: {tileForPosition}. Replacing tile with: {selectedTile}");

                RemoveTile(tileForPosition);
            }
        }

        _terrainGenerator.CreateAndPlaceTile(selectedTile, x, y);
    }

    public bool RemoveTile(Tile tile) {
        if (tile != null && tiles.Contains(tile) && (tile.data.isBreakable || tile.data.inBackground)) {
            // Debug.Log($"REMOVING Tile: {tile.data} [{tile.X},{tile.Y}]");

            if (tile.data.dropTile) {
                DropTile(tile);
            }
            
            tiles.Remove(tile);
            Destroy(tile.gameObject);

            //  Check whether we need to replace stone or ore tile with stone wall after removing it
            Biome biome = _terrainGenerator.GetBiomeForPosition(tile.X, tile.Y);
            if (biome != null) {
                if (tile.data == biome.tileAtlas.stone || tile.data is Ore) {
                    PlaceTile(biome.tileAtlas.stoneWall, tile.X, tile.Y, false);
                }else if (tile.data == biome.tileAtlas.dirt) {
                    PlaceTile(biome.tileAtlas.dirtWall, tile.X, tile.Y, false);
                }
            }

            return true;
        }
        
        Debug.LogWarning("Tile not in world or unbreakable...");

        return false;
    }
    
    public bool RemoveTile(int x, int y, bool dropTile) {
        Tile tileToRemove = TileAtPosition(x, y);

        if (tileToRemove != null) {
            return RemoveTile(tileToRemove);
        }

        return false;
    }
    
    public void DropTile(Tile tile) {
        float xPos = tile.X + 0.5f + Random.Range(-0.5f, 0.5f);
        GameObject tileObject = Instantiate(tileDropPrefab, new Vector2(xPos, tile.Y + 1f), Quaternion.identity);

        TileDrop tileDrop = tileObject.GetComponent<TileDrop>();
        SpriteRenderer tileSprite = tileObject.GetComponent<SpriteRenderer>();

        if (tileDrop == null || tileSprite == null) {
            Debug.LogError("TileDrop missing Script or SpriteRenderer!");
            return;
        }
        
        if (tile.data is Ore ore) {
            tileDrop.data = ore;
            tileSprite.sprite = ore.oreSprite;
        }
        else {
            tileDrop.data = tile.data;
            tileSprite.sprite = tile.data.sprites[0];
        }

        if (tile.data.isSolid) {
            tileObject.tag = "Ground";
        }
    }

    public float GetDistanceToMouse(Vector2 source) {
        UpdateMousePosition();
        
        return GetDistance(source, mousePosition);
    }

    public float GetDistance(Vector2 source, Vector2 target) {
        return Vector2.Distance(source, target);
    }
    
    public void SetPlace(bool place) {
        if (placeText != null) {
            placeText.text = place ? "Place" : "Destroy";
        }
    }

    // private void UpdateWorldTiles() {
    //     List<Tile> tilesToDestroy = new List<Tile>();
    //     
    //     foreach (Tile tile in tiles) {
    //         if (tile.data.tileName.Equals("Grass")) {
    //             //  Check tile below grass
    //             if (TileAtPosition(tile.X, tile.Y - 1) == null) {
    //                 tilesToDestroy.Add(tile);
    //             }
    //         }
    //     }
    //
    //     foreach (Tile tile in tilesToDestroy) {
    //         tilesToDestroy.Remove(tile);
    //         Destroy(tile.gameObject);
    //     }
    // }

    private void SpawnPlayer() {
        Vector2 playerSpawn = DeterminePlayerSpawn();

        if (playerSpawn == Vector2.zero) {
            Debug.LogWarning("Could not find valid spawn position for player...");
            return;
        }
        
        player = Instantiate(playerPrefab, new Vector3(playerSpawn.x, playerSpawn.y + 5, 0), Quaternion.identity);
    }
    
    private Vector2 DeterminePlayerSpawn() {
        if (playerPrefab == null) {
            Debug.LogWarning("No player prefab found to spawn...");
            return Vector2.zero;
        }
        
        //  Player can spawn on any "Grass" tile type
        List<Vector2> possibleSpawnLocations = new List<Vector2>();
        foreach (Tile tile in tiles.Where(tile => tile.data.tileType == TileData.TileType.Grass)) {
            possibleSpawnLocations.Add(new Vector2(tile.X, tile.Y));
        }

        if (possibleSpawnLocations.IsNullOrEmpty()) {
            Debug.LogWarning("No spawn position found...");
            return Vector2.zero;
        }

        return possibleSpawnLocations[Random.Range(0, possibleSpawnLocations.Count)];
    }
    
    private void UpdateMousePosition() {
        mousePosition.x = Mathf.RoundToInt(_camera.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePosition.y = Mathf.RoundToInt(_camera.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);

        if (coordinatesText != null) {
            coordinatesText.text = $"X: {mousePosition.x}, Y: {mousePosition.y}";
        }
    }
}
