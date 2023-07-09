using UnityEngine.AI;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform Player;
    
    [SerializeField] private float overshootAmount;


    private void Update(){
        CharacterController playerController = Player.GetComponent<CharacterController>();
        Vector3 predictedPosition = Player.position + playerController.velocity * overshootAmount;
        agent.SetDestination(Player.position);
    }
}
