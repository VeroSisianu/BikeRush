using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelCollision : MonoBehaviour
{
    public AIController parentAIController;

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log(parentAIController.countWheels);

    //    if (other.CompareTag("Ground"))
    //    {
    //        parentAIController.countWheels++;
    //        if (parentAIController.countWheels == 2)
    //        {
    //            parentAIController.rb.constraints = RigidbodyConstraints.FreezeRotationX;
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Ground"))
    //    {
    //        parentAIController.countWheels--;
    //        if (parentAIController.countWheels == 0)
    //        {
    //            parentAIController.rb.constraints = parentAIController.unFreezed;
    //        }
    //    }
    //}
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(parentAIController.countWheels);

        if (collision.gameObject.CompareTag("Ground"))
        {
            parentAIController.countWheels++;
            if (parentAIController.countWheels == 2)
            {
                parentAIController.rb.constraints = RigidbodyConstraints.FreezeRotationX;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            parentAIController.countWheels--;
            if (parentAIController.countWheels == 0)
            {
                parentAIController.rb.constraints = parentAIController.unFreezed;
            }
        }
    }
}
