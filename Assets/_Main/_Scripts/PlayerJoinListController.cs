using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerJoinListController : MonoBehaviour
{
    [SerializeField] GameObject roomNamePrefab;

    List<GameObject> buttonList = new List<GameObject>();

    public void Add(string playerName)
    {
        GameObject obj = Instantiate(roomNamePrefab, transform);
        obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerName;
        obj.name = playerName;

        buttonList.Add(obj);
    }

    public void remove(string playerName)
    {
        GameObject obj = buttonList.Find(x => x.name == playerName);
        buttonList.Remove(obj);
        Destroy(obj);
    }
}
