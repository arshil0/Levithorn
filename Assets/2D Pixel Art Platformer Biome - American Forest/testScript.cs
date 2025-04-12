using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour
{

    float moveSpeed = 15;
    [SerializeField] LayerMask groundLayer;
    Animator animator;
    Rigidbody2D rb;

    float jumpForce = 3500f;


    //set up a jump queue timer (when the player presses jump in the air, queue that jump command for a short period of time, for better player experience)
    //if the current time for jump queue is 0, that means that there is no jump command queued
    float jumpQueueCurrentTime = 0f;
    //how long will the queue last
    float jumpQueueTime = 0.2f;

    //Coyote time is the amount of time the player can be in the air to make a jump (they fall off a cliff and then they jump)
    //this removes the frustration of not jumping when pressing jump on the edge
    //same logic as the jumpQueue timer
    float coyoteCurrentTime = 0f;
    float coyoteTime = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal");

        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        //throw a sphere cast and see if the player is colliding with the ground
        //THESE ARE HARD-CODED VALUES, IF THE PLAYER SIZE CHANGES THESE SHOULD CHANGE!
        if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.5f, -transform.up, 0.25f, groundLayer))
        {
            coyoteCurrentTime = coyoteTime;
        }

        input();
        timers();
    }

    void input()
    {
        //if the player presses "jump"
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpQueueCurrentTime = jumpQueueTime;
            //jumping is handled under "CoyoteCurrentTime" in the "timers" function
        }
    }

    void timers()
    {
        //if there is a jump queued, reduce the time
        if (jumpQueueCurrentTime > 0)
        {
            jumpQueueCurrentTime = Mathf.Max(0, jumpQueueCurrentTime - Time.deltaTime);
        }

        if (coyoteCurrentTime > 0)
        {
            //if there is a jump command queued
            if (jumpQueueCurrentTime > 0)
            {
                //reset the y velocity (so the jump doesn't get cancled by a quickly falling player)
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                //jump
                rb.AddForce(new Vector2(0f, jumpForce));
                jumpQueueCurrentTime = 0f;
                coyoteCurrentTime = 0f;

                animator.SetTrigger("Jump");
            }
            coyoteCurrentTime = Mathf.Max(0, coyoteCurrentTime - Time.deltaTime);
        }
    }
}
