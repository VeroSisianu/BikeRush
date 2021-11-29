using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    int index;
    public Rigidbody rb;
    public Vector3 velocity = new Vector3();
    public Vector3 direction = new Vector3();
    public float AnimatorSpeed;
    public float VelocityGrowthModifier;
    private float originalZVelocity;
    Animator anim;
    Coroutine routine;
    public RigidbodyConstraints unFreezed = new RigidbodyConstraints();
    [HideInInspector]
    public int countWheels = 0;
    private WheelCollider[] wheelCollliders;
    private bool isGroundedWithBothWheels;

    List<Coroutine> routines = new List<Coroutine>();
    List <Collider> colsList = new List<Collider>();

    private bool hasBikeOnLeft;
    private bool hasBikeOnRight;


    void Awake()
    {
        wheelCollliders = GetComponentsInChildren<WheelCollider>();
        originalZVelocity = velocity.z;
        rb = GetComponent<Rigidbody>();
        unFreezed = rb.constraints;
        anim = GetComponentInChildren<Animator>();
        anim.speed = AnimatorSpeed;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }
    private void Update()
    {
        CheckIfTheWheelsTouchTheGround();

        if (colsList.Count > 0)
        {
            foreach (Collider col in colsList)
            {
                if (Vector3.Distance(col.transform.position, transform.position) < 3 && routine == null)
                    CalculateRoute();
            }
        }
    }

    private void CheckIfTheWheelsTouchTheGround()
    {
        var countWheels = 0;

        foreach (var wheel in wheelCollliders)
        {
            if (wheel.GetGroundHit(out WheelHit hit))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    countWheels++;
                    if (countWheels == 2 && rb.freezeRotation!=true)
                    {
                        rb.freezeRotation = true;
                        Debug.Log("FREEZE1");
                    }
                }
                else
                {
                    if(rb.freezeRotation == true)
                        rb.constraints = unFreezed;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.CompareTag("Obstacle") && other.isTrigger) || (other.CompareTag("Player") && other.isTrigger))
        {
            StopAllCoroutines();
            colsList.Add(other);
            //CalculateRoute();
        }
        else if(other.CompareTag("BoosterZone"))
        {
            if(colsList.Count == 0)
            {
                var BoosterDirection = new Vector3(other.transform.position.x, other.transform.position.y, other.transform.position.z - 9);
                velocity.x = (BoosterDirection - transform.position).normalized.x * VelocityGrowthModifier;
            }
        }
        else if(other.CompareTag("Booster") && other.isTrigger)
        {
            rb.constraints = unFreezed;
            Debug.Log("UNFREEZE");
            //velocity.x = 0;
            rb.AddForce(Vector3.forward * 2, ForceMode.VelocityChange);
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BoosterZone"))
        {
            if (colsList.Count == 0)
            {
                var BoosterDirection = new Vector3(other.transform.position.x, other.transform.position.y, other.transform.position.z - 9);
                velocity.x = (BoosterDirection - transform.position).normalized.x * VelocityGrowthModifier;
            }
        }
        else if (other.CompareTag("Booster") && other.isTrigger)
        {
            if (colsList.Count == 0 && transform.position.x <= other.transform.position.x + 0.001f && transform.position.x >= other.transform.position.x - 0.001f)
            {
                velocity.x = 0;
            }
            rb.AddForce(Vector3.forward * 2, ForceMode.VelocityChange);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Player"))
        {
            if (colsList.Contains(other))
            {
                StopAllCoroutines();
                colsList.Remove(other);
                routine = null;
                velocity.x = 0;
                if (colsList.Count == 0)
                    velocity.z = originalZVelocity;
                //if(colsList.Count > 0)
                //    CalculateRoute();
            }
        }
    }

    private void CalculateRoute()
    {
        if(colsList.Count == 1)
        {
            StopAllCoroutines();
            Debug.Log("route1");
            routine = StartCoroutine(MoveAway(colsList[0]));
        }
        else
        {
            hasBikeOnLeft = false;
            hasBikeOnRight = false;
            float maxDistanceBetweenBikes = 0;
            index = -1;
            StopAllCoroutines();
            maxDistanceBetweenBikes = CheckIfCanGoBetweenBikes(maxDistanceBetweenBikes);
            if (maxDistanceBetweenBikes > 2f)
            {
                direction = (colsList[index].transform.position + colsList[index + 1].transform.position - transform.position) / 2;
                routine = StartCoroutine(MoveAway(colsList));
            }
            else
            {
                CalculateRouteOnTheLeftOrRight();
            }
        }
    }

    private void CalculateRouteOnTheLeftOrRight()
    {
        var listOfXPositions = new List<float>();

        CheckIfHasBikeOnRightOrLeft(listOfXPositions);

        listOfXPositions.Sort();

        if (listOfXPositions[0] >= 34 && !hasBikeOnLeft)
        {
            direction.x = listOfXPositions[0] - 1f;// direction is left
            routine = StartCoroutine(MoveAway(colsList));
        }
        else if (listOfXPositions[colsList.Count - 1] <= 37 && !hasBikeOnRight)
        {
            direction.x = listOfXPositions[colsList.Count - 1] + 1f;//direction is right
            routine = StartCoroutine(MoveAway(colsList));
        }
        else
        {
            if (velocity.z >= 0.1f)
                velocity.z -= 0.01f;
            direction = Vector3.zero;
        }
    }

    private float CheckIfCanGoBetweenBikes(float maxDistance)
    {
        for (int i = 0; i < colsList.Count - 1; i++)
        {
            for (int j = i + 1; j < colsList.Count; j++)
            {
                if (Vector3.Distance(colsList[j].transform.position, colsList[i].transform.position) > maxDistance)
                {
                    maxDistance = Vector3.Distance(colsList[j].transform.position, colsList[i].transform.position);
                    index = i;
                }
                Debug.Log("route2");
            }
        }
        return maxDistance;
    }
    private void CheckIfHasBikeOnRightOrLeft(List<float> listOfXPos)
    {
        foreach (var i in colsList)
        {
            listOfXPos.Add(i.transform.position.x);
            if (i.transform.position.z < transform.position.z + 3 && i.transform.position.z > transform.position.z - 3)
            {
                if (i.transform.position.x < transform.position.x)
                    hasBikeOnLeft = true;
                else
                {
                    hasBikeOnRight = true;
                }
            }
        }
    }

    private void CheckIfOtherColliderIsBehindOrInFront()
    {
        if (colsList[0].transform.position.x != transform.position.x)
        {
            direction = (colsList[0].transform.position - transform.position).normalized;
            //he is not on the same x position
        }
        else if(colsList[0].transform.position.x == transform.position.x)
        {
            if (colsList[0].transform.position.z > transform.position.z)
            {
                if (velocity.z >= 0.1f)
                    velocity.z -= 0.01f;
                direction = Vector3.zero;
            }
            else if (colsList[0].transform.position.z < transform.position.z)
            {
                velocity.z += 0.01f;
                direction = Vector3.zero;
            }
            else
            {
                if (velocity.z >= 0.1f)
                    velocity.z += Random.Range(-0.3f, 0.3f);
                direction.x = 0;
            }
        }
    }

    private IEnumerator MoveAway(Collider other)
    {
        while (colsList.Count > 0)
        {
            CheckIfOtherColliderIsBehindOrInFront();
            if (direction.x < 0 && transform.position.x < 38)
            {
                velocity.x = -direction.x * VelocityGrowthModifier;
            }
            else if (direction.x > 0 && transform.position.x > 33)
            {
                velocity.x = -direction.x * VelocityGrowthModifier;
            }
            else
            {
                if (other.transform.position.z > transform.position.z)
                    if (velocity.z >= 0.1f)
                        velocity.z -= 0.2f;
                else
                    velocity.z += 0.2f;
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
                velocity.x = -direction.x * VelocityGrowthModifier;
            }
            else if (direction.x < 0 && transform.position.x > 33)
            {
                velocity.x = -direction.x * VelocityGrowthModifier;
            }
            else
            {
                velocity.x = 0;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
}
