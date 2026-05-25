using System;
using UnityEngine;

public class RotatableRuneBlock : MonoBehaviour, IInteractable
{
    [SerializeField] private ConnectorShapeEnum connectorShape;
    [SerializeField] private int startingRotation = 0;
    [SerializeField] private bool isStartingInactive;
    [SerializeField] private bool isLit;
    [SerializeField] private Sprite[] rotationElbowSpritesUnlit;
    [SerializeField] private Sprite[] rotationElbowSpritesLit;
    //[SerializeField] private Sprite[] rotationNEUnlitRed;
    private float lastRotateTime = -Mathf.Infinity;
    [SerializeField] private float rotateCooldown = 0.3f;

    private GridManager gridManager;
    private RunePowerSystem runePowerSystem;
    private Vector2Int gridPosition;
    private int currentRotation;
    private bool[] activeConnections;

    public bool[] ActiveConnections => activeConnections;
    public Vector2Int GridPosition => gridPosition;

    [SerializeField] private GameObject visual;

    // Optional: assign a SpriteRenderer in the inspector. If left null the script will try to find one on `visual` or this GameObject.
    [SerializeField] private SpriteRenderer visualSpriteRenderer;


    private GameEventListener gameEventListener;

    void Start()
    {
        gridManager = GridManager.Instance;
        runePowerSystem = RunePowerSystem.Instance;
        gridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        currentRotation = startingRotation;
        SetSprite(currentRotation); 
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

    public void SetPowered(bool powered)
    {
        if (isLit == powered) return; // no change, skip sprite swap
        isLit = powered;
        SetSprite(currentRotation);
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
        if (Time.time - lastRotateTime < rotateCooldown) return;
        lastRotateTime = Time.time;

        currentRotation = (currentRotation + 1) % 4;
        activeConnections = Connections.Get(connectorShape, currentRotation);

        Debug.Log($"[ROTATE] {gameObject.name} rotated to {currentRotation}");
        Debug.Log($"[ROTATE] Connections N:{activeConnections[0]} E:{activeConnections[1]} S:{activeConnections[2]} W:{activeConnections[3]}");

        SetSprite(currentRotation);
        AudioManager.Instance.PlayRotateRune();

        runePowerSystem.RunEnergyThrough();
    }

    void SetSprite(int rotation)
    {
        Sprite[] sprites = isLit ? rotationElbowSpritesLit : rotationElbowSpritesUnlit;

        // Guard: fall back to unlit if lit array isn't populated yet
        if (sprites == null || sprites.Length == 0)
            sprites = rotationElbowSpritesUnlit;

        int index = Mathf.Clamp(rotation, 0, sprites.Length - 1);
        SetSprite(sprites[index]);
    }

    ///// <summary>
    ///// Sets the sprite on this object's SpriteRenderer. The script will use the serialized
    ///// <see cref="visualSpriteRenderer"/> if assigned, otherwise it will try to find a
    ///// SpriteRenderer on the `visual` GameObject or on this GameObject.
    ///// </summary>
    ///// <param name="sprite">The sprite to set. If null the operation is ignored.</param>
    public void SetSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"[{nameof(RotatableRuneBlock)}] SetSprite called with null sprite on '{gameObject.name}'.");
            return;
        }

        if (visualSpriteRenderer == null)
        {
            if (visual != null)
                visualSpriteRenderer = visual.GetComponent<SpriteRenderer>();
            if (visualSpriteRenderer == null)
                visualSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (visualSpriteRenderer == null)
        {
            Debug.LogWarning($"[{nameof(RotatableRuneBlock)}] No SpriteRenderer found on '{gameObject.name}'.");
            return;
        }

        visualSpriteRenderer.sprite = sprite;
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
        float size = 0.7f;

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
                Gizmos.DrawSphere(center + directions[i] * size, 0.24f);
            }
            else
            {
                // Inactive connection — faint red dot
                Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.3f);
                Gizmos.DrawSphere(center + directions[i] * size, 0.18f);
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