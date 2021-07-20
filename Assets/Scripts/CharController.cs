using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    Camera cam;
    Vector3 moveDir;
    float rotSpeed = 1f;
    public Animator animator;
    Rigidbody rb;
    public float groundCheck = 0.2f;
    public float jumpPower = 5f;

    GameObject mainCamera; 
    UIScript _UIScript;

    float moveFwd, moveSide;

    void Awake()
    {
        cam = FindObjectOfType<Camera>();
        rb = GetComponent<Rigidbody>();
        mainCamera = GameObject.Find("Main Camera");
        _UIScript = GameObject.Find("UIScript").GetComponent<UIScript>();
    }

    void Start() {
        moveDir = Vector3.zero;
    }

    void Update()
    {
        if (!animator.GetBool("GrabCanceled"))
            return;

        moveFwd = Input.GetAxis("Vertical");
        moveSide = Input.GetAxis("Horizontal");

        if (_UIScript.currentMode != GameModes.Modes.Pathfinding) {
	        moveDir = cam.transform.right * moveSide + cam.transform.forward * moveFwd;
	    }

        moveDir.y = 0;
        moveDir = moveDir.normalized;

        if (moveFwd != 0 												// daca apas doar pe arrowLeft sau arrowRight, fara sa apas pe arrowForward, caracterul NU se va urni din loc, dar se va roti doar (deci NU vom seta velocity daca NU apas pe arrowForward)
        	|| _UIScript.currentMode == GameModes.Modes.Pathfinding)	// daca nu puneam conditia asta, nu trecea if-ul doar cu conditia de mai sus, deoarece la pathfinding nu trebuie sa apesi pe niciun arrow (nu neaparat), ci personajul trebuie sa se miste (sa capete velocity) singur
            GetComponent<Rigidbody>().velocity = moveDir * 2f;

        float theta = Mathf.Acos(Vector3.Dot(transform.forward, moveDir));
        theta *= Mathf.Rad2Deg;

        if ((moveDir - transform.forward).magnitude > 0.01f && // if we move anything but straight forward
            (moveDir + transform.forward).magnitude > 0.01f) { // or if we move anything but straight backward

            transform.rotation = Quaternion.AngleAxis(theta * Time.deltaTime * rotSpeed, Vector3.Cross(transform.forward, moveDir)) * transform.rotation;
        }

        if ((moveDir + transform.forward).magnitude < 0.01f)
        {
            transform.rotation = Quaternion.AngleAxis(2f, Vector3.up) * transform.rotation;
        }

        ComputeAnimatorParams(moveDir);
    }

    void ComputeAnimatorParams(Vector3 dir)
    {
        Vector3 moveDirCharacterSpace = transform.InverseTransformDirection(moveDir);

        if (Input.GetKey(KeyCode.LeftShift))    // merge, nu alearga
        {
            moveDirCharacterSpace *= 0.5f;
        }   // else, alearga

        animator.SetFloat("Forward", moveDirCharacterSpace.z, 0.2f, Time.deltaTime);
        animator.SetFloat("Right", moveDirCharacterSpace.x, 0.2f, Time.deltaTime);
    }

    public void SetMoveDir(Vector3 newMoveDir) {
    	moveDir = newMoveDir;
    }
}
