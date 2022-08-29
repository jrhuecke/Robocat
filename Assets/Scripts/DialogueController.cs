using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{

    private bool inRange;
    public PlayerMovement playerMovement;
    private bool Dialoguing;

    private void Awake()
    {
        inRange = false;
        Dialoguing = false;
    }

    private void Update()
    {
        if (Dialoguing)
        {
            //This is where text will happen
            playerMovement.canMove = true;
            Dialoguing = false;
        }

        //When the player is in range check for player input to start dialogue
        else if (inRange)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerMovement.canMove = false;
                Dialoguing = true;
            }
        }
    }
    //Checks when player is in range by checking if player is in the missing cats hitbox
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.tag == "Player")
        {
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            inRange = false;
        }
    }
}
