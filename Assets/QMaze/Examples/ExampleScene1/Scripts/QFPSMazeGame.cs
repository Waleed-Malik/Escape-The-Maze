using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using qtools.qmaze;
using UnityEngine.SceneManagement;

namespace qtools.qmaze.example1
{
	public class QFPSMazeGame : MonoBehaviour 
	{
		public QMazeEngine mazeEngine;
		public GameObject finishTriggerPrefab;
        public Text levelText;
		private bool needGenerateNewMaze = true;
        private int currentLevel = 0;

		public Text timerText;
		public GameObject gameOverPanel;
		public float timeLimit = 120f;
		private float timeRemaining;
		private bool timerStarted = false;
		private QFPSController fpsController;
		private bool isGameOver = false;
		private List<GameObject> finishTriggerInstances = new List<GameObject>();
		public GameObject skeletonPrefab;
		public Transform playerTransform;
		private GameObject skeletonInstance;
		private bool levelFinished = false;

		// Flag to track if skeleton is spawned
		private bool isSkeletonSpawned = false;

		// void OnGUI()
		// {
		// 	if (!isGameOver && Input.GetMouseButtonDown(0))
		// 	{
		// 		Cursor.visible = false;
		// 		Cursor.lockState = CursorLockMode.Locked;
		// 	}

		// 	if (Input.GetKeyDown(KeyCode.Escape))
		// 	{
		// 		Cursor.lockState = CursorLockMode.None;
		// 		Cursor.visible = true;
		// 	}
		// }

		void Start()
		{
			fpsController = FindFirstObjectByType<QFPSController>().GetComponent<QFPSController>();
		}

		void Update()
		{
			if (needGenerateNewMaze)
			{
				Debug.Log("Generating new maze, moving to level " + (currentLevel + 1));
				needGenerateNewMaze = false;
				generateNewMaze();
			}

			UpdateTimer();
		}

		public void finishHandler()
		{
			if (levelFinished) return; // prevent multiple calls
			levelFinished = true;
			
			// Reset key status from the FPS controller instead of just the unlock status
			if (fpsController != null)
			{
				fpsController.ResetKeyStatus();
			}
			
			needGenerateNewMaze = true;
			Debug.Log("Level completed! Key status reset. Generating new level.");
		}

