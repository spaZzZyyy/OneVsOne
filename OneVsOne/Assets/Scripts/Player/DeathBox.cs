using UnityEngine;

public class DeathBox : MonoBehaviour
{
    [SerializeField] Vector3 spawnPosition;
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            // Try to grab a CharacterController if the player has one
            if (other.TryGetComponent<CharacterController>(out CharacterController controller))
            {
                controller.enabled = false; // Turn it off
                other.transform.position = spawnPosition; // Move
                controller.enabled = true; // Turn it back on
            }
            else
            {
                // Fallback for standard transforms / rigidbodies
                other.transform.position = spawnPosition;
            }
            
            Debug.Log("Died");
        }
    }
}
