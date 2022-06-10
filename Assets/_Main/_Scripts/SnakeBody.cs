using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBody : MonoBehaviour
{    
    public Snake myHead;
    public BoxCollider2D mycollider;

    private void Awake() {
        mycollider = GetComponent<BoxCollider2D>();
        mycollider.enabled = false;
        // Invoke(nameof(ColliderEnable), 1.0f);
    }
}
