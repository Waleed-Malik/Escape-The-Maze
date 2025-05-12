using UnityEngine;

namespace qtools.qmaze.example1
{
    /// <summary>
    /// Assigns the "Key" tag to the GameObject this script is attached to.
    /// Also validates that the GameObject has a collider for key pickup detection.
    /// </summary>
    public class KeyTagAssigner : MonoBehaviour
    {
        void Awake()
        {
            // Assign the Key tag
            if (gameObject.tag != "Key")
            {
                gameObject.tag = "Key";
                Debug.Log($"Assigned 'Key' tag to {gameObject.name}");
            }
            
            // Check for collider
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogWarning($"Key object {gameObject.name} has no collider! Adding a box collider.");
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                boxCol.isTrigger = true; // Set to trigger for OnTriggerEnter detection
            }
            else if (!col.isTrigger)
            {
                Debug.LogWarning($"Key object {gameObject.name} collider is not a trigger. Setting to trigger mode.");
                col.isTrigger = true;
            }
        }
    }
} 