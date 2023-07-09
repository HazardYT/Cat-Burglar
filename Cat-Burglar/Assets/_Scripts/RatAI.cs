using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class RatAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform Player;


    private void Update(){
        agent.SetDestination(Player.position);

    }

}
