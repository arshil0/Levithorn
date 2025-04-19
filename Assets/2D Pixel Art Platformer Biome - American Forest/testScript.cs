using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour
{

    float moveSpeed = 15;
    [SerializeField] LayerMask groundLayer;
    Animator animator;
    public Rigidbody2D rb;

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

    //keeps track of how many controllable blocks are nearby
    int nearbyBlocks = 0;
    //if canMove is false, the player can't move, used for entering the object manipulation phase.
    bool canMove = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        input();
        timers();
    }

    void input()
    {
        if (canMove)
        {
            var horizontal = Input.GetAxis("Horizontal");

            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

            //throw a sphere cast and see if the player is colliding with the ground
            //THESE ARE HARD-CODED VALUES, IF THE PLAYER SIZE CHANGES THESE SHOULD CHANGE!
            if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.5f, -transform.up, 0.25f, groundLayer))
            {
                coyoteCurrentTime = coyoteTime;
            }

            //if the player presses "jump"
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                jumpQueueCurrentTime = jumpQueueTime;
                //jumping is handled under "CoyoteCurrentTime" in the "timers" function
            }

            //the button to manipulate objects
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (nearbyBlocks > 0)
                {
                    StartCoroutine(manipulationPhase(0.2f));
                }
            }
        }
        else
        {
            rb.velocity /= 1.3f;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        Block otherScript = other.gameObject.GetComponent<Block>();
        //if the collided object is a "Block" object, it can be manipulated
        if (otherScript != null)
        {
            otherScript.nearPlayer = true;
            nearbyBlocks += 1;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Block otherScript = other.gameObject.GetComponent<Block>();
        //if the collided object is a "Block" object, it can't be manipulated anymore (got far)
        if (otherScript != null)
        {
            otherScript.nearPlayer = false;
            nearbyBlocks -= 1;
        }

    }


    IEnumerator manipulationPhase(float seconds)
    {
        canMove = false;
        //play some animation here before yield

        yield return new WaitForSeconds(seconds);
        GameObject.Destroy(gameObject);
    }
}
