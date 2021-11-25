using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerController : MonoBehaviour
{
    int index;
    Rigidbody rb;
    public Vector3 velocity = new Vector3();
    public Vector3 direction = new Vector3();
    public float AnimatorSpeed;
    public float VelocityGrowthModifier;
    Animator anim;
    Coroutine routine;

    List<Coroutine> routines = new List<Coroutine>();
    List <Collider> colsList = new List<Collider>();

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
        if((other.CompareTag("Obstacle") && other.isTrigger) || (other.CompareTag("Player") && other.isTrigger))
        {
            StopAllCoroutines();
            Debug.Log("entered trigger");
            colsList.Add(other);
            CalculateRoute();
        }
    }

    private void CalculateRoute()
    {
        if(colsList.Count == 1)
        {
            StopAllCoroutines();
            Debug.Log("route1");
            CheckIfOtherColliderIsBehindOrInFront();
            routine = StartCoroutine(MoveAway(colsList[0]));
        }
        else
        {
            float maxDistance = 0;
            index = -1;
            StopAllCoroutines();
            for (int i = 0; i < colsList.Count - 1; i++)
            {
                for (int j = i + 1; j < colsList.Count; j++)
                {
                    if (Vector3.Distance(colsList[j].transform.position, colsList[i].transform.position) > maxDistance)
                    {
                        index = i;
                    }
                    Debug.Log("route2");
                }
            }
            direction = ((colsList[index].transform.position + colsList[index+1].transform.position)-transform.position)/2;
            routine = StartCoroutine(MoveAway(colsList));
        }
    }

    private void CheckIfOtherColliderIsBehindOrInFront()
    {
        //if (colsList[0].transform.position.z > transform.position.z)
        {
            direction = (colsList[0].transform.position - transform.position).normalized;
            //Debug.Log("other is in front");
        }
        //else
        //{
        //    direction = (transform.position - colsList[0].transform.position);
        //    //direction.x = -direction.x;
        //    Debug.Log("other is behind");
        //}
    }

    private IEnumerator MoveAway(Collider other)
    {
        while (colsList.Count > 0)
        {
            if (direction.x < 0 && transform.position.x < 38)
            {
                velocity.x = direction.x * VelocityGrowthModifier;
            }
            else if (direction.x > 0 && transform.position.x > 33)
            {
                velocity.x = direction.x * VelocityGrowthModifier;
            }
            else
            {
                velocity.x = 0;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator MoveAway(List<Collider> colsList)
    {
        while(colsList.Count>0)
        {
            if (direction.x > 0 && transform.position.x < 38)
            {
                Debug.Log("to right");
                velocity.x = -direction.x * VelocityGrowthModifier;
            }
            else if (direction.x < 0 && transform.position.x > 33)
            {
                Debug.Log("to left");
                velocity.x = -direction.x * VelocityGrowthModifier;
            }
            else
            {
                velocity.x = 0;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Player"))
        {
            if(colsList.Contains(other))
            {
                StopAllCoroutines();
                colsList.Remove(other);
                routine = null;
                velocity.x = 0;
                Debug.Log("exited trigger");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    { 
        if (Vector3.Distance(other.transform.position, transform.position) < 2f && routine == null)
        {
            CalculateRoute();
            Debug.Log("calculating on triggerstay route");
        }
    }
}
