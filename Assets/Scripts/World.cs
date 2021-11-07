using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller;
using UnityEngine;

public class World : MonoBehaviour {
    public static World Instance;

    public CameraController cameraController;

    public GameObject player;
    
    public GameObject playerPrefab;

    public List<Tile> tiles = new List<Tile>();
    
    private TerrainGenerator _terrainGenerator;

    private Vector2 _playerSpawn;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        _terrainGenerator = GetComponent<TerrainGenerator>();
    }

    private void Start() {
        _terrainGenerator.Generate();
        DeterminePlayerSpawn();

        if (player != null && cameraController != null) {
            cameraController.FollowTarget(player);
        }
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
    
    public bool RemoveTile(int x, int y) {
        Tile tileToRemove = TileAtPosition(x, y);

        // Debug.Log("Trying to remove: " + tileToRemove);

        if (tileToRemove != null) {

            Debug.Log($"REMOVING Tile: {tileToRemove.data} [{tileToRemove.X},{tileToRemove.Y}]");
            
            tiles.Remove(tileToRemove);
            Destroy(tileToRemove.gameObject);

            return true;
        }
        else {
            
        }

        return false;
    }

    private void DeterminePlayerSpawn() {
        if (playerPrefab == null) {
            Debug.LogWarning("No player prefab found to spawn...");
            return;
        }

        foreach (Tile tile in tiles.Where(tile => tile.data.tileName.Equals("Grass"))) {
            _playerSpawn = new Vector2(tile.X, tile.Y);
            break;
        }

        if (_playerSpawn == null) {
            Debug.LogWarning("No spawn position found...");
            return;
        }

        player = Instantiate(playerPrefab, new Vector3(_playerSpawn.x, _playerSpawn.y + 5, 0), Quaternion.identity);
    }
}
