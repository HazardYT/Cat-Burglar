using UnityEngine.AI;
using UnityEngine;
using System.Collections;
public class RatAI : MonoBehaviour
{
    private GameManager manager;
     private float _footstepDistanceCounter;
    public RatFootstepManager footstepManager;
    [SerializeField] private float EasySpeed = 8, NormalSpeed = 10, HardSpeed = 14;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform[] startPoints;
    [SerializeField] private AudioClip[] Squeaks;
    [SerializeField] private Transform Player;
    [SerializeField] private LayerMask mask;
    public AudioSource squeakAudioSource;
    public float tweakFootsteps = 3;
    public float Radius;
    [SerializeField] float attackRange = 2;
    void Start(){
        manager = FindObjectOfType<GameManager>();
        StartCoroutine(SqueakCoroutine());
        SetDifficulty();
        agent.Warp(startPoints[Random.Range(0, startPoints.Length)].position);
    }
    private void Update()
    {
        FootSteps(agent.speed);
        Collider[] colliders = Physics.OverlapSphere(transform.position, Radius, mask);
        foreach (Collider collider in colliders)
        {
            if (collider.transform.CompareTag("Player"))
            {
                if (Vector3.Distance(transform.position, collider.transform.position) > attackRange)
                {
                    NavMeshHit hit;
                    if (NavMesh.FindClosestEdge(collider.transform.position, out hit, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        if (agent.velocity == Vector3.zero)
                        {
                            anim.SetBool("Walking",false);
                            attackRange += 0.001f;
                            // Rotate to face the player on the y-axis
                            Vector3 direction = collider.transform.position - transform.position;
                            direction.y = 0f; // Ignore y-axis rotation
                            if (direction != Vector3.zero)
                            {
                                Quaternion rotation = Quaternion.LookRotation(direction);
                                transform.rotation = rotation;
                            }
                        }
                        else
                        {
                            attackRange = 2;
                            anim.SetBool("Walking",true);
                        }
                    }
                }
                else
                {
                    manager.Lose();
                    Destroy(gameObject);
                }
            }
        }
    }


    IEnumerator SqueakCoroutine(){
        while (true){
            float i = Random.Range(1, 5);
            yield return new WaitForSeconds(i);
            PlaySqueak();
        }
    }
    public void PlaySqueak(){
        int i = Random.Range(0, Squeaks.Length);
        squeakAudioSource.clip = Squeaks[i];
        squeakAudioSource.Play();
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
