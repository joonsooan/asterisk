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
    public Grid grid;

    private Vector3 _currentWaypoint;
    private Queue<Vector3> _path = new Queue<Vector3>();
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
            Vector3[] pathArray = _path.ToArray();
            Gizmos.color = Color.yellow;
            Vector3 prevPos = transform.position;
            foreach (Vector3 waypoint in pathArray) {
                Gizmos.DrawLine(prevPos, waypoint);
                prevPos = waypoint;
            }
        }
    }

    public void SetNewTarget(Vector2 targetPosition)
    {
        Vector3Int targetCellPos = grid.WorldToCell(targetPosition);
        Vector3 finalTargetPos = grid.GetCellCenterWorld(targetCellPos);

        _path = FindPath(transform.position, finalTargetPos);

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

        Vector3 direction = (_currentWaypoint - transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;
        _spriteController?.UpdateSpriteDirection(direction);

        if (Vector3.Distance(transform.position, _currentWaypoint) < waypointTolerance) {
            if (_path.Count > 0) {
                _currentWaypoint = _path.Dequeue();
            }
            else {
                StopMovement();
            }
        }
    }

    private Queue<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
    {
        Vector3Int startCellPos = grid.WorldToCell(startPos);
        Vector3Int endCellPos = grid.WorldToCell(endPos);

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();

        Node startNode = new Node { Position = startCellPos, GCost = 0, HCost = GetDistance(startCellPos, endCellPos) };
        allNodes.Add(startCellPos, startNode);
        openList.Enqueue(startNode, startNode.FCost);

        while (openList.Count > 0) {
            Node currentNode = openList.Dequeue();

            if (currentNode.Position == endCellPos) {
                return ReconstructPath(currentNode);
            }

            foreach (Vector3Int neighborPos in GetNeighbors(currentNode.Position)) {
                // 인접 노드가 장애물인지 확인하는 로직 추가 필요 (예: grid.HasTile(neighborPos) 등)
                // if (isObstacle(neighborPos)) continue; 

                Node neighborNode;
                float newGCost = currentNode.GCost + GetDistance(currentNode.Position, neighborPos);

                if (allNodes.TryGetValue(neighborPos, out neighborNode)) {
                    if (newGCost < neighborNode.GCost) {
                        neighborNode.Parent = currentNode;
                        neighborNode.GCost = newGCost;
                        neighborNode.HCost = GetDistance(neighborPos, endCellPos);

                        openList.Enqueue(neighborNode, neighborNode.FCost);
                    }
                }
                else {
                    neighborNode = new Node {
                        Position = neighborPos,
                        GCost = newGCost,
                        HCost = GetDistance(neighborPos, endCellPos),
                        Parent = currentNode
                    };
                    allNodes.Add(neighborPos, neighborNode);
                    openList.Enqueue(neighborNode, neighborNode.FCost);
                }
            }
        }

        return new Queue<Vector3>();
    }

    private float GetDistance(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return Mathf.Max(dx, dy);
    }

    private Queue<Vector3> ReconstructPath(Node endNode)
    {
        Queue<Vector3> path = new Queue<Vector3>();
        Node currentNode = endNode;
        while (currentNode != null) {
            path.Enqueue(grid.GetCellCenterWorld(currentNode.Position));
            currentNode = currentNode.Parent;
        }

        Vector3[] pathArray = path.ToArray();
        Array.Reverse(pathArray);

        return new Queue<Vector3>(pathArray);
    }

    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int position)
    {
        yield return new Vector3Int(position.x + 1, position.y, 0);
        yield return new Vector3Int(position.x - 1, position.y, 0);
        yield return new Vector3Int(position.x, position.y + 1, 0);
        yield return new Vector3Int(position.x, position.y - 1, 0);
        yield return new Vector3Int(position.x + 1, position.y + 1, 0);
        yield return new Vector3Int(position.x + 1, position.y - 1, 0);
        yield return new Vector3Int(position.x - 1, position.y + 1, 0);
        yield return new Vector3Int(position.x - 1, position.y - 1, 0);
    }

    private class Node
    {
        public float GCost;
        public float HCost;
        public Node Parent;
        public Vector3Int Position;

        public float FCost {
            get {
                return GCost + HCost;
            }
        }
    }
}
