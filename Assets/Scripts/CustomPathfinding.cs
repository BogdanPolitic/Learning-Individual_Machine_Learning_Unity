using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPathfinding : MonoBehaviour
{
	public static CustomPathfinding instance;

	public Transform seeker, target;
	public CustomGrid grid;

	public float speed = 100.0f;
	public float nextWaypointDistance;
	int currentWaypoint = 0;
	bool reachedEndOfPath = false;

	public CharController playerController;
	public float minDistanceToTarget;
	public bool orientatingToTargetInPlace;	// When the character gets to the minimum allowed distance point to an object, it may firstly have an orientation that is NOT equal to the direction to the focused object.
	// This variable is TRUE while the character is adjusting its rotation until in perfectly faces the object he should be focused on. This variable cannot be TRUE if the character didn't arrive close to the focused object yet.

	bool drawGiz;
	HashSet<CustomNode> closedSet;

	void Awake()
	{
		instance = this;

		//playerController = GetComponent<MyController
		closedSet = new HashSet<CustomNode>();
	}

    private void Start()
    {
		InvokeRepeating("UpdatePath", 0f, 0.5f);
		drawGiz = false;
		orientatingToTargetInPlace = false;
    }

	void UpdatePath()
    {
		if (target == null) return;
		if (Vector3.Distance(seeker.position, target.position) < minDistanceToTarget) return;

		SwitchHouseColliders.instance.SetCollidersActiveRecursively(target, false);
		CustomGrid.instance.CreateGrid();

		orientatingToTargetInPlace = false;
		reachedEndOfPath = false;
		FindPath(seeker.position, target.position);
	}

    private void FixedUpdate()
    {
		if (grid.path == null || reachedEndOfPath || orientatingToTargetInPlace) return;

		if (currentWaypoint >= grid.path.Count)
        {
			reachedEndOfPath = true;
			return;
		}

		if ((target.position - seeker.position).magnitude < minDistanceToTarget)
        {
            reachedEndOfPath = true;
			//playerController.SetMoveDir(Vector3.zero);
			playerController.SetMoveDir(target.position - seeker.position);
			orientatingToTargetInPlace = true;
			return;
        }

        Vector3 direction = (grid.path[currentWaypoint].worldPosition - transform.position).normalized;
		playerController.SetMoveDir(direction);

		float distance = Vector3.Distance(seeker.position, grid.path[currentWaypoint].worldPosition);
		if (distance < nextWaypointDistance)
			currentWaypoint++;
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
	{
		//Vector3 targetToStopWalking = GetTargetCloseContact(startPos, targetPos, minDistanceToTarget);

		Debug.Log("target position = " + targetPos);

		CustomNode startCustomNode = grid.NodeFromWorldPoint(startPos);
		//CustomNode targetCustomNode = grid.NodeFromWorldPoint(targetToStopWalking);
		CustomNode targetCustomNode = grid.NodeFromWorldPoint(targetPos);

		List<CustomNode> openSet = new List<CustomNode>();
		//HashSet<CustomNode> closedSet = new HashSet<CustomNode>();
		closedSet = new HashSet<CustomNode>();
		openSet.Add(startCustomNode);

		while (openSet.Count > 0)
		{
			// Aflarea nodului de suma minima din openSet (setul de explorare)
			CustomNode node = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{
				if (openSet[i].fCost <= node.fCost && openSet[i].hCost < node.hCost)
					node = openSet[i];
			}

			openSet.Remove(node);
			closedSet.Add(node);

			if (node == targetCustomNode)
			{
				RetracePath(startCustomNode, targetCustomNode);
				currentWaypoint = 0;
				SwitchHouseColliders.instance.SetCollidersActiveRecursively(target, true);
				drawGiz = true;
				return;
			}

			foreach (CustomNode neighbour in grid.GetNeighbours(node))
			{
				if (!neighbour.walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
				if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetCustomNode);
					neighbour.parent = node;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}

		Debug.Log("ajung aici");

		SwitchHouseColliders.instance.SetCollidersActiveRecursively(target, true);
		drawGiz = true;
	}

	void RetracePath(CustomNode startCustomNode, CustomNode endCustomNode)
	{
		List<CustomNode> path = new List<CustomNode>();
		CustomNode currentCustomNode = endCustomNode;

		int totalC = 0;

		bool reachedTargetToStopWalking = false;
		while (currentCustomNode != startCustomNode)
		{
			totalC++;
			if (!reachedTargetToStopWalking && Vector3.Distance(currentCustomNode.worldPosition, target.position) < minDistanceToTarget)
				reachedTargetToStopWalking = true;

			if (reachedTargetToStopWalking)
				path.Add(currentCustomNode);

			currentCustomNode = currentCustomNode.parent;
		}
		path.Reverse();

		grid.path = path;

	}

	int GetDistance(CustomNode nodeA, CustomNode nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}

	Vector3 GetTargetCloseContact(Vector3 seekerPos, Vector3 targetPos, float minDistanceToTarget)
    {
		float distanceToTarget = (targetPos - seekerPos).magnitude;

		// Too close to target (already in target range):
		if (distanceToTarget < minDistanceToTarget)
			return seekerPos;

		return new Vector3(
			targetPos.x + (minDistanceToTarget / distanceToTarget) * (seekerPos.x - targetPos.x),
			targetPos.y + (minDistanceToTarget / distanceToTarget) * (seekerPos.y - targetPos.y),
			targetPos.z + (minDistanceToTarget / distanceToTarget) * (seekerPos.z - targetPos.z)
		);
    }

    /*private void OnDrawGizmos()
    {
		if (closedSet == null || drawGiz == false) return;

		Gizmos.color = Color.red;
		foreach (CustomNode node in closedSet)
        {
			//Gizmos.DrawCube(node.worldPosition, 0.25f * Vector3.one);
        }

		//drawGiz = false;
    }*/
}
