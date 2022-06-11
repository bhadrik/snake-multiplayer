using System;
using System.Collections;
using System.IO;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public const byte GAME_START_EVENT = 22;
    public const byte GAME_OVER = 23;
    public const byte FOOD_EAT_HOST = 24;
    public const byte FOOD_EAT_OTHER = 25;
    public const byte NEW_GAME = 43;

    private Snake myPlayer;
    private Snake otherPlayer;
    // private GameObject food;

    [Header("General")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Food food;

    [SerializeField] public Color myPlayerColor;
    [SerializeField] public Color otherPlayerColor;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPopup;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private Transform[] startPositions;

    
    [Header("Multiplayer")]
    [SerializeField] private int gameTime = 90;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject multiplayerGameOverPanel;
    [SerializeField] private GameObject multiplayerScoreGroup;
    [SerializeField] private GameObject singlePlayerScore;
    [SerializeField] private TextMeshProUGUI hostScoreText;
    [SerializeField] private TextMeshProUGUI otherScoreText;
    [SerializeField] private TextMeshProUGUI hostScoreText_end;
    [SerializeField] private TextMeshProUGUI otherScoreText_end;
    [SerializeField] private GameObject win;
    [SerializeField] private GameObject lost;
    [SerializeField] private GameObject restartButton;

    
    private int myScore = -1;
    private int otherScore = -1;

    public int myPlayerRandId;

    int highestScore;
    int score;

    public bool isMultiplayer = true;


    private void Awake() {

        Time.timeScale = 1;

        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this.gameObject);
        }

        highestScore = PlayerPrefs.GetInt("snake_score");
        highScoreText.text = "High Score: " + highestScore.ToString();

        score = -1;
        ScoreUpdate();

        PhotonNetwork.NetworkingClient.EventReceived += OnNetworkStartEvent;

        var input = new SnakeInputs();
        input.General.Enable();
        input.General.Quit.performed += OnQuit;

    }

    private void OnQuit(InputAction.CallbackContext obj)
    {
        Application.Quit();
    }

    public void ScoreUpdate()
    {
        score++;
        scoreText.text = score.ToString();

        if(score > highestScore){
            highestScore = score;
            highScoreText.text = "High Score: " + highestScore.ToString();
        }
        if(myPlayer != null)
        myPlayer.IncreaseSnakeLength();
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        gameOverPopup.SetActive(true);
        gameOverPopup.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
        PlayerPrefs.SetInt("snake_score", highestScore);
        Time.timeScale = 0;
    }

    public void RestartGame(){
        SceneManager.LoadSceneAsync(0);
    }


    // When host hit Start button
    public void StartGame(){
        Debug.Log("Click on start button");
        PhotonNetwork.RaiseEvent(GAME_START_EVENT, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        OnStartGame();
    }

    private void OnStartGame()
    {
        Debug.Log("Start game now");
        menuCanvas.SetActive(false);

        singlePlayerScore.SetActive(false);
        multiplayerScoreGroup.SetActive(true);

        myScore = 0;
        hostScoreText.text = "My Score: " + myScore.ToString();
        hostScoreText.color = myPlayerColor;

        otherScore = 0;
        otherScoreText.text = "Other Score: " + otherScore.ToString();
        otherScoreText.color = otherPlayerColor;

        Transform t;
        if(NetworkManager.Instance.isHost)
        t = startPositions[0];
        else
        t = startPositions[1];

        string path = "Photon prefab/Snake head - Player";
        myPlayer = PhotonNetwork.Instantiate(path, t.position, t.rotation).GetComponent<Snake>();
        myPlayer.onHitDanger.AddListener(GameOver);
        // player.onFoodEat.AddListener(ScoreUpdate);
        // player.onFoodEat_m.AddListener(MultiplayerScoreUpdate);

        if(NetworkManager.Instance.isHost){
            path = "Photon prefab/Food";
            food = PhotonNetwork.Instantiate(path, Vector3.zero, Quaternion.identity).GetComponent<Food>();
        }

        StartCoroutine(StartTimer());
    }

    float timer = 0;
    IEnumerator StartTimer(){
        while(timer <= gameTime){
            timer += Time.deltaTime;

            timerText.text = "Time: " + (gameTime - Mathf.Floor(timer));

            yield return null;
        }

        //After timer end
        if(NetworkManager.Instance.isHost){
            OnMultiplayerOver();
            PhotonNetwork.RaiseEvent(GameManager.GAME_OVER, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }
    }


    public void MultiplayerScoreUpdate(bool byHost){
        if(NetworkManager.Instance.isHost){
            //Only host will randomize the food
            food.Randomize();

            // host player eat food
            if(!byHost){
                Debug.Log("<color=green>I eat food</color>");
                myScore++;
                hostScoreText.text = "My Score: " + myScore.ToString();

                if(myScore > highestScore){
                    highestScore = myScore;
                    highScoreText.text = "High Score: " + highestScore.ToString();
                }
                myPlayer.IncreaseSnakeLength();
            }
            else{
                Debug.Log("<color=green>Other eat food</color>");
                otherScore++;
                otherScoreText.text = "Other Score: " + otherScore.ToString();

                //Increaese other player length
                var players = GameObject.FindGameObjectsWithTag("Player");
                foreach(var player in players){
                    if(!player.GetComponent<Snake>().isMine){
                        player.GetComponent<Snake>().IncreaseSnakeLength();
                    }
                }
            }
        }
        else{
            //Non host player eat food
            if(byHost){
                // Debug.Log("<color=green>I eat food</color>");
                myScore++;
                hostScoreText.text = "My Score: " + myScore.ToString();

                if(myScore > highestScore){
                    highestScore = myScore;
                    highScoreText.text = "High Score: " + highestScore.ToString();
                }
                myPlayer.IncreaseSnakeLength();
            }
            else{
                // Debug.Log("<color=green>Other eat food</color>");
                otherScore++;
                otherScoreText.text = "Other Score: " + otherScore.ToString();
                
                //Increaese other player length
                var players = GameObject.FindGameObjectsWithTag("Player");
                foreach(var player in players){
                    if(!player.GetComponent<Snake>().isMine){
                        player.GetComponent<Snake>().IncreaseSnakeLength();
                    }
                }
            }
        }
    }

    private void MultiplayerRestart(){
        myScore = 0;
        hostScoreText.text = "My Score: " + myScore.ToString();

        otherScore = 0;
        otherScoreText.text = "Other Score: " + otherScore.ToString();

        Transform t;
        if(NetworkManager.Instance.isHost)
        t = startPositions[0];
        else
        t = startPositions[1];

        myPlayer.transform.position = t.position;
        myPlayer.transform.rotation = t.rotation;

        var players = GameObject.FindGameObjectsWithTag("Player");
        players[0].GetComponent<Snake>().ResetSnake();
        players[1].GetComponent<Snake>().ResetSnake();

        players[0].GetComponent<Snake>().RotToDir();
        players[1].GetComponent<Snake>().RotToDir();

        win.SetActive(false);
        lost.SetActive(false);
        multiplayerGameOverPanel.SetActive(false);

        StopAllCoroutines();
        timer = 0;
        StartCoroutine(StartTimer());
    }


    public void OnRestartButtonClick(){
        Debug.Log("NEW_GAME click");
        PhotonNetwork.RaiseEvent(NEW_GAME, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        MultiplayerRestart();
    }


    private void OnNetworkStartEvent(EventData obj)
    {
        if (obj.Code == GAME_START_EVENT){
            OnStartGame();
        }
        else if (obj.Code == GAME_OVER){
            OnMultiplayerOver();
        }
        else if(obj.Code == FOOD_EAT_HOST){
            MultiplayerScoreUpdate(true);
        }
        else if(obj.Code == FOOD_EAT_OTHER){
            MultiplayerScoreUpdate(false);
        }
        else if(obj.Code == NEW_GAME){
            MultiplayerRestart();
        }
    }

    public void OnMultiplayerOver()
    {
        multiplayerGameOverPanel.SetActive(true);

        hostScoreText_end.text = myScore.ToString();
        otherScoreText_end.text = otherScore.ToString();

        if(myScore > otherScore){
            //Win text
            win.SetActive(true);
        }
        else{
            //Loose text
            lost.SetActive(true);
        }

        if(!NetworkManager.Instance.isHost)
            restartButton.SetActive(false);

        PlayerPrefs.SetInt("snake_score", highestScore);

        var players = GameObject.FindGameObjectsWithTag("Player");
        players[0].transform.position = new Vector3(100, 100, 0);
        players[1].transform.position = new Vector3(100, 100, 0);
    }

    public void StartSinglePlayer(){
        isMultiplayer = false;
        singlePlayerScore.SetActive(true);
        multiplayerScoreGroup.SetActive(false);
        myPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Snake>();
        menuCanvas.SetActive(false);
        myPlayer.onHitDanger.AddListener(GameOver);
        // player.onFoodEat.AddListener(ScoreUpdate);

        food = Instantiate(foodPrefab, Vector3.right * 8, Quaternion.identity).GetComponent<Food>();
        timerText.gameObject.SetActive(false);
    }


    bool isPause;
    public void PauseGame(){
        if(isPause)
            Time.timeScale = 1;
        else
           Time.timeScale = 0;

        isPause = !isPause;
    }
}
