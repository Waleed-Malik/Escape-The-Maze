using UnityEngine;

public class GameObjectSpawner : MonoBehaviour
{
    public GameObject knightPrefab;
    public GameObject swordPrefab;
    public GameObject shieldPrefab;
    public Vector3 knightRotation = new Vector3(0f, 0f, 0f);
    public Vector3 knightScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 swordRotation = new Vector3(0f, 0f, 0f);
    public Vector3 swordScale = new Vector3(0.3f, 0.3f, 0.3f);
    public Vector3 shieldRotation = new Vector3(0f, 0f, 0f);
    public Vector3 shieldScale = new Vector3(0.5f, 0.5f, 0.5f);

    private static bool knightSpawned = false;
    private static bool swordSpawned = false;
    private static bool shieldSpawned = false;

    private static int spawnCounter = 0;

    void Start()
    {
        spawnCounter++;

        if (!knightSpawned && knightPrefab != null && spawnCounter == 2)
        {
            knightSpawned = true;
            Spawn(knightPrefab, transform.position + new Vector3(0f, 0f, -0.5f), knightRotation, knightScale);
        }
        else if (!swordSpawned && swordPrefab != null && spawnCounter == 1)
        {
            swordSpawned = true;
            Spawn(swordPrefab, transform.position + new Vector3(-0.374f, 1.347f, -21f), swordRotation, swordScale);
        }
        else if (!shieldSpawned && shieldPrefab != null && spawnCounter == 3)
        {
            shieldSpawned = true;
            Spawn(shieldPrefab, transform.position + new Vector3(0.3f, 0f, 0f), swordRotation, swordScale);
        }
    }

    void Spawn(GameObject prefab, Vector3 localOffset, Vector3 rotationEuler, Vector3 scale)
    {
        // Offset and rotation relative to the wall's transform
        Vector3 position = transform.position + transform.TransformDirection(localOffset);
        Quaternion rotation = transform.rotation * Quaternion.Euler(rotationEuler);

        GameObject obj = Instantiate(prefab, position, rotation);
        obj.transform.localScale = scale;

        Debug.Log("Spawned " + obj.name + " at " + position + " with scale " + scale);
    }
}

