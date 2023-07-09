using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class RatAI : MonoBehaviour
{
    private GameManager manager;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform[] startPoints;
    [SerializeField] private Transform Player;
    [SerializeField] private LayerMask mask;
    public float attackRadius;
    void Start(){
        manager = FindObjectOfType<GameManager>();
        agent.Warp(startPoints[Random.Range(0, startPoints.Length)].position);
    }
    private void Update(){
        agent.SetDestination(Player.position);
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius, mask);
        foreach(Collider collider in colliders){
            if (collider != null){
                manager.Lose();
            }
        }
    }

}
