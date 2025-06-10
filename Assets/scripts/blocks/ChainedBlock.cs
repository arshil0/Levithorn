using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this block is chained to 2 or more other blocks, it tries to be in the center of the chained blocks
public class ChainedBlock : Block
{

    //speed of the block moving
    float speed = 250f;
    //manipulation power of the player while trying to move this block
    float manipulationSpeed = 650f;
    //list of blocks this block is attatched to (defined through the level editor from Unity)
    [SerializeField] Block[] blocksChainedTo;
    [SerializeField] LineRenderer connectionLines;
    [SerializeField] GameObject exitLight;
    [SerializeField] SpriteRenderer middleLightSprite;

    [SerializeField] Material connectionLineMaterial;

    void Start()
    {
        base.Start();
        middleLightSprite.color = new Color(1f, 1f, 1f, 1f);
        connectionLineMaterial.SetVector("_EmissionColor", new Color(1f, 1f, 1f));

        connectionLines.positionCount = blocksChainedTo.Length * 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (base.input() == "left")
        {
            exitLight.SetActive(false);
            middleLightSprite.color = new Color(1f, 1f, 1f, 1f);
            connectionLineMaterial.SetVector("_EmissionColor", new Color(1f, 1f, 1f));
        }

        setUpLinesWithEachBlock();

        Vector3 centerPosition = new Vector3(0f, 0f, 0f);
        foreach (Block b in blocksChainedTo)
        {
            centerPosition += b.transform.position;
        }
        centerPosition /= blocksChainedTo.Length;

        Vector3 moveDirection = centerPosition - transform.position;

        rb.velocity *= 0.95f;
        rb.angularVelocity *= 0.95f;

        if (base.beingControlled)
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            Vector2 playerForce = new Vector2(horizontal, vertical);
            playerForce.Normalize();
            playerForce *= manipulationSpeed;

            rb.AddForce(playerForce * Time.deltaTime * rb.mass);
            setupExitLight();
        }


        rb.AddForce(moveDirection * speed * Time.deltaTime * rb.mass);
        //transform.position = centerPosition;
    }

    private void setUpLinesWithEachBlock()
    {
        for (int i = 0; i < blocksChainedTo.Length * 2; i++)
        {
            if (i % 2 == 0)
            {
                connectionLines.SetPosition(i, transform.position);
            }
            else
                connectionLines.SetPosition(i, blocksChainedTo[(i - 1) / 2].transform.position);
        }

        //create an animation curve, to animate the connection lines smoothly, so it doesn't look static
        AnimationCurve curve = new AnimationCurve();
        //the start and end of the animation curve will be 0.1f
        curve.AddKey(0.0f, 0.1f);
        curve.AddKey(1.0f, 0.1f);
        //some random sin and cos function usage to have an animation
        curve.AddKey(0.5f + Mathf.Cos(Time.realtimeSinceStartup * 3) / 5f, 0.1f + Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup) / 6f));
        connectionLines.widthCurve = curve;
    }

    //rotates the player blue light displaying the exit direction, and the Q button display
    public void setupExitLight()
    {
        //probably not a good idea to call this every time, it's easier to develop :D
        exitLight.SetActive(true);
        middleLightSprite.color = new Color(0.04f, 0.9f, 1);
        connectionLineMaterial.SetVector("_EmissionColor", new Color(0.04f, 0.9f, 1f));

        //disable the hitbox while checking for walls (again, probably a bad solution)
        base.hitbox.SetActive(false);
        //shameless copy paste from the parent "input()" function
        // throw a sphere cast and see if there is a wall above (if the block is scaled up or down, take that into consideration as well, for all direction checks)
        exitLight.transform.eulerAngles = new Vector3(
            0,
            0,
            0
        );
        base.qButtonDisplay.transform.position = new Vector3(0f, 0.4f, 0f) * transform.localScale.y + transform.position;
        if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f + (0.5f * (transform.localScale.y - 1)), transform.up, 0.25f, groundLayer))
        {
            //the light and Q display will be on the left
            exitLight.transform.eulerAngles = new Vector3(
                0,
                0,
                90
            );
            base.qButtonDisplay.transform.position = new Vector3(-1.075f, -0.675f, 0f) * transform.localScale.x + transform.position;
            // throw a sphere cast and see if there is a wall on the left (nested inside of the if, as I want to check only if above is blocked)
            if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f + (0.5f * (transform.localScale.x - 1)), -transform.right, 0.25f, groundLayer))
            {
                //the light will be on the right
                exitLight.transform.eulerAngles = new Vector3(
                    0,
                    0,
                    270
                );
                base.qButtonDisplay.transform.position = new Vector3(1.075f, -0.675f, 0f) * transform.localScale.x + transform.position;
                // throw a sphere cast and see if there is a wall on the right (again, nested, this is the 3rd check)
                if (Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y), 0.45f + (0.5f * (transform.localScale.x - 1)), transform.right, 0.25f, groundLayer))
                {
                    //otherwise the light will glow from below
                    exitLight.transform.eulerAngles = new Vector3(
                        0,
                        0,
                        180
                    );
                    base.qButtonDisplay.transform.position = new Vector3(0f, -1.75f, 0f) * transform.localScale.y + transform.position;
                }
            }

        }
        base.hitbox.SetActive(true);
    }
}
