using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSelector : MonoBehaviour
{
    public static RoomSelector Instance;
    ToggleGroup group;
    public GameObject selectRoomButton;
    List<Toggle> allToggles = new List<Toggle>();

    private void Awake() {
        group = GetComponent<ToggleGroup>();
        if(Instance == null){
            Instance = this;
        }
    }

    public void RenewList(List<RoomInfo> rooms){
        ClearList();

        foreach(var room in rooms){
            if(room.RemovedFromList){
                remove(room.Name);
                return;
            }
            var instance = Instantiate(selectRoomButton, transform);
            instance.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = room.Name;
            var t = instance.GetComponent<Toggle>();
            t.group = group;
            allToggles.Add(t);
        }
    }

    public void ClearList(){
        foreach(var obj in allToggles){
            Destroy(obj.gameObject);
        }
        allToggles.Clear();
    }

    public string GetSelectedRoomName(){
        foreach(var t in allToggles){
            if(t.isOn){
                return t.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
            }
        }
        return "NoRoomSelected";
    }

    public void remove(string roomName)
    {
        Toggle obj = allToggles.Find(x => x.name == roomName);
        allToggles.Remove(obj);
        Destroy(obj.gameObject);
    }
}
