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
            input();

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
                        directionLightSprite.color = new Color(0.04f, 0.9f, 1);
                    else
                        directionLightSprite.color = Color.white;

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
