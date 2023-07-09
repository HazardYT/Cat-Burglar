using System.Collections.Generic;
using UnityEngine;

//this script is not part of the portals system, it's only for the demo scene,
//only to ilustrate how other object can (or can't) cross portals

namespace DamianGonzalez {

    public class ShootBalls : MonoBehaviour {
        public GameObject prefabBall;
        public float shortThrowForce = 100f;
        public float longThrowForce = 250f;
        public int maxAmount = 10;
        Transform recycleBin;
        Queue<GameObject> ballsPool = new Queue<GameObject>();

        private void Start() {
            recycleBin = (GameObject.Find("recycle bin") ?? new GameObject("recycle bin")).transform;
            //InvokeRepeating(nameof(ThrowProjectile), 1f, 1f);
        }

        void Update() {
            if (Input.GetMouseButtonDown(1)) ThrowProjectile();


            //restart scene (only on the demo scene)
            if (Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene(0);

        }

        void ThrowProjectile() {
            //new projectile slightly in front of player
            GameObject projectile = NewBall();

            projectile.transform.position = transform.position + (transform.forward * 1f);

            //add force
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            float force = shortThrowForce;
            if (Input.GetKey(KeyCode.LeftShift)) force *= 2.5f;
            if (Input.GetKey(KeyCode.LeftControl)) force *= .2f;
            rb.AddForce(transform.forward * force, ForceMode.Impulse);



        }

        GameObject NewBall() {
            GameObject ball;
            if (ballsPool.Count < maxAmount) {
                //instantiate
                ball = Instantiate(
                    prefabBall,
                    recycleBin
                );

                //random color
                ball.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();

                //and a name
                ball.name = "ball";
            } else {
                ball = ballsPool.Dequeue();
                ball.transform.position = transform.position + (transform.forward * 1f);
            }
            ballsPool.Enqueue(ball);
            return ball;
        }
    }
}