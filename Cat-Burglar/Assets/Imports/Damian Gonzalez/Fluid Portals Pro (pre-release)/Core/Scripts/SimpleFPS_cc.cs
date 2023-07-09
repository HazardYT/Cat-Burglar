using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamianGonzalez {
    public class SimpleFPS_cc : MonoBehaviour {
        public static Transform thePlayer; //easy access
        CharacterController cc;
        Transform cam;
        float rotX = 0;
        public float walkSpeed = 10f;
        public float runSpeed = 20f;
        public float slowSpeed = 2f;
        public float mouseSensitivity = 1f;

        Transform groundReference;

        public LayerMask floorLayerMask;
        public bool grounded;

        public float gravity = 9.81f;
        float verticalVelocity = 0;

        public float jumpForce = 4f;

        private void Awake() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }

        void Start() {
            thePlayer = transform;
            cc = GetComponent<CharacterController>();
            cam = transform.GetChild(0);

            groundReference = transform.GetChild(1);
            rotX = cam.eulerAngles.x;
        }


        void Update() {


            grounded = Physics.CheckSphere(groundReference.position, .1f, floorLayerMask);

            //if not grounded, make the player fall
            if (!grounded) {
                verticalVelocity -= gravity * Time.deltaTime;
            } else {
                verticalVelocity = 0;
                //since he's grounded, he can jump
                if (Input.GetButtonDown("Jump")) verticalVelocity = jumpForce;
            }


            float speed = walkSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) speed = runSpeed;
            if (Input.GetKey(KeyCode.LeftControl)) speed = slowSpeed;

            Vector3 forwardNotTilted = new Vector3(transform.forward.x, 0, transform.forward.z);



            cc.Move(
                forwardNotTilted * speed * Input.GetAxis("Vertical") * Time.deltaTime    //move forward
                +
                transform.right * speed * Input.GetAxis("Horizontal") * Time.deltaTime  //slide to sides
                +
                Vector3.up * verticalVelocity * Time.deltaTime //jump and fall
            );

            //rotate player left/right, and try to make player stand still if tilted
            transform.rotation = (Quaternion.Lerp(
                transform.rotation * Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0),
                Quaternion.Euler(0, transform.eulerAngles.y, 0),
                .1f
            ));

            //look up and down
            rotX += Input.GetAxis("Mouse Y") * mouseSensitivity * -1;
            rotX = Mathf.Clamp(rotX, -90f, 90f); //clamp look 
            cam.localRotation = Quaternion.Euler(rotX, 0, 0);

        }
    }
}