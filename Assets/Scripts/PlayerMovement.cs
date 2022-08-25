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
        RESPAWNING
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
    [SerializeField] private float slowFallSpeed;
    private float jumpBufferTimer;
    private float onGroundTimer;
    private bool usedDoubleJump;

    //Exploding
    private float respawnTimer;
    private float explodingTimer;
    


    //Player/Game Components components
    [SerializeField] private LayerMask groundLayer;
    private Rigidbody2D body;
    private Animator anim;
    private Transform tf;
    private BoxCollider2D playerCollider;
    private State state;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        tf = GetComponent<Transform>();
        playerCollider = GetComponent<BoxCollider2D>();
        state = State.STANDING;
        usedDoubleJump = false;
        respawnTimer = 0.5f;
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
        //also resets double jump when on ground
        if (onGround())
        {
            onGroundTimer = coyoteTimeBuffer;
            usedDoubleJump = false;
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

        if (tf.position.y < -5 && !(state == State.EXPLODING) && !(state == State.RESPAWNING))
        {
            Explode();
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

        switch (state)
        {
            case State.STANDING:
                //checks if player is starting to jump
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetTrigger("Jumping");
                    Jump();
                    print(state);
                }
                //checking for double jumping after walking off platform
                else if (jumpBufferTimer > 0)
                {
                    state = State.DOUBLE_JUMPING;
                    Jump();
                    anim.SetTrigger("DoubleJumping");
                    print(state);
                }
                //checks if player is starting to move
                else if (horizontalInput != 0)
                {
                    state = State.RUNNING;
                    anim.SetTrigger("Running");
                    print(state);
                }
                break;

            case State.RUNNING:
                MovePlayer();
                //checks if player is starting to jump
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetTrigger("Jumping");
                    Jump();
                    print(state);
                    break;
                }
                //checking for double jumping after walking off platform
                else if (jumpBufferTimer > 0)
                {
                    state = State.DOUBLE_JUMPING;
                    Jump();
                    anim.SetTrigger("DoubleJumping");
                    print(state);
                    break;
                }
                //checks if player has stopped moving. Checks if left/right are being pressed down as well as horizontalinput
                //so that the standing animation doesnt play when changing directions
                else if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow) && horizontalInput == 0)
                {
                    state = State.STANDING;
                    anim.SetTrigger("Standing");
                    print(state);
                }
                break;

            case State.JUMPING:
                MovePlayer();
                //checks if player is back on the ground, also checks velocity that way it doesnt trigger on the first frames of the jump
                if (onGround() && body.velocity.y <= 0)
                {
                    if (horizontalInput == 0)
                    {
                        state = State.STANDING;
                        anim.SetTrigger("Standing");
                        print(state);
                    }
                    else
                    {
                        state = State.RUNNING;
                        anim.SetTrigger("Running");
                        print(state);
                    }
                }
                //checks if player is trying to double jump
                else if (jumpBufferTimer > 0 && !usedDoubleJump)
                {
                    usedDoubleJump = true;
                    state = State.DOUBLE_JUMPING;
                    Jump();
                    anim.SetTrigger("DoubleJumping");
                    print(state);
                }
                else if (body.velocity.y < 0 && Input.GetKey(KeyCode.UpArrow))
                {
                    state = State.SLOW_FALLING;
                    anim.SetTrigger("DoubleJumping");
                    print(state);
                }
                break;

            case State.DOUBLE_JUMPING:
                MovePlayer();
                //checks if player is back on ground
                if (onGround() && body.velocity.y <= 0)
                {
                    if (horizontalInput == 0)
                    {
                        state = State.STANDING;
                        anim.SetTrigger("Standing");
                        print(state);
                    }
                    else
                    {
                        state = State.RUNNING;
                        anim.SetTrigger("Running");
                        print(state);
                    }
                }
                //checks if player is at the top of their jump should now be slow falling
                else if (body.velocity.y < 0 && Input.GetKey(KeyCode.UpArrow))
                {
                    state = State.SLOW_FALLING;
                    print(state);
                }
                else if (!Input.GetKey(KeyCode.UpArrow))
                {
                    state = State.JUMPING;
                    anim.SetTrigger("Jumping");
                    print(state);
                }
                break;

            case State.SLOW_FALLING:
                MovePlayer();
                //Checks if the player wants to stop falling slowly. We want to play the jumping animation here
                //but keep the player in the double jumping state
                if (!Input.GetKey(KeyCode.UpArrow))
                {
                    state = State.JUMPING;
                    anim.SetTrigger("Jumping");
                    print(state);
                } 
                //checks if player is back on the ground
                else if (onGround() && body.velocity.y <= 0)
                {
                    if (horizontalInput == 0)
                    {
                        state = State.STANDING;
                        anim.SetTrigger("Standing");
                        print(state);
                    }
                    else
                    {
                        state = State.RUNNING;
                        anim.SetTrigger("Running");
                        print(state);
                    }
                }
                break;

            case State.EXPLODING:
                body.velocity = new Vector3(0, 0, 0);
                if (explodingTimer <= 0)
                {
                    state = State.RESPAWNING;
                    anim.SetTrigger("Jumping");
                    respawnTimer = 0.5f;
                    tf.position = new Vector3(0, 0, 0);
                    print(state);
                }
                else
                {
                    explodingTimer -= Time.deltaTime;
                }
                break;

            case State.RESPAWNING:
                body.velocity = new Vector3(0, 0, 0);
                if (respawnTimer <= 0)
                {
                    state = State.JUMPING;
                    print(state);
                } else
                {
                    respawnTimer -= Time.deltaTime;
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
        //freeze player in air when exploding
        if (state == State.EXPLODING || state == State.RESPAWNING)
        {
            body.gravityScale = 0f;
        }
        //propeller slow fall speed
        else if (state == State.SLOW_FALLING)
        {
            body.gravityScale = slowFallSpeed;
        }
        //makes player fall faster that way jump is less floaty
        else if (body.velocity.y < 0)
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

    private void Explode()
    {
        state = State.EXPLODING;
        explodingTimer = 0.25f;
        anim.SetTrigger("Exploding");
        print(state);
    }
}
