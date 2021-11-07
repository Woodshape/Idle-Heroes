using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    public TileData data;

    public int X;
    public int Y;

    public void Create(TileData data, int x, int y) {
        this.data = data;
        this.X = x;
        this.Y = y;
    }

    public bool IsAtPosition(int x, int y) {
        return X == x && Y == y;
    }
}
