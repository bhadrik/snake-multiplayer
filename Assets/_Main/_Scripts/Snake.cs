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
    [HideInInspector]
    public UnityEvent<PhotonView> onFoodEat_m;

    
    public bool isMultiplayer = true;

    public List<GameObject> parts = new List<GameObject>();
    SnakeInputs input;
    Transform lastAddedBodyPart;
    Vector3 nextRotation;
    PhotonView photonView;
    public bool isMine{
        get{
            return photonView.IsMine;
        }
    }
    Direction currentDirection = Direction.Right;
    public int randId;
    
    bool inputApplied;

    private void Awake() {
        photonView = GetComponent<PhotonView>();
        lastAddedBodyPart = this.transform;
    }

    private void Start() {
        randId = UnityEngine.Random.Range(3451,1398729);
        GameManager.Instance.myPlayerRandId = randId;

        RotToDir();

        nextRotation = transform.eulerAngles;
        

        input = new SnakeInputs();
        input.Snake.Enable();
        
        input.Snake.Up.performed += OnUp;
        input.Snake.Down.performed += OnDown;
        input.Snake.Left.performed += OnLeft;
        input.Snake.Right.performed += OnRight;

        isMultiplayer = GameManager.Instance.isMultiplayer;

        // if(isMultiplayer)
        // GameManager.Instance.MultiplayerScoreUpdate(photonView.IsMine);
    }

    public void RotToDir(){
        int rot = (int)transform.eulerAngles.z;

        if(rot == 0){
            currentDirection = Direction.Right;
        }
        else if (rot == 90){
            currentDirection = Direction.Up;
        }
        else if (rot == 180){
            currentDirection = Direction.Left;
        }
        else if (rot == 270){
            currentDirection = Direction.Down;
        }
    }


    private void OnRight(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Left || !inputApplied) return;
        nextRotation = Vector3.zero;
        currentDirection = Direction.Right;
        inputApplied = false;
    }

    private void OnLeft(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Right || !inputApplied) return; 
        nextRotation = Vector3.forward * 180;
        currentDirection = Direction.Left;
        inputApplied = false;
    }

    private void OnDown(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Up || !inputApplied) return;
        nextRotation = Vector3.forward * 270;
        currentDirection = Direction.Down;
        inputApplied = false;
    }

    private void OnUp(InputAction.CallbackContext obj)
    {
        if(currentDirection == Direction.Down || !inputApplied) return;
        nextRotation = Vector3.forward * 90;
        currentDirection = Direction.Up;
        inputApplied = false;
    }


    private void FixedUpdate() {
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
            transform.rotation  = Quaternion.Euler(nextRotation);
            transform.position = transform.position + transform.right * snakSpeed;
        }
        inputApplied = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(!other.gameObject.CompareTag("Danger")) return;

        if(!isMultiplayer){
            onHitDanger.Invoke();
            return;
        }

        if(!NetworkManager.Instance.isHost) return;

        try{
            //If its is no my body part then return
            if(other.gameObject.GetComponent<SnakeBody>().myHead.randId != randId){
                Debug.Log("<color=yellow>Collide with other player return</color>");
                return;
            }
        }catch{}

        //Let both player know game is over
        PhotonNetwork.RaiseEvent(GameManager.GAME_OVER, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        GameManager.Instance.OnMultiplayerOver();
    }

    public void IncreaseSnakeLength() {
        Vector2 spawnPos = lastAddedBodyPart.position - (lastAddedBodyPart.right * snakSpeed);
        Quaternion spawnRot = lastAddedBodyPart.localRotation;

        //For first body part
        if(parts.Count == 0){
            spawnPos = transform.position - (transform.right * (snakSpeed /* + 0.05f/2 */));
            spawnRot = transform.localRotation;
        }

        var body = Instantiate(snakeBodyPrefab, spawnPos, Quaternion.identity);
        body.GetComponent<SnakeBody>().myHead = this;
        if(parts.Count > 2) body.GetComponent<SnakeBody>().mycollider.enabled = true;
        lastAddedBodyPart = body.transform;
        parts.Add(lastAddedBodyPart.gameObject);
    }

    public void ResetSnake(){
        Debug.Log("Delete all body part my player");
        foreach(var go in parts){
            Destroy(go);
        }
        parts.Clear();

        nextRotation = transform.eulerAngles;
        lastAddedBodyPart = this.transform;

    }
}

public enum Direction{
    Up,
    Down,
    Left,
    Right
}