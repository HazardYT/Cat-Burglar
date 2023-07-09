using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]

public class CatController : MonoBehaviour
{

    public bool canRun = true;
    public float maxStam = 100f;
    public float curStam;
    public float stamReductionSpeed = 1.0f;

    public bool blinking = false;

    public RawImage stamBar;

    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float meowChance = 0.5f;
    bool jumping = false;
    public Camera playerCamera;
    public Animator anim;
    public AudioClip[] meows;
    public AudioSource audioSource;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    int previousSound;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {

        curStam = maxStam;
        canRun = true;

        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && canRun;
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if(!canRun){isRunning = false;}

        if(isRunning){

            curStam -= stamReductionSpeed * Time.deltaTime;
            if(curStam <= 10){

                canRun = false;
            }
        }
        else{

            curStam += (stamReductionSpeed / 2) * Time.deltaTime;
            if(curStam >= 50){

                canRun = true;
            }
        }
        curStam = Mathf.Clamp(curStam, 0f, 100f);
        stamBar.transform.localScale = new Vector3(curStam/100f, 1, 1);
        if(curStam >= 50 && !isRunning){

            stamBar.color = new Color(255, 255, 255, Mathf.Lerp(stamBar.color.a, 0, 2 * Time.deltaTime));
        } else{

            stamBar.color = new Color(255, 255, 255, Mathf.Lerp(stamBar.color.a, 100, Time.deltaTime));
        }
        if(curStam <= 40 && isRunning && !blinking){

            StartCoroutine(BinkStamBar(0.25f));
        }

        if (isRunning) { anim.SetBool("Running", true); } else anim.SetBool("Running", false);
        if (!isRunning && characterController.velocity.magnitude > 0) { anim.SetBool("Walking",true); } else anim.SetBool("Walking",false);
        if (Input.GetButtonDown("Jump") && canMove && characterController.isGrounded && !jumping)
        {
            jumping = true;
            if (Random.value > meowChance){
                int i = Random.Range(0, meows.Length);
                if (i == previousSound) { i = Random.Range(0, meows.Length); }
                previousSound = i;
                audioSource.clip = meows[i];
                audioSource.Play();
            }
            anim.SetBool("Jumping", true);
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        if (characterController.isGrounded) { anim.SetBool("Jumping",false); jumping = false;}
    }


    IEnumerator BinkStamBar(float waitTime){

        blinking = true;
        stamBar.enabled = false;
        yield return new WaitForSeconds(waitTime);
        stamBar.enabled = true;
        yield return new WaitForSeconds(waitTime);
        blinking = false;
    }
}