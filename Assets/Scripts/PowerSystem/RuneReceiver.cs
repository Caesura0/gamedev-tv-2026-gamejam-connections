using UnityEngine;

public class RuneReceiver : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite unpoweredSprite;
    [SerializeField] private Sprite poweredSprite;

    [SerializeField] private SpriteRenderer visualSpriteRenderer;

    private GridManager gridManager;
    private Vector2Int gridPosition;

    public Vector2Int GridPosition => gridPosition;

    void Start()
    {
        gridManager = GridManager.Instance;
        gridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        gridManager.RegisterReceiver(gridPosition.x, gridPosition.y, this);

        visualSpriteRenderer.sprite = unpoweredSprite;
    }

    void OnDestroy()
    {
        if (gridManager != null)
            gridManager.RegisterReceiver(gridPosition.x, gridPosition.y, null);
    }

    public void SetPowered(bool powered)
    {
        visualSpriteRenderer.sprite = powered ? poweredSprite : unpoweredSprite;
    }

    public bool TryInteract(PlayerBehaviour player) => false;
    public bool TryInteractAlternate(PlayerBehaviour player) => false;
}