using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject _catnip;
    public GameObject _food;
    public int EasyDifficultyFoodSpawns = 12;
    public int NormalDifficultyFoodSpawns = 10;
    public int HardDifficultyFoodSpawns = 6;
    public int EasyDifficultyCatnipSpawns = 6;
    public int NormalDifficultyCatnipSpawns = 12;
    public int HardDifficultyCatnipSpawns = 16;
    public List<Transform> CatnipSpawns = new List<Transform>();
    public List<Transform> FoodSpawns = new List<Transform>();

    // public GameObject cnspl;
    // public GameObject fspl;

    public static ItemSpawner instance;

    // void Awake(){

    //     Transform[] cnsplChildren = cnspl.GetComponentsInChildren<Transform>(); 
    //     foreach (Transform child in cnsplChildren){
            
    //         if(child != cnspl.transform)
    //             CatnipSpawns.Add(child);
    //     }
    //     Transform[] fsplChildren = fspl.GetComponentsInChildren<Transform>(); 
    //     foreach (Transform child in fsplChildren){

    //         if(child != fspl.transform)
    //             FoodSpawns.Add(child);
    //     }
    // }

    void Start()
    {
        instance = this;

        switch(Menu.difficulty){
            case 0:
                StartCoroutine(SpawnItems(EasyDifficultyFoodSpawns, EasyDifficultyCatnipSpawns));
                break;
            case 1:
                StartCoroutine(SpawnItems(NormalDifficultyFoodSpawns, NormalDifficultyCatnipSpawns));
                break;
            case 2:
                StartCoroutine(SpawnItems(HardDifficultyFoodSpawns, HardDifficultyCatnipSpawns));
                break;
        }
    }
    IEnumerator SpawnItems(int foodAmount, int catnipAmount){
        for (int i = 0; i < foodAmount; i++)
        {
            int n = Random.Range(0, FoodSpawns.Count);
            Instantiate(_food, FoodSpawns[n].position, FoodSpawns[n].rotation);
            FoodSpawns.Remove(FoodSpawns[n]);
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < catnipAmount; i++)
        {
            int n = Random.Range(0, CatnipSpawns.Count);
            Instantiate(_catnip, CatnipSpawns[n].position, CatnipSpawns[n].rotation);
            CatnipSpawns.Remove(CatnipSpawns[n]);
            yield return new WaitForEndOfFrame();
        }
    }
}
