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
    public bool showMazeParent = true;

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
        
        // Draw connection to maze parent if available
        if (showMazeParent && skeletonAI.mazeParent != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, skeletonAI.mazeParent.transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (skeletonAI == null || skeletonAI.player == null) return;

        if (!showChasingLine) return;

        float distance = Vector3.Distance(transform.position, skeletonAI.player.position);
        
        // Show line to player with color based on distance
        Gizmos.color = (distance <= skeletonAI.chaseDistance) ? chasingLineColor : Color.gray;
        Gizmos.DrawLine(transform.position, skeletonAI.player.position);
        
        // Draw arrow pointing to player if being chased
        if (distance <= skeletonAI.chaseDistance)
        {
            Vector3 direction = (skeletonAI.player.position - transform.position).normalized;
            Vector3 arrowPos = transform.position + direction * 2f;
            Gizmos.DrawSphere(arrowPos, 0.2f);
        }
    }
}