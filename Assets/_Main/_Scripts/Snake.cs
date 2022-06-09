using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Snake : MonoBehaviour
{
    [Header("Snake config")]
    [SerializeField] Vector3 snakeDirection;
    [SerializeField] float snakSpeed;
    [SerializeField] GameObject snakeBodyPrefab;

    [Header("Food config")]
    [Tooltip("x: min and y: max")]
    [SerializeField] Vector2 LevelRange_x;
    [SerializeField] Vector2 LevelRange_y;


    [HideInInspector]
    public UnityEvent onHitDanger;
    public UnityEvent onFoodEat;
    
    List<GameObject> parts = new List<GameObject>();
    GameObject food;
    SnakeInputs input;
    Transform lastAddedBodyPart;
    Vector3 nextRotation;

    private void Awake() {
        food = GameObject.Find("Food");
    }

    private void Start() {

        //Starting direction of snake
        snakeDirection = transform.right;

        input = new SnakeInputs();
        input.Snake.Enable();
        
        input.Snake.Joystick.performed += OnInputChange;

        RandomizeFood();

        lastAddedBodyPart = this.transform;
    }

    bool inputApplied;
    private void OnInputChange(InputAction.CallbackContext obj) {

        if(!inputApplied) return;

        Vector2 input = obj.ReadValue<Vector2>();
        input.x = Mathf.RoundToInt(input.x);
        input.y = Mathf.RoundToInt(input.y);


        // (1, 0) (0, 1) (-1, 0) (0, -1)
        if(input == (Vector2)snakeDirection){
            // Debug.Log("Same return");
            return;
        } else if(input == (Vector2)(-snakeDirection)) {
            // Debug.Log("Inverse return");
            return;
        }
        else if (Mathf.Abs(input.x) == Mathf.Abs(input.y)){
            // Debug.Log("Diagonal return");
            return;
        }   

        if(input.x > 0){
            // Debug.Log("X > 0");
            nextRotation = Vector3.zero;
            snakeDirection = input;
            inputApplied = false;
        }
        else if(input.x < 0){
            // Debug.Log("X < 0");
            nextRotation = Vector3.forward * 180;
            snakeDirection = input;
            inputApplied = false;
        }
        else if(input.y > 0){
            // Debug.Log("Y > 0");
            nextRotation = Vector3.forward * 90;
            snakeDirection = input;
            inputApplied = false;
        }
        else if(input.y < 0){
            // Debug.Log("Y < 0");
            nextRotation = Vector3.forward * 270;
            snakeDirection = input;
            inputApplied = false;
        }
    }

    void FixedUpdate() {
        // start move from last to first body part
        if(parts.Count > 1)
        for(int i = parts.Count - 1; i > 0; i--){
            parts[i].transform.position = parts[i-1].transform.position;
            parts[i].transform.localRotation = parts[i-1].transform.localRotation;
        }

        if(parts.Count > 0){
            parts[0].transform.position = transform.position;
            parts[0].transform.localRotation = transform.localRotation;
        }

        //Head move forward 1 step each frame
        transform.rotation = Quaternion.Euler(nextRotation);
        transform.position = transform.position + transform.right * snakSpeed;
        inputApplied = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Food")){
            RandomizeFood();
            IncreaseSnakeLength();
        }
        else if(other.gameObject.CompareTag("Danger")){
            onHitDanger.Invoke();
        }
    }

    void RandomizeFood() {
        food.transform.position = new Vector2(
            UnityEngine.Random.Range(LevelRange_x.x, LevelRange_x.y),
            UnityEngine.Random.Range(LevelRange_y.x, LevelRange_y.y));
    }

    void IncreaseSnakeLength() {
        Vector2 spawnPos = lastAddedBodyPart.position - (lastAddedBodyPart.right * snakSpeed);
        Quaternion spawnRot = lastAddedBodyPart.localRotation;

        //For first body part
        if(parts.Count == 0){
            spawnPos = transform.position - (transform.right * snakSpeed);
            spawnRot = transform.localRotation;
        }

        lastAddedBodyPart = Instantiate(snakeBodyPrefab, spawnPos, Quaternion.identity).transform;
        parts.Add(lastAddedBodyPart.gameObject);

        onFoodEat.Invoke();
    }
}
