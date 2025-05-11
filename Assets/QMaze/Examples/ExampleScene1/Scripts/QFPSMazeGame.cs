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

		void OnGUI()
		{
			if (Input.GetMouseButtonDown(0))
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
				GameObject finishTriggerInstance = (GameObject)GameObject.Instantiate(finishTriggerPrefab);
				finishTriggerInstance.transform.parent = mazeEngine.transform;
				finishTriggerInstance.transform.localPosition = new Vector3(finishPosition.x * mazeEngine.getMazePieceWidth(), 0.01f, - finishPosition.y * mazeEngine.getMazePieceHeight());
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
					if (gameOverPanel != null)
					{
						gameOverPanel.SetActive(true);
						timerText.text = "";
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

		public void RetryLevel()
		{
			Debug.Log("RetryLevel button clicked");
    		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}




	} 
}








