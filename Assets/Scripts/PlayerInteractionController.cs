using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("設定")]
    public Camera topDownCamera;
    public LayerMask groundLayer;
    public GameObject foodPrefab;

    [Header("インタラクション設定")]
    public float scareRadius = 15f;
    public float scareForce = 50f;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 通常モードでのみFキーで餌を出す
        if (gameManager.currentState != GameManager.GameState.Submarine && Input.GetKeyDown(KeyCode.F))
        {
            HandleFeeding();
        }

        // Gキーで魚を驚かす (潜水艦モード以外)
        if (gameManager.currentState != GameManager.GameState.Submarine && Input.GetKeyDown(KeyCode.G))
        {
            HandleScaring();
        }

        
    }

    private void HandleFeeding()
    {
        Camera currentCamera = gameManager.topDownCamera;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
        {
            Vector3 foodPosition = hit.point;

            if (gameManager.currentMode == GameManager.ExecutionMode.CPU)
            {
                if (foodPrefab != null)
                {
                    GameObject newFood = Instantiate(foodPrefab, foodPosition, Quaternion.identity);
                    gameManager.boidManager.RegisterFood(newFood);
                }
            }
        }
    }

    private void HandleScaring()
    {
        Camera currentCamera = (gameManager.currentState == GameManager.GameState.Submarine) ? gameManager.submarineCamera : gameManager.topDownCamera;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            Vector3 scarePoint = hit.point;

            if (gameManager.currentMode == GameManager.ExecutionMode.CPU)
            {
                gameManager.boidManager.ScareBoidsAt(scarePoint, scareRadius, scareForce);
            }
        }
    }
}
