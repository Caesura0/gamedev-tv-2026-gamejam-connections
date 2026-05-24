using UnityEngine;

public class PressurePlateVisual : MonoBehaviour
{
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;

    private SpriteRenderer spriteRenderer;
    private int column, row;

    public void Initialise(int column, int row)
    {
        this.column = column;
        this.row = row;

        spriteRenderer = GetComponent<SpriteRenderer>();

        GridManager.Instance.OnPressurePlateStateChanged += HandleStateChanged;

        GroundTileData tile = GridManager.Instance.GetTileAt(column, row);
        if (tile != null)
            SetSprite(tile.IsPressurePlateActivated);
    }

    void OnDestroy()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.OnPressurePlateStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(int col, int r, bool isActivated)
    {
        if (col == column && r == row)
            SetSprite(isActivated);
    }

    void SetSprite(bool isActivated)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sprite = isActivated ? activeSprite : inactiveSprite;
    }
}