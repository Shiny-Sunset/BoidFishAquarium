using UnityEngine;

public class SubmarineController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("移動速度")]
    public float moveSpeed = 15f;

    [Header("視点操作設定")]
    [Tooltip("マウスの感度")]
    public float mouseSensitivity = 100f;
    [Tooltip("操作対象の潜水艦カメラ")]
    public Camera submarineCamera; // インスペクターから設定

    [Header("インタラクション設定")]
    [Tooltip("射出する餌のプレハブ")]
    public GameObject foodPrefab;
    [Tooltip("CPUモードで射出する障害物のプレハブ")]
    public GameObject obstaclePrefab;
    [Tooltip("餌や障害物を射出する位置")]
    public Transform shootPoint;
    [Header("判定用設定")]
    [Tooltip("障害物として認識するレイヤー")]
    public LayerMask obstacleLayer;

    // MovableObstacle設定用の変数を追加
    public Material selectedMaterial;
    public LayerMask groundLayer;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovementAndLook();
        HandleInteractions();
    }

    // レイヤーを再帰的に設定するヘルパーメソッド
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void HandleMovementAndLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        // 潜水艦の回転を適用
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // カメラは潜水艦の回転に追従するため、ローカル回転はリセット
        if (submarineCamera != null)
        {
            submarineCamera.transform.localRotation = Quaternion.identity;
        }

        float forwardInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        Vector3 forwardDirection = submarineCamera.transform.forward;
        Vector3 rightDirection = submarineCamera.transform.right;

        Vector3 moveDirection = (forwardDirection * forwardInput + rightDirection * horizontalInput).normalized;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleInteractions()
    {
        // Fキーで餌を射出 (全モード共通)
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (shootPoint == null || foodPrefab == null) return;

            if (IsPlacementValid(shootPoint.position))
            {
                // 有効な場所なら、通常通り餌やり処理を実行
                if (GameManager.Instance.currentMode == GameManager.ExecutionMode.CPU)
                {
                    GameObject newFood = Instantiate(foodPrefab, shootPoint.position, shootPoint.rotation);
                    GameManager.Instance.boidManager.RegisterFood(newFood);
                }
                else // GPUモード
                {
                    // GameManager.Instance.gpuController.RegisterFood(shootPoint.position);
                }
            }
            else
            {
                // デバッグ用に、無効な場所だったことをログに表示
                Debug.Log("水槽の外、または障害物の内部のため餌を設置できません。");
            }
        }

        // Gキーで障害物を射出 (CPUモードのみ)
        if (GameManager.Instance.currentMode == GameManager.ExecutionMode.CPU)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (shootPoint == null || obstaclePrefab == null) return;

                if (IsPlacementValid(shootPoint.position))
                {
                    if (obstaclePrefab != null)
                    {
                        GameObject newObstacle = Instantiate(obstaclePrefab, shootPoint.position, shootPoint.rotation);

                        // MovableObstacleコンポーネントを追加して設定
                        MovableObstacle movable = newObstacle.AddComponent<MovableObstacle>();
                        movable.selectedMaterial = selectedMaterial;
                        

                        

                        // _Environmentの子にする
                        newObstacle.transform.SetParent(GameObject.Find("_Environment")?.transform);
                    }
                }
                else
                {
                    // デバッグ用に、無効な場所だったことをログに表示
                    Debug.Log("水槽の外、または障害物の内部のため障害物を設置できません。");
                }
            }
        }
    }

    /// <summary>
    /// 指定された位置が、餌や障害物を設置するのに有効な場所かチェックします。
    /// </summary>
    /// <param name="position">チェックしたいワールド座標</param>
    /// <returns>有効な場所ならtrueを返す</returns>
    private bool IsPlacementValid(Vector3 position)
    {
        // --- ① 水槽の範囲内かチェック ---
        Vector3 bounds = GameManager.Instance.GetCurrentBounds();
        Vector3 halfBounds = bounds / 2f;
        // （水槽の中心が原点(0,0,0)であると仮定）
        if (Mathf.Abs(position.x) > halfBounds.x ||
            Mathf.Abs(position.y) > halfBounds.y ||
            Mathf.Abs(position.z) > halfBounds.z)
        {
            return false; // 範囲外なら無効
        }


        // --- ② 障害物の中ではないかチェック ---
        // 指定した位置に小さな球を作り、障害物レイヤーのコライダーと重なっていないか調べる
        if (Physics.CheckSphere(position, 0.1f, obstacleLayer))
        {
            return false; // 重なっていたら（＝障害物の中なら）無効
        }

        // 全てのチェックをパスしたら有効
        return true;
    }
}