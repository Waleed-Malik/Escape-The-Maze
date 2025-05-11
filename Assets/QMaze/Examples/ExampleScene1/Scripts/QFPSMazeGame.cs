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
		private GameObject finishTriggerInstance;
		public GameObject skeletonPrefab;
		public Transform playerTransform;
		private GameObject skeletonInstance;
		void OnGUI()
		{
			if (!isGameOver && Input.GetMouseButtonDown(0))
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		void Update()
		{
			if (needGenerateNewMaze)
			{
				needGenerateNewMaze = false;
				generateNewMaze();
			}

			UpdateTimer();

		}

		void finishHandler()
		{
			needGenerateNewMaze = true;
		}

		void generateNewMaze()
		{
			timeRemaining = timeLimit;
			timerStarted = false;

			this.fpsController = FindFirstObjectByType<QFPSController>();


			mazeEngine.destroyImmediateMazeGeometry();
			mazeEngine.generateMaze();

			List<QVector2IntDir> finishPointList = mazeEngine.getFinishPositionList();
			for (int i = 0; i < finishPointList.Count; i++)
			{
				QVector2IntDir finishPosition = finishPointList[i];
				finishTriggerInstance = Instantiate(finishTriggerPrefab);
				finishTriggerInstance.transform.parent = mazeEngine.transform;
				finishTriggerInstance.transform.localPosition = new Vector3(finishPosition.x * mazeEngine.getMazePieceWidth(), 0.01f, - finishPosition.y * mazeEngine.getMazePieceHeight());
				finishTriggerInstance.SetActive(false); // Hide until chest is unlocked
			}


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
			}

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


            currentLevel++;
            levelText.text = "LEVEL: " + currentLevel;

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


		public void RetryLevel()
		{
			Debug.Log("RetryLevel button clicked");
    		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}




	} 
}








