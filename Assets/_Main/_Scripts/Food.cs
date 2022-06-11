using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Food : MonoBehaviour
{
    [Tooltip("x: min and y: max")]
    [SerializeField] Vector2 LevelRange_x = new Vector2(-8, 8);
    [SerializeField] Vector2 LevelRange_y = new Vector2(-4, 4);

    private int count;


    private void Start() {
        Randomize();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        //It will only collide with snake
        
        if(!GameManager.Instance.isMultiplayer){
            GameManager.Instance.ScoreUpdate();
            Randomize();
            return;
        }

        if(!NetworkManager.Instance.isHost) return;

        if(other.gameObject.GetComponent<Snake>().randId == GameManager.Instance.myPlayerRandId){
            PhotonNetwork.RaiseEvent(GameManager.FOOD_EAT_HOST, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            GameManager.Instance.MultiplayerScoreUpdate(true);
        }
        else{
            PhotonNetwork.RaiseEvent(GameManager.FOOD_EAT_OTHER, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            GameManager.Instance.MultiplayerScoreUpdate(false);
        }

        count++;
        Debug.Log("Count: " + count);
    }

    float difference = 0.5f;

    private void FixedUpdate() {
        if(count%2 == 0 && NetworkManager.Instance.isHost){
            // Debug.Log("Moving");
            if(transform.position.y == LevelRange_y.y)
                difference = -0.5f;
            if(transform.position.y == LevelRange_y.x)
                difference = 0.5f;

            transform.position += Vector3.up * difference;
        }
    }

    public void Randomize(){
        if(!NetworkManager.Instance.isHost && GameManager.Instance.isMultiplayer) return;

        float random1 = UnityEngine.Random.Range(LevelRange_x.x, LevelRange_x.y);
        float random2 = UnityEngine.Random.Range(LevelRange_y.x, LevelRange_y.y);

        float snaped1 = (Mathf.Floor(random1) + Mathf.Ceil(random1))/2;
        float snaped2 = (Mathf.Floor(random2) + Mathf.Ceil(random2))/2;

        transform.position = new Vector2(snaped1, snaped2);
    }
}
