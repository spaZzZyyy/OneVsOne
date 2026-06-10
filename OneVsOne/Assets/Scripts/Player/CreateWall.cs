using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CreateWall : MonoBehaviour
{

    [Header("Stats")]
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] float spawnRange = 2f;
    [SerializeField] float srX = 0;
    [SerializeField] float srY = 0;
    [SerializeField] float srZ = 0;





    void OnCreateWall(InputValue value){
    if (!value.isPressed) return;
    Debug.Log("Wall spawned");
    Vector3 startPos = this.transform.position;
    Vector3 offset = this.transform.forward * spawnRange;
    Vector3 spawnPosition = startPos + offset;
    
    // Keeps your current rotation calculation
    Quaternion spawnRotation = this.transform.rotation * Quaternion.Euler(srX,srY,srZ);

    // --- NEW SNAP LOGIC ---
    // 1. Get the width of the wall prefab from its BoxCollider
    BoxCollider wallCollider = objectToSpawn.GetComponent<BoxCollider>();
    if (wallCollider != null)
    {
        // Calculate the actual width factoring in the prefab's local scale
        float wallWidth = wallCollider.size.x * objectToSpawn.transform.localScale.x;
        
        // Halve the size for Unity's physics extent calculations
        Vector3 halfExtents = wallCollider.size / 2f; 

        // 2. Loop and check if a wall is already in the way. 
        // If it finds something, it shifts the spawn position forward by 1 wall width.
        int safetyCounter = 0;
        while (Physics.CheckBox(spawnPosition, halfExtents, spawnRotation) && safetyCounter < 10)
        {
            safetyCounter++; // Prevents an infinite loop freezing Unity
            return;
        }
    }
    // -----------------------

    Instantiate(objectToSpawn, spawnPosition, spawnRotation);
    }
}
