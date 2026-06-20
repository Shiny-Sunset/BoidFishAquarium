using UnityEngine;
using UnityEngine.EventSystems; // UI要素の判定のために必要

public class ObstaclePlacementController : MonoBehaviour
{
    [Header("障害物")]
    [Tooltip("配置可能な障害物のプレハブ配列")]
    public GameObject[] obstaclePrefabs;
    [Tooltip("プレビュー用の半透明なマテリアル")]
    public Material previewMaterial;
    [Tooltip("レイキャストに使用するカメラ")]
    public Camera placementCamera;
    [Tooltip("障害物を配置できる地面のレイヤー")]
    public LayerMask groundLayer;
    [Tooltip("選択可能な障害物のレイヤー")]
    public LayerMask selectableObstacleLayer; // 選択可能な障害物専用のレイヤーマスク
    public float scaleSensitivity = 0.1f; // スケール調整の感度
    public float minScale = 0.2f; // スケールの最小値
    public float maxScale = 10f; // スケールの最大値

    public static event System.Action<bool> OnPlacementModeChanged;

    private GameObject previewObstacle;
    private Material[] originalMaterials;
    private bool isCpuMode;
    private int selectedPrefabIndex = -1;
    private MovableObstacle selectedMovableObstacle;
    private bool isManipulating = false; // 移動/回転操作中かどうかのフラグ
    private Quaternion grabRotationOffset; // 潜水艦モードで掴んだ時の相対回転を保存

    private GameManager gameManager;

    void Awake()
    {
        gameManager = GameManager.Instance;
    }

    void OnEnable()
    {
        if (gameManager != null)
        {
            placementCamera = gameManager.topDownCamera;
        }
    }

    void Update()
    {
        if (gameManager != null)
        {
            if (gameManager.currentState == GameManager.GameState.Submarine)
            {
                placementCamera = gameManager.submarineCamera;
            }
            else
            {
                placementCamera = gameManager.topDownCamera;
            }
        }

        if (previewObstacle != null)
        {
            MovePreviewToMousePosition();
            ScalePreviewWithMouseScroll();
            PlaceObjectOnClick();
        }
        else
        {
            if (gameManager.currentState == GameManager.GameState.Setup || gameManager.currentState == GameManager.GameState.Simulating || gameManager.currentState == GameManager.GameState.Submarine)
            {
                HandleSelectionAndManipulation();
            }
        }
    }

    private void HandleSelectionAndManipulation()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Camera currentCamera = (gameManager.currentState == GameManager.GameState.Submarine) ? gameManager.submarineCamera : gameManager.topDownCamera;
        if (currentCamera == null) return;

