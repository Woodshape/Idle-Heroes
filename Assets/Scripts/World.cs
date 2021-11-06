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

    private Tile.TilePosition _playerSpawn;

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

    private void DeterminePlayerSpawn() {
        if (playerPrefab == null) {
            Debug.LogWarning("No player prefab found to spawn...");
            return;
        }

        foreach (Tile tile in tiles.Where(tile => tile.tileName.Equals("Grass"))) {
            _playerSpawn = tile.GetPosition();
            break;
        }

        if (_playerSpawn == null) {
            Debug.LogWarning("No spawn position found...");
            return;
        }

        player = Instantiate(playerPrefab, new Vector3(_playerSpawn.x, _playerSpawn.y + 5, 0), Quaternion.identity);
    }
}
