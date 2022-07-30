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

    //Movement variables
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float groundDrag;
    private float horizontalInput;
    private bool changingDirection;

    //Jumping variables
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpBuffer;
    [SerializeField] private float coyoteTimeBuffer;
    [SerializeField] private float airDrag;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float lowJumpMultiplier;
    private float jumpBufferTimer;
    private float onGroundTimer;


    //Player/Game Components components
    [SerializeField] private LayerMask groundLayer;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D playerCollider;
    private State state;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider2D>();
        state = State.STANDING;
    }
    
    private void Update()
    {
        //Stores current left/right arrow input as: -1, 0, or 1 where -1 is left, 0 is nothing, and 1 is right, used for choosing direction to move player
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

        //allows for coyote time (lets player jump for a short period of time after leaving a platform)
        if (onGround())
        {
            onGroundTimer = coyoteTimeBuffer;
        }
        else
        {
            if (onGroundTimer >= 0)
            {
                onGroundTimer -= Time.deltaTime;
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
        //applies drag to player (needs to happen regardless of state)
        if (onGround())
        {
            ApplyGroundDrag();
        }
        else
        {
            ApplyAirDrag();
        }

        //adjusts players fall speed (player can be falling while in multiple different states)
        AdjustFallSpeed();
        
        //check if player is inputting regardless of state
        MovePlayer();

        switch (state)
        {
            case State.STANDING:
                //checks if player is starting to jump
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetBool("Standing", false);
                    anim.SetBool("Jumping", true);
                    Jump();
                    print(state);
                }
                //checks if player is starting to move
                else if (horizontalInput != 0)
                {
                    state = State.RUNNING;
                    anim.SetBool("Standing", false);
                    anim.SetBool("Running", true);
                    print(state);
                }
                break;

            case State.RUNNING:
                //checks if player is starting to jump
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetBool("Running", false);
                    anim.SetBool("Jumping", true);
                    Jump();
                    print(state);
                }
                //checks if player has stopped moving. Checks if left/right are being pressed down as well as horizontalinput
                //so that the standing animation doesnt play when changing directions
                else if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow) && horizontalInput == 0)
                {
                    state = State.STANDING;
                    anim.SetBool("Running", false);
                    anim.SetBool("Standing", true);
                    print(state);   
                }       
                break;

            case State.JUMPING:
           
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
        //adds force to the player until they have reached a max speed
        body.AddForce(new Vector2(horizontalInput, 0f) * acceleration);
        
        if (Mathf.Abs(body.velocity.x) > maxSpeed)
        {
            body.velocity = new Vector2(Mathf.Sign(body.velocity.x) * maxSpeed, body.velocity.y);
        }
    }

    private void Jump()
    {
        //applies upward force to player (impulse makes it instant)
        body.drag = airDrag;
        body.velocity = new Vector2(body.velocity.x, 0f);
        body.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
    }

    private void AdjustFallSpeed()
    {
        //makes player fall faster that way jump is less floaty
        if (body.velocity.y < 0)
        {
            body.gravityScale = fallMultiplier;
        } 
        //allows play to do short jumps by increasing gravity when they let go of jump
        else if (body.velocity.y > 0 && !Input.GetKey(KeyCode.UpArrow)) {
            body.gravityScale = lowJumpMultiplier;
        }
        //resets player gravity when not jumping
        else
        {
            body.gravityScale = 1f;
        }
    }

    private void ApplyGroundDrag()
    {
        //checks if the player is currently trying to change directions by comparing player velocity to current input
        if ((body.velocity.x > 0f && horizontalInput < 0f) || (body.velocity.x < 0f && horizontalInput > 0f))
        {
            changingDirection = true;
        }
        else
        {
            changingDirection = false;
        }
        
        //applies drag when the player is not inputting left/right or when the player is trying to change directions
        if (Mathf.Abs(horizontalInput) < 0.8f || changingDirection)
        {
            body.drag = groundDrag;
        }
        else
        {
            body.drag = 0f;
        }
    }

    private void ApplyAirDrag()
    {
        body.drag = airDrag;
    }

    private bool onGround()
    {
        //returns null if there is no collider found 0.05f below player
        RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.down, 0.05f, groundLayer);
        return raycastHit.collider != null;
    }
}
