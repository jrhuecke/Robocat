using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    private bool inRange;
    public TextMeshProUGUI itemPickupText;
    public RectTransform itemPickupTextTF;
    private float textTimer;
    private bool displayedText;
    private bool pickedUp;
    public GameObject newPlayer;
    public GameObject oldPlayer;
    public PlayerMovement playerMovement;
    private SpriteRenderer pickupSprite;

    private void Awake()
    {
        displayedText = false;
        pickedUp = false;
        pickupSprite = GetComponent<SpriteRenderer>();
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

        //checking for player interacting with item
        if (inRange && !pickedUp)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                pickedUp = true;
                textTimer = 5f;
                displayedText = true;
                //giving player the upgrades
                if (gameObject.name == "Tail Pickup")
                {
                    itemPickupTextTF.position = new Vector3(itemPickupTextTF.position.x, itemPickupTextTF.position.y + 1, itemPickupTextTF.position.z);
                    itemPickupText.fontSize = itemPickupText.fontSize * 1.5f;
                    itemPickupText.text = "Tail restored!\n Press SPACE while jumping to double jump.\n Continue holding SPACE to slow fall.";
                    newPlayer.transform.position = oldPlayer.transform.position;
                    newPlayer.transform.localScale = oldPlayer.transform.localScale;
                    oldPlayer.SetActive(false);
                    newPlayer.SetActive(true);
                    pickupSprite.enabled = false;
                    playerMovement.hasTail = true;
                } else if (gameObject.name == "Claws Pickup")
                {
                    itemPickupTextTF.position = new Vector3(itemPickupTextTF.position.x, itemPickupTextTF.position.y + 1, itemPickupTextTF.position.z);
                    itemPickupText.fontSize = itemPickupText.fontSize * 1.5f;
                    itemPickupText.text = "Claws restored!\n Press LEFT or RIGHT against a wall to cling to it.\n Press SPACE while clinging to wall jump.";
                    pickupSprite.enabled = false;
                    playerMovement.hasClaws = true;
                }   
            }
        }
    }

    //Checks when player is in range by checking if player is in the items hitbox
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
