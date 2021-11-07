using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDrop : MonoBehaviour {
    public TileData data;
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Player")) {
            Debug.Log("Player walked on tile " + data);
            Destroy(this.gameObject);
        }
    }
}
