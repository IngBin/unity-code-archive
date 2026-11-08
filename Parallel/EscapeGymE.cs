using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeGymE : MonoBehaviour
{
    public Animator NOpenTheDoor,HOpenTheDoor; // 문 열리는 애니메이션

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // NOpenTheDoor.Play("NGymWoodDoorOpen");
            // HOpenTheDoor.Play("HGymWoodDoorOpen");
            ManagerCS.MInstance.Doors[6].GetComponent<DoorData>()._openCheck = true;
            ManagerCS.MInstance.Doors[7].GetComponent<DoorData>()._openCheck = true;
            HorrorEvent.isGymEvent = false;
            this.GetComponent<BoxCollider>().enabled = false;
            MannequinEnemy.isTrace = false;
            HorrorEvent.isGymClear = true;
        }
    }
}
