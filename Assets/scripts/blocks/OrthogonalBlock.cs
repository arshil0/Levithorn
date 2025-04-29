using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthogonalBlock : Block
{
    public float acceleration = 500f;
    public float velocity = 0f;
    public float maxSpeed = 500f;

    public Vector2 direction = Vector2.down;

    SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        input();
        //player is currently controlling this block
        if (beingControlled && canMove)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                velocity = 0.5f;
                direction = Vector2.right;
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                velocity = 0.5f;
                direction = Vector2.up;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                velocity = 0.5f;
                direction = Vector2.left;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                velocity = 0.5f;
                direction = Vector2.down;
            }
        }


        if (direction != Vector2.zero)
        {
            rb.AddForce(direction * velocity * Time.deltaTime * rb.mass);
            velocity = Mathf.Min(maxSpeed, velocity + acceleration);


            //if the block is not moving, or moving very slightly, you are allowed to change its gravity
            if (rb.velocity.magnitude < 0.5f)
            {
                canMove = true;
                sprite.color = Color.black;
            }
            //if the block is moving around, you can't change its gravity
            else
            {
                canMove = false;
                sprite.color = Color.red;
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
            sprite.color = Color.black;
        }
    }
}
