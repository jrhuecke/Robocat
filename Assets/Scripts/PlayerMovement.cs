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
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float decelartion;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpBuffer;
    [SerializeField] private LayerMask groundLayer;
    private float jumpBufferTimer;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D playerCollider;
    private State state;
    private float horizontalInput;
    private bool changingDirection;


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider2D>();
        state = State.STANDING;
    }
    
    private void Update()
    {
        //Stores current left/right arrow input as: -1, 0, or 1 where -1 is left, 0 is nothing, and 1 is right
        horizontalInput = Input.GetAxisRaw("Horizontal");

        //jump buffer (stores jump so that they can press jump right before they hit the ground)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpBufferTimer = jumpBuffer;
        } 
        else
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
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.STANDING:
                //checks if player is starting to jump
                if (jumpBufferTimer > 0 && onGround())
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
                    MovePlayer();
                    ApplyDrag();
                    print(state);
                }
                break;

            case State.RUNNING:
                MovePlayer();
                ApplyDrag();
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
                else if (jumpBufferTimer > 0 && onGround())
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
                MovePlayer();
                ApplyDrag();
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

    private void MovePlayer()
    {
        body.AddForce(new Vector2(horizontalInput, 0f) * acceleration);
        
        if (Mathf.Abs(body.velocity.x) > maxSpeed)
        {
            body.velocity = new Vector2(Mathf.Sign(body.velocity.x) * maxSpeed, body.velocity.y);
        }
    }

    private void ApplyDrag()
    {
        //checks if the player is currently trying to change directions by comparing player velocity to current input
        if ((body.velocity.x > 0f && horizontalInput < 0f) || (body.velocity.y < 0f && horizontalInput > 0f))
        {
            changingDirection = true;
        }
        else
        {
            changingDirection = false;
        }
        
        //applies drag when the player is not inputting left/right or when the player is trying to change directions
        if (Mathf.Abs(horizontalInput) < 1f || changingDirection)
        {
            body.drag = decelartion;
        }
        else
        {
            body.drag = 0f;
        }
    }

    private bool onGround()
    {
        //returns null if there is no collider found 0.05f below player
        RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.down, 0.05f, groundLayer);
        return raycastHit.collider != null;
    }
}
