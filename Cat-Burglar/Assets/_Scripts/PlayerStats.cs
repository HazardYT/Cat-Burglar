using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CatController catController;
    [SerializeField] private TMP_Text Hud;
    [SerializeField] private TMP_Text itemsGrabbedText;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask mask;
    [SerializeField] private int distance;
    [SerializeField] private GameManager manager;
    [SerializeField] private Volume volume;
    public int CatnipNeeded = 12;
    float volumeIntensity = 0f;
    public int ItemsGrabbed = 0;
    void Start(){
        manager = FindObjectOfType<GameManager>();
        switch(Menu.difficulty){
            case 0:
                CatnipNeeded = ItemSpawner.instance.EasyDifficultyCatnipSpawns;
                catController.stamReductionSpeed = 5;
                break;
            case 1:
                CatnipNeeded = ItemSpawner.instance.NormalDifficultyCatnipSpawns;
                catController.stamReductionSpeed = 10;
                break;
            case 2:
                CatnipNeeded = ItemSpawner.instance.HardDifficultyCatnipSpawns;
                catController.stamReductionSpeed = 15;
                break;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)){
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distance, mask)){
                Debug.DrawLine(cam.transform.position, hit.point, Color.red);
                if (hit.transform.CompareTag("Catnip")){
                    Destroy(hit.transform.gameObject);
                    if (ItemsGrabbed < CatnipNeeded){
                        ItemsGrabbed++;
                        volumeIntensity += 0.1f;
                        volume.weight = volumeIntensity;
                        itemsGrabbedText.text = $"Catnip Stolen: {ItemsGrabbed}";
                        catController.PlayHappy();
                        StopCoroutine(HudTextPickup());
                        StartCoroutine(HudTextPickup(hit.transform.name));
                    }
                    else{
                        manager.Win();
                    }
                }
                else if (hit.transform.CompareTag("Food")){
                    Destroy(hit.transform.gameObject);
                    StartCoroutine(Munch());
                    catController.curStam += 25;
                    catController.canRun = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)){
            manager.MenuToggle();
        }
    }
    IEnumerator Munch(){
        catController.PlayMunch();
        yield return new WaitUntil(() => !catController.audioSource.isPlaying);
        manager.PlayPurr();
    }
    IEnumerator HudTextPickup(string name = ""){
        Hud.text = $"Grabbed {name}";
        yield return new WaitForSeconds(1f);
        Hud.text = "";
    }
}