		public void generateNewMaze()
		{
			levelFinished = false; // allow next level trigger again
			timeRemaining = timeLimit;
			timerStarted = false;
			isSkeletonSpawned = false; // Reset skeleton spawn flag for new level

			// Find the FPS controller first 
			fpsController = FindFirstObjectByType<QFPSController>();

			// Make sure key status is reset at the beginning of a new level
			if (fpsController != null)
			{
				fpsController.ResetKeyStatus();
			}

			mazeEngine.destroyImmediateMazeGeometry();
			mazeEngine.generateMaze();

			// finishTriggerInstances.Clear(); // Clear old ones

			// List<QVector2IntDir> finishPointList = mazeEngine.getFinishPositionList();
			// foreach (QVector2IntDir finishPosition in finishPointList)
			// {
			// 	GameObject trigger = Instantiate(finishTriggerPrefab);
			// 	trigger.transform.parent = mazeEngine.transform;
			// 	trigger.transform.localPosition = new Vector3(finishPosition.x * mazeEngine.getMazePieceWidth(), 0.01f, - finishPosition.y * mazeEngine.getMazePieceHeight());
			// 	trigger.SetActive(false); // Hide initially
			// 	finishTriggerInstances.Add(trigger);
			// }


			// QFinishTrigger[] finishTriggerArray = FindObjectsByType<QFinishTrigger>(FindObjectsSortMode.None);
			// if (finishTriggerArray != null)
			// {
			// 	for (int i = 0; i < finishTriggerArray.Length; i++)
			// 		finishTriggerArray[i].triggerHandlerEvent += finishHandler;
			// }

			// Instantiate finish triggers
			finishTriggerInstances.Clear();
			List<QVector2IntDir> finishPointList = mazeEngine.getFinishPositionList();
			foreach (QVector2IntDir finishPosition in finishPointList)
			{
				GameObject trigger = Instantiate(finishTriggerPrefab);
				trigger.transform.parent = mazeEngine.transform;
				trigger.transform.localPosition = new Vector3(finishPosition.x * mazeEngine.getMazePieceWidth(), 0.01f, - finishPosition.y * mazeEngine.getMazePieceHeight());
				trigger.SetActive(false); // Hide initially
				finishTriggerInstances.Add(trigger);
			}

			// Hook the finishHandler AFTER instantiating the triggers
			QFinishTrigger[] finishTriggerArray = FindObjectsByType<QFinishTrigger>(FindObjectsSortMode.None);
			if (finishTriggerArray != null)
			{
				for (int i = 0; i < finishTriggerArray.Length; i++)
					finishTriggerArray[i].triggerHandlerEvent += finishHandler;
			}


			List<QVector2IntDir> startPointList = mazeEngine.getStartPositionList();

			// No need to reassign fpsController as it was already set above
			if (fpsController != null)
			{
				if (startPointList.Count == 0)
					fpsController.gameObject.transform.position = new Vector3(0, 1, 0);
				else
				{
					QVector2IntDir startPoint = startPointList[0];
					fpsController.gameObject.transform.position = new Vector3(startPoint.x * mazeEngine.getMazePieceWidth(), 0.9f, - startPoint.y * mazeEngine.getMazePieceHeight());
					fpsController.setRotation(Quaternion.AngleAxis((int)startPoint.direction * 90, Vector3.up));
				}
				
				// Call the new method to refresh chest references
				fpsController.RefreshChestReference();
			}

			currentLevel++;
            levelText.text = "LEVEL: " + currentLevel;

			// Don't spawn the skeleton yet - it will be spawned when chest is unlocked
			if (skeletonInstance != null)
			{
				Destroy(skeletonInstance);
				skeletonInstance = null;
			}
			
			// Spawn keys for this level
			SpawnKeysForLevel();
		}

