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

    public static ItemSpawner instance;


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
            GameObject g = Instantiate(_food, FoodSpawns[n].position, FoodSpawns[n].rotation);
            g.name = _food.name;
            FoodSpawns.Remove(FoodSpawns[n]);
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < catnipAmount; i++)
        {
            int n = Random.Range(0, CatnipSpawns.Count);
            GameObject g = Instantiate(_catnip, CatnipSpawns[n].position, CatnipSpawns[n].rotation);
            g.name = _catnip.name;
            CatnipSpawns.Remove(CatnipSpawns[n]);
            yield return new WaitForEndOfFrame();
        }
    }
}
