using UnityEngine;
using UnityEngine.Events; // 1. REQUIRED for UnityEvents

public class OpenTheBox : MonoBehaviour
{
    // 2. Create the UnityEvent (this will show up in your Inspector)
    [SerializeField] private UnityEvent onStartBoxOpen;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            // 3. Trigger the event safely
            onStartBoxOpen?.Invoke();
        }
    }
}