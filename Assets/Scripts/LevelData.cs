using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelIndex;
    public float levelScreenTime;
    public PlayerData playerData;
    public CanonData canonData;
    public RoundData[] rounds;
}

[System.Serializable]
public class PlayerData
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public float playerSpeed;
    public float movementRange;
}

[System.Serializable]
public class CanonData
{
    public GameObject canonPrefab;
    public Transform spawnPoint;
}

[System.Serializable]
public class RoundData
{
    public int roundIndex;
    public float roundTimeLimit;
    public float roundScreenTimeLimit;
    public int throwsLimit;
    public BallData ballData;
}

[System.Serializable]
public class BallData
{
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public float ballSpeed;
}
