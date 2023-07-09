using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class RatAI : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform Player;
    [SerializeField] private float radius;
    public bool warptimer = false;


    private void Update(){
        //agent.SetDestination(Player.position);
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, mask);
        foreach(Collider collider in colliders){
            if (collider != null){
                if (!collider.CompareTag("Player") && !warptimer && Vector3.Distance(transform.position, Player.position) > 25){
                    StartCoroutine(CheckDistance(collider.transform));
                }
                else agent.SetDestination(Player.position);
            }
        }
    }
    IEnumerator CheckDistance(Transform value)
    {
        agent.SetDestination(value.position);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, value.position) < 2);
        Warp warp = value.GetComponent<Warp>();
        bool i = agent.Warp(warp.warpTransform.position);
        if (i) {
            warptimer = true;
            yield return new WaitForSeconds(2f);
            warptimer = false;
        }
    }
    void ResetWarp(){
        warptimer = false;
    }
    void OnGizmosDraw(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
