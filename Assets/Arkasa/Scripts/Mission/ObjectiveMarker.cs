using UnityEngine;

namespace Arkasa
{
    // A pulsing ring that floats on the current objective so the participant knows where to go.
    public class ObjectiveMarker : MonoBehaviour
    {
        public Transform target;
        public Transform head;
        public GameObject ring;
        public Vector3 offset = new Vector3(0, 0.25f, 0);
        public float size = 0.25f;

        void Start()
        {
            SetTarget(target);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            ring.SetActive(target != null);
        }

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.position + offset;
            transform.rotation = Quaternion.LookRotation(transform.position - head.position, head.up);
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 4f) * 0.12f;
            transform.localScale = Vector3.one * size * pulse;
        }
    }
}
