using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float cameraSpeed;
    [SerializeField] private float aheadDistance;
    [SerializeField] private Transform player;
    private float lookAhead;

    public Vector3 minPos, maxPos;

    private void Update()
    {
        Vector3 boundPosition = new Vector3(
            Mathf.Clamp(player.position.x + lookAhead, minPos.x, maxPos.x), 
            Mathf.Clamp(player.position.y, minPos.y, maxPos.y), 
            Mathf.Clamp(player.position.z, minPos.z, maxPos.z));

        //Makes the camera follow a little ahead of the player
        transform.position = new Vector3(boundPosition.x, boundPosition.y, transform.position.z);
        lookAhead = Mathf.Lerp(lookAhead, (aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed);
    }
}
