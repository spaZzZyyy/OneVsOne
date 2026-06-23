using UnityEngine;

public class Clothes : MonoBehaviour
{
    // Variables to store the initial transform data
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;
    [SerializeField] float topHeight;
    [SerializeField] float bottomHeight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Record position, rotation, and scale
        RecordStartStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RecordStartStats()
    {
        startPos = this.transform.position;
        startRot = this.transform.rotation;
        startScale = this.transform.localScale;
    }

    // This built-in Unity function fires automatically when something enters the trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched us is tagged "Player"
        if (other.CompareTag("Player"))
        {
            // Pass the player's transform to your wear function
            OnWearThis(other.transform);
        }
    }

    // Note: 'Transform' must be capitalized in C#
    void OnWearThis(Transform playerTransform)
    {
        // Set position to (0,0,0)
        this.transform.localPosition = Vector3.zero; 

        // Set rotation to zero (no rotation)
        this.transform.localRotation = Quaternion.identity; 

        // Set scale to (0,0,0) - Note: This makes the object invisible/infinitely small!
        this.transform.localScale = Vector3.one;

        // 1. Create a copy of the current local position
        Vector3 currentLocalPos = this.transform.localPosition;

        // 2. Modify the y value based on the tag
        // (Using CompareTag is also recommended here as it is faster and avoids typos)
        if (this.CompareTag("Top"))
        {
            currentLocalPos.y += topHeight;
        }
        else if (this.CompareTag("Bottom"))
        {
            currentLocalPos.y += bottomHeight;
        } else {
            currentLocalPos.y = currentLocalPos.y;
        }

        // 3. Assign the modified vector back to the transform
        this.transform.localPosition = currentLocalPos;

        // Parent the clothes to the player. 
        // Setting the second argument to 'false' resets the local position/rotation 
        // so it snaps precisely to where the player is located.
        this.transform.SetParent(playerTransform, false);

    }
}