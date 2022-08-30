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

    private void Update()
    {
        if (inRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerMovement.spawnPoint = checkpointSpawn;
                
            }
        }
    }

    //Checks when player is in range by checking if player is in the cat beds hitbox
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.tag == "Player")
        {
            respawnPointText.text = "Press e to sleep";
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            respawnPointText.text = "";
            inRange = false;
        }
    }
}
