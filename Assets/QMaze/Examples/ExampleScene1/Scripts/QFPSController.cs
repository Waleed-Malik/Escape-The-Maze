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
        private bool isChestUnlocked = false;
		public int playerLives = 3;
		public Image life1;
		public Image life2;
		public Image life3;

		public SkeletonAI skeletonAI; // Drag the skeleton AI script here


		// private bool isDragging = false;
		// private Vector3 lastMousePosition;

		void Start () 
		{
			rotationTargetHorizontal = transform.rotation;
			rigidBody = GetComponent<Rigidbody>();
			cameraTransform = transform.GetChild(0);
			rotationTargetVertical = cameraTransform.rotation;
		}

		void Update () 
		{
			if(chestTransform == null){
			chestTransform = GameObject.Find("treasure_chest_closed")?.GetComponent<Transform>();

			}

			Vector3 velocity = transform.right   * Input.GetAxis("Horizontal") * moveScaleX;
					velocity+= transform.forward * Input.GetAxis("Vertical")   * moveScaleY;
			velocity = Vector3.ClampMagnitude(velocity, moveMaxSpeed);
			velocity *= moveLerp;
			velocity.y = rigidBody.linearVelocity.y;
			rigidBody.linearVelocity = velocity;

			rotationTargetHorizontal *= Quaternion.Euler (0, Input.GetAxis("Mouse X") * mouseSensitivityX, 0f);
			transform.rotation = rotationTargetHorizontal;

			Quaternion nextRotationTargetVertical = rotationTargetVertical * Quaternion.Euler(Input.GetAxis("Mouse Y") * mouseSensitivityY, 0, 0);
			if (nextRotationTargetVertical.eulerAngles.x < 90 || nextRotationTargetVertical.eulerAngles.x > 270)
			{
				rotationTargetVertical = nextRotationTargetVertical;
				cameraTransform.localRotation = rotationTargetVertical;
			}

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
				Debug.Log("Checking Proximity");
                CheckChestProximity();
            }

			if (chestTransform == null)
			{
				Debug.Log("Woh Null Hai");
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
            if (hasKey && !isChestUnlocked)
            {
                isChestUnlocked = true;
                unlockButtonUI.SetActive(false);
                keyStatusText.text = "Key used: ✅";
                Debug.Log("Chest unlocked!");
            }
        }

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
			if (playerLives <= 0) return;

			playerLives--;

			if (playerLives == 2)
			{
				life3.enabled = false;
			}
			else if (playerLives == 1)
			{
				life2.enabled = false;
			}
			else if (playerLives == 0)
			{
				life1.enabled = false;
				Debug.Log("Game Over");
				// Add game over logic here
				return;
			}

			skeletonAI.RepositionSkeleton();
		}


	}
}