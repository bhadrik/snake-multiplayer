using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private Button createButtton;
    [SerializeField] private GameObject nonHostPanel;
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private PlayerJoinListController hostList;
    [SerializeField] private PlayerJoinListController nonHostList;


    List<string> playersInRoom = new List<string>();
    public bool isHost = false;


    private void Awake() {

        if(Instance == null){
            Instance = this;
        }else{
            Debug.LogWarning("Multiple Singletone Instance Found");
            Destroy(this.gameObject);
        }

        PhotonNetwork.OfflineMode = false;
    }

    void Start()
    {
        string pName = Random.Range(545, 6756).ToString();
        PhotonNetwork.LocalPlayer.NickName = pName;
        playerName.text = "Playe Name: " + pName;
    }

    public void StartMultiplayer(){
        PhotonNetwork.ConnectUsingSettings();
        createButtton.interactable = false;
    }


#region Photon Callback
    //-------------------------- PHOTON -------------------------//
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created");
        isHost = true;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobby joined");
        createButtton.interactable = true;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room joined");
        if(isHost){
            hostPanel.SetActive(true);
            //Register itself to the player joined room list
            OnPlayerEnteredRoom(PhotonNetwork.PlayerList[0]);
        }
        else{
            nonHostPanel.SetActive(true);
            foreach(var p in PhotonNetwork.PlayerList){
                string name = p.NickName;
                if(p.IsMasterClient){
                    name = name + " (Host)";
                }
                nonHostList.Add(name);
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list updated [Callback]: " + roomList.Count);
        RoomSelector.Instance.RenewList(roomList);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player Enter");

        playersInRoom.Add(newPlayer.NickName);

        string name = newPlayer.NickName;

        
        if(newPlayer.IsMasterClient){
            name = name + " (Host)";
        }

        hostList.Add(name);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player Exit [callback]");

        playersInRoom.Remove(otherPlayer.NickName);
        // PlayerJoinListController.Instance.remove(otherPlayer.NickName);
        if(isHost){
            hostList.remove(otherPlayer.NickName);
        }
        else{
            nonHostList.remove(otherPlayer.NickName);
        }
    }
    //-------------------------------------------------------------//
#endregion


    //Call from button create room
    public void CreateNewRoom(){
        if(!PhotonNetwork.IsConnected || string.IsNullOrEmpty(roomName.text)) return;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.BroadcastPropsChangeToAll = true;
        roomOptions.PublishUserId = true;

        PhotonNetwork.CreateRoom(roomName.text, roomOptions, TypedLobby.Default);
    }

    //Call from button join
    public void JoinSelectedRoom(){
        PhotonNetwork.JoinRoom(RoomSelector.Instance.GetSelectedRoomName());
    }
}
