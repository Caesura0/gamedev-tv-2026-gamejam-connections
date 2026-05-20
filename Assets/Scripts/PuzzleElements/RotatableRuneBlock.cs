using System;
using UnityEngine;

public class RotatableRuneBlock : MonoBehaviour, IInteractable
{
    [SerializeField] private ConnectorShapeEnum connectorShape;
    [SerializeField] private int startingRotation = 0;
    [SerializeField] private bool isStartingInactive;

    private GridManager gridManager;
    private RunePowerSystem runePowerSystem;
    private Vector2Int gridPosition;
    private int currentRotation;
    private bool[] activeConnections;

    public bool[] ActiveConnections => activeConnections;
    public Vector2Int GridPosition => gridPosition;

    [SerializeField] private GameObject visual;

    private GameEventListener gameEventListener;

    void Start()
    {
        gridManager = GridManager.Instance;
        runePowerSystem = RunePowerSystem.Instance;
        gridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        currentRotation = startingRotation;
        activeConnections = Connections.Get(connectorShape, currentRotation);

        if (TryGetComponent(out gameEventListener))
            gameEventListener.OnFullConditionMet += OnConditionMet;

        if (isStartingInactive)
        {
            SetActive(false);
            return;
        }
        else
        {
            SetActive(true);
        }

    }

    private void OnConditionMet(bool obj)
    {
        SetActive(obj);
    }

    void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.RegisterRotatableRuneBlock(gridPosition.x, gridPosition.y, null);
            gridManager.RegisterInteractable(gridPosition.x, gridPosition.y, null);
            gridManager.SetCellMoveableOccupancy(gridPosition.x, gridPosition.y, false);
        }
    }

    // ── IInteractable ──

    public bool TryInteract(PlayerBehaviour player)
    {
        Rotate();
        return true;
    }

    public bool TryInteractAlternate(PlayerBehaviour player)
    {
        return false;
    }

    // ── Rotation ──

    void Rotate()
    {
        currentRotation = (currentRotation + 1) % 4;
        activeConnections = Connections.Get(connectorShape, currentRotation);

        Debug.Log(
            $"[ROTATE] {gameObject.name} rotated to {currentRotation}"
        );

        Debug.Log(
            $"[ROTATE] Connections N:{activeConnections[0]} E:{activeConnections[1]} S:{activeConnections[2]} W:{activeConnections[3]}"
        );

        transform.rotation = Quaternion.Euler(0f, 0f, -90f * currentRotation);

        runePowerSystem.RunEnergyThrough();
    }


    void SetActive(bool isActive)
    {
        visual.SetActive(isActive);

        if (isActive)
        {
            gridManager.RegisterRotatableRuneBlock(gridPosition.x, gridPosition.y, this);
            gridManager.RegisterInteractable(gridPosition.x, gridPosition.y, this);
            gridManager.SetCellMoveableOccupancy(gridPosition.x, gridPosition.y, true);
        }
        else
        {
            gridManager.RegisterRotatableRuneBlock(gridPosition.x, gridPosition.y, null);
            gridManager.RegisterInteractable(gridPosition.x, gridPosition.y, null);
            gridManager.SetCellMoveableOccupancy(gridPosition.x, gridPosition.y, true);
        }
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Show starting rotation connections in edit mode
            bool[] editConnections = Connections.Get(connectorShape, startingRotation);
            DrawConnectionGizmos(editConnections);
        }
        else
        {
            DrawConnectionGizmos(activeConnections);
        }
    }

    void DrawConnectionGizmos(bool[] connections)
    {
        if (connections == null) return;

        Vector3 center = transform.position;
        float size = 0.35f;

        // Direction vectors for N, E, S, W
        Vector3[] directions = new Vector3[]
        {
            Vector3.up,
            Vector3.right,
            Vector3.down,
            Vector3.left,
        };

        string[] labels = new string[] { "N", "E", "S", "W" };

        for (int i = 0; i < 4; i++)
        {
            if (connections[i])
            {
                // Active connection — bright cyan line with arrow
                Gizmos.color = new Color(0.0f, 1.0f, 0.9f, 1.0f);
                Gizmos.DrawLine(center, center + directions[i] * size);
                Gizmos.DrawSphere(center + directions[i] * size, 0.06f);
            }
            else
            {
                // Inactive connection — faint red dot
                Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.3f);
                Gizmos.DrawSphere(center + directions[i] * size, 0.04f);
            }

            UnityEditor.Handles.color = connections[i]
                ? new Color(0.0f, 1.0f, 0.9f, 0.9f)
                : new Color(1.0f, 0.2f, 0.2f, 0.3f);

            UnityEditor.Handles.Label(center + directions[i] * (size + 0.1f), labels[i]);
        }

        // Draw the shape name
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(center + Vector3.up * 0.6f, $"{connectorShape} r{(Application.isPlaying ? currentRotation : startingRotation)}");
    }
#endif
}