using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "tile", menuName = "Tiles/Tile")]
public class Tile : ScriptableObject {
    public string tileName;
    public Sprite[] sprites;

    private int x;
    private int y;

    public void CreateAt(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public bool IsAtPosition(int x, int y) {
        return this.x == x && this.y == y;
    }
}
