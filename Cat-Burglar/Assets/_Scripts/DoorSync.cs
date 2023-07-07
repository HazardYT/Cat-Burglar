using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSync : MonoBehaviour
{
    [SerializeField] private Transform otherDoor;
    [SerializeField] private Transform thisDoor;
    public bool shouldCopyThisDoor = true;
    public bool shouldCopyOtherDoor;

    public void Update(){
        if (shouldCopyThisDoor){
            otherDoor.localRotation = thisDoor.localRotation;
            shouldCopyThisDoor = false;
            shouldCopyOtherDoor = true;
        }
        else if (shouldCopyOtherDoor){
            thisDoor.localRotation = otherDoor.localRotation;
            shouldCopyThisDoor = true;
            shouldCopyOtherDoor = false;
        }


    }
}
