using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabController : MonoBehaviour
{
    public Animator animator;
	public GameObject objectToGrab;
	public GameObject LH_bone;		// Osul mainii stangi.
	public GameObject RH_bone;		// Osul mainii drepte.

	float proximityToObject = 1.0f;	// La mai putin de aceasta distanta (in metri) putem interactiona cu obiectul.
	GrabBounds gbLH, gbRH;	// grab bounds (vezi clasa de mai jos GrabBounds) pentru LeftHand si grab bounds pentru RightHand
	Vector3 objectOffset;	// La ce offset se va afla (la fiecare frame) sfera (obiectul) in raport cu pozitia mainii care il va tine.
	bool isGrabbedByLeftHand, isGrabbedByRightHand;

	enum Direction {	// Directia state machine-ului: o ia inspre actiunile mainii stangi sau inspre actiunile mainii drepte (sau None, in cazul in care nu suntem in faza de grabbing).
		None,
		LeftHand,
		RightHand
	}

	class GrabBounds {				// Cat de departe poate merge un brat intr-o anumita extrema.
		public Vector3 bottomLeftNear;
		public Vector3 bottomLeftFar;
		public Vector3 bottomRightNear;
		public Vector3 bottomRightFar;
		public Vector3 topLeftNear;
		public Vector3 topLeftFar;
		public Vector3 topRightNear;
		public Vector3 topRightFar;
	}

	// sphere: z = 0.7 (const), x = 

	float LerpBetweenFloats(float low, float high, float current) {	// lerp intre -1 si 1, depinzand de apropierea de low si/sau de high
		return (current - low) / (high - low) * 2.0f - 1.0f;
	}

	Vector3 LerpBetweenVector3s(Vector3 current, Direction dir) {	// Practic, mapeaza pozitia obiectului pentru grabbing in coordonate 2D (x, y), unde x si y apartin  intervalului [-1, 1].
		return dir == Direction.LeftHand
			?	new Vector3(
					LerpBetweenFloats(gbLH.bottomLeftNear.x, gbLH.bottomRightNear.x, current.x),
					LerpBetweenFloats(gbLH.bottomLeftNear.y, gbLH.topLeftNear.y, current.y),
					LerpBetweenFloats(gbLH.bottomLeftNear.z, gbLH.bottomLeftFar.z, current.z)
				)
			:	new Vector3(
					LerpBetweenFloats(gbRH.bottomLeftNear.x, gbRH.bottomRightNear.x, current.x),
					LerpBetweenFloats(gbRH.bottomLeftNear.y, gbRH.topLeftNear.y, current.y),
					LerpBetweenFloats(gbRH.bottomLeftNear.z, gbRH.topLeftFar.z, current.z)
				);
	}

	bool InBounds(float fixValue, float error, float value) {		// Is the value (called "value") in range of the fixValue by an error of "error"?
		return value > fixValue - error && value < fixValue + error;
	}

    void Awake()
    {
    	// Setarea extremelor bratului stang.
        gbLH = new GrabBounds();
        gbLH.bottomLeftFar = new Vector3(-0.4f, 1.0f, 0.7f);
        gbLH.bottomRightFar = new Vector3(0.0f, 1.0f, 0.7f);
        gbLH.topLeftFar = new Vector3(-0.4f, 1.52f, 0.7f);
        gbLH.topRightFar = new Vector3(0.0f, 1.52f, 0.7f);

        gbLH.bottomLeftNear = new Vector3(-0.4f, 1.0f, 0.35f);
        gbLH.bottomRightNear = new Vector3(0.0f, 1.0f, 0.35f);
        gbLH.topLeftNear = new Vector3(-0.4f, 1.52f, 0.35f);
        gbLH.topRightNear = new Vector3(0.0f, 1.52f, 0.35f);

        // Setarea extremelor bratului drept.
        gbRH = new GrabBounds();
        gbRH.bottomLeftFar = new Vector3(0.0f, 1.0f, 0.7f);
        gbRH.bottomRightFar = new Vector3(0.4f, 1.0f, 0.7f);
        gbRH.topLeftFar = new Vector3(0.0f, 1.52f, 0.7f);
        gbRH.topRightFar = new Vector3(0.4f, 1.52f, 0.7f);

        gbRH.bottomLeftNear = new Vector3(0.0f, 1.0f, 0.35f);
        gbRH.bottomRightNear = new Vector3(0.4f, 1.0f, 0.35f);
        gbRH.topLeftNear = new Vector3(0.0f, 1.52f, 0.35f);
        gbRH.topRightNear = new Vector3(0.4f, 1.52f, 0.35f);

        isGrabbedByLeftHand = false;
        isGrabbedByRightHand = false;
    }

    void Start() {
    	animator.SetInteger("GrabDirection", (int) Direction.None);
    }

    void Update()
    {
        float distanceLH_O = (objectToGrab.transform.position - LH_bone.transform.position).magnitude;	// distance LeftHand - Object (object to grab)
        float distanceRH_O = (objectToGrab.transform.position - RH_bone.transform.position).magnitude;	// distance RightHand - Object (object to grab)


        // Daca suntem in starea de locomotie, verificam daca minimul dintre cele doua distante este mai mic decat proximitatea. Daca este, atunci alegem directia spre care avem distanta minima.
        //if (animator.GetInteger("GrabDirection") == (int) Direction.None) {
        if (!animator.GetBool("GrabCanceled")) {		//  && InBounds(180.0f, 2.0f, transform.rotation.eulerAngles.y)	// Conditia ca player-ul sa fie CAT DE CAT (APROXIMATIV) paralel cu cubul pe care se afla sfera. Playerul trebuie sa fie incadrat intre (180 - 2)gr si (180 + 2)gr, dpdv al rotatiei in jurul axei Y (axa de inaltime).
        	if (distanceLH_O < proximityToObject) {
        		if (distanceRH_O < distanceLH_O)
        			animator.SetInteger("GrabDirection", (int) Direction.RightHand);
        		else
        			animator.SetInteger("GrabDirection", (int) Direction.LeftHand);
        	} else if (distanceRH_O < proximityToObject)
        		animator.SetInteger("GrabDirection", (int) Direction.RightHand);
        }



        // Controlarea bratului care va face grab: care brat va face grab si in ce pozitie se va duce acesta pentru a face grab? :
        if (Input.GetKeyDown(KeyCode.Space)) {
        	animator.SetBool("GrabCanceled", false);	// GrabCanceled functioneaza ca un trigger pentru grab. Orice NU e GrabCanceled e trigger pozitiv pentru grab.
        } else if (Input.GetKeyUp(KeyCode.Space)) {
        	animator.SetBool("GrabCanceled", true);
        }

        /*if (Input.GetKeyDown(KeyCode.Space)) {
        	Debug.Log("inv = " + transform.InverseTransformPoint(objectToGrab.transform.position));
        	Debug.Log("euler = " + transform.rotation.eulerAngles.y);
        }*/

        if (animator.GetInteger("GrabDirection") == (int) Direction.LeftHand) {
        	Vector3 lerpedHandPos = LerpBetweenVector3s(transform.InverseTransformPoint(objectToGrab.transform.position), Direction.LeftHand);
	        animator.SetFloat("Right", lerpedHandPos.x);
	        animator.SetFloat("Forward", lerpedHandPos.y);
	        animator.SetFloat("BodyForward", lerpedHandPos.z);
        }

        if (animator.GetInteger("GrabDirection") == (int) Direction.RightHand) {
        	Vector3 lerpedHandPos = LerpBetweenVector3s(transform.InverseTransformPoint(objectToGrab.transform.position), Direction.RightHand);
	        animator.SetFloat("Right", lerpedHandPos.x);
	        animator.SetFloat("Forward", lerpedHandPos.y);
	        animator.SetFloat("BodyForward", lerpedHandPos.z);
        }



        // Grabbing-ul propriu-zis: preluarea sferei (mingei) in mana si mentinerea ei in mana:
        AnimatorStateInfo m_CurrentClipLength = animator.GetCurrentAnimatorStateInfo(0);
        //Debug.Log("clip name = " + m_CurrentClipLength.name);	// There is NO .name attribute. You can only check the name with .IsName(...) like in the below lines.

        if (m_CurrentClipLength.IsName("LeftHandGrabForward")) {
        	objectOffset = objectToGrab.transform.position - LH_bone.transform.position;
        }
        if (m_CurrentClipLength.IsName("RightHandGrabForward")) {
        	objectOffset = objectToGrab.transform.position - RH_bone.transform.position;
        }
        if (m_CurrentClipLength.IsName("LeftHandGrabBackward")) {
        	isGrabbedByLeftHand = true;
        	isGrabbedByRightHand = false;
        }
        if (m_CurrentClipLength.IsName("RightHandGrabBackward")) {
        	isGrabbedByRightHand = true;
        	isGrabbedByLeftHand = false;
        }

        if (isGrabbedByLeftHand) {
        	objectToGrab.transform.position = LH_bone.transform.position + objectOffset;
        }

        if (isGrabbedByRightHand) {
        	objectToGrab.transform.position = RH_bone.transform.position + objectOffset;
        }
    }
}
