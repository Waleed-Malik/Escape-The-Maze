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
			fpsController.isChestUnlocked = false;
			needGenerateNewMaze = true;
		}


		public void generateNewMaze()
		{
			levelFinished = false; // allow next level trigger again
			timeRemaining = timeLimit;
			timerStarted = false;

			this.fpsController = FindFirstObjectByType<QFPSController>();


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

			QFPSController fpsController = fpsController = FindFirstObjectByType<QFPSController>();

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

			// --- SKELETON SPAWN ---
			if (skeletonInstance != null)
				Destroy(skeletonInstance); // Destroy old one if respawning for next level

			Vector3 skeletonSpawnPosition = GetRandomMazePosition();
			skeletonInstance = Instantiate(skeletonPrefab, skeletonSpawnPosition, Quaternion.identity);

			// Pass the player reference to the SkeletonAI script
			SkeletonAI skeletonAI = skeletonInstance.GetComponent<SkeletonAI>();
			if (skeletonAI != null)
			{
				skeletonAI.player = playerTransform;
				skeletonAI.fpsController = fpsController;
			}

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

		public void ActivateFinishTriggers()
		{
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

	} 
}








