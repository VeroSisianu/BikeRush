using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerFinish : MonoBehaviour
{
    private int countPlace = 1;
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") && !other.isTrigger)
        {
            var hasAI = other.TryGetComponent(out AIController controller);
            if(hasAI)
            {
                controller.rb.velocity -= controller.rb.velocity* 0.8f;
                controller.Place = countPlace;
                Debug.Log(controller.Place);
                countPlace++;
                controller.anim.StopPlayback();
                controller.enabled = false;
            }
            else
            {
                var isMainPlayer = other.TryGetComponent(out PlayerController playerController);
                if (isMainPlayer)
                {
                    playerController.Finished = true;
                    playerController.Place = countPlace;
                    Debug.Log(playerController.Place);
                    countPlace++;
                    playerController.anim.StopPlayback();
                }
            }
            
        }
    }
}
