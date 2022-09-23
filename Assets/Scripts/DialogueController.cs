using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{

    private bool inRange;
    public PlayerMovement playerMovement;
    private bool Dialoguing;
    public TextMeshProUGUI dialogueText;
    private float count;
    public Transform player;
    private Transform cat;

    private void Awake()
    {
        inRange = false;
        Dialoguing = false;
        count = 1;
        cat = GetComponent<Transform>();
    }

    private void Update()
    {
        if (Dialoguing)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                switch (count)
                {
                    case 1:
                        dialogueText.text = "Thank you so much for finding me!";
                        count++;
                        break;
                    case 2:
                        dialogueText.text = "I heard about that terrible accident...";
                        count++;
                        break;
                    case 3:
                        dialogueText.text = "I hope you get better soon!";
                        count++;
                        break;
                    case 4:
                        dialogueText.text = "Well, I'll get going now. Thanks again!";
                        count++;
                        break;
                    case 5:
                        playerMovement.canMove = true;
                        Dialoguing = false;
                        this.gameObject.SetActive(false);
                        break;
                }
            }
        }

        //When the player is in range check for player input to start dialogue
        else if (inRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerMovement.canMove = false;
                Dialoguing = true;
                dialogueText.text = "Oh! It's RoboCat!";
                player.position = new Vector3(cat.position.x - 2, cat.position.y, player.position.z);
                player.localScale = new Vector3(Mathf.Abs(player.localScale.x), player.localScale.y, player.localScale.z);
            }
        }
    }
    //Checks when player is in range by checking if player is in the missing cats hitbox
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.tag == "Player")
        {
            dialogueText.text = "Press e to talk";
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            dialogueText.text = "";
            inRange = false;
        }
    }
}
