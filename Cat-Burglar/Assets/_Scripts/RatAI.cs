using UnityEngine.AI;
using UnityEngine;

public class RatAI : MonoBehaviour
{
    private GameManager manager;
     private float _footstepDistanceCounter;
    public RatFootstepManager footstepManager;
    [SerializeField] private float EasySpeed = 8, NormalSpeed = 10, HardSpeed = 14;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform[] startPoints;
    [SerializeField] private Transform Player;
    [SerializeField] private LayerMask mask;
    public float tweakFootsteps = 3;
    public float Radius;
    void Start(){
        manager = FindObjectOfType<GameManager>();
        SetDifficulty();
        agent.Warp(startPoints[Random.Range(0, startPoints.Length)].position);
    }
    private void Update(){
        FootSteps(agent.speed);
        Collider[] colliders = Physics.OverlapSphere(transform.position, Radius, mask);
        foreach(Collider collider in colliders){
            if (collider.transform.CompareTag("Player")){
                if (Vector3.Distance(transform.position, collider.transform.position) > 2){
                    agent.SetDestination(Player.position);
                }
                else{
                    manager.Lose();
                    Destroy(this.gameObject);
                }
            } 
        }
    }
    private void SetDifficulty(){
        switch(Menu.difficulty){
            case 0:
                agent.speed = EasySpeed;
                break;
            case 1:
                agent.speed = NormalSpeed;
                break;
            case 2:
                agent.speed = HardSpeed;
                break;
        }
    }
    public void FootSteps(float speed){
        if (agent.velocity != Vector3.zero)
        {
            _footstepDistanceCounter += speed * Time.deltaTime;
            if (_footstepDistanceCounter >= tweakFootsteps)
            {
                _footstepDistanceCounter = 0;
                footstepManager.PlayFootstep();
            }
        }
    }
}
