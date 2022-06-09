using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    
    [SerializeField] private Snake player;
    [SerializeField] private GameObject gameOverPopup;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;


    int highestScore;
    int score;

    private void Awake() {

        Time.timeScale = 1;

        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this.gameObject);
        }

        player.onHitDanger.AddListener(GameOver);
        player.onFoodEat.AddListener(ScoreUpdate);

        highestScore = PlayerPrefs.GetInt("snake_score");
        highScoreText.text = "High Score: " + highestScore.ToString();

        score = -1;
        ScoreUpdate();
    }

    private void ScoreUpdate()
    {
        score++;
        scoreText.text = score.ToString();

        if(score > highestScore){
            highestScore = score;
            highScoreText.text = "High Score: " + highestScore.ToString();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        gameOverPopup.SetActive(true);
        gameOverPopup.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
        PlayerPrefs.SetInt("snake_score", highestScore);
        Time.timeScale = 0;
    }

    public void ResetGame(){
        SceneManager.LoadSceneAsync(0);
    }


}
