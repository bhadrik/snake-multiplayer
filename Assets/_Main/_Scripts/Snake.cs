using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Snake : MonoBehaviour
{
    [Header("Snake config")]
    [SerializeField] Vector3 snakeDirection;
    [SerializeField] float snakSpeed;
    [SerializeField] GameObject snakeBodyPrefab;


    [HideInInspector]
    public UnityEvent onHitDanger;
    public UnityEvent onFoodEat;
    public UnityEvent<PhotonView> onFoodEat_m;

    
    public bool isMultiplayer = true;

    List<GameObject> parts = new List<GameObject>();
    GameObject food;
    SnakeInputs input;
    Transform lastAddedBodyPart;
    Vector3 nextRotation;
    PhotonView photonView;
    Direction currentDirection = Direction.Right;

    

    private void Awake() {
        food = GameObject.Find("Food");
        photonView = GetComponent<PhotonView>();
        lastAddedBodyPart = this.transform;
    }

    private void Start() {

        int rot = (int)transform.eulerAngles.z;

        if(rot == 0){
            Debug.Log("Right");
            currentDirection = Direction.Right;
        }
        else if (rot == 90){
            Debug.Log("UP");
            currentDirection = Direction.Up;
        }
        else if (rot == 180){
            Debug.Log("left");
            currentDirection = Direction.Left;
        }
        else if (rot == 270){
            Debug.Log("Down");
            currentDirection = Direction.Down;
        }

        nextRotation = transform.eulerAngles;
        

        input = new SnakeInputs();
        input.Snake.Enable();
        
        input.Snake.Up.performed += OnUp;
        input.Snake.Down.performed += OnDown;
        input.Snake.Left.performed += OnLeft;
        input.Snake.Right.performed += OnRight;

        isMultiplayer = GameManager.Instance.isMultiplayer;

        if(isMultiplayer)
        GameManager.Instance.MultiplayerScoreUpdate(photonView);
    }


    private void OnRight(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Left) return;
        nextRotation = Vector3.zero;
        currentDirection = Direction.Right;
    }

    private void OnLeft(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Right) return; 
        nextRotation = Vector3.forward * 180;
        currentDirection = Direction.Left;
    }

    private void OnDown(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Up) return;
        nextRotation = Vector3.forward * 270;
        currentDirection = Direction.Down;
    }

    private void OnUp(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Down) return;
        nextRotation = Vector3.forward * 90;
        currentDirection = Direction.Up;
    }


    void FixedUpdate() {
        // start move from last to first body part
        if(parts.Count > 1)
        for(int i = parts.Count - 1; i > 0; i--){
            parts[i].transform.position = parts[i-1].transform.position;
            parts[i].transform.localRotation = parts[i-1].transform.localRotation;
        }

        if(parts.Count > 0){
            parts[0].transform.position = transform.position /* - (snakeDirection * 0.05f/2) */;
            parts[0].transform.localRotation = transform.localRotation;
        }

        //Head move forward 1 step each frame
        if(photonView.IsMine || !isMultiplayer){
            transform.rotation = Quaternion.Euler(nextRotation);
            transform.position = transform.position + transform.right * snakSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Food")){
            GameManager.Instance.RandomizeFood();
            IncreaseSnakeLength();

            if(isMultiplayer)
                onFoodEat_m.Invoke(photonView);
            else
                onFoodEat.Invoke();
        }
        else if(other.gameObject.CompareTag("Danger")){
            try{
                //If its is no my body part then return
                if(other.gameObject.GetComponent<SnakeBody>().myHead.gameObject.name != gameObject.name) return;
            }
            catch(Exception e){}

            Debug.Log("<color=red>Denger is : </color>" + other.gameObject.name);

            if(isMultiplayer)
                PhotonNetwork.RaiseEvent(GameManager.GAME_OVER, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            else
                onHitDanger.Invoke();

            if(NetworkManager.Instance.isHost){
                GameManager.Instance.OnMultiplayerOver();
            }
        }
    }

    void IncreaseSnakeLength() {
        Vector2 spawnPos = lastAddedBodyPart.position - (lastAddedBodyPart.right * snakSpeed);
        Quaternion spawnRot = lastAddedBodyPart.localRotation;

        //For first body part
        if(parts.Count == 0){
            spawnPos = transform.position - (transform.right * (snakSpeed /* + 0.05f/2 */));
            spawnRot = transform.localRotation;
        }

        lastAddedBodyPart = Instantiate(snakeBodyPrefab, spawnPos, Quaternion.identity).transform;
        parts.Add(lastAddedBodyPart.gameObject);
    }
}

public enum Direction{
    Up,
    Down,
    Left,
    Right
}