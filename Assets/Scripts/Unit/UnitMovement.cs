using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class UnitMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Pathfinding")]
    public float waypointTolerance = 0.1f;

    private Vector2 _currentWaypoint;
    private Queue<Vector2> _path = new Queue<Vector2>();
    private Rigidbody2D _rb;
    private UnitSpriteController _spriteController;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteController = GetComponent<UnitSpriteController>();
    }

    private void OnDrawGizmosSelected()
    {
        if (_path.Count > 0) {
            Vector2[] pathArray = _path.ToArray();
            Gizmos.color = Color.yellow;
            Vector2 prevPos = transform.position;
            foreach (Vector2 waypoint in pathArray) {
                Gizmos.DrawLine(prevPos, waypoint);
                prevPos = waypoint;
            }
        }
    }

    public void SetNewTarget(Vector2 targetPosition)
    {
        _path = FindPath(transform.position, targetPosition);

        if (_path.Count > 0) {
            _currentWaypoint = _path.Dequeue();
        }
    }

    public void StopMovement()
    {
        _rb.linearVelocity = Vector2.zero;
        _path.Clear();
    }

    public void MoveToTarget()
    {
        if (_path.Count == 0) {
            StopMovement();
            return;
        }

        Vector2 direction = (_currentWaypoint - (Vector2)transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;
        _spriteController?.UpdateSpriteDirection(direction);

        if (Vector2.Distance(transform.position, _currentWaypoint) < waypointTolerance) {
            if (_path.Count > 0) {
                _currentWaypoint = _path.Dequeue();
            }
            else {
                StopMovement();
                transform.position = GetGridPosition(transform.position);
            }
        }
    }

    public Vector2 GetGridPosition(Vector2 position)
    {
        return new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
    }

    private Queue<Vector2> FindPath(Vector2 startPos, Vector2 endPos)
    {
        Vector2 startGridPos = GetGridPosition(startPos);
        Vector2 endGridPos = GetGridPosition(endPos);

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<Vector2> closedList = new HashSet<Vector2>();

        Node startNode = new Node { Position = startGridPos, GCost = 0, HCost = Vector2.Distance(startGridPos, endGridPos) };
        openList.Enqueue(startNode, startNode.FCost);

        while (openList.Count > 0) {
            Node currentNode = openList.Dequeue();

            if (currentNode.Position == endGridPos) {
                return ReconstructPath(currentNode);
            }

            closedList.Add(currentNode.Position);

            foreach (Vector2 neighborPos in GetNeighbors(currentNode.Position)) {
                if (closedList.Contains(neighborPos)) continue;

                float newGCost = currentNode.GCost + Vector2.Distance(currentNode.Position, neighborPos);

                Node neighborNode = new Node { Position = neighborPos, GCost = newGCost, HCost = Vector2.Distance(neighborPos, endGridPos), Parent = currentNode };
                openList.Enqueue(neighborNode, neighborNode.FCost);
            }
        }

        return new Queue<Vector2>();
    }

    private Queue<Vector2> ReconstructPath(Node endNode)
    {
        Queue<Vector2> path = new Queue<Vector2>();
        Node currentNode = endNode;
        while (currentNode != null) {
            path.Enqueue(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        Vector2[] pathArray = path.ToArray();
        Array.Reverse(pathArray);

        return new Queue<Vector2>(pathArray);
    }

    private IEnumerable<Vector2> GetNeighbors(Vector2 position)
    {
        yield return new Vector2(position.x + 1, position.y);
        yield return new Vector2(position.x - 1, position.y);
        yield return new Vector2(position.x, position.y + 1);
        yield return new Vector2(position.x, position.y - 1);
        yield return new Vector2(position.x + 1, position.y + 1);
        yield return new Vector2(position.x + 1, position.y - 1);
        yield return new Vector2(position.x - 1, position.y + 1);
        yield return new Vector2(position.x - 1, position.y - 1);
    }

    private class Node
    {
        public float GCost;
        public float HCost;
        public Node Parent;
        public Vector2 Position;

        public float FCost {
            get {
                return GCost + HCost;
            }
        }
    }
}
