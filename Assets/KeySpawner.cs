using System.Collections.Generic;
using qtools.qmaze;
using UnityEngine;

namespace qtools.qmaze.example1
{
    /// <summary>
    /// Spawns keys at random positions in the maze.
    /// This script should be attached to the QFPSMazeGame object.
    /// </summary>
    public class KeySpawner : MonoBehaviour
    {
        [Header("Key Spawning")]
        public GameObject keyPrefab;
        public int numberOfKeys = 3;
        public float keyHeight = 0.8f;
        public float minDistanceFromStart = 10f;
        
        private QMazeEngine mazeEngine;
        private QFPSMazeGame mazeGame;
        private List<GameObject> spawnedKeys = new List<GameObject>();
        
        private void Start()
        {
            mazeEngine = GetComponent<QMazeEngine>();
            mazeGame = GetComponent<QFPSMazeGame>();
            
            if (mazeEngine == null)
            {
                Debug.LogError("KeySpawner requires a QMazeEngine component on the same GameObject!");
                return;
            }
            
            // Make sure the key prefab has the KeyTagAssigner
            if (keyPrefab != null && keyPrefab.GetComponent<KeyTagAssigner>() == null)
            {
                Debug.LogWarning("KeyPrefab doesn't have KeyTagAssigner component. Adding one to the prefab.");
                keyPrefab.AddComponent<KeyTagAssigner>();
            }
        }
        
        /// <summary>
        /// Spawns keys in the maze after it has been generated.
        /// This should be called from QFPSMazeGame after maze generation.
        /// </summary>
        public void SpawnKeys(Transform playerTransform)
        {
            if (keyPrefab == null || mazeEngine == null)
            {
                Debug.LogError("Cannot spawn keys: Missing keyPrefab or mazeEngine reference");
                return;
            }
            
            // Clear any existing keys
            ClearKeys();
            
            int mazeWidth = mazeEngine.getMazeWidth();
            int mazeHeight = mazeEngine.getMazeHeight();
            float pieceWidth = mazeEngine.getMazePieceWidth();
            float pieceHeight = mazeEngine.getMazePieceHeight();
            
            Vector3 playerPos = playerTransform.position;
            
            // Try to spawn all requested keys
            int keysSpawned = 0;
            int maxAttempts = numberOfKeys * 10; // Limit attempts to avoid infinite loop
            int attempts = 0;
            
            while (keysSpawned < numberOfKeys && attempts < maxAttempts)
            {
                attempts++;
                
                // Generate random position
                int x = Random.Range(0, mazeWidth);
                int z = Random.Range(0, mazeHeight);
                
                Vector3 keyPos = new Vector3(
                    x * pieceWidth, 
                    keyHeight, 
                    -z * pieceHeight
                );
                
                // Check if far enough from player
                float distToPlayer = Vector3.Distance(
                    new Vector3(keyPos.x, 0, keyPos.z),
                    new Vector3(playerPos.x, 0, playerPos.z)
                );
                
                if (distToPlayer >= minDistanceFromStart)
                {
                    // Spawn the key
                    GameObject keyInstance = Instantiate(
                        keyPrefab, 
                        keyPos, 
                        Quaternion.Euler(0, Random.Range(0, 360), 0)
                    );
                    
                    keyInstance.name = "Key_" + keysSpawned;
                    spawnedKeys.Add(keyInstance);
                    keysSpawned++;
                    
                    Debug.Log($"Spawned key {keysSpawned} at position {keyPos}, distance from player: {distToPlayer}");
                }
            }
            
            Debug.Log($"Spawned {keysSpawned} keys out of {numberOfKeys} requested (attempts: {attempts})");
        }
        
        /// <summary>
        /// Clears all spawned keys.
        /// </summary>
        public void ClearKeys()
        {
            foreach (GameObject key in spawnedKeys)
            {
                if (key != null)
                {
                    Destroy(key);
                }
            }
            
            spawnedKeys.Clear();
        }
    }
} 