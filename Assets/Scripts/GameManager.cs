using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    [Header("CSV FIELDS")]
    public LevelData levelData;
    public CSVData csvData;
    private string path;

    private List<int> roundScores = new List<int>(); // List to store round scores
    private float[] reactionTimes; // Array to store reaction times for each ball
    private List<float> roundResponseTimes = new List<float>(); // List to store round response times
    private int[] ballScores; // Array to store scores for each ball

    [Header("CANVAS SCREENS")]
    public GameObject InGameScreen;
    public GameObject RoundScreen;
    public GameObject GameOverScreen;
    public GameObject LevelScreen;

    [Header("CANVAS TEXT FIELDS")]
    public Text Level_Txt;
    public Text Round_Txt;
    public Text Ball_Txt;
    public Text Score_Txt;
    public Text RoundScreen_Txt;
    public Text LevelScreen_Txt;

    [Header("TIMERS")]
    public Text RoundScreen_Timer_Txt;
    public Text Round_Timer_Txt;

    [Header("NEXT SCENE")]
    public string NextSceneName;

    [HideInInspector]
    [Header("LEVEL")]
    private float currentlevel;
    private float LSTime;
    private float levelStartTime;
    private float levelCompletionTime;

    [HideInInspector]
    [Header("ROUND")]
    private int currentRoundIndex = 0; // The index of the current round in LevelData.rounds
    private float RSTime;
    private float RoundStartTime;
    private float RoundCompletionTIme;
    private string formattedroundCompletionTime;

    [HideInInspector]
    [Header("BALL")]
    private int throwCount = 0; // The current number of throws
    private float maxThrows; // Max No. of balls
    private int TotalBalls; // Total number of balls in each round private const int TotalBalls = 3
    private float ballThrowTime;

    [HideInInspector]
    [Header("DATA")]
    private int Score;
    private static float finalscore;

    private float accuracy;
    private static float finalaccuracy;

    private static float finalresponsetime;

    private int Successfullhit = 0;
    private int grandTotalSuccessfullHits = 0;
    private int grandTotalBalls = 0;

    private GameObject playerInstance;
    private float playerSpawnTime;

    private bool isgameStarted = false;

    [HideInInspector]
    [Header("VR")]
    private static bool isDeviceSearchCompleted = false;

    private InputDevice targetDevice;
    private List<InputDevice> devices = new List<InputDevice>();

    // Add a boolean flag to control the throwing action
    private bool canThrowBall = true;


    void Start()
    {
        levelStartTime = Time.time;
        TotalBalls = levelData.rounds[currentRoundIndex].throwsLimit;
        currentlevel = levelData.levelIndex + 1;
        Level_Txt.text = "LEVEL : " + currentlevel;
        ballScores = new int[TotalBalls];
        reactionTimes = new float[TotalBalls];

        // Set the file path
        path = csvData.csvFilePath;

        // Create or overwrite the file with column names
        using (StreamWriter writer = File.AppendText(path))
        {
            writer.WriteLine($"Level {levelData.levelIndex + 1}");
            writer.Write("Round");
            for (int i = 1; i <= TotalBalls; i++)
            {
                writer.Write($", Score Ball {i}");
                writer.Write($", Reaction Time {i}");
            }
            writer.WriteLine(", Total Score,Accuracy,Response Time,Round Time");
        }
        LevelScreen_Txt.text = "LEVEL : " + (currentlevel);

        if (!isDeviceSearchCompleted)
        {
            StartCoroutine(GetInputDevicesCoroutine());
            isDeviceSearchCompleted = true;
        }
        StartCoroutine(Level());
        
    }

    IEnumerator GetInputDevicesCoroutine()
    {
        yield return new WaitForSeconds(1f); // Delay for 1 second

        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);

        foreach (var item in devices)
        {
            Debug.Log(item.name + item.characteristics);  // PRINTING THE DEVICE with its Characteristics
        }
        Debug.Log("DEVICES : " + devices.Count);

        if (devices.Count > 0)
        {
            targetDevice = devices[0]; // Selecting Device
        }
    }

    IEnumerator Level()
    {
        LevelScreen.SetActive(true);
        yield return new WaitForSeconds(5);
        LevelScreen.SetActive(false);
        StartRound();
    }


    void Update()
    {
        if (!isgameStarted)
        {
            return;
        }

        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);

        if (Input.GetButtonDown("Fire1") || (triggerValue == 1f && canThrowBall))
        {
            // Check if the currentRoundIndex is within the bounds of the rounds array
            if (currentRoundIndex >= 0 && currentRoundIndex < levelData.rounds.Length)
            {
                // Check if the maximum number of throws has not been reached
                if (throwCount < maxThrows)
                {
                    // Disable throwing action temporarily
                    canThrowBall = false;
                    // Start the coroutine to enable throwing action after a delay
                    StartCoroutine(EnableThrowingAction());
                    // Throw the ball
                    ThrowBall();
                    // Increment the throw count
                    throwCount++;

                    reactionTimes[throwCount - 1] = ballThrowTime - playerSpawnTime;

                    float reactionTime = ballThrowTime - playerSpawnTime;
                    Debug.Log("Reaction Time: " + reactionTime.ToString("F2") + " seconds");
                    playerSpawnTime = Time.time;

                    Ball_Txt.text = "BALLS REMAINING : " + (maxThrows - throwCount);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // save any game data here
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    IEnumerator EnableThrowingAction()
    {
        // Wait for a specific duration before enabling the throwing action again
        yield return new WaitForSeconds(0.5f);
        canThrowBall = true;
    }

    void StartRound()
    {
        maxThrows = levelData.rounds[currentRoundIndex].throwsLimit;
        isgameStarted = false;
        throwCount = 0;
        RoundScreen.SetActive(true);
        StartCoroutine(RSTimer());
        RoundStartTime = Time.time;
        Debug.Log("Round : " + (currentRoundIndex + 1));
        Round_Txt.text = "ROUND : " + (currentRoundIndex + 1);
        Ball_Txt.text = "BALLS REMAINING : " + (levelData.rounds[currentRoundIndex].throwsLimit);
        InGameScreen.SetActive(true);
        SpawnPlayer();
    }

    IEnumerator RSTimer()
    {
        RSTime = levelData.rounds[currentRoundIndex].roundScreenTimeLimit;
        RoundScreen_Txt.text = "ROUND : " + (currentRoundIndex + 1);
        while (RSTime > 0)
        {
            RoundScreen_Timer_Txt.text = "ROUND WILL START IN: " + RSTime;
            yield return new WaitForSeconds(1);
            RSTime -= 1;
        }
        RoundScreen.SetActive(false);
        playerSpawnTime = Time.time;
        StartCoroutine(RoundTimer());
        isgameStarted = true;
    }

    IEnumerator RoundTimer()
    {
        StopCoroutine(RSTimer());
        float timeRemaining = levelData.rounds[currentRoundIndex].roundTimeLimit;
        bool roundEnded = false;
        while (!roundEnded)
        {
            Round_Timer_Txt.text = "TIME REMAINING : " + timeRemaining;
            yield return new WaitForSeconds(1);
            timeRemaining -= 1;
            // Check if the round has ended
            if (timeRemaining <= 0 || throwCount >= maxThrows)
            {
                roundEnded = true;
            }
        }
        // The round has ended, so end it
        Invoke("EndRound", 1f);
    }

    void EndRound()
    {
        InGameScreen.SetActive(false);
        Destroy(playerInstance);

        RoundCompletionTIme = Time.time - RoundStartTime;
        TimeSpan rct = TimeSpan.FromSeconds(RoundCompletionTIme);
        formattedroundCompletionTime = string.Format("{0:D2}:{1:D2}", rct.Minutes, rct.Seconds);

        int roundScore = CalculateRoundScore();
        roundScores.Add(roundScore);

        Debug.Log("Round " + (currentRoundIndex + 1) + " Ended");
        Debug.Log("SCORE : " + finalscore);
        Debug.Log("Total ball : " + TotalBalls);
        Debug.Log("HIT's : " + Successfullhit);
        Debug.Log("Accuracy : " + ((float)Successfullhit / TotalBalls * 100));
        Debug.Log("ROUND TIME : " + formattedroundCompletionTime);

        grandTotalSuccessfullHits += Successfullhit;
        grandTotalBalls += TotalBalls;

        float roundResponseTime = CalculateRoundResponseTime();
        roundResponseTimes.Add(roundResponseTime);

        accuracy = CalculateAccuracy();
        // Export the round scores to the CSV file
        ExportRoundScoresToCSV();

        // Reset the ball scores
        ballScores = new int[TotalBalls];
        Successfullhit = 0;

        if (currentRoundIndex < (levelData.rounds.Length - 1))
        {
            currentRoundIndex++;
            StartRound();
        }
        else
        {
            Destroy(playerInstance);
            isgameStarted = false;
            levelCompletionTime = Time.time - levelStartTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(levelCompletionTime);
            string formattedlevelCompletionTime = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

            Debug.Log("TIME TAKEN : " + formattedlevelCompletionTime);
            using (StreamWriter writer = File.AppendText(path))
            {
                writer.WriteLine($" Total{GetEmptyFields(TotalBalls)}, {Score}, {CalculateOverallAccuracy()}, {CalculateAverageResponseTime(roundResponseTimes)},{formattedlevelCompletionTime}");
                writer.WriteLine("");
            }
            //GameOverScreen.SetActive(true);
            Debug.Log("Level Over");
            if (NextSceneName != "GameOver")
            {
                Debug.Log("If LOOp");
                SceneManager.LoadScene(NextSceneName);
            }
            else
            {
                Debug.Log("ELSE LOOP");
                PlayerPrefs.SetFloat("FinalScore", finalscore);

                PlayerPrefs.SetFloat("FinalAccuracy", finalaccuracy);

                PlayerPrefs.SetFloat("FinalResponseTime", finalresponsetime);

                SceneManager.LoadScene(NextSceneName);
            }
        }
    }


    private float CalculateRoundResponseTime()
    {
        float roundResponseTime = 0f;
        for (int i = 0; i < TotalBalls; i++)
        {
            roundResponseTime += reactionTimes[i];
        }
        roundResponseTime /= TotalBalls;
        return roundResponseTime;
    }

    private float CalculateAverageResponseTime(List<float> responseTimes)
    {
        float averageResponseTime = 0f;
        foreach (float time in responseTimes)
        {
            averageResponseTime += time;
        }
        averageResponseTime /= responseTimes.Count;
        finalresponsetime += averageResponseTime;
        return averageResponseTime;
    }

    private float CalculateAccuracy()
    {
        return (float)Successfullhit / TotalBalls * 100;
    }

    private float CalculateOverallAccuracy()
    {
        finalaccuracy += (float)grandTotalSuccessfullHits / grandTotalBalls * 100;
        return (float)grandTotalSuccessfullHits / grandTotalBalls * 100;
    }

    private string GetEmptyFields(int count)
    {
        string emptyFields = "";
        for (int i = 0; i < 2 * count; i++)
        {
            emptyFields += ",";
        }
        return emptyFields;
    }


    private int CalculateRoundScore()
    {
        int roundScore = 0;
        if (throwCount >= 1 && throwCount <= TotalBalls)
        {
            roundScore = ballScores[throwCount - 1];
        }
        return roundScore;
    }

    public void UpdateScoreBall(int score)
    {
        if (throwCount >= 1 && throwCount <= TotalBalls)
        {
            ballScores[throwCount - 1] += score;
        }
    }

    public void ExportRoundScoresToCSV()
    {
        // Check if the file exists
        bool fileExists = File.Exists(path);

        // Open the file in append mode
        using (StreamWriter writer = File.AppendText(path))
        {
            // Write the column names if the file doesn't exist
            if (!fileExists)
            {
                writer.WriteLine($"{levelData.levelIndex}");
                writer.Write("Round");
                for (int i = 1; i <= TotalBalls; i++)
                {
                    writer.Write($", Score Ball {i}");
                    writer.Write($", Reaction Time {i}");
                }
                writer.WriteLine(", Total Score,Accuracy,Response Time,Round Time");
            }

            // Calculate the total score for the round
            int totalScore = CalculateTotalScore(ballScores);

            // Write the row entry for the round
            writer.Write($"{currentRoundIndex + 1},");
            for (int i = 0; i < TotalBalls; i++)
            {
                writer.Write($"{ballScores[i]}");
                writer.Write($", {reactionTimes[i].ToString("F2")}");
                if (i < TotalBalls - 1)
                {
                    writer.Write(",");
                }
            }
            writer.WriteLine($",{totalScore},{accuracy},{CalculateRoundResponseTime()},{formattedroundCompletionTime}");
        }

        Debug.Log($"Round {currentRoundIndex + 1} scores exported to {path} successfully.");
    }


    private int CalculateTotalScore(int[] scores)
    {
        int totalScore = 0;
        for (int i = 0; i < TotalBalls; i++)
        {
            totalScore += scores[i];
        }
        return totalScore;
    }

    void ThrowBall()
    {
        var _ball = Instantiate(levelData.rounds[currentRoundIndex].ballData.ballPrefab, levelData.rounds[currentRoundIndex].ballData.spawnPoint.position, levelData.rounds[currentRoundIndex].ballData.spawnPoint.rotation);
        _ball.GetComponent<Rigidbody>().velocity = -levelData.rounds[currentRoundIndex].ballData.spawnPoint.forward * levelData.rounds[currentRoundIndex].ballData.ballSpeed;
        ballThrowTime = Time.time;
    }

    public int GetThrowCount()
    {
        return throwCount;

    }

    public void SpawnPlayer()
    {
        float movementRange = 6f;

        // Get the spawn position from level data
        Vector3 spawnPosition = levelData.playerData.spawnPoint.position;

        // Generate a random offset within the movement range
        float randomOffset = UnityEngine.Random.Range(-movementRange, movementRange);

        // Calculate the final spawn position with the random offset
        Vector3 finalSpawnPosition = new Vector3(spawnPosition.x + randomOffset, spawnPosition.y, spawnPosition.z);

        // Create the player instance at the final spawn position
        playerInstance = Instantiate(levelData.playerData.playerPrefab, finalSpawnPosition, levelData.playerData.spawnPoint.rotation);
    }

    public void DestroyPlayer()
    {
        
        Destroy(playerInstance);
    }

    public void updateScore(int score)
    {
        finalscore += score;
        Score += score;
        Score_Txt.text = "SCORE : " + Score;
    }

    public void UpdateScore(int score)
    {
        ballScores[throwCount - 1] = score;
    }

    public int GetThrowslimit()
    {
        return TotalBalls;
    }

    public void UpdateSuccessfullhit()
    {
        Successfullhit += 1;
    }

    public float GetFinalScore()
    {
        return finalscore;
    }

    public float GetFinalAccuracy()
    {
        return finalaccuracy;
    }
    public float GetFinalResponseTime()
    {
        return finalresponsetime;
    }
    public LevelData GetLevelData()
    {
        return levelData;
    }
}