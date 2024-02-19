using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHit : MonoBehaviour
{
    private GameManager gameManager;
    private SoundManager sound;

    private void Start()
    {
        sound = FindObjectOfType<SoundManager>();
        gameManager = FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            sound.PlaySound();
            Debug.Log("BALL HITTED");
            int throwCount = gameManager.GetThrowCount();
            int Throwslimit = gameManager.GetThrowslimit();
            gameManager.UpdateSuccessfullhit();
            gameManager.DestroyPlayer();
            int score;

            if (throwCount == 1)
            {
                score = 10;
                gameManager.UpdateScoreBall(score);
                gameManager.updateScore(10);

            }
            else if (throwCount == 2)
            {
                score = 7;
                gameManager.UpdateScoreBall(score);
                gameManager.updateScore(7);
            }

            else if (throwCount >= 3)
            {
                score = 5;
                gameManager.UpdateScoreBall(score);
                gameManager.updateScore(5);
            }


            if (throwCount < Throwslimit)
            {

                gameManager.CancelInvoke("SpawnPlayer");
                gameManager.Invoke("SpawnPlayer", 1);
            }

        }
    }
}