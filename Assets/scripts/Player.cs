using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    // DON'T USE [SERIALIZEFIELD], THE PLAYER GETS DELETED AND REINITIALIZED!

    float moveSpeed = 11f;

    // this should be fixed later
    LayerMask groundLayer;
    Animator animator;
    public Rigidbody2D rb;

    float jumpForce = 3000f;

    // set up a jump queue timer (when the player presses jump in the air, queue that jump command for a short period of time, for better player experience)
    // if the current time for jump queue is 0, that means that there is no jump command queued
    float jumpQueueCurrentTime = 0f;
    // how long will the queue last
    float jumpQueueTime = 0.2f;

    // coyote time is the amount of time the player can be in the air to make a jump (they fall off a cliff and then they jump)
    // this removes the frustration of not jumping when pressing jump on the edge
    // same logic as the jumpQueue timer
    float coyoteCurrentTime = 0f;
    float coyoteTime = 0.2f;

    // keeps track of how many controllable blocks are nearby
    int nearbyBlocks = 0;
    // if canMove is false, the player can't move, used for entering the object manipulation phase and transitioning stages.
    bool canMove = true;

    //how long the player stops when transitioning between stages, it's a bit less than the actual transition time so it feels better for the player
    float transitionStopTime = 1f;

    // store the initial scale of the player (I scaled up the player and its size gets changed on play mode)
    private Vector3 initialScale;

    // start is called before the first frame update
    void Start()
    {
        //setup the ground layer (This was how the Unity api used it)
        groundLayer = LayerMask.GetMask("Ground");


        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // store the initial scale of the player
        initialScale = transform.localScale;
    }

    // update is called once per frame
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

            // move the player based on the horizontal input
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

            // flip the sprite based on movement direction
            if (horizontal > 0) // moving right
            {
                transform.localScale = new Vector3(initialScale.x, initialScale.y, initialScale.z); // keep original scale and normal facing direction
            }
            else if (horizontal < 0) // moving left
            {
                transform.localScale = new Vector3(-initialScale.x, initialScale.y, initialScale.z); // flip only x axis, keep original scale for y and z
            }

            // throw a sphere cast and see if the player is colliding with the ground
            if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f, -transform.up, 0.25f, groundLayer))
            {
                coyoteCurrentTime = coyoteTime;
            }

            // if the player presses "jump"
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                jumpQueueCurrentTime = jumpQueueTime;
            }

            // the button to manipulate objects
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (nearbyBlocks > 0)
                {
                    StartCoroutine(manipulationPhase(0.2f));
                }
            }

            // set "isRunning" based on horizontal movement
            animator.SetBool("isRunning", Mathf.Abs(horizontal) > 0.1f);

            // set "isIdle" parameter if the player is not moving and not jumping
            if (Mathf.Abs(horizontal) < 0.1f && coyoteCurrentTime <= 0)
            {
                animator.SetBool("isRunning", false);
            }
        }
        else
        {
            rb.velocity /= 1.3f;
        }
    }

    void timers()
    {
        // if there is a jump queued, reduce the time
        if (jumpQueueCurrentTime > 0)
        {
            jumpQueueCurrentTime = Mathf.Max(0, jumpQueueCurrentTime - Time.deltaTime);
        }

        if (coyoteCurrentTime > 0)
        {
            // if there is a jump command queued
            if (jumpQueueCurrentTime > 0)
            {
                // reset the y velocity (so the jump doesn't get cancelled by a quickly falling player)
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                // jump
                rb.AddForce(new Vector2(0f, jumpForce));
                jumpQueueCurrentTime = 0f;
                coyoteCurrentTime = 0f;

                // trigger the Jump animation

                animator.SetTrigger("isJumping");
            }
            coyoteCurrentTime = Mathf.Max(0, coyoteCurrentTime - Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Block otherScript = other.gameObject.GetComponent<Block>();
        // if the collided object is a "Block" object, it can be manipulated
        if (otherScript != null)
        {
            otherScript.nearPlayer = true;
            nearbyBlocks += 1;
        }

        if (other.gameObject.tag == "Coin")
        {
            GameObject.Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "TransitionPoint")
        {

            //try to see if the camera will change, if so, stop player movement for some time
            if (other.gameObject.GetComponent<TransitionPoint>().WarpToLevel())
            {
                StartCoroutine(StopMovement(transitionStopTime));
            }


        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Block otherScript = other.gameObject.GetComponent<Block>();
        // if the collided object is a "Block" object, it can't be manipulated anymore (got far)
        if (otherScript != null)
        {
            otherScript.nearPlayer = false;
            nearbyBlocks -= 1;
        }
    }

    IEnumerator manipulationPhase(float seconds)
    {
        transform.localScale = initialScale;
        animator.SetTrigger("DisappearTrigger");

        // wait for the animation to finish 
        yield return new WaitForSeconds(0.5f);

        // Nnw destroy the player 
        GameObject.Destroy(gameObject);
    }

    //stop player movement for some time
    IEnumerator StopMovement(float seconds)
    {
        print("STOP");
        canMove = false;
        yield return new WaitForSeconds(seconds);
        print("MOVE");
        canMove = true;
    }
}
