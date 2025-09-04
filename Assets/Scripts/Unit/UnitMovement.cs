using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class UnitMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Pathfinding")]
    public float waypointTolerance = 0.1f;
    
    private Rigidbody2D _rb;
    private UnitSpriteController _spriteController;
    private Grid _grid;
    
    private Queue<Vector3> _path = new Queue<Vector3>();
    private Vector3 _currentWaypoint;
    private Vector3 _finalTargetPosition;
    private float _finalStoppingDistance;
    private static readonly List<Node> NodePool = new List<Node>();
    private static int _nodePoolIndex = 0;
    private static readonly Vector3Int[] NeighborOffsets = 
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)
    };
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteController = GetComponent<UnitSpriteController>();
    }

    private void Start()
    {
        _grid = BuildingManager.Instance.grid;
    }
    
    private void OnEnable()
    {
        BuildingManager.OnTilemapChanged += HandleTilemapChange;
    }
    
    private void OnDisable()
    {
        BuildingManager.OnTilemapChanged -= HandleTilemapChange;
    }

    private void OnDrawGizmosSelected()
    {
        if (_path.Count == 0) return;
        
        Gizmos.color = Color.yellow;
        Vector3 prevPos = transform.position;
        foreach (Vector3 waypoint in _path)
        {
            Gizmos.DrawLine(prevPos, waypoint);
            prevPos = waypoint;
        }
    }
    
    public bool SetNewTarget(Vector2 targetPosition, float stoppingDistance)
    {
        Vector3Int targetCellPos = _grid.WorldToCell(targetPosition);
        _finalTargetPosition = _grid.GetCellCenterWorld(targetCellPos);
        _finalStoppingDistance = stoppingDistance;

        Queue<Vector3> newPath = FindPath(transform.position, _finalTargetPosition);

        if (newPath != null && newPath.Count > 0)
        {
            _path = newPath;
            _currentWaypoint = _path.Dequeue();
            return true;
        }
        return false;
    }

    public void MoveToTarget()
    {
        if (_path.Count == 0 && _currentWaypoint == default)
        {
            StopMovement();
            return;
        }

        Vector3 direction = (_currentWaypoint - transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;
        _spriteController?.UpdateSpriteDirection(direction);

        float distanceToWaypoint = Vector3.Distance(transform.position, _currentWaypoint);
        
        if (distanceToWaypoint < waypointTolerance)
        {
            if (_path.Count > 0)
            {
                _currentWaypoint = _path.Dequeue();
            }
            else
            {
                _currentWaypoint = default;
                StopMovement();
            }
        }
    }

    public void StopMovement()
    {
        _rb.linearVelocity = Vector2.zero;
        _path.Clear();
        _currentWaypoint = default;
    }
    
    private void HandleTilemapChange(Vector3Int changedCellPosition)
    {
        if (_path.Count == 0 && _rb.linearVelocity == Vector2.zero) return;

        bool pathIsBlocked = _path.Any(waypoint => _grid.WorldToCell(waypoint) == changedCellPosition);
        if (_grid.WorldToCell(_currentWaypoint) == changedCellPosition)
        {
            pathIsBlocked = true;
        }
        
        if (pathIsBlocked)
        {
            SetNewTarget(_finalTargetPosition, _finalStoppingDistance);
        }
    }

    private Queue<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
    {
        _nodePoolIndex = 0; 
        
        Vector3Int startCell = _grid.WorldToCell(startPos);
        Vector3Int endCell = _grid.WorldToCell(endPos);

        List<Node> openList = new List<Node>();
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();

        Node startNode = GetNodeFromPool(startCell, null, 0, GetDistance(startCell, endCell));
        openList.Add(startNode);
        allNodes.Add(startCell, startNode);

        int iterations = 0;
        const int maxIterations = 2000;

        while (openList.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentNode.FCost || 
                    (Mathf.Approximately(openList[i].FCost, currentNode.FCost) && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.position);
            
            if (currentNode.position == endCell)
            {
                return ReconstructPath(currentNode);
            }

            foreach (Vector3Int offset in NeighborOffsets)
            {
                Vector3Int neighborPos = currentNode.position + offset;
                if (closedList.Contains(neighborPos)) continue;
                
                if (neighborPos != endCell && 
                    (BuildingManager.Instance.IsResourceTile(neighborPos) || BuildingManager.Instance.IsBuildingTile(neighborPos)))
                {
                    continue;
                }
                
                float newGCost = currentNode.gCost + GetDistance(currentNode.position, neighborPos);

                if (!allNodes.TryGetValue(neighborPos, out Node neighborNode))
                {
                    neighborNode = GetNodeFromPool(neighborPos, currentNode, newGCost, GetDistance(neighborPos, endCell));
                    allNodes.Add(neighborPos, neighborNode);
                    openList.Add(neighborNode);
                }
                else if (newGCost < neighborNode.gCost)
                {
                    neighborNode.parent = currentNode;
                    neighborNode.gCost = newGCost;
                }
            }
        }
        
        Debug.LogWarning("Path not found or search limit exceeded.");
        return new Queue<Vector3>();
    }

    private Queue<Vector3> ReconstructPath(Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;
        while (currentNode != null)
        {
            path.Add(_grid.GetCellCenterWorld(currentNode.position));
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return new Queue<Vector3>(path);
    }
    
    private float GetDistance(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx + dy;
    }

    private Node GetNodeFromPool(Vector3Int position, Node parent, float gCost, float hCost)
    {
        if (_nodePoolIndex >= NodePool.Count)
        {
            NodePool.Add(new Node());
        }
        
        Node node = NodePool[_nodePoolIndex++];
        node.position = position;
        node.parent = parent;
        node.gCost = gCost;
        node.hCost = hCost;
        return node;
    }

    private class Node
    {
        public Vector3Int position;
        public Node parent;
        public float gCost;
        public float hCost;
        public float FCost {
            get {
                return gCost + hCost;
            }
        }
    }
}