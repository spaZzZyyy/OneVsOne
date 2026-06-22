using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CreateWall : MonoBehaviour
{
    [Header("Stats")]
    public SO_Walls wallLibrary;
    float spawnRange = 2f;
    [SerializeField] float MaxWallSpawnRange = 10f;
    [SerializeField] float MinWallSpawnRange = 2f;
    [SerializeField] float srX = 0f;
    [SerializeField] float srY = 0f;
    [SerializeField] float srZ = 0f;

    [Header("Preview & Highlight Setup")]
    [SerializeField] Material previewMaterial;     // Transparent ghost material for building
    [SerializeField] Material highlightMaterial;   // Red/Glowing material for deleting
    [SerializeField] float maxDeleteDistance = 10f;

    private GameObject objectToSpawn;
    private GameObject previewInstance;
    private GameObject highlightInstance;          // The visual ghost frame for deletion
    private GameObject currentTargetedWall;        // Tracks which wall we are looking at
    private bool isHoldingButton = false;
    
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
        // 1. Handle Building Preview
        if (isHoldingButton)
        {
            ClearHighlight(); // Don't show delete highlight while trying to build
            CalculateWallPlacement();
            UpdatePreviewPosition();
        }
        // 2. Handle Deletion Highlight (Only look for targets if we aren't currently building)
        else
        {
            CheckForDeletionTarget();
        }
    }



    void OnCreateWall(InputValue value)
    {
        if (value.isPressed)
        {
            isHoldingButton = true;
            CreatePreviewObject();
        }
        else
        {
            isHoldingButton = false;
            BuildActualWall();
        }
    }

    void OnWallPlacement(InputValue value)
    {
        Vector2 scrollVector = value.Get<Vector2>();
        float scrollDelta = scrollVector.y;

        if (scrollDelta != 0)
        {
            float speedMultiplier = 0.5f; 
            spawnRange += Mathf.Sign(scrollDelta) * speedMultiplier;
            spawnRange = Mathf.Clamp(spawnRange, MinWallSpawnRange, MaxWallSpawnRange);
        }
    }

    void OnDeleteWall(InputValue value)
    {
        // If we are looking at a valid wall and press delete, destroy it
        if (value.isPressed && currentTargetedWall != null)
        {
            Destroy(currentTargetedWall);
            ClearHighlight(); // Instantly clean up the floating highlight mesh
        }
    }

    void CheckForDeletionTarget()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxDeleteDistance))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if what we hit is a wall (and make sure it isn't an active placement preview)
            if (hitObject.GetComponent<BoxCollider>() != null && hitObject != previewInstance && hitObject.tag == "WallMat")
            {
                // If we switched to a NEW wall, clean up old references and build a new highlight
                if (hitObject != currentTargetedWall)
                {
                    ClearHighlight();
                    currentTargetedWall = hitObject;
                    CreateHighlightObject(currentTargetedWall);
                }
                return; // Target found, exit method cleanly
            }
        }

        // If the raycast hits nothing or non-walls, remove the highlight
        ClearHighlight();
    }

    void CreateHighlightObject(GameObject targetWall)
    {
        if (highlightMaterial == null) return;

        // Duplicate the targeted wall to get a perfect overlay copy
        highlightInstance = Instantiate(targetWall, targetWall.transform.position, targetWall.transform.rotation);

        // Strip the collider off the duplicate highlight so it doesn't break player physics/raycasts
        if (highlightInstance.TryGetComponent<Collider>(out Collider col))
        {
            Destroy(col);
        }

        // Apply our custom highlight material (e.g., translucent bright red or a wireframe outline)
        Renderer[] renderers = highlightInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.material = highlightMaterial;
        }
    }

    void ClearHighlight()
    {
        if (highlightInstance != null)
        {
            Destroy(highlightInstance);
        }
        currentTargetedWall = null;
    }

    // --- YOUR EXISTING BUILD MATH METHODS ---

    void CalculateWallPlacement()
    {
        if (objectToSpawn == null) return;

        // 1. Get the Main Camera's forward direction and flatten it on the Y axis
        Vector3 cameraForward = Camera.main.transform.forward;
        //cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 startPos = this.transform.position;
        Vector3 offset = cameraForward * spawnRange;
        finalSpawnPosition = startPos + offset;
        
        finalSpawnRotation = Quaternion.LookRotation(cameraForward) * Quaternion.Euler(srX, srY, srZ);

        BoxCollider wallCollider = objectToSpawn.GetComponent<BoxCollider>();
        if (wallCollider != null)
        {
            // Get absolute local size boundaries adjusted by world scale
            float wallWidth = wallCollider.size.x * objectToSpawn.transform.localScale.x;
            float wallLength = wallCollider.size.z * objectToSpawn.transform.localScale.z;
            Vector3 halfExtents = wallCollider.size / 2f; 

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, spawnRange + 5f))
            {
                if (hit.collider.gameObject.GetComponent<BoxCollider>() != null && hit.collider.gameObject != previewInstance && hit.collider.gameObject.tag == "WallMat")
                {
                    GameObject hitWall = hit.collider.gameObject;

                    // 2. Project your look direction onto the target wall's local coordinate system
                    float lookAlongForward = Vector3.Dot(cameraForward, hitWall.transform.forward);
                    float lookAlongRight = Vector3.Dot(cameraForward, hitWall.transform.right);

                    Vector3 snapDirection = Vector3.zero;
                    float thicknessOffset = 0f;

                    // --- RE-TUNED SELECTION THRESHOLD ---
                    // We check if your view direction matches the local Right vector more than Forward
                    if (Mathf.Abs(lookAlongRight) >= Mathf.Abs(lookAlongForward))
                    {
                        // Player is pushing HORIZONTALLY along the wall's X-Axis (Left / Right edges)
                        snapDirection = hitWall.transform.right * Mathf.Sign(lookAlongRight);
                        
                        // If your wall mesh's long side is modeled on Z, use width here. If modeled on X, use length.
                        thicknessOffset = wallWidth; 
                    }
                    else
                    {
                        // Player is pushing VERTICALLY/DEPTH-WISE along the wall's Z-Axis (Front / Back edges)
                        snapDirection = hitWall.transform.forward * Mathf.Sign(lookAlongForward);
                        thicknessOffset = wallLength;
                    }

                    // Calculate clean target node position
                    finalSpawnPosition = hitWall.transform.position + (snapDirection * thicknessOffset);
                    finalSpawnRotation = hitWall.transform.rotation;
                    
                    // Personal space check
                    float minSafetyDistance = 2.5f; 
                    if (Vector3.Distance(this.transform.position, finalSpawnPosition) < minSafetyDistance)
                    {
                        finalSpawnPosition = this.transform.position + (cameraForward * minSafetyDistance);
                    }
                    return;
                }
            }

            // Fallback safety stacker
            int safetyCounter = 0;
            while (Physics.CheckBox(finalSpawnPosition, halfExtents, finalSpawnRotation) && safetyCounter < 10)
            {
                finalSpawnPosition += cameraForward * wallWidth;
                safetyCounter++; 
            }

            float currentDist = Vector3.Distance(this.transform.position, finalSpawnPosition);
            if (currentDist < MinWallSpawnRange)
            {
                finalSpawnPosition = this.transform.position + (cameraForward * MinWallSpawnRange);
            }
        }
    }

    void CreatePreviewObject()
    {
        if (objectToSpawn == null || previewInstance != null) return;

        CalculateWallPlacement();
        previewInstance = Instantiate(objectToSpawn, finalSpawnPosition, finalSpawnRotation);

        if (previewInstance.TryGetComponent<Collider>(out Collider col))
        {
            Destroy(col);
        }

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
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        if (objectToSpawn == null) return;

        Instantiate(objectToSpawn, finalSpawnPosition, finalSpawnRotation);
        Debug.Log("Wall spawned on release");
    }
}