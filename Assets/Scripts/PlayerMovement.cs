using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        STANDING,
        RUNNING,
        JUMPING,
        DOUBLE_JUMPING,
        SLOW_FALLING,
        CLINGING,
        WALL_JUMPING,
        EXPLODING,
    }
    [SerializeField] private float speed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] public float jumpBuffer;
    private float jumpBufferTimer;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D playerCollider;
    private State state;
    private float horizontalInput;


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider2D>();
        state = State.STANDING;
    }
    
    private void Update()
    {
        //Stores current left/right arrow input as a num between -1 and 1 where -1 is left, 0 is nothing, and 1 is right
        horizontalInput = Input.GetAxis("Horizontal");

        //jump buffer (stores jump so that they can press jump right before they hit the ground)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpBufferTimer = jumpBuffer;
        } else
        {
            if (jumpBufferTimer >= 0)
            {
                jumpBufferTimer -= Time.deltaTime;
            }
        }

        //chanings which way sprite is facing based on input
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(4, 4, 4);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-4, 4, 4);
        }

        switch (state)
        {
            case State.STANDING:
                //checks if player is starting to jump
                if (jumpBufferTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetBool("Standing", false);
                    anim.SetBool("Jumping", true);
                    body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                    print(state);
                }
                //checks if player is starting to move
                else if (horizontalInput != 0)
                {
                    state = State.RUNNING;
                    anim.SetBool("Standing", false);
                    anim.SetBool("Running", true);
                    body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
                    print(state);
                }
                break;

            case State.RUNNING:
                body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
                //checks if player has stopped moving. Checks if left/right are being pressed down on top of horizontalinput
                //so that the standing animation doesnt play when changing directions while running
                if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow) && horizontalInput == 0)
                {
                    state = State.STANDING;
                    anim.SetBool("Running", false);
                    anim.SetBool("Standing", true);
                    print(state);
                }
                //checks if player is starting to jump
                else if (jumpBufferTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetBool("Running", false);
                    anim.SetBool("Jumping", true);
                    body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                    print(state);
                }
                break;

            case State.JUMPING:
                body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
                //checks if player is back on the ground, also checks velocity that way it doesnt trigger on the first frames of the jump
                if (onGround() && body.velocity.y <= 0)
                {
                    state = State.STANDING;
                    anim.SetBool("Jumping", false);
                    anim.SetBool("Standing", true);
                    print(state);
                }
                break;

        }
    }
    private bool onGround()
    {
        //returns null if there is no collider found 0.1f below player
        RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
}
