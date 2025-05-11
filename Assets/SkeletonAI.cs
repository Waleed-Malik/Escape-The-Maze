using qtools.qmaze.example1;
using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    public Transform player;
    public float chaseDistance = 10f;
    public float catchDistance = 1.5f;
    public float moveSpeed = 2f;
    public GameObject mazeParent;

    public QFPSController fpsController;

    void Start()
    {
        fpsController = player.GetComponent<QFPSController>();
    }

    void Update()
    {
        if (player == null || fpsController == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        if (distance <= catchDistance)
        {
            fpsController.LoseLifeAndResetSkeleton(); // Let FPS controller handle lives/UI
        }
    }

    public void RepositionSkeleton()
    {
        if (mazeParent == null || mazeParent.transform.childCount == 0) return;

        Transform randomTile = mazeParent.transform.GetChild(Random.Range(0, mazeParent.transform.childCount));
        Vector3 newPos = randomTile.position + Vector3.up * 1f;
        transform.position = newPos;
    }
}
