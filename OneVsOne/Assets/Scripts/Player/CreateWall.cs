using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CreateWall : MonoBehaviour
{
    [Header("Stats")]
    public SO_Walls wallLibrary;
    [SerializeField] float spawnRange = 2f;
    [SerializeField] float srX = 0;
    [SerializeField] float srY = 0;
    [SerializeField] float srZ = 0;

    [Header("Preview Setup")]
    [SerializeField] Material previewMaterial; // Drag your transparent ghost material here in the Inspector

    private GameObject objectToSpawn;
    private GameObject previewInstance;
    private bool isHoldingButton = false;
    
    // We store these variables so Update can track them, and Release can use them
    private Vector3 finalSpawnPosition;
    private Quaternion finalSpawnRotation;

    private void Start() {
        if (wallLibrary != null)
        {
            objectToSpawn = wallLibrary.Testwall;
        }
    }

    private void Update()
    {
        // Every frame the button is held down, update the placement math and move the preview object
        if (isHoldingButton)
        {
            CalculateWallPlacement();
            UpdatePreviewPosition();
        }
    }

    void OnCreateWall(InputValue value)
    {
        // 1. BUTTON PRESSED DOWN -> Start previewing
        if (value.isPressed)
        {
            isHoldingButton = true;
            CreatePreviewObject();
        }
        // 2. BUTTON RELEASED -> Destroy preview and spawn actual solid wall
        else
        {
            isHoldingButton = false;
            BuildActualWall();
        }
    }

    void CalculateWallPlacement()
    {
        if (objectToSpawn == null) return;

        // Get the Main Camera's forward direction and flatten it on the Y axis
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // Calculate base position and rotation using the flat camera direction
        Vector3 startPos = this.transform.position;
        Vector3 offset = cameraForward * spawnRange;
        finalSpawnPosition = startPos + offset;
        
        // Face the camera direction, then apply your custom inspector offsets
        finalSpawnRotation = Quaternion.LookRotation(cameraForward) * Quaternion.Euler(srX, srY, srZ);

        // --- SNAP LOGIC ---
        BoxCollider wallCollider = objectToSpawn.GetComponent<BoxCollider>();
        if (wallCollider != null)
        {
            float wallWidth = wallCollider.size.x * objectToSpawn.transform.localScale.x;
            Vector3 halfExtents = wallCollider.size / 2f; 

            int safetyCounter = 0;
            // Loop and shift the position forward along the camera view line if space is occupied
            while (Physics.CheckBox(finalSpawnPosition, halfExtents, finalSpawnRotation) && safetyCounter < 10)
            {
                finalSpawnPosition += cameraForward * wallWidth;
                safetyCounter++; 
            }
        }
    }

    void CreatePreviewObject()
    {
        if (objectToSpawn == null || previewInstance != null) return;

        // Run calculation once immediately so it doesn't flash at (0,0,0) for a frame
        CalculateWallPlacement();

        // Spawn a clone to use as our visual tracker
        previewInstance = Instantiate(objectToSpawn, finalSpawnPosition, finalSpawnRotation);

        // Remove the collider on the preview so it doesn't mess with physics or block future placement loops
        if (previewInstance.TryGetComponent<Collider>(out Collider col))
        {
            Destroy(col);
        }

        // Apply your see-through preview material to the ghost object
        if (previewMaterial != null)
        {
            Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material = previewMaterial;
            }
        }
    }

    void UpdatePreviewPosition()
    {
        if (previewInstance != null)
        {
            previewInstance.transform.position = finalSpawnPosition;
            previewInstance.transform.rotation = finalSpawnRotation;
        }
    }

    void BuildActualWall()
    {
        // Destroy the ghost outline
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        if (objectToSpawn == null) return;

        // Instantiate the final solid wall at the exact calculated preview coordinates
        Instantiate(objectToSpawn, finalSpawnPosition, finalSpawnRotation);
        Debug.Log("Wall spawned on release");
    }
}