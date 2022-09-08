using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RespawnPointController : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Vector3 checkpointSpawn;
    private bool inRange;
    public TextMeshProUGUI respawnPointText;
    private bool checkpointSet;
    private float checkpointSetTimer;

    private void Awake()
    {
        checkpointSet = false;
    }

    private void Update()
    {
        //Timers used to stop showing "Checkpoint set" text after amount of time
        if (checkpointSetTimer > 0)
        {
            checkpointSetTimer -= Time.deltaTime;
        }
        if (checkpointSetTimer <= 0 && checkpointSet)
        {
            respawnPointText.text = "";
        }

        //Setting player's checkpoint
        if (inRange && !checkpointSet)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                checkpointSet = true;
                playerMovement.spawnPoint = checkpointSpawn;
                respawnPointText.text = "Checkpoint set!";
                checkpointSetTimer = 1.5f;
                
            }
        }
    }

    //Checks when player is in range by checking if player is in the cat beds hitbox
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (!checkpointSet && collision.tag == "Player")
        {
            respawnPointText.text = "Press e to sleep";
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!checkpointSet && collision.tag == "Player")
        {
            respawnPointText.text = "";
            inRange = false;
        }
    }
}
