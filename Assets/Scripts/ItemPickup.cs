using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    public SpriteRenderer playerSprite;
    private bool inRange;
    public TextMeshProUGUI itemPickupText;
    public PlayerMovement playerMovement;
    private float textTimer;
    private bool displayedText;
    private bool pickedUp;
    public Sprite tailSprite;
    public Animator anim;

    private void Awake()
    {
        displayedText = false;
        pickedUp = false;
    }

    private void Update()
    {
        //Timers used to stop showing text after amount of time
        if (textTimer > 0)
        {
            textTimer -= Time.deltaTime;
        }
        if (textTimer <= 0 && displayedText)
        {
            itemPickupText.text = "";
            displayedText = true;
        }

        //Setting player's checkpoint
        if (inRange && !pickedUp)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                pickedUp = true;
                itemPickupText.text = "TAIL!";
                textTimer = 2.5f;
                displayedText = true;
                playerSprite.sprite = tailSprite;


            }
        }
    }

    //Checks when player is in range by checking if player is in the cat beds hitbox
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (!pickedUp && collision.tag == "Player")
        {
            itemPickupText.text = "Press e to upgrade!";
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!pickedUp && collision.tag == "Player")
        {
            itemPickupText.text = "";
            inRange = false;
        }
    }
}