        if (gameManager.currentState == GameManager.GameState.Submarine)
        {
            HandleSubmarineModeManipulation(currentCamera);
        }
        else
        {
            HandleTopDownModeManipulation(currentCamera);
        }
    }

    private void HandleTopDownModeManipulation(Camera currentCamera)
    {
        // --- クリック開始 --- 
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 200f, selectableObstacleLayer))
            {
                MovableObstacle clickedObstacle = hit.collider.GetComponentInParent<MovableObstacle>();
                
                // すでに選択されているオブジェクトをクリックした場合、操作を開始
                if (clickedObstacle != null && clickedObstacle == selectedMovableObstacle)
                {
                    isManipulating = true;
                    if (selectedMovableObstacle.currentToolMode == MovableObstacle.ToolMode.Move)
                    {
                        selectedMovableObstacle.StartMove(currentCamera);
                    }
                    else // Rotate Mode
                    {
                        selectedMovableObstacle.StartRotate();
                    }
                }
                else // 別のオブジェクトをクリックした場合、選択を切り替え
                {
                    SelectOrDeselectObstacle(clickedObstacle);
                }
            }
            else // 何もヒットしなかったら選択解除
            {
                SelectOrDeselectObstacle(null);
            }
        }

        // --- ドラッグ中 --- 
        if (Input.GetMouseButton(0) && isManipulating && selectedMovableObstacle != null)
        {
            if (selectedMovableObstacle.currentToolMode == MovableObstacle.ToolMode.Move)
            {
                selectedMovableObstacle.PerformMove(currentCamera);
            }
            else // Rotate Mode
            {
                selectedMovableObstacle.PerformRotate();
            }
        }

        // --- クリック終了 --- 
        if (Input.GetMouseButtonUp(0))
        {
            isManipulating = false;
        }

        // --- その他のキー入力 ---
        if (selectedMovableObstacle != null)
        {
            // Rキーでツールモードを切り替え
            if (Input.GetKeyDown(KeyCode.R))
            {
                selectedMovableObstacle.SetToolMode(
                    selectedMovableObstacle.currentToolMode == MovableObstacle.ToolMode.Move 
                    ? MovableObstacle.ToolMode.Rotate 
                    : MovableObstacle.ToolMode.Move
                );
            }

            // Xキーで選択中の障害物を削除
            if (Input.GetKeyDown(KeyCode.X))
            {
                Destroy(selectedMovableObstacle.gameObject);
                selectedMovableObstacle = null;
            }
        }

        // 右クリックで選択解除
        if (Input.GetMouseButtonDown(1))
        {
            SelectOrDeselectObstacle(null);
        }
    }

    private void HandleSubmarineModeManipulation(Camera currentCamera)
    {
        // (Submarine mode logic remains unchanged)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50f, selectableObstacleLayer))
            {
                MovableObstacle clickedObstacle = hit.collider.GetComponentInParent<MovableObstacle>();
                if (clickedObstacle != null)
                {
                    if (selectedMovableObstacle != null && selectedMovableObstacle != clickedObstacle)
                    {
                        selectedMovableObstacle.SetSelected(false);
                    }
                    selectedMovableObstacle = clickedObstacle;
                    selectedMovableObstacle.SetSelected(true);
                    selectedMovableObstacle.SetToolMode(MovableObstacle.ToolMode.Move);
                    // 掴んだ時のカメラとオブジェクトの相対的な回転を保存
                    grabRotationOffset = Quaternion.Inverse(currentCamera.transform.rotation) * selectedMovableObstacle.transform.rotation;
                }
                else
                {
                    SelectOrDeselectObstacle(null);
                }
            }
            else
            {
                SelectOrDeselectObstacle(null);
            }
        }

        if (selectedMovableObstacle != null && selectedMovableObstacle.IsSelected() && selectedMovableObstacle.currentToolMode == MovableObstacle.ToolMode.Move)
        {
            Vector3 targetPos = currentCamera.transform.position + currentCamera.transform.forward * 10f; // 10fは仮の距離
            selectedMovableObstacle.transform.position = Vector3.Lerp(selectedMovableObstacle.transform.position, targetPos, Time.deltaTime * 5f); // スムーズに移動

            // 掴んだ時の相対回転を維持しつつ、カメラの回転に合わせてオブジェクトを回転させる
            Quaternion targetRot = currentCamera.transform.rotation * grabRotationOffset;
            selectedMovableObstacle.transform.rotation = Quaternion.Slerp(selectedMovableObstacle.transform.rotation, targetRot, Time.deltaTime * 5f); // スムーズに回転
        }

        if (selectedMovableObstacle != null && Input.GetKeyDown(KeyCode.X))
        {
            Destroy(selectedMovableObstacle.gameObject);
            selectedMovableObstacle = null;
        }

        if (Input.GetMouseButtonDown(1))
        {
            SelectOrDeselectObstacle(null);
        }
    }

    private void SelectOrDeselectObstacle(MovableObstacle obstacleToSelect)
    {
        if (selectedMovableObstacle != null && selectedMovableObstacle != obstacleToSelect)
        {
            selectedMovableObstacle.SetSelected(false);
        }
        selectedMovableObstacle = obstacleToSelect;
        if (selectedMovableObstacle != null)
        {
            selectedMovableObstacle.SetSelected(true);
        }
    }

    public void SetMode(bool isCpu)
    {
        isCpuMode = isCpu;
    }

    public void SelectObstacle(int prefabIndex)
    {
        if (previewObstacle != null) Destroy(previewObstacle);
        if (prefabIndex < 0 || prefabIndex >= obstaclePrefabs.Length) return;
        selectedPrefabIndex = prefabIndex;
        previewObstacle = Instantiate(obstaclePrefabs[selectedPrefabIndex]);
        OnPlacementModeChanged?.Invoke(true);
        if (previewObstacle.GetComponent<Collider>() != null) previewObstacle.GetComponent<Collider>().enabled = false;
        var renderer = previewObstacle.GetComponent<Renderer>();
        if (renderer != null && previewMaterial != null)
        {
            originalMaterials = renderer.materials;
            renderer.material = previewMaterial;
        }
    }

    private void MovePreviewToMousePosition()
    {
        Ray ray = placementCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            var renderer = previewObstacle.GetComponent<Renderer>();
            if (renderer != null)
            {
                var bounds = renderer.bounds;
                float offsetMagnitude = Vector3.Dot(bounds.extents, new Vector3(Mathf.Abs(hit.normal.x), Mathf.Abs(hit.normal.y), Mathf.Abs(hit.normal.z)));
                previewObstacle.transform.position = hit.point + hit.normal * offsetMagnitude;
            }
            else
            {
                previewObstacle.transform.position = hit.point;
            }
        }
    }

    private void ScalePreviewWithMouseScroll()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            Vector3 newScale = previewObstacle.transform.localScale + Vector3.one * scrollInput * scaleSensitivity;
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
            newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);
            previewObstacle.transform.localScale = newScale;
        }
    }

    private void PlaceObjectOnClick()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var placedObject = previewObstacle;
            var renderer = placedObject.GetComponent<Renderer>();
            if (renderer != null) renderer.materials = originalMaterials;
            placedObject.GetComponent<Collider>().enabled = true;
            MovableObstacle movable = placedObject.AddComponent<MovableObstacle>();
            movable.selectedMaterial = previewMaterial;
            placedObject.transform.SetParent(GameObject.Find("_Environment")?.transform);
            previewObstacle = null;
            originalMaterials = null;
            if (isCpuMode) SelectObstacle(selectedPrefabIndex);
        }
        if (Input.GetMouseButtonDown(1)) StopPlacementAndClearPreviews();
    }

    public void StopPlacementAndClearPreviews()
    {
        if (previewObstacle != null)
        {
            Destroy(previewObstacle);
            previewObstacle = null;
            originalMaterials = null;
            selectedPrefabIndex = -1;
            OnPlacementModeChanged?.Invoke(false);
        }
    }
}