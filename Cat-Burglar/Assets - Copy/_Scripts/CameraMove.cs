using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class CameraMove : MonoBehaviour
{
 
    [SerializeField] private float YMin = -25.0f;
    [SerializeField] private float YMax = 90.0f;
 
    public Transform lookAt;
 
    public float distance = 10.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    public float sensivity = 4.0f;
 
 
    // Update is called once per frame
    void LateUpdate()
    {
 
        currentX += Input.GetAxis("Mouse X") * sensivity * Time.deltaTime;
        currentY += Input.GetAxis("Mouse Y") * sensivity * Time.deltaTime;
 
        currentY = Mathf.Clamp(currentY, YMin, YMax);
 
        Vector3 Direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = lookAt.position + rotation * Direction;
 
        transform.LookAt(lookAt.position);
 
     
 
    }
}
 