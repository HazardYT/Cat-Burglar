using System.Collections;
using UnityEngine;
using TMPro;
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private TMP_Text Hud;
    [SerializeField] private TMP_Text itemsGrabbedText;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask mask;
    [SerializeField] private int distance;
    [SerializeField] private GameManager manager;
    public int ItemsGrabbed = 0;
    void Start(){
        manager = FindObjectOfType<GameManager>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)){
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distance, mask)){
                Debug.DrawLine(cam.transform.position, hit.point, Color.red);
                Destroy(hit.transform.gameObject);
                ItemsGrabbed++;
                itemsGrabbedText.text = $"Catnip Stolen: {ItemsGrabbed}";
                StopCoroutine(HudTextPickup());
                StartCoroutine(HudTextPickup(hit.transform.name));
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)){
            manager.MenuToggle();
        }
    }
    IEnumerator HudTextPickup(string name = ""){
        Hud.text = $"Grabbed {name}";
        yield return new WaitForSeconds(1f);
        Hud.text = "";
    }
}
