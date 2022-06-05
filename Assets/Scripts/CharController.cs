using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    Camera cam;
    [SerializeField] GameObject _camera;
    [SerializeField] CharacterSoundPlayer _characterSoundPlayer;


    public Vector3 moveDir, prevMoveDir;
    float rotSpeed = 270.0f;
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
        moveDir = prevMoveDir = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!animator.GetBool("GrabCanceled"))
        {
            EndFrameActions();
            return;
        }

        moveFwd = Input.GetAxis("Vertical");
        moveSide = Input.GetAxis("Horizontal");

        if (_UIScript.currentMode != GameModes.Modes.Pathfinding) {
	        moveDir = cam.transform.right * moveSide + cam.transform.forward * moveFwd;
	    }

        moveDir.y = 0;
        moveDir = moveDir.normalized;

        ComputeAnimatorParams(moveDir);

        if (_UIScript.currentMode != GameModes.Modes.Pathfinding && (moveFwd == 0.0f && moveSide == 0.0f))
        {
            if (moveDir == Vector3.zero && prevMoveDir != Vector3.zero)
                _characterSoundPlayer.StopPlayingWalkingSound();

            EndFrameActions();
            return;
        }

        if (_UIScript.currentMode == GameModes.Modes.Pathfinding	// la pathfinding nu trebuie sa apesi pe niciun arrow (nu neaparat), ci personajul trebuie sa se miste (sa capete velocity) singur
            || !CustomPathfinding.instance.orientatingToTargetInPlace)
            GetComponent<Rigidbody>().velocity = moveDir * 2f;

        Vector3 camTrFwdProjected = Vector3.Normalize(Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up));
        float theta = Mathf.Acos(Vector3.Dot(transform.forward, camTrFwdProjected));
        theta *= Mathf.Rad2Deg;

        if ((camTrFwdProjected - transform.forward).magnitude > 0.01f && // if we move anything but straight forward
            (camTrFwdProjected + transform.forward).magnitude > 0.01f) { // or if we move anything but straight backward
            
            if (_UIScript.currentMode == GameModes.Modes.Pathfinding)
                transform.rotation = Quaternion.AngleAxis(Time.deltaTime * rotSpeed, Vector3.Cross(transform.forward, moveDir)) * transform.rotation;
            else
                transform.rotation = Quaternion.AngleAxis(theta * Time.deltaTime * 18.0f, Vector3.Cross(transform.forward, camTrFwdProjected)) * transform.rotation;
        }


        if (_UIScript.currentMode == GameModes.Modes.Pathfinding && CustomPathfinding.instance.orientatingToTargetInPlace)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            if ((moveDir - transform.forward).magnitude < 0.01f)
            {
                CustomPathfinding.instance.orientatingToTargetInPlace = false;
                moveDir = Vector3.zero;
            }
        }

        if (moveDir != Vector3.zero && prevMoveDir == Vector3.zero)
            _characterSoundPlayer.PlayWalkingSound();

        Debug.Log("moveDir = " + moveDir + " and prev = " + prevMoveDir);

        EndFrameActions();
    }

    void EndFrameActions()
    {
        prevMoveDir = moveDir;
    }

    void ComputeAnimatorParams(Vector3 dir)
    {
        Vector3 moveDirCharacterSpace = transform.InverseTransformDirection(moveDir);

        animator.SetFloat("Forward", moveDirCharacterSpace.z, 0.2f, Time.deltaTime);
        animator.SetFloat("Right", moveDirCharacterSpace.x, 0.2f, Time.deltaTime);
    }

    public void SetMoveDir(Vector3 newMoveDir) {
    	moveDir = newMoveDir;
    }
}
