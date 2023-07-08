using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private TMP_Text Hud;
    [SerializeField] private TMP_Text itemsGrabbedText;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask mask;
    [SerializeField] private int distance;
    public int ItemsGrabbed = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)){
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distance, mask)){
                Debug.DrawLine(cam.transform.position, hit.point, Color.red);
                Destroy(hit.transform.gameObject);
                ItemsGrabbed++;
                itemsGrabbedText.text = $"{ItemsGrabbed}";
                StopCoroutine(HudTextPickup());
                StartCoroutine(HudTextPickup(hit.transform.name));
            }
        }
    }
    IEnumerator HudTextPickup(string name = ""){
        Hud.text = $"Grabbed {name}";
        yield return new WaitForSeconds(1f);
        Hud.text = "";
    }
}
