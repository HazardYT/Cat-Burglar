using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject _catnip;
    public GameObject _food;
    [SerializeField] private int EasyDifficultyFoodSpawns = 12;
    [SerializeField] private int NormalDifficultyFoodSpawns = 10;
    [SerializeField] private int HardDifficultyFoodSpawns = 6;
    [SerializeField] private int EasyDifficultyCatnipSpawns = 6;
    [SerializeField] private int NormalDifficultyCatnipSpawns = 12;
    [SerializeField] private int HardDifficultyCatnipSpawns = 16;
    public List<Transform> CatnipSpawns = new List<Transform>();
    public List<Transform> FoodSpawns = new List<Transform>();
    void Start()
    {
        switch(Menu.instance.difficulty){
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
