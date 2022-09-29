using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        print("Camera y: " + boundPosition.y + "Player y: " + player.position.y);
        transform.position = new Vector3(boundPosition.x, boundPosition.y, transform.position.z);
        lookAhead = Mathf.Lerp(lookAhead, (aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed);

        // scene specific camera movement
        if (SceneManager.GetActiveScene().name == "Level1")
        {
            if (player.position.x < 75)
            {
                minPos = new Vector3(minPos.x, Mathf.Lerp(minPos.y, 1, Time.deltaTime), minPos.z);
                maxPos = new Vector3(maxPos.x, Mathf.Lerp(maxPos.y, 1, Time.deltaTime), maxPos.z);
            }
            else if (player.position.x > 75 && player.position.x < 138)
            {
                minPos = new Vector3(minPos.x, Mathf.Lerp(minPos.y, 5, Time.deltaTime), minPos.z);
                maxPos = new Vector3(maxPos.x, Mathf.Lerp(maxPos.y, 5, Time.deltaTime), maxPos.z);
            }
            else if (player.position.x > 138)
            {
                minPos = new Vector3(minPos.x, Mathf.Lerp(minPos.y, 1.5f, Time.deltaTime), minPos.z);
                maxPos = new Vector3(maxPos.x, Mathf.Lerp(maxPos.y, 1.5f, Time.deltaTime), maxPos.z);
            }
        }
    }
}