		void UpdateTimer()
		{
			if (!timerStarted && fpsController != null && fpsController.isMoving())
			{
				timerStarted = true;
			}

			if (timerStarted)
			{
				if (timeRemaining > 0)
				{
					timeRemaining -= Time.deltaTime;
					UpdateTimerDisplay(timeRemaining);
				}
				else
				{
					timeRemaining = 0;
					timerStarted = false;

					// Show Game Over UI
					// In UpdateTimer()
				if (gameOverPanel != null)
				{
					gameOverPanel.SetActive(true);
					timerText.text = "";
					isGameOver = true; // <-- Add this
				}


					// Disable player movement
					if (fpsController != null)
						fpsController.enabled = false;

					// Unlock cursor
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
			}
		}

		void UpdateTimerDisplay(float timeToDisplay)
		{
			int minutes = Mathf.FloorToInt(timeToDisplay / 60);
			int seconds = Mathf.FloorToInt(timeToDisplay % 60);
			timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
		}

		private Vector3 GetRandomMazePosition()
		{
			int mazeWidth = mazeEngine.getMazeWidth();
			int mazeHeight = mazeEngine.getMazeHeight();
			float pieceWidth = mazeEngine.getMazePieceWidth();
			float pieceHeight = mazeEngine.getMazePieceHeight();

			int randX = Random.Range(0, mazeWidth);
			int randY = Random.Range(0, mazeHeight);

			return new Vector3(randX * pieceWidth, 0.9f, -randY * pieceHeight);
		}

		// New method to spawn skeleton when chest is unlocked
		public void SpawnSkeletonAfterChestUnlock()
		{
			if (isSkeletonSpawned) return; // Don't spawn if already spawned
			if (skeletonPrefab == null)
			{
				Debug.LogError("Skeleton prefab is not assigned in QFPSMazeGame!");
				return;
			}

			if (playerTransform == null)
			{
				// Try to find player if reference is missing
				QFPSController controller = FindObjectOfType<QFPSController>();
				if (controller != null)
				{
					playerTransform = controller.transform;
					fpsController = controller;
				}
				else
				{
					Debug.LogError("No player transform assigned and could not find one in scene!");
					return;
				}
			}

			isSkeletonSpawned = true;
			Debug.Log("Spawning skeleton after chest unlock");

			// Spawn at a position away from the player
			Vector3 skeletonSpawnPosition = GetRandomDistantMazePosition();
			
			// Force low y position to ensure skeleton is on the ground
			skeletonSpawnPosition.y = 0.1f;
			
			// Instantiate the skeleton
			if (skeletonInstance != null)
			{
				Destroy(skeletonInstance);
			}
			
			skeletonInstance = Instantiate(skeletonPrefab, skeletonSpawnPosition, Quaternion.identity);
			
			// Set a name to make debugging easier
			skeletonInstance.name = "Skeleton_Enemy";
			
			// Make sure GameObject is active
			skeletonInstance.SetActive(true);

			// Pass the player reference to the SkeletonAI script
			SkeletonAI skeletonAI = skeletonInstance.GetComponent<SkeletonAI>();
			if (skeletonAI != null)
			{
				// Ensure references are set
				skeletonAI.player = playerTransform;
				skeletonAI.fpsController = fpsController;
				skeletonAI.mazeParent = mazeEngine.gameObject; // Use maze engine as parent
				skeletonAI.chaseDistance = 15f; // Increase chase distance
				skeletonAI.moveSpeed = 3f; // Increase speed slightly
				
				// Debug log 
				Debug.Log($"Skeleton created at {skeletonSpawnPosition} with player={playerTransform.name}, controller={fpsController.name}");
			}
			else
			{
				Debug.LogError("SkeletonAI component not found on skeleton prefab!");
			}
		}

		// New method to get a spawn position that's far from the player
		private Vector3 GetRandomDistantMazePosition()
		{
			int mazeWidth = mazeEngine.getMazeWidth();
			int mazeHeight = mazeEngine.getMazeHeight();
			float pieceWidth = mazeEngine.getMazePieceWidth();
			float pieceHeight = mazeEngine.getMazePieceHeight();

			// Try to find a position that's distant from the player
			Vector3 playerPos = playerTransform.position;
			float minDistance = 10f; // Minimum distance from player
			
			for (int attempt = 0; attempt < 10; attempt++)
			{
				int randX = Random.Range(0, mazeWidth);
				int randY = Random.Range(0, mazeHeight);
				Vector3 potentialPos = new Vector3(randX * pieceWidth, 0.9f, -randY * pieceHeight);
				
				if (Vector3.Distance(potentialPos, playerPos) > minDistance)
				{
					return potentialPos;
				}
			}
			
			// If no suitable position found, return a default position
			int defaultX = Random.Range(0, mazeWidth);
			int defaultY = Random.Range(0, mazeHeight);
			return new Vector3(defaultX * pieceWidth, 0.9f, -defaultY * pieceHeight);
		}

		public void ActivateFinishTriggers()
		{
			// First spawn the skeleton
			SpawnSkeletonAfterChestUnlock();
			
			// Then activate finish triggers
			foreach (GameObject trigger in finishTriggerInstances)
			{
				if (trigger != null)
					trigger.SetActive(true);
			}
		}

		public void RetryLevel()
		{
			Debug.Log("RetryLevel button clicked");
    		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		// New method to spawn keys
		private void SpawnKeysForLevel()
		{
			// Try to get the KeySpawner component
			KeySpawner keySpawner = GetComponent<KeySpawner>();
			
			// If no KeySpawner found, try to add one
			if (keySpawner == null)
			{
				keySpawner = gameObject.AddComponent<KeySpawner>();
				Debug.Log("Added KeySpawner component to QFPSMazeGame");
				
				// Set default values - these should be configured in the editor
				if (keySpawner.keyPrefab == null)
				{
					Debug.LogWarning("No key prefab assigned to KeySpawner. Keys won't spawn until a prefab is assigned.");
				}
			}
			
			// Spawn keys if we have a valid spawner and player reference
			if (keySpawner != null && playerTransform != null)
			{
				keySpawner.SpawnKeys(playerTransform);
			}
			else
			{
				Debug.LogWarning("Cannot spawn keys: Missing KeySpawner or playerTransform reference");
			}
		}

	} 
}








