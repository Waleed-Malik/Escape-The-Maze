using UnityEngine;

[RequireComponent(typeof(SkeletonAI))]
public class SkeletonVisualizer : MonoBehaviour
{
    private SkeletonAI skeletonAI;

    [Header("Visual Settings")]
    public Color chaseRangeColor = Color.yellow;
    public Color catchRangeColor = Color.red;
    public Color chasingLineColor = Color.green;
    public bool showRanges = true;
    public bool showChasingLine = true;

    void Start()
    {
        skeletonAI = GetComponent<SkeletonAI>();
    }

    void OnDrawGizmos()
    {
        if (skeletonAI == null)
        {
            skeletonAI = GetComponent<SkeletonAI>();
            if (skeletonAI == null) return;
        }

        if (!showRanges) return;

        // Draw chase range
        Gizmos.color = chaseRangeColor;
        Gizmos.DrawWireSphere(transform.position, skeletonAI.chaseDistance);

        // Draw catch range
        Gizmos.color = catchRangeColor;
        Gizmos.DrawWireSphere(transform.position, skeletonAI.catchDistance);
    }

    void OnDrawGizmosSelected()
    {
        if (skeletonAI == null || skeletonAI.player == null) return;

        if (!showChasingLine) return;

        float distance = Vector3.Distance(transform.position, skeletonAI.player.position);
        if (distance <= skeletonAI.chaseDistance)
        {
            Gizmos.color = chasingLineColor;
            Gizmos.DrawLine(transform.position, skeletonAI.player.position);
        }
    }
}