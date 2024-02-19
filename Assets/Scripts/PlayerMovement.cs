using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 initialPosition;

    public Animator animator;
    public float distanceTravelled;

    private float speed;
    public Transform minPoint;
    public Transform maxPoint;

    private bool movingRight = true;

    void Start()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            LevelData levelData = gameManager.GetLevelData();
            if (levelData != null)
            {
                speed = levelData.playerData.playerSpeed;
            }
            else
            {
                Debug.LogError("LevelData not found in GameManager.");
            }
        }
        else
        {
            Debug.LogError("GameManager not found in the scene.");
        }

        initialPosition = transform.position;
        Debug.Log("SPEED : " + speed);
    }

    void Update()
    {
        distanceTravelled = Mathf.Abs(initialPosition.x - transform.position.x);

        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
            animator.Play("Run");

            if (transform.position.x > maxPoint.position.x)
            {
                // Change direction to move left
                movingRight = false;
                transform.rotation = Quaternion.Euler(0, -90, 0);
            }
        }
        else
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            animator.Play("Run");

            if (transform.position.x < minPoint.position.x)
            {
                // Change direction to move right
                movingRight = true;
                transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }
    }
}