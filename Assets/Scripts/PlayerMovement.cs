using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        STATE_STANDING,
        STATE_WALKING,
        STATE_JUMPING,
        STATE_DUB_JUMPING,
        STATE_SLOW_FALLING,
        STATE_CLINGING,
        STATE_WALL_JUMPING,
    }
    [SerializeField] private float speed;
    private Rigidbody2D body;
    private Animator anim;
    private State state;
    
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        state = State.STATE_STANDING;
    }
    
    private void Update()
    {
        switch (state)
        {
            case State.STATE_STANDING:
                if (Input.GetAxis("Horizontal") != 0)
                {
                    state = State.STATE_WALKING;
                    body.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, body.velocity.y);
                }
        }
        //body.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, body.velocity.y);
    }
}
