using UnityEngine;

public class BarrelPlacementValidator : MonoBehaviour
{
    public GameObject barrel; // Assign this in the inspector (child GameObject)

    void Start()
    {
        // Check if barrel's position is overlapping with something (like a wall)
        Collider[] hits = Physics.OverlapBox(
            barrel.transform.position,
            barrel.transform.localScale * 0.5f, // Half extents
            barrel.transform.rotation,
            LayerMask.GetMask("Wall") // Make sure walls are on this layer
        );

        if (hits.Length > 0)
        {
            barrel.SetActive(false); // Hide the barrel if it’s overlapping with a wall
        }
    }
}
