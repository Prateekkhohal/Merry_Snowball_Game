using System.Collections;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // Start is called before the first frame update
    public CSVData csvData;
    [Header("Canvas Text Fields")]
    public Text FinalScore_txt;
    public Text FinalAccuracy_txt;
    public Text FinalResponse_txt;
    public Text FinalTime_txt;

    private float score;
    private float accuracy;
    private float responsetime;
    private float gametime;
    private float gamecompletiontime;
    private string formattedtime;
    private string path;

    void Start()
    {
        // Get the file path from the csvData variable
        path = csvData.csvFilePath;

        // Get the start time of the game from PlayerPrefs
        gametime = PlayerPrefs.GetFloat("STARTIME", 0f);

        // Calculate the game completion time
        gamecompletiontime = Time.time - gametime;

        // Convert the completion time to TimeSpan to format it
        TimeSpan rot = TimeSpan.FromSeconds(gamecompletiontime);

        // Format the time as "MM:SS"
        formattedtime = string.Format("{0:D2}:{1:D2}", rot.Minutes, rot.Seconds);

        // Log the formatted time
        Debug.Log("TIME : " + formattedtime);

        // Get the final score from PlayerPrefs
        score = PlayerPrefs.GetFloat("FinalScore", 0f);

        // Get the final accuracy from PlayerPrefs and calculate the average accuracy
        accuracy = PlayerPrefs.GetFloat("FinalAccuracy", 0f);
        float averageAccuracy = (float)accuracy / 6;

        // Log the average accuracy
        Debug.Log("FINAL ACCURACY : " + averageAccuracy);

        // Get the final response time from PlayerPrefs and calculate the average response time
        responsetime = PlayerPrefs.GetFloat("FinalResponseTime", 0f);
        float averageResponseTime = (float)responsetime / 6;

        // Log the average response time
        Debug.Log("FINAL RESPONSE TIME : " + averageResponseTime);

        // Update the UI text elements with the final scores and stats
        FinalScore_txt.text = "SCORE : " + score;
        FinalAccuracy_txt.text = "ACCURACY : " + averageAccuracy.ToString("F2");
        FinalResponse_txt.text = "RESPONSE TIME : " + averageResponseTime.ToString("F2");
        FinalTime_txt.text = "TIME : " + formattedtime;

        // Append the final scores and stats to the CSV file
        using (StreamWriter writer = File.AppendText(path))
        {
            writer.WriteLine("");
            writer.WriteLine("<b><center>Grand Total, SCORE,ACCURACY,RESPONSE TIME,TIME</center></b>");
            writer.WriteLine($",<b>{score}</b>,<b>{averageAccuracy}</b>,<b>{averageResponseTime}</b>,<b>{formattedtime}</b>");
        }

        // Start the game timer coroutine
        StartCoroutine(GameTimer());
    }

    // Coroutine to wait for 5 seconds and then load the first scene
    IEnumerator GameTimer()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(0);
    }
}
