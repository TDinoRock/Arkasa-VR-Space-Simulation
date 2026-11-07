using UnityEngine;
using UnityEngine.Events;

namespace Arkasa
{
    // A box (this object's position/rotation and the size below) that notices when the player's head is inside.
    // The green wire box shows the zone in the Scene view.
    public class ZoneTrigger : MonoBehaviour
    {
        public Vector3 size = new Vector3(2, 2, 2);
        [Tooltip("Mission event reported on entering (optional)")]
        public string reportEvent;
        public bool onlyOnce = true;
        public UnityEvent onEnter;

        [Header("Live values (read only)")]
        public bool playerInside;
        public bool hasFired;

        void Update()
        {
            if (PlayerRig.Instance == null) return;
            Vector3 p = transform.InverseTransformPoint(PlayerRig.Instance.head.position);
            bool inside = Mathf.Abs(p.x) < size.x / 2 && Mathf.Abs(p.y) < size.y / 2 && Mathf.Abs(p.z) < size.z / 2;

            if (inside && !playerInside && !(onlyOnce && hasFired))
            {
                hasFired = true;
                if (!string.IsNullOrEmpty(reportEvent)) TaskSequencer.Report(reportEvent);
                onEnter.Invoke();
            }
            playerInside = inside;
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 1, 0.5f, 0.6f);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}
