using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerController : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 velocity;
    private Vector3 direction;
    public float AnimatorSpeed;
    public float VelocityGrowthModifier;
    Animator anim;
    List <Coroutine> routines = new List<Coroutine>();
    Coroutine routine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        anim.speed = AnimatorSpeed;
    }

    void FixedUpdate()
    {
        rb.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Obstacle") || other.CompareTag("Player"))
        {
            direction = other.transform.position - transform.position;
            routine = StartCoroutine(MoveAway(other));
            routines.Add(routine);
        }
    }
    private IEnumerator MoveAway(Collider other)
    {
        if((other.transform.position - transform.position).magnitude > 3f)
        {
            velocity.x = 0;
            StopCoroutine(MoveAway(other));
            Debug.Log("stop");
        }
        else if(direction.x > 0 && transform.position.x > 33)
        {
            velocity.x = -direction.x * VelocityGrowthModifier;
            Debug.Log("clamped 1 X");
        }
        else if(direction.x < 0 && transform.position.x < 38)
        {
            velocity.x = -direction.x * VelocityGrowthModifier;
            Debug.Log("clamped 2 X");
        }
        else
        {
            velocity.x = 0;
        }
        yield return new WaitForFixedUpdate();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Player"))
        {
            StopAllCoroutines();
            velocity.x = 0;
        }
    }
}
