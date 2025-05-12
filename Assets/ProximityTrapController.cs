using UnityEngine;

namespace qtools.qmaze.example1
{
    public class ProximityTrapController : MonoBehaviour
    {
        public float damageRadius = 1.5f;      // How close the player needs to be
        public float checkInterval = 0.2f;     // How often to check (seconds)
        public float trapCooldown = 2f;        // Time before trap can damage again

        private QFPSController playerController;
        private bool trapActive = true;
        private float timer = 0f;

        void Start()
        {
            playerController = FindFirstObjectByType<QFPSController>();
            if (playerController == null)
            {
                Debug.LogError("ProximityTrapController: Player controller not found!");
            }
        }

        void Update()
        {
            if (!trapActive || playerController == null) return;

            timer += Time.deltaTime;

            // Only check periodically to save performance
            if (timer >= checkInterval)
            {
                timer = 0f;

                // Check distance between trap and player
                float distance = Vector3.Distance(transform.position, playerController.transform.position);

                if (distance <= damageRadius)
                {
                    Debug.Log("Player in proximity of trap! Distance: " + distance);
                    playerController.TakeTrapDamage();
                    StartCoroutine(TrapCooldown());
                }
            }
        }

        private System.Collections.IEnumerator TrapCooldown()
        {
            trapActive = false;
            Debug.Log("Trap deactivated for cooldown");
            yield return new WaitForSeconds(trapCooldown);
            trapActive = true;
            Debug.Log("Trap reactivated");
        }

        // Optional: Visualize the damage radius in the editor
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
}