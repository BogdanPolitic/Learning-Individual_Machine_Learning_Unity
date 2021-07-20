using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTheObject : MonoBehaviour
{
	class GrabBounds {
		public Vector3 bottomLeft;
		public Vector3 bottomRight;
		public Vector3 topLeft;
		public Vector3 topRight;
	}

	// center = (0.25, 1.1, 0.8), 0.8 = CONST
	// sfera = (0, 1.3, 0.8)
	// result with bug = (2.3, -1.2, 0.8)

	float LerpBetweenFloats(float low, float high, float current) {	// lerp intre -1 si 1
		return (current - low) / (high - low) * 2.0f - 1.0f;
	}

	Vector3 LerpBetweenVector3s(Vector3 current) {
		return new Vector3(
				LerpBetweenFloats(0.01f, 0.49f, current.x),
				LerpBetweenFloats(0.86f, 1.52f, current.y),
				0.8f
			);
	}

	public Animator animator;
	public GameObject objectToGrab;
	GrabBounds gb;

    void Awake()
    {
        gb = new GrabBounds();
        gb.bottomLeft = new Vector3(0.04f, 0.92f, 0.8f);
        gb.bottomRight = new Vector3(0.45f, 0.8f, 0.8f);
        gb.topLeft = new Vector3(-0.02f, 1.6f, 0.8f);
        gb.topRight = new Vector3(0.53f, 1.45f, 0.8f);
    }

    void Update()
    {
        Vector3 lerpedHandPos = LerpBetweenVector3s(objectToGrab.transform.position);
        animator.SetFloat("Horizontal", lerpedHandPos.x);
        animator.SetFloat("Vertical", lerpedHandPos.y);
    }
}
