using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is an abstract class for all gravitational blocks and has whatever is shared between each block type
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
    // Start is called before the first frame update
    void Start()
    {
        beingControlled = false;
    }

    public void input()
    {
        //if player is controlling this block and Q is pressed, leave:
        if (Input.GetKeyDown(KeyCode.Q) && beingControlled && canMove)
        {
            var player = Instantiate(playerPrefab, transform);
            player.transform.position += Vector3.up / 3;
            beingControlled = false;
        }


        //player is nearby and E is pressed, now the block is being controlled
        if (Input.GetKeyDown(KeyCode.E) && nearPlayer)
        {
            beingControlled = true;
        }
    }
}
