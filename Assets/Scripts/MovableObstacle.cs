using UnityEngine;

public class MovableObstacle : MonoBehaviour
{
    public Material selectedMaterial;
    private Material originalMaterial;
    private Renderer objectRenderer;

    private bool isSelected = false;

    // --- Manipulation variables ---
    private Plane movementPlane;
    private float distanceToCamera;
    private Vector3 offset;
    private float initialMouseX;
    private float initialMouseY;

    public enum ToolMode { Move, Rotate }
    public ToolMode currentToolMode = ToolMode.Move;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (objectRenderer != null)
        {
            objectRenderer.material = isSelected ? selectedMaterial : originalMaterial;
        }
        if (!isSelected)
        {
            currentToolMode = ToolMode.Move;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void SetToolMode(ToolMode mode)
    {
        currentToolMode = mode;
        Debug.Log($"Tool Mode for {gameObject.name}: {currentToolMode}");
    }

    // --- Public Manipulation Methods ---

    public void StartMove(Camera camera)
    {
        movementPlane = new Plane(camera.transform.forward, transform.position);
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (movementPlane.Raycast(ray, out distanceToCamera))
        {
            offset = transform.position - ray.GetPoint(distanceToCamera);
        }
    }

    public void PerformMove(Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (movementPlane.Raycast(ray, out distanceToCamera))
        {
            transform.position = ray.GetPoint(distanceToCamera) + offset;
        }
    }

    public void StartRotate()
    {
        initialMouseX = Input.mousePosition.x;
        initialMouseY = Input.mousePosition.y;
    }

    public void PerformRotate()
    {
        float deltaX = Input.mousePosition.x - initialMouseX;
        float deltaY = Input.mousePosition.y - initialMouseY;

        transform.Rotate(Vector3.up, deltaX * 0.5f, Space.World);
        transform.Rotate(Vector3.right, -deltaY * 0.5f, Space.Self);

        initialMouseX = Input.mousePosition.x;
        initialMouseY = Input.mousePosition.y;
    }
}