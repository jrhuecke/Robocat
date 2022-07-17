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

    void Update()
    {
        if (movingRight)
        {
            if (platform.position.x > center + distance)
            {
                movingRight = false;
            } else
            {
                platform.position = new Vector3(platform.position.x + (speed * Time.deltaTime), platform.position.y, platform.position.z);
            }
        } else
        {
            if (platform.position.x < center - distance)
            {
                movingRight = true;
            }
            else
            {
                platform.position = new Vector3(platform.position.x - (speed * Time.deltaTime), platform.position.y, platform.position.z);
            }
        }
    }
}
