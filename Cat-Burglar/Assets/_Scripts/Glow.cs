using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glow : MonoBehaviour
{

    [SerializeField] Material glowMat;
    [SerializeField] Material normalMat;
    [SerializeField] MeshRenderer rend;

    public bool toggle;


    // Update is called once per frame
    void Update()
    {
        if(toggle){

            rend.material = glowMat;
        }
        else{
            rend.material = normalMat;
        }
    }
}
