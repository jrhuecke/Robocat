using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float distance;
    private Transform platform;
    private float center;
    private bool movingRight;

    void Awake()
    {
        platform = GetComponent<Transform>();
        center = platform.position.x;
        movingRight = true;
    }

    void FixedUpdate()
    {
        //moves the platform left and right based off a speed and distance
        if (movingRight)
        {
            if (platform.position.x > center + distance)
            {
                movingRight = false;
            } else
            {
                platform.position = new Vector3(platform.position.x + (speed/100), platform.position.y, platform.position.z);
            }
        } else
        {
            if (platform.position.x < center - distance)
            {
                movingRight = true;
            }
            else
            {
                platform.position = new Vector3(platform.position.x - (speed/100), platform.position.y, platform.position.z);
            }
        }
    }

    //Makes it so the player moves along with the platform by making the player a child of the platform when they collide with it
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.transform.parent = transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.transform.parent = null;
        }
    }
}
