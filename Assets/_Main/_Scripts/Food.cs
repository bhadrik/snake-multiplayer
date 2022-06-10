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
    }

    public void Randomize(){
        if(!NetworkManager.Instance.isHost && GameManager.Instance.isMultiplayer) return;

        transform.position = new Vector2(
            UnityEngine.Random.Range(LevelRange_x.x, LevelRange_x.y),
            UnityEngine.Random.Range(LevelRange_y.x, LevelRange_y.y));
    }
}
