using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthogonalBlock : Block
{
    float acceleration = 1500f;
    float velocity = 0f;
    float maxSpeed = 2000f;

    //this is the maximum magnitude of the rigidbody velocity that this block can move
    float maxRBSpeed = 12;

    public Vector2 direction = Vector2.down;

    SpriteRenderer sprite;

    [SerializeField] GameObject directionLight;
    SpriteRenderer directionLightSprite;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        directionLightSprite = directionLight.GetComponent<SpriteRenderer>();

        setLightDirection(direction);
    }

    // Update is called once per frame
    void Update()
    {

        //call the parent input function (E to enter and Q to leave)
        input();
        //player is currently controlling this block
        if (beingControlled && canMove)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                velocity = 0.5f;
                direction = Vector2.right;
                setLightDirection(direction);
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                velocity = 0.5f;
                direction = Vector2.up;
                setLightDirection(direction);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                velocity = 0.5f;
                direction = Vector2.left;
                setLightDirection(direction);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                velocity = 0.5f;
                direction = Vector2.down;
                setLightDirection(direction);
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
                canMove = true;

                //I don't know honestly, just slapped some numbers until it worked, so the color doesn't reset to blue as soon as gravity is changed.
                if (velocity > maxSpeed / 5f)
                {
                    directionLightSprite.color = Color.blue;
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
            print(rb.velocity);
        }

        //hit a ground
        else
        {
            //I may remove this, it's unnecessary for now
            canMove = true;
        }
    }


    //rotate the direction light depending on which way the block is moving
    //ALSO, adjust the color of the light to be red with alpha 0
    private void setLightDirection(Vector2 dir)
    {
        directionLightSprite.color = new Color(1f, 0f, 0f, 0f);
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
}
