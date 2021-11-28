using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float velocityChanger;
    public float VelocityDelta = 0.2f;
    public float MaxVelocity = 4f;
    public float GrowVelocityX = 1000f;
    Rigidbody rb;
    Animator anim;

    private Vector3 startPos;
    private Vector3 currentPos;
    private Vector2 bounds = new Vector2(32.5f, 38);


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        var clampedX = Mathf.Clamp(transform.position.x, bounds.x, bounds.y);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    private void FixedUpdate()
    {
        ReceiveInput();
    }

    private void ReceiveInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = ConvertPos(Input.mousePosition);
            anim.speed = 2.5f;
        }

        if (Input.GetMouseButton(0))
        {
            if (velocityChanger < MaxVelocity)
            {
                velocityChanger += VelocityDelta;
            }
            currentPos = ConvertPos(Input.mousePosition);
            var deltaPos = currentPos - startPos;

            SetVelocityDependingOnInput(deltaPos);
        }
        else
        {
            if (velocityChanger > 0)
            {
                velocityChanger -= VelocityDelta;
                rb.velocity = new Vector3(0, rb.velocity.y, velocityChanger);
            }
            else if (!rb.IsSleeping())
            {
                anim.speed = 0;
                rb.Sleep();
            }
        }

        if (Input.GetMouseButton(0))
        {
            startPos = ConvertPos(Input.mousePosition);
        }
    }

    private void SetVelocityDependingOnInput(Vector3 deltaPos)
    {
        if (deltaPos.x > 0 && transform.position.x < bounds.y)
        {
            rb.velocity = new Vector3(deltaPos.x * GrowVelocityX, rb.velocity.y, velocityChanger);
        }
        else if (deltaPos.x < 0 && transform.position.x > bounds.x)
        {
            rb.velocity = new Vector3(deltaPos.x * GrowVelocityX, rb.velocity.y, velocityChanger);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, velocityChanger);
        }
    }

    public Vector3 ConvertPos(Vector3 position)
    {
        return new Vector3(position.x / Screen.width, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Booster"))
        {
            rb.AddForce(transform.forward * 2, ForceMode.VelocityChange);
        }
    }
    private void OnTriggerStay(Collider other)
    {
       if (other.CompareTag("Booster"))
       {
            rb.AddForce(transform.forward * 2, ForceMode.VelocityChange);
       }
    }
}
