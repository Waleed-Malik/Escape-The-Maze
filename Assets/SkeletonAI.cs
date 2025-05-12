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

    [Header("Animation")]
    public bool useAnimation = true;
    public string runAnimParameter = "Speed";
    public string attackAnimTrigger = "LeftAttack";
    
    private Animator animator;
    private bool isChasing = false;
    private float lastCatchTime = 0f;
    private float catchCooldown = 1.5f; // Time between catches
    private float respawnDelay = 1.0f; // Delay before repositioning
    private float yPositionOffset = 0.1f; // Lower y-offset to keep skeleton on the ground
    private bool initialized = false;

    void Awake()
    {
        // Force position to be at ground level immediately
        Vector3 pos = transform.position;
        pos.y = yPositionOffset;
        transform.position = pos;
    }

    void Start()
    {
        InitializeReferences();
        
        // Ensure skeleton is at ground level
        Vector3 pos = transform.position;
        pos.y = yPositionOffset;
        transform.position = pos;
        
        Debug.Log("Skeleton position after Start: " + transform.position);
    }
    
    void InitializeReferences()
    {
        if (initialized) return;
        
        // Try to find the player if not set
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Found player by tag: " + player.name);
            }
            else
            {
                // Try finding by FPS controller type as backup
                QFPSController[] controllers = FindObjectsOfType<QFPSController>();
                if (controllers != null && controllers.Length > 0)
                {
                    player = controllers[0].transform;
                    Debug.Log("Found player by controller type: " + player.name);
                }
                else
                {
                    Debug.LogError("Cannot find player by tag or controller type!");
                }
            }
        }
        
        // Get reference to the fpsController if not already set
        if (fpsController == null && player != null)
        {
            fpsController = player.GetComponent<QFPSController>();
            if (fpsController == null)
            {
                Debug.LogError("Player has no QFPSController component!");
            }
        }

        // Get maze parent if not already set
        if (mazeParent == null)
        {
            // Try to find maze game object
            mazeParent = GameObject.Find("QMazeGame");
            if (mazeParent == null)
            {
                QFPSMazeGame mazeGame = FindObjectOfType<QFPSMazeGame>();
                if (mazeGame != null)
                {
                    mazeParent = mazeGame.gameObject;
                    Debug.Log("Found maze parent by type: " + mazeParent.name);
                }
                else
                {
                    Debug.LogWarning("Cannot find maze parent. Using scene root as fallback.");
                    mazeParent = GameObject.Find("QMaze");
                    if (mazeParent == null && player != null)
                    {
                        // Last resort: use the player's parent
                        mazeParent = player.parent?.gameObject;
                    }
                }
            }
        }

        // Get animator if available and animation is enabled
        if (useAnimation)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning("No animator found on skeleton or its children");
                }
            }
        }
        
        initialized = true;
        Debug.Log("Skeleton initialized with player: " + (player != null ? player.name : "null") + 
                 ", controller: " + (fpsController != null ? "valid" : "null") + 
                 ", mazeParent: " + (mazeParent != null ? mazeParent.name : "null"));
    }

    void FixedUpdate()
    {
        // Use FixedUpdate for more consistent physics-based movement
        UpdateSkeletonBehavior();
    }
    
    void Update()
    {
        // Keep reference initialization in Update to keep trying if not found yet
        if (!initialized || player == null || fpsController == null)
        {
            InitializeReferences();
        }
        
        // Make sure we're on the ground
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.y - yPositionOffset) > 0.5f)
        {
            pos.y = yPositionOffset;
            transform.position = pos;
        }
    }
    
    void UpdateSkeletonBehavior()
    {
        if (player == null)
        {
            Debug.LogWarning("Missing player reference in SkeletonAI");
            return; 
        }

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z), 
            new Vector3(player.position.x, 0, player.position.z)
        );
        
        // Debug log distance to player
        if (Time.frameCount % 60 == 0) // Only log every 60 frames to avoid spam
        {
            Debug.Log($"Skeleton distance to player: {distance}, chase distance: {chaseDistance}");
        }

        // Check if within chase range
        if (distance <= chaseDistance)
        {
            isChasing = true;
            
            // Calculate direction to player (ignore Y axis)
            Vector3 targetPosition = player.position;
            targetPosition.y = transform.position.y; // Keep same height
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            // Rotate skeleton to look at player
            if (direction != Vector3.zero) // Avoid rotation error if vectors are identical
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
            
            // Move skeleton towards player
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            
            // Update animation if available
            if (animator != null && useAnimation)
            {
                animator.SetFloat(runAnimParameter, 1f); // Set run animation
            }
            
            // Check if within catch distance and cooldown expired
            if (distance <= catchDistance && Time.time > lastCatchTime + catchCooldown)
            {
                // Play attack animation if available
                if (animator != null && useAnimation)
                {
                    animator.SetTrigger(attackAnimTrigger);
                }
                
                // Damage player and reset skeleton
                if (fpsController != null)
                {
                    fpsController.LoseLifeAndResetSkeleton();
                    lastCatchTime = Time.time;
                    
                    // Wait briefly before repositioning to allow attack animation
                    Invoke("RepositionSkeleton", respawnDelay);
                }
            }
        }
        else
        {
            // When not chasing, stop movement animation
            if (isChasing && animator != null && useAnimation)
            {
                animator.SetFloat(runAnimParameter, 0f);
                isChasing = false;
            }
        }
    }

    public void RepositionSkeleton()
    {
        if (!initialized)
        {
            InitializeReferences();
        }
        
        if (mazeParent == null || player == null)
        {
            Debug.LogWarning("Missing references for skeleton repositioning");
            return;
        }

        if (mazeParent.transform.childCount == 0)
        {
            Debug.LogWarning("No child objects found in maze parent for skeleton respawn");
            return;
        }

        // Pick a random position in the maze away from the player
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int randomIndex = Random.Range(0, mazeParent.transform.childCount);
            Transform randomTile = mazeParent.transform.GetChild(randomIndex);
            Vector3 newPos = randomTile.position;
            newPos.y = yPositionOffset; // Keep skeleton at ground level
            
            // Make sure the new position is far enough from player
            float distanceToPlayer = Vector3.Distance(newPos, player.position);
            if (distanceToPlayer > chaseDistance * 0.8f)
            {
                transform.position = newPos;
                Debug.Log("Skeleton respawned at " + newPos);
                return;
            }
        }
        
        // If all attempts failed, just pick the last attempted position
        Transform fallbackTile = mazeParent.transform.GetChild(Random.Range(0, mazeParent.transform.childCount));
        Vector3 fallbackPos = fallbackTile.position;
        fallbackPos.y = yPositionOffset; // Keep skeleton at ground level
        transform.position = fallbackPos;
        Debug.Log("Skeleton respawned at fallback position");
    }
}
