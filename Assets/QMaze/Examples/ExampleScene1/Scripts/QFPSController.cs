using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace qtools.qmaze.example1
{
	public class QFPSController : MonoBehaviour
	{
		public float mouseSensitivityX = 1.0f;
		public float mouseSensitivityY = 1.0f;
		public float moveScaleX = 1.0f;
		public float moveScaleY = 1.0f;
		public float moveMaxSpeed = 1.0f;
		public float moveLerp = 0.9f;

		private Rigidbody rigidBody;
		private Quaternion rotationTargetHorizontal;
		private Quaternion rotationTargetVertical;
		private Transform cameraTransform;

		public bool hasKey = false;
		public Text keyStatusText;
		public GameObject unlockButtonUI;
		public Transform chestTransform;
		public float chestProximityDistance = 3f;
		public bool isChestUnlocked = false;
		public GameObject chestClosed;
		public GameObject chestOpen;
		public GameObject finishLine;
		public int playerLives = 3;
		public Image life1;
		public Image life2;
		public Image life3;

		public SkeletonAI skeletonAI; // Drag the skeleton AI script here

		// Added skeleton-related variables
		public float invulnerabilityDuration = 2f;
		private bool isInvulnerable = false;

		public GameObject gameOverScreen;


		// private bool isDragging = false;
		// private Vector3 lastMousePosition;

		void Start()
		{
			rotationTargetHorizontal = transform.rotation;
			rigidBody = GetComponent<Rigidbody>();
			cameraTransform = transform.GetChild(0);
			rotationTargetVertical = cameraTransform.rotation;

			if (chestClosed != null) chestClosed.SetActive(true);
			if (chestOpen != null) chestOpen.SetActive(false);
			if (finishLine != null) finishLine.SetActive(false);

			StartCoroutine(FindChestAfterDelay());
		}

		// Add a public method to refresh the chest reference
		public void RefreshChestReference()
		{
			hasKey = false; // Reset key status for the new level
			isChestUnlocked = false;
			
			// Update UI
			if (keyStatusText != null)
				keyStatusText.text = "Key: Not Found";

			// Start looking for the chest in the new level
			StartCoroutine(FindChestAfterDelay());
		}

		IEnumerator FindChestAfterDelay()
		{
			yield return new WaitForSeconds(0.1f); // Wait a bit for scene objects to load
			GameObject chestObj = GameObject.Find("treasure_chest_closed");
			if (chestObj != null)
			{
				chestTransform = chestObj.transform;
				Debug.Log("Chest found in scene: " + chestObj.name);
			}
			else
			{
				// Try alternative names or search by tag
				GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
				foreach (GameObject obj in allObjects)
				{
					if (obj.name.ToLower().Contains("chest") || obj.name.ToLower().Contains("treasure"))
					{
						chestTransform = obj.transform;
						Debug.Log("Alternative chest found: " + obj.name);
						break;
					}
				}
				
				if (chestTransform == null)
					Debug.LogWarning("Chest object not found in this scene.");
			}
		}


		void Update()
		{
			// Try to find chest if it's still null but we have the key
			if (hasKey && chestTransform == null)
			{
				StartCoroutine(FindChestAfterDelay());
			}

			Vector3 velocity = transform.right * Input.GetAxis("Horizontal") * moveScaleX;
			velocity += transform.forward * Input.GetAxis("Vertical") * moveScaleY;
			velocity = Vector3.ClampMagnitude(velocity, moveMaxSpeed);
			velocity *= moveLerp;
			velocity.y = rigidBody.linearVelocity.y;
			rigidBody.linearVelocity = velocity;

			rotationTargetHorizontal *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivityX, 0f);
			transform.rotation = rotationTargetHorizontal;

			Quaternion nextRotationTargetVertical = rotationTargetVertical * Quaternion.Euler(Input.GetAxis("Mouse Y") * mouseSensitivityY, 0, 0);
			if (nextRotationTargetVertical.eulerAngles.x < 90 || nextRotationTargetVertical.eulerAngles.x > 270)
			{
				rotationTargetVertical = nextRotationTargetVertical;
				cameraTransform.localRotation = rotationTargetVertical;
			}

			if (Input.GetMouseButtonDown(0)) Debug.Log("Mouse Clicked");


			// if (Input.GetMouseButtonDown(0))
			// {
			// 	isDragging = true;
			// 	lastMousePosition = Input.mousePosition;
			// }
			// if (Input.GetMouseButtonUp(0))
			// {
			// 	isDragging = false;
			// }

			// if (isDragging)
			// {
			// 	Vector3 delta = Input.mousePosition - lastMousePosition;

			// 	rotationTargetHorizontal *= Quaternion.Euler(0f, delta.x * mouseSensitivityX * 0.1f, 0f);
			// 	transform.rotation = rotationTargetHorizontal;

			// 	Quaternion nextRotationTargetVertical = rotationTargetVertical * Quaternion.Euler(-delta.y * mouseSensitivityY * 0.1f, 0f, 0f);
			// 	if (nextRotationTargetVertical.eulerAngles.x < 90 || nextRotationTargetVertical.eulerAngles.x > 270)
			// 	{
			// 		rotationTargetVertical = nextRotationTargetVertical;
			// 		cameraTransform.localRotation = rotationTargetVertical;
			// 	}

			// 	lastMousePosition = Input.mousePosition;
			// }

			if (hasKey && chestTransform != null)
			{
				CheckChestProximity();
			}

			if (chestTransform == null)
			{
				// Debug.Log("Woh Null Hai");
			}
		}

		// Key pickup logic
		void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Key"))
			{
				hasKey = true;
				keyStatusText.text = "Key: Obtained";
				Destroy(other.gameObject);
			}
		}

		// Chest unlocking logic
		void CheckChestProximity()
		{
			if (chestTransform == null || isChestUnlocked) return;

			float distance = Vector3.Distance(transform.position, chestTransform.position);

			if (distance <= chestProximityDistance)
			{
				unlockButtonUI.SetActive(true);
			}
			else
			{
				unlockButtonUI.SetActive(false);
			}
		}

		public void OnUnlockButtonPressed()
		{
			Debug.Log("Butoon Unlocked");
			if (hasKey && !isChestUnlocked)
			{
				Debug.Log("Inside if");
				isChestUnlocked = true;
				unlockButtonUI.SetActive(false);
				keyStatusText.text = "Chest unlocked! - Find The Finish Line";
				Debug.Log("Chest unlocked!");

				QFPSMazeGame mazeGame = FindAnyObjectByType<QFPSMazeGame>();
				if (mazeGame != null)
				{
					mazeGame.ActivateFinishTriggers();
				}

				// Chest swap
				if (chestClosed != null) chestClosed.SetActive(false);
				if (chestOpen != null) chestOpen.SetActive(true);

				// Reveal finish line
				if (finishLine != null) finishLine.SetActive(true);
			}
		}

		// public void OnUnlockButtonPressed()
		// {
		// 	if (hasKey && !isChestUnlocked)
		// 	{
		// 		isChestUnlocked = true;
		// 		unlockButtonUI.SetActive(false);
		// 		keyStatusText.text = "Key used: ✅";
		// 		Debug.Log("Chest unlocked!");
		// 	}
		// }

		public void setRotation(Quaternion rotation)
		{
			this.rotationTargetHorizontal = rotation;
			transform.rotation = rotation;

			rotationTargetVertical = Quaternion.identity;
			cameraTransform.rotation = rotationTargetVertical;
		}

		public bool isMoving()
		{
			return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
		}

		public void LoseLifeAndResetSkeleton()
		{
			if (isInvulnerable || playerLives <= 0) return;

			playerLives--;
			UpdateLifeUI();
			Debug.Log($"Life lost! Lives remaining: {playerLives}");

			if (playerLives <= 0)
			{
				Debug.Log("Game Over");
				// Add game over logic here or call retry
				RetryTest retryComponent = FindFirstObjectByType<RetryTest>();
				if (retryComponent != null)
				{
					retryComponent.Retry();
				}
			}
			else
			{
				// Start invulnerability period
				StartCoroutine(InvulnerabilityPeriod());

				// Reposition skeleton
				if (skeletonAI != null)
					skeletonAI.RepositionSkeleton();
			}
		}

		// Added: Invulnerability coroutine
		IEnumerator InvulnerabilityPeriod()
		{
			isInvulnerable = true;

			// Optional: Add visual feedback for invulnerability
			// You could flash the player or add a shield effect here

			yield return new WaitForSeconds(invulnerabilityDuration);

			isInvulnerable = false;
		}

		// Added: Dedicated method for updating life UI
		void UpdateLifeUI()
		{
			if (life3 != null && playerLives <= 2)
				life3.enabled = false;
			if (life2 != null && playerLives <= 1)
				life2.enabled = false;
			if (life1 != null && playerLives <= 0)
				life1.enabled = false;
		}

		public void TakeTrapDamage()
		{
			if (isInvulnerable || playerLives <= 0) return;

			playerLives--;
			Debug.Log("Player stepped on a trap! Lives remaining: " + playerLives);
			UpdateLifeUI();

			if (playerLives <= 0)
			{
				Debug.Log("Out of lives - Showing Game Over screen");

				if (gameOverScreen != null)
				{
					gameOverScreen.SetActive(true);
					Time.timeScale = 0f; // Optional: Pause the game
				}
				else
				{
					Debug.LogWarning("Game Over screen not assigned!");
				}
			}
			else
			{
				// Start invulnerability period for trap damage too
				StartCoroutine(InvulnerabilityPeriod());
			}
		}


	}
}