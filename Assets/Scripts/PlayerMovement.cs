using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        EXPLODING,
        RESPAWNING
    }
    //Level variables
    public bool hasTail;
    public bool hasClaws;

    //Movement variables
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float groundDrag;
    private float horizontalInput;
    private bool changingDirection;
    public bool canMove;

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

    //Wall Jump/Cling variables
    [SerializeField] private float wallJumpSpeed;
    [SerializeField] private float clingingFallSpeed;
    [SerializeField] private float clingBuffer;
    private float clingBufferTimer;

    //Exploding/Respawning
    private float respawnTimer;
    private float explodingTimer;
    public Vector3 spawnPoint;
    
    //Player/Game Components components
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask spikesLayer;
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
        canMove = true;

        //movement bools based on level, only on level3 do you START with an upgrade
        if (SceneManager.GetActiveScene().name == "Level3")
        {
            hasTail = true;
            hasClaws = false;
        } else
        {
            hasTail = false;
            hasClaws = false;
        }
    }
    
    private void Update()
    {
        if (state != State.EXPLODING && state != State.RESPAWNING && canMove)
        {
            //Stores current left/right arrow input as: -1, 0, or 1 where -1 is left, 0 is nothing, and 1 is right, used for choosing direction to move player
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        //jump buffer (stores jump so that they can press jump right before they hit the ground)
        if (Input.GetKeyDown(KeyCode.Space) && canMove)
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

        //chanings which way sprite is facing based on input (doesnt change when player is clinging to a wall)
        if (state != State.CLINGING)
        {
            if (horizontalInput > 0.01f)
            {
                transform.localScale = new Vector3(4, 4, 4);
            }
            else if (horizontalInput < -0.01f)
            {
                transform.localScale = new Vector3(-4, 4, 4);
            }
        }

        //Checks if player has fallen off map
        if (tf.position.y < -5 && !(state == State.EXPLODING) && !(state == State.RESPAWNING))
        {
            Explode();
        }
        
        //Checks if player has hit a spike
        if (onSpike()) {
            tf.position = new Vector3(tf.position.x, tf.position.y + 0.1f, tf.position.z);
            Explode();
        }

        //Counts down the timer used for giving the player a window for wall jumping after they leave a wall
        if (clingBufferTimer > 0)
        {
            clingBufferTimer -= Time.deltaTime;
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
                else if (hasTail && jumpBufferTimer > 0)
                {
                    state = State.DOUBLE_JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetTrigger("DoubleJumping");
                    usedDoubleJump = true;
                    Jump();
                    print(state);
                    break;
                }
                //checks if player is starting to move
                else if (horizontalInput != 0)
                {
                    state = State.RUNNING;
                    anim.SetTrigger("Running");
                    print(state);
                }
                //checking if player is starting to cling to wall
                else if (hasClaws && onWall())
                {
                    state = State.CLINGING;
                    anim.SetTrigger("Clinging");
                    body.velocity = new Vector2(body.velocity.x, body.velocity.y / 2);
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
                //checking if player is starting to cling to wall
                else if (hasClaws && onWall())
                {
                    state = State.CLINGING;
                    anim.SetTrigger("Clinging");
                    body.velocity = new Vector2(body.velocity.x, body.velocity.y / 2);
                    print(state);
                }
                //checking for double jumping after walking off platform
                else if (hasTail && jumpBufferTimer > 0)
                {
                    state = State.DOUBLE_JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetTrigger("DoubleJumping");
                    usedDoubleJump = true;
                    Jump();
                    print(state);
                    break;
                }
                //checks if player has stopped moving. Checks if left/right are being pressed down as well as horizontalinput
                //so that the standing animation doesnt play when changing directions
                else if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && horizontalInput == 0)
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
                //used for allowing playing to wall jump right after leaving wall
                else if (hasClaws && jumpBufferTimer > 0 && clingBufferTimer > 0)
                {
                    jumpBufferTimer = 0;
                    clingBufferTimer = 0;
                    WallJump();
                }
                //checking if player is starting to cling to wall
                else if (hasClaws && onWall())
                {
                    state = State.CLINGING;
                    anim.SetTrigger("Clinging");
                    body.velocity = new Vector2(body.velocity.x, body.velocity.y / 2);
                    print(state);
                }
                //checks if player is trying to double jump
                else if (hasTail && jumpBufferTimer > 0 && !usedDoubleJump)
                {
                    usedDoubleJump = true;
                    state = State.DOUBLE_JUMPING;
                    Jump();
                    anim.SetTrigger("DoubleJumping");
                    print(state);
                }
                //checks if player is trying to slow fall (this for for when the player has already double jumped)
                else if (hasTail && body.velocity.y < 0 && Input.GetKey(KeyCode.Space) && usedDoubleJump)
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
                //checking if player is starting to cling to wall
                else if (hasClaws && onWall())
                {
                    state = State.CLINGING;
                    anim.SetTrigger("Clinging");
                    body.velocity = new Vector2(body.velocity.x, body.velocity.y / 2);
                    print(state);
                }
                //checks if player is at the top of their jump should now be slow falling
                else if (body.velocity.y < 0 && Input.GetKey(KeyCode.Space))
                {
                    state = State.SLOW_FALLING;
                    print(state);
                }
                else if (!Input.GetKey(KeyCode.Space))
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
                if (!Input.GetKey(KeyCode.Space))
                {
                    state = State.JUMPING;
                    anim.SetTrigger("Jumping");
                    print(state);
                }
                //checking if player is starting to cling to wall
                else if (hasClaws && onWall())
                {
                    state = State.CLINGING;
                    anim.SetTrigger("Clinging");
                    body.velocity = new Vector2(body.velocity.x, body.velocity.y / 2);
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

            case State.CLINGING:
                MovePlayer();
                //checks if player is no longer on the wall
                if (!onWall())
                {
                    state = State.JUMPING;
                    anim.SetTrigger("Jumping");
                    usedDoubleJump = false;
                    //used to give the player a tiny window to wall jump after leaving the wall
                    clingBufferTimer = clingBuffer;
                    print(state);
                }
                //checks if player is trying to wall jump
                else if (jumpBufferTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    anim.SetTrigger("Jumping");
                    usedDoubleJump = false;
                    WallJump();
                    print(state);
                }
                break;

            case State.EXPLODING:
                body.velocity = new Vector3(0, 0, 0);
                //player is in the exploding animation for a set amount of time and then is respawned
                if (explodingTimer <= 0)
                {
                    state = State.RESPAWNING;
                    anim.SetTrigger("Jumping");
                    respawnTimer = 0.5f;
                    tf.position = spawnPoint;
                    print(state);
                }
                else
                {
                    explodingTimer -= Time.deltaTime;
                }
                break;

            case State.RESPAWNING:
                body.velocity = new Vector3(0, 0, 0);
                //holds the player in the air for certain amount of time
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

    private void WallJump()
    {
        //launches player diagonally away from wall they are touching
        body.drag = airDrag;
        body.velocity = new Vector2(body.velocity.x, 0f);
        if (clingingLeft())
        {
            body.AddForce(new Vector2(1, 1) * wallJumpSpeed, ForceMode2D.Impulse);
        }
        else
        {
            body.AddForce(new Vector2(-1, 1) * wallJumpSpeed, ForceMode2D.Impulse);
        }
        //faces them away from wall
        transform.localScale = new Vector3(-transform.localScale.x, 4, 4);
    }

    private void AdjustFallSpeed()
    {
        //freeze player in air when exploding
        if (state == State.EXPLODING || state == State.RESPAWNING)
        {
            body.gravityScale = 0f;
        }
        //clinging slow fall
        else if (state == State.CLINGING)
        {
            body.gravityScale = clingingFallSpeed;
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
        else if (body.velocity.y > 0 && !Input.GetKey(KeyCode.Space)) {
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
        //checks if there are any groundLayer colliders below the player
        RaycastHit2D raycastHitGround = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.down, 0.05f, groundLayer);
        return raycastHitGround.collider != null;
    }

    private bool onWall()
    {
        
        RaycastHit2D raycastHitWallLeft = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.left, 0.02f, groundLayer);
        RaycastHit2D raycastHitWallRight = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.right, 0.02f, groundLayer);
        return ((raycastHitWallLeft.collider != null) || (raycastHitWallRight.collider != null)) && !onGround();
    }

    private bool onSpike() {
        RaycastHit2D raycastHitSpikeDown = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.down, 0.05f, spikesLayer);
        return (raycastHitSpikeDown.collider != null);
    }

    private bool clingingRight()
    {
        RaycastHit2D raycastHitWallRight = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.right, 0.02f, groundLayer);
        return raycastHitWallRight.collider != null;
    }

    private bool clingingLeft()
    {
        RaycastHit2D raycastHitWallLeft = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.left, 0.02f, groundLayer);
        return raycastHitWallLeft.collider != null;
    }

    private void Explode()
    {
        state = State.EXPLODING;
        explodingTimer = 0.25f;
        anim.SetTrigger("Exploding");
        print(state);
    }
}
