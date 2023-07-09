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
    [SerializeField] private Vector3 walkPoint;
    [SerializeField] private float walkPointRange;
    [SerializeField] private bool isWalkPointSet;
    public float tweakFootsteps = 3;
    public float Radius;
    void Start(){
        manager = FindObjectOfType<GameManager>();
        SetDifficulty();
        SearchWalkPoint();
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
            else {
                Patrolling();
                print ("patrolling");
            }
        }
    }
    public void Patrolling()
    {
        if (!isWalkPointSet) SearchWalkPoint();

        //Debug.Log("(Patrolling) - Function called");
        if (isWalkPointSet)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(walkPoint);
                Debug.DrawLine(transform.position, walkPoint, Color.yellow);
            }
            else { isWalkPointSet = false; }
        }
        if (Vector3.Distance(transform.position, walkPoint) < 2f) { isWalkPointSet = false; }
    }
    public void SearchWalkPoint()
    {
        bool validWalkPoint = false;
        int attempts = 0;

        do
        {
            attempts++;

            float randomAngle = Random.Range(-Mathf.PI / 4, Mathf.PI / 4);
            float x = walkPointRange * Mathf.Cos(randomAngle);
            float z = walkPointRange * Mathf.Sin(randomAngle);

            Vector3 rotatedDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(x, 0, z);
            Vector3 walkPointDirection = new Vector3(transform.position.x + rotatedDirection.x, transform.position.y, transform.position.z + rotatedDirection.z);
            NavMesh.SamplePosition(walkPointDirection, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas);

            agent.SetDestination(hit.position);

            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                validWalkPoint = true;
            }
            if (validWalkPoint)
            {
                walkPoint = hit.position;
                isWalkPointSet = true;
                //Debug.Log("(SearchWalkPoint) - Found Valid Walkpoint");
                return;
            }
            else
            {
                //Debug.Log("(SearchWalkPoint) - Finding Random Walkpoint");
                Vector3 randomDirection = Random.insideUnitSphere * walkPointRange;
                randomDirection += transform.position;
                NavMesh.SamplePosition(randomDirection, out hit, walkPointRange, NavMesh.AllAreas);

                agent.SetDestination(hit.position);

                if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    walkPoint = hit.position;
                    isWalkPointSet = true;
                    return;
                }
            }
        } while (!validWalkPoint && attempts < 3);
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
