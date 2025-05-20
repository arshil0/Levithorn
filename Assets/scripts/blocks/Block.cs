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

    AudioSource manipulationSound;

    public GameObject qButtonDisplay;

    public LayerMask groundLayer;
    [SerializeField] public GameObject hitbox;

    // start is called before the first frame update
    public void Start()
    {
        beingControlled = false;

        manipulationSound = transform.Find("manipulationSound").GetComponent<AudioSource>();
        qButtonDisplay = transform.Find("Canvas/QButtonDisplay").gameObject;
        qButtonDisplay.SetActive(false);

        groundLayer = LayerMask.GetMask("Ground");
    }

    public string input()
    {
        // if player is controlling this block and Q is pressed, leave
        if (Input.GetKeyDown(KeyCode.Q) && beingControlled && canMove)
        {
            // track where the player should be when they leave the block (default is above, then left, then right and finally down)
            Vector2 exitPosition = transform.position + Vector3.up * 0.5f;

            //probably an awful solution, but disable the hitbox while checking for nearby walls, to not collide with its own hitbox
            hitbox.SetActive(false);

            // throw a sphere cast and see if there is a wall above (if the block is scaled up or down, take that into consideration as well, for all direction checks)
            if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f + (0.5f * (transform.localScale.y - 1)), transform.up, 0.25f, groundLayer))
            {
                exitPosition = transform.position - Vector3.right * 0.5f;
                // throw a sphere cast and see if there is a wall on the left (nested inside of the if, as I want to check only if above is blocked)
                if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f + (0.5f * (transform.localScale.x - 1)), -transform.right, 0.25f, groundLayer))
                {
                    exitPosition = transform.position + Vector3.right * 0.5f;
                    // throw a sphere cast and see if there is a wall on the right (again, nested, this is the 3rd check)
                    if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f + (0.5f * (transform.localScale.x - 1)), transform.right, 0.25f, groundLayer))
                    {

                        //otherwise below should be open (if not, how in the world did the player end up in this situation??)
                        exitPosition = transform.position - Vector3.up * 0.5f;
                    }
                }

            }

            hitbox.SetActive(true);

            // instantiate the player at the exit position
            var player = Instantiate(playerPrefab, exitPosition, Quaternion.identity);

            // set player to the correct position (if needed, adjust this based on the block's size)
            player.transform.position = exitPosition;

            beingControlled = false;
            qButtonDisplay.SetActive(false);
            return "left";
        }

        return "";


    }

    //this is when the player is about to manipulate this object, it's called inside of the player script
    public void manipulate()
    {
        beingControlled = true;

        manipulationSound.pitch = Random.Range(0.9f, 1.55f);
        manipulationSound.Play();
        qButtonDisplay.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "TransitionPoint")
        {

            //try to transition to the other level
            other.gameObject.GetComponent<TransitionPoint>().WarpToLevel();

            //update player checkpoint position
            Player.lastCheckpointPosition = transform.position;
        }
    }


}
