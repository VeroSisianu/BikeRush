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
    private bool IsOnBooster = false;
    private WheelCollider[] wheelCollliders;
    private bool canRotate;
    private Vector3 BoosterDirection; 

    List<Coroutine> routines = new List<Coroutine>();
    List <Collider> colsList = new List<Collider>();

    private bool hasBikeOnLeft;
    private bool hasBikeOnRight;

    private int wheelsOnTheGround = 2;
    private bool isGrounded = true;
    public float zVelocityModifier = 2f;
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
        rb.velocity = new Vector3(velocity.x, velocity.y, velocity.z);
    }
    private void Update()
    {
        CheckIfTheWheelsHitTheGround();
        if (colsList.Count > 0)
        {
            foreach (Collider col in colsList)
            {
                if (Vector3.Distance(col.transform.position, transform.position) < 3 && routine == null)
                    CalculateRoute();
            }
        }
    }

    private void CheckIfTheWheelsHitTheGround()
    {
        wheelsOnTheGround = 0;
        WheelHit hit;
        foreach (var wheel in wheelCollliders)
        {
            if (wheel.GetGroundHit(out hit))
            {
                velocity.z = originalZVelocity;
                isGrounded = true;



                if (hit.collider.CompareTag("Ground"))
                {
                    wheelsOnTheGround++;
                    if (wheelsOnTheGround == 2 && rb.freezeRotation != true)
                    {
                        rb.freezeRotation = true;
                    }
                }
            }
        }
        foreach (var wheel in wheelCollliders)
        {
            if (wheel.GetGroundHit(out hit))
            {
                velocity.z = originalZVelocity;
                isGrounded = true;
                if (hit.collider.CompareTag("Booster") || hit.collider.CompareTag("BoosterHill"))
                {
                    if (hit.collider.CompareTag("BoosterHill") && !IsOnBooster && rb.constraints != unFreezed)
                    {
                        rb.constraints = unFreezed;
                        Debug.Log("unfreezing1");
                    }
                    wheelsOnTheGround++;
                    if (wheelsOnTheGround == 2)
                    {
                        IsOnBooster = true;
                    }
                }
            }
        }
       // RotateIfInTheAir();
    }

    private void RotateIfInTheAir()
    {
        if (wheelsOnTheGround == 0 && !IsOnBooster)
        {
            transform.Rotate(Vector3.right, 25);
            isGrounded = false;
        }
        else
        {
            if ((transform.eulerAngles.x < 15 && transform.eulerAngles.x > -15) && !isGrounded && transform.position.y<1.5f)
            {

                Debug.Log("ground is near, FREEEZE");
                rb.angularVelocity -= rb.angularVelocity/2;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.CompareTag("Obstacle") && other.isTrigger) || (other.CompareTag("Player") && other.isTrigger))
        {
            StopAllCoroutines();
            colsList.Add(other);
        }
        else if(other.CompareTag("BoosterZone"))
        {
            if(colsList.Count == 0)
            {
                BoosterDirection = new Vector3(other.transform.GetChild(0).position.x, transform.position.y, other.transform.GetChild(0).position.z - 3);
                velocity.x = (BoosterDirection - transform.position).normalized.x * VelocityGrowthModifier;
            }
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BoosterZone"))
        {
            if (colsList.Count == 0)
            {
                velocity.x = (BoosterDirection - transform.position).normalized.x * VelocityGrowthModifier;
            }
        }
        else if ((other.CompareTag("Booster") || other.CompareTag("BoosterHill")) && other.isTrigger)
        {
            if (colsList.Count == 0 && transform.position.x <= other.transform.position.x + 0.001f && transform.position.x >= other.transform.position.x - 0.001f)
            {
                velocity.x = 0;
            }
            if (IsOnBooster == true)
            {
                if(rb.freezeRotation == false)
                    rb.freezeRotation = true;
                velocity.z += zVelocityModifier;
            }
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
            }
        }
        else if(other.CompareTag("Booster") || other.CompareTag("BoosterHill"))
        {
            velocity.z += zVelocityModifier;
            IsOnBooster = false;
        }
    }

    private void CalculateRoute()
    {
        if(colsList.Count == 1)
        {
            StopAllCoroutines();
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
