using UnityEngine;

public class RuneBeamVisual : MonoBehaviour
{
    [SerializeField] private GameObject horizontalBeamPrefab;
    [SerializeField] private GameObject verticalBeamPrefab;

    [SerializeField] private float horizontalBeamOffsetY = 0f;
    [SerializeField] private float verticalBeamOffsetX = 0f;

    private GameObject horizontalBeamInstance;
    private GameObject verticalBeamInstance;

    private Vector2Int gridPosition;
    private GridManager gridManager;

    public Vector2Int GridPosition => gridPosition;

    void Awake()
    {
        horizontalBeamInstance = Instantiate(horizontalBeamPrefab, transform);
        horizontalBeamInstance.name = "Beam_Horizontal";
        horizontalBeamInstance.transform.localPosition = new Vector3(0f, horizontalBeamOffsetY, 0f);
        horizontalBeamInstance.SetActive(false);

        verticalBeamInstance = Instantiate(verticalBeamPrefab, transform);
        verticalBeamInstance.name = "Beam_Vertical";
        verticalBeamInstance.transform.localPosition = new Vector3(verticalBeamOffsetX, 0f, 0f);
        verticalBeamInstance.SetActive(false);
    }

    void Start()
    {
        gridManager = GridManager.Instance;
        gridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        gridManager.RegisterBeamVisual(gridPosition.x, gridPosition.y, this);
    }

    void OnDestroy()
    {
        if (gridManager != null)
            gridManager.RegisterBeamVisual(gridPosition.x, gridPosition.y, null);
    }

    public void SetPowered(bool horizontalBeamShouldBeActive, bool verticalBeamShouldBeActive)
    {
        horizontalBeamInstance.SetActive(horizontalBeamShouldBeActive);
        verticalBeamInstance.SetActive(verticalBeamShouldBeActive);
    }
}