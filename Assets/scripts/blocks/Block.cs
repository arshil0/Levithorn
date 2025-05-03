using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class is an abstract class for all gravitational blocks and has whatever is shared between each block type
public class Block : MonoBehaviour
{
    // if the player is currently controlling the block, this variable will be true
    public bool beingControlled = false;

    public Rigidbody2D rb;

    //this will be true if the player is nearby
    public bool nearPlayer = false;

    //if canMove is true, you can change gravity, it will turn false after changing the gravity, until hitting a ground
    public bool canMove = true;

    [SerializeField] GameObject playerPrefab;

    // track where the player should be when they leave the block
    private Vector2 exitPosition;

    AudioSource manipulationSound;

    public GameObject qButtonDisplay;

    // start is called before the first frame update
    public void Start()
    {
        beingControlled = false;

        manipulationSound = transform.Find("manipulationSound").GetComponent<AudioSource>();
        qButtonDisplay = transform.Find("Canvas/QButtonDisplay").gameObject;
        qButtonDisplay.SetActive(false);
    }

    public void input()
    {
        // if player is controlling this block and Q is pressed, leave
        if (Input.GetKeyDown(KeyCode.Q) && beingControlled && canMove)
        {
            // store the position where the player should exit
            exitPosition = transform.position + Vector3.up * 0.5f; // You can adjust the Y offset here based on your game design

            // instantiate the player at the exit position
            var player = Instantiate(playerPrefab, exitPosition, Quaternion.identity);

            // set player to the correct position (if needed, adjust this based on the block's size)
            player.transform.position = exitPosition;

            beingControlled = false;
            qButtonDisplay.SetActive(false);
        }

        // player is nearby and E is pressed, now the block is being controlled
        if (Input.GetKeyDown(KeyCode.E) && nearPlayer)
        {
            beingControlled = true;

            manipulationSound.pitch = Random.Range(0.9f, 1.55f);
            manipulationSound.Play();
            qButtonDisplay.SetActive(true);
        }


    }



}
