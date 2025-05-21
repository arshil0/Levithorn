using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    // DON'T USE [SERIALIZEFIELD], THE PLAYER GETS DELETED AND REINITIALIZED!
    public AnimatorOverrideController defaultAnimator;
    public static bool newSkinUnlocked = false;
    private static bool useNewSkin = false;
    public AnimatorOverrideController newSkinAnimator;
    private static int collectedItems = 0;
    public static bool skinChanged = false;
    public GameObject skinUnlockText;





    float moveSpeed = 11f;

    LayerMask groundLayer;
    Animator animator;
    public Rigidbody2D rb;

    float jumpForce = 3000f;

    SpriteRenderer sprite;

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

    // there is a jump cooldown so the player doesn't instantly jump multiple times
    float jumpCooldownCurrentTime = 0f;
    float jumpCooldown = 0.21f;


    // keeps track of nearby blocks that can be manipulated
    List<GameObject> nearbyBlocks = new List<GameObject>();

    //I couldn't find a Size() function for the list class, so I keep an extra integer
    int numberOfNearbyBlocks = 0;
    // if canMove is false, the player can't move, used for entering the object manipulation phase and transitioning stages.
    bool canMove = true;

    //how long the player stops when transitioning between stages, it's a bit less than the actual transition time so it feels better for the player
    float transitionStopTime = 0.5f;

    // store the initial scale of the player (I scaled up the player and its size gets changed on play mode)
    private Vector3 initialScale;

    AudioSource walkingSound;
    AudioSource jumpSound;

    GameObject eButtonDisplay;

    //when you transition to a new level, your position is saved, to load back in the last checkpoint upon restarting
    public static Vector3 lastCheckpointPosition;
    //if this variable is set to false, the player shouldn't spawn, that means that the last checkpoint was reached by a manipulated block (WIP)
    public static bool shouldSpawn = true;

    //ignores the stopping condition (not being able to move for some time) upon entering a new level, used to avoid waiting after restarting
    bool ignoreTransitionStop = false;

    // start is called before the first frame update
    void Start()
    {
        CollectibleManager.instance.SetPlayer(this);
        //setup the ground layer (This was how the Unity api used it)
        groundLayer = LayerMask.GetMask("Ground");

        sprite = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
        if (newSkinUnlocked)
        {
            animator.runtimeAnimatorController = useNewSkin ? newSkinAnimator : defaultAnimator;
        }

        // store the initial scale of the player
        initialScale = transform.localScale;

        walkingSound = transform.Find("walkingSound").GetComponent<AudioSource>();
        walkingSound.volume = 0f;

        jumpSound = transform.Find("jumpSound").GetComponent<AudioSource>();

        eButtonDisplay = transform.Find("Canvas/EButtonDisplay").gameObject;
        eButtonDisplay.SetActive(false);

        //if the game is restarting (or pressing continue), try to spawn at the last checkpoint
        if (GlobalScript.restarting)
        {
            GlobalScript.restarting = false;
            if (!shouldSpawn)
            {
                Destroy(gameObject);
            }

            //if there is no checkpoint saved, check to see if there is a save
            if (lastCheckpointPosition == new Vector3(0f, 0f, 0f))
            {
                float x = PlayerPrefs.GetFloat("lastCheckpointPositionX");
                float y = PlayerPrefs.GetFloat("lastCheckpointPositionY");

                lastCheckpointPosition = new Vector3(x, y, 0f);
            }

            //if there is still nothing, start from 0
            if (lastCheckpointPosition != new Vector3(0f, 0f, 0f))
            {
                transform.position = lastCheckpointPosition;
            }
        }

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
                sprite.flipX = false;
            }
            else if (horizontal < 0) // moving left
            {
                sprite.flipX = true;
            }

            // throw a sphere cast and see if the player is colliding with the ground
            if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f, -transform.up, 0.25f, groundLayer))
            {
                coyoteCurrentTime = coyoteTime;
            }

            // if the player presses "jump"
            if (jumpCooldownCurrentTime <= 0f && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                jumpQueueCurrentTime = jumpQueueTime;
            }

            // the button to manipulate objects
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (numberOfNearbyBlocks > 0)
                {
                    StartCoroutine(manipulationPhase(0.5f));
                }
            }

            handleAnimationAndSound(horizontal);
        }
        else
        {
            rb.velocity /= 1.3f;
        }

        if (newSkinUnlocked && Input.GetKeyDown(KeyCode.K))
        {
            useNewSkin = !useNewSkin;
            animator.runtimeAnimatorController = useNewSkin ? newSkinAnimator : defaultAnimator;
        }
    }

    void handleAnimationAndSound(float horizontal)
    {
        // set "isRunning" based on horizontal movement
        animator.SetBool("isRunning", Mathf.Abs(horizontal) > 0.1f);

        //play a walking sound if the player is walking
        if (Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                if (!walkingSound.isPlaying)
                {
                    walkingSound.Play();
                }
                if (walkingSound.volume < 1)
                {
                    walkingSound.volume = Mathf.Min(1, walkingSound.volume + Time.deltaTime * 4);
                }
            }
            else
            {
                //fade the walking sound out, instead of instantly stopping it, so it feels more natural
                if (walkingSound.volume > 0)
                {
                    walkingSound.volume = Mathf.Max(0, walkingSound.volume - Time.deltaTime * 4);
                }
            }
        }
        else
        {
            //fade the walking sound out, instead of instantly stopping it, so it feels more natural
            if (walkingSound.volume > 0)
            {
                walkingSound.volume = Mathf.Max(0, walkingSound.volume - Time.deltaTime * 4);
            }
        }

        // set "isIdle" parameter if the player is not moving and not jumping
        if (Mathf.Abs(horizontal) < 0.1f && coyoteCurrentTime <= 0)
        {
            animator.SetBool("isRunning", false);
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
            if (jumpQueueCurrentTime > 0 && Time.timeScale > 0)
            {
                // reset the y velocity (so the jump doesn't get cancelled by a quickly falling player)
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                // jump
                rb.AddForce(new Vector2(0f, jumpForce));
                jumpQueueCurrentTime = 0f;
                coyoteCurrentTime = 0f;
                jumpCooldownCurrentTime = jumpCooldown;

                //randomize jump sound properties and then play it
                jumpSound.pitch = Random.Range(0.92f, 1.4f);
                jumpSound.volume = Random.Range(0.45f, 0.75f);
                jumpSound.Play();

                // trigger the Jump animation

                animator.SetTrigger("isJumping");
            }
            coyoteCurrentTime = Mathf.Max(0, coyoteCurrentTime - Time.deltaTime);
        }

        if (jumpCooldownCurrentTime > 0)
        {
            jumpCooldownCurrentTime = Mathf.Max(0, jumpCooldownCurrentTime - Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Block otherScript = other.gameObject.GetComponent<Block>();
        // if the collided object is a "Block" object, it can be manipulated
        if (otherScript != null)
        {
            otherScript.nearPlayer = true;
            numberOfNearbyBlocks += 1;
            nearbyBlocks.Add(other.gameObject);
            eButtonDisplay.SetActive(true);
        }

        if (other.gameObject.tag == "Collectible")
        {
            string objName = other.gameObject.name;
            Destroy(other.gameObject);
            collectedItems++;
            CollectibleManager.instance.CollectItem(objName);

            if (collectedItems >= 5 && !newSkinUnlocked)
            {
                newSkinUnlocked = true;
                useNewSkin = true;
                animator.runtimeAnimatorController = newSkinAnimator;

                // unlock text at player's position
                if (skinUnlockText != null)
                {
                    GameObject textObj = Instantiate(skinUnlockText, GameObject.Find("Canvas").transform); // make sure it's under the canvas
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3f, 0));
                    textObj.transform.position = screenPos;
                    Destroy(textObj, 2.5f);

                }
            }
        }

        if (other.gameObject.tag == "TransitionPoint")
        {

            //try to see if the camera will change, if so, stop player movement for some time
            if (other.gameObject.GetComponent<TransitionPoint>().WarpToLevel())
            {
                StartCoroutine(StopMovement(transitionStopTime));
            }

            lastCheckpointPosition = transform.position;
            PlayerPrefs.SetFloat("lastCheckpointPositionX", lastCheckpointPosition.x);
            PlayerPrefs.SetFloat("lastCheckpointPositionY", lastCheckpointPosition.y);
        }
    }

    void ChangeSkin()
    {
        animator.runtimeAnimatorController = newSkinAnimator;
        skinChanged = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Block otherScript = other.gameObject.GetComponent<Block>();
        // if the collided object is a "Block" object, it can't be manipulated anymore (got far)
        if (otherScript != null)
        {
            otherScript.nearPlayer = false;
            numberOfNearbyBlocks -= 1;
            nearbyBlocks.Remove(other.gameObject);

            if (numberOfNearbyBlocks <= 0)
            {
                eButtonDisplay.SetActive(false);
            }
        }
    }


    //while entering the manipulation phase, find the nearest object and enter that (if multiple are nearby)
    GameObject findNearestBlock()
    {
        if (numberOfNearbyBlocks <= 0)
        {
            return null;
        }
        else if (numberOfNearbyBlocks == 1)
        {
            return nearbyBlocks[0];
        }

        GameObject nearestObject = nearbyBlocks[0];
        float nearestBlockDistance = Vector2.Distance(nearestObject.transform.position, transform.position);

        for (int i = 1; i < numberOfNearbyBlocks; i++)
        {
            GameObject obj = nearbyBlocks[i];

            float distance = Vector2.Distance(obj.transform.position, transform.position);

            if (distance < nearestBlockDistance)
            {
                nearestObject = obj;
                nearestBlockDistance = distance;
            }
        }

        return nearestObject;
    }

    //this is when the player is about to manipulate some object(s)
    IEnumerator manipulationPhase(float seconds)
    {
        GameObject nearestBlock = findNearestBlock();

        if (nearestBlock == null)
        {
            yield return null;
        }
        else
        {
            canMove = false;
            nearestBlock.GetComponent<Block>().manipulate();
            transform.localScale = initialScale;
            animator.SetTrigger("DisappearTrigger");
            eButtonDisplay.SetActive(false);

            // wait for the animation to finish 
            yield return new WaitForSeconds(seconds);

            // Now destroy the player 
            GameObject.Destroy(gameObject);
        }


    }

    //stop player movement for some time
    IEnumerator StopMovement(float seconds)
    {
        if (ignoreTransitionStop)
        {
            yield return null;
            ignoreTransitionStop = false;
        }
        else
        {
            canMove = false;
            rb.velocity = new Vector2(0f, 0f);
            walkingSound.Stop();
            yield return new WaitForSeconds(seconds);
            canMove = true;
        }
    }
}
