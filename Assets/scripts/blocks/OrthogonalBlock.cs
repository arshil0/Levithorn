using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class OrthogonalBlock : Block
{
    float acceleration = 1500f;
    float velocity = 0f;
    float maxSpeed = 2000f;

    //this is the maximum magnitude of the rigidbody velocity that this block can move
    float maxRBSpeed = 12;

    bool moving = false;

    public Vector2 direction = Vector2.down;

    SpriteRenderer sprite;

    [SerializeField] GameObject directionLight;
    [SerializeField] GameObject exitLight;

    SpriteRenderer directionLightSprite;


    [SerializeField] AudioSource gravityChangeSound;
    [SerializeField] AudioSource impactSound;
    [SerializeField] AudioSource windSound;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        directionLightSprite = directionLight.GetComponent<SpriteRenderer>();

        setLightDirection(direction);
    }

    // Update is called once per frame
    void Update()
    {

        //call the parent input function (E to enter and Q to leave), ONLY IF the block is not moving
        if (!moving)
            //if the player left the block, hide the player light.
            if (input() == "left")
            {
                exitLight.SetActive(false);
            }

        //player is currently controlling this block
        if (beingControlled && canMove)
        {
            bool moved = false;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                velocity = 0.5f;
                direction = Vector2.right;
                moved = true;
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                velocity = 0.5f;
                direction = Vector2.up;
                moved = true;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                velocity = 0.5f;
                direction = Vector2.left;
                moved = true;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                velocity = 0.5f;
                direction = Vector2.down;
                moved = true;
            }


            if (moved)
            {
                setLightDirection(direction);
                exitLight.SetActive(false);
                moving = true;

                gravityChangeSound.pitch = Random.Range(0.4f, 1.2f);
                gravityChangeSound.Play();

                windSound.pitch = Random.Range(0.75f, 1.15f);
                windSound.Play();

                //hide the Q button indicator to leave the block
                base.qButtonDisplay.SetActive(false);
                canMove = false;
            }

        }


        if (direction != Vector2.zero)
        {
            if (velocity < maxSpeed)
            {
                //update the velocity over time
                velocity = Mathf.Min(maxSpeed, velocity + acceleration * Time.deltaTime);

                //adjust the light color so it fades in towards the movement direction
                Color c = directionLightSprite.color;

                //check if the alpha value of the light color is less than 1, then increase it
                if (c.a < 1f)
                {
                    c.a = velocity / maxSpeed;
                    directionLightSprite.color = c;
                }

            }

            //if the block hasn't reached its max moving speed, increase it
            if (rb.velocity.magnitude < maxRBSpeed)
            {
                rb.AddForce(direction * velocity * Time.deltaTime * rb.mass);
            }

            //if the block is not moving, or moving very slightly, you are allowed to change its gravity
            if (rb.velocity.magnitude < 0.5f)
            {
                //I don't know honestly, just slapped some numbers until it worked, so the color doesn't reset to blue as soon as gravity is changed.
                if (velocity > maxSpeed / 5f)
                {
                    if (beingControlled)
                    {
                        directionLightSprite.color = new Color(0.04f, 0.9f, 1);
                        exitLight.SetActive(true);
                    }

                    else
                    {
                        directionLightSprite.color = Color.white;
                        exitLight.SetActive(false);
                    }


                    //this is when the ground was hit (so it's called once)
                    if (moving)
                    {
                        float impactStrength = velocity / maxSpeed - 0.25f;
                        impactSound.volume = Mathf.Max(0, impactStrength / 2.5f);
                        windSound.Stop();
                        impactSound.Play();
                        moving = false;
                        canMove = true;

                        //reveal the Q button display to leave the block
                        base.qButtonDisplay.SetActive(true);


                        //A pretty long line of code but here we go:
                        //first we get the main camera
                        //then we get the "Cinemachine brain"
                        //from that we get the active virtual camera
                        //finally, we have to transfer that to a "CinemachineVirtualCameraBase" object, as the original "ICinemaChine" object doesn't have a "transform"
                        var cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCameraBase;

                        //start a screen shake
                        StartCoroutine(screenShake(cam, impactStrength / 6));

                        setupExitLight();
                    }
                }


            }
            //if the block is moving around, you can't change its gravity
            else
            {
                canMove = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //whatever collided, is not a gound object (3 is the ground layer)
        if (other.gameObject.layer != 3)
        {

        }

        //hit a ground
        else
        {
            canMove = true;
        }
    }


    //rotate the direction light depending on which way the block is moving
    //ALSO, adjust the color of the light to be red with alpha 0
    private void setLightDirection(Vector2 dir)
    {
        directionLightSprite.color = new Color(0.8f, 0f, 0f, 0f);
        if (dir == Vector2.down)
        {
            directionLight.transform.eulerAngles = new Vector3(
                0,
                0,
                180
            );
        }

        else if (dir == Vector2.up)
        {
            directionLight.transform.eulerAngles = new Vector3(
                0,
                0,
                0
            );
        }

        else if (dir == Vector2.left)
        {
            directionLight.transform.eulerAngles = new Vector3(
                0,
                0,
                90
            );
        }

        //right
        else
        {
            directionLight.transform.eulerAngles = new Vector3(
                0,
                0,
                270
            );
        }
    }

    //rotates the player blue light displaying the exit direction, and the Q button display
    public void setupExitLight()
    {
        //display the player light, in the direction that they will come out after pressing Q
        exitLight.SetActive(true);
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

    //honestly found this from a youtube video, not exactly sure why a regular function didn't work but an IEnumerator works
    IEnumerator screenShake(CinemachineVirtualCameraBase cam, float effectSeconds)
    {
        Vector3 originalCamPosition = cam.transform.position;

        //start from deltatime instead of 0, to not shake the screen for very short distances
        float timer = Time.deltaTime;

        while (timer < effectSeconds)
        {
            timer += Time.deltaTime;
            float shakeStrength = timer / effectSeconds;
            cam.transform.position = new Vector3(
                    originalCamPosition.x + Random.Range(-(1 - shakeStrength) / 8, (1 - shakeStrength) / 8),
                    originalCamPosition.y + Random.Range(-(1 - shakeStrength) / 8, (1 - shakeStrength) / 8),
                    originalCamPosition.z);
            yield return null;
        }

        cam.transform.position = originalCamPosition;
    }
}
