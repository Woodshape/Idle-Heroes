using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "tile", menuName = "Tiles/Tile")]
public class Tile : ScriptableObject {
    public string tileName;
    public Sprite[] sprites;

    public bool isSolid;

    private TilePosition position;

    private int x;
    private int y;

    public void CreateAt(int x, int y) {
        position = new TilePosition(x, y);
        
        this.x = x;
        this.y = y;
    }

    public bool IsAtPosition(int x, int y) {
        if (position == null) {
            return false;
        }
        
        return position.x == x && position.y == y;
        // return this.x == x && this.y == y;
    }

    public TilePosition GetPosition() {
        return position;
    }

    public class TilePosition {
        public int x;
        public int y;

        public TilePosition(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
}
