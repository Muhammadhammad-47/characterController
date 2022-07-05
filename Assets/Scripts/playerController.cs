using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum inputType { touch, keyoard};
public class playerController : MonoBehaviour
{
    [Header("Platform")]
    public inputType inputSystem;

    [Header("Controller Variables")]
    [SerializeField] float walkSpeed = 5;
    [SerializeField] float runSpeed = 10;
    [SerializeField] float jumpHeight = 5;
    [SerializeField] float rotateSpeed = 15;
    float moveSpeed = 0;
    bool canAttack = true;

    [Header("Gravity Variables")]
    [SerializeField] float groundDistanceCheck =0.2f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded = false;
    bool doSprint = false;

    [Header("Modify Controller")]
    [SerializeField] bool jumpAllowed = false;
    [SerializeField] bool sprintAllowed = false;
    [SerializeField] bool healthSystemAllowed = false;

    [Header("Health System")]
    [SerializeField] int Health;

    [Header("References")]
    [SerializeField] FixedJoystick movementJoystick;
    [SerializeField] FixedTouchField touchField;
    [SerializeField] GameObject androidUI_Panel;
    [SerializeField] GameObject JumpBtn_android;
    [SerializeField] GameObject androidCam;
    [SerializeField] GameObject keyboardCams;

    [Header("UI")]
    [SerializeField] GameObject gameOver_Panel;

    float verticalInput = 0, horizontalInput = 0;
    float cameraAngle, cameraAngleSpeed = 0.2f;
    Vector3 moveDirection;
    Vector3 velocity;

    Rigidbody rb;
    CharacterController characterController;
    Animator anim;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        if (inputSystem ==inputType.touch)
        {
            keyboardCams.SetActive(false);

            androidCam.SetActive(true);
            androidUI_Panel.SetActive(true);

            if (jumpAllowed)
            {
                JumpBtn_android.SetActive(true);
            }
            else
            {
                JumpBtn_android.SetActive(false);
            }
        }
        else
        {
            androidCam.SetActive(false);
            androidUI_Panel.SetActive(false);

            keyboardCams.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }
    private void Update()
    {
        checkGround();
        inputManager();
        Movement_Z();
        Rotate();

        if (inputSystem == inputType.keyoard && Input.GetMouseButtonDown(0) && canAttack)
        {
            canAttack = false;
            StartCoroutine(Attack());
        }
    }


    void Movement_Z()
    {
        moveDirection = new Vector3(0, 0, verticalInput);

        //change move direction from world to local
        moveDirection = transform.TransformDirection(moveDirection);

        if (isGrounded)
        {
            if (moveDirection != Vector3.zero && doSprint && sprintAllowed)
            {
                //run
                Run();
            }
            else if (moveDirection != Vector3.zero && !doSprint)
            {
                //walk
                Walk();
            }
            else
            {
                //idle
                Idle();
            }

            moveDirection *= moveSpeed;

            if (Input.GetKeyDown(KeyCode.Space) && jumpAllowed  /*&& moveDirection.z > 0*/)
            {
                //jump
                Jump();
            }
        }
        //Move
        characterController.Move(moveDirection * Time.deltaTime);

        //Calculate Gravity
        velocity.y += gravity * Time.deltaTime;

        //Apply gravity
        characterController.Move(velocity * Time.deltaTime);
    }
    void Rotate()
    {
        float rotateMultiplier = horizontalInput * rotateSpeed * 100f * Time.deltaTime;
        transform.Rotate(Vector3.up, rotateMultiplier);
    }
    void checkGround()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundDistanceCheck, groundLayer);
        
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void Walk()
    {
        moveSpeed = walkSpeed;
        anim.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
    }
    void Run()
    {
        moveSpeed = runSpeed;
        anim.SetFloat("Speed", 1f, 0.1f, Time.deltaTime);
    }
    void Idle()
    {
        anim.SetFloat("Speed", 0);
    }
    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    IEnumerator Attack(UnityEngine.UI.Button attackBtn)
    {
        anim.SetLayerWeight(anim.GetLayerIndex("attack Layer"), 1);
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(1.35f);

        anim.SetLayerWeight(anim.GetLayerIndex("attack Layer"), 0);
        canAttack = true;
        attackBtn.interactable = true;
    }
    IEnumerator Attack()
    {
        anim.SetLayerWeight(anim.GetLayerIndex("attack Layer"), 1);
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(1.35f);

        anim.SetLayerWeight(anim.GetLayerIndex("attack Layer"), 0);
        canAttack = true;
    }

    void inputManager()
    {
        if (inputSystem == inputType.touch)
        {
            verticalInput = movementJoystick.Vertical;
            horizontalInput = movementJoystick.Horizontal;

            //rotateAndroidCam();
        }
        else
        {
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            doSprint = Input.GetKey(KeyCode.LeftShift) ? true : false;
        }
    }

    public void sprintBtn(bool state)
    {
        doSprint = state;
    }
    public void attackBtn(UnityEngine.UI.Button attackBtn)
    {
        if (canAttack)
        {
            canAttack = false;
            attackBtn.interactable = false;
            StartCoroutine(Attack(attackBtn));
        }
    }
    public void jumpBtn()
    {
        Jump();
    }
    public void restartSceneBtn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void removeHealth()
    {
        if (healthSystemAllowed)
        {
            if (Health > 1)
            {
                Health--;
            }
            else
            {
                gameOver_Panel.SetActive(true);

                Destroy(gameObject);
            }
        }
    }
}
