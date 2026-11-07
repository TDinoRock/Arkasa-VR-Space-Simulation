using UnityEngine;

namespace Arkasa
{
    // An estimate of the torso. In the gyroscope rig the head turns with the body, so this follows the
    // head's full rotation (smoothed, so quick head glances are ignored). The stowed tool kit hangs off it.
    public class BodyFrame : MonoBehaviour
    {
        public Transform head;
        public float followSpeed = 3f;
        public float neckLength = 0.25f;

        void LateUpdate()
        {
            float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, head.rotation, t);
            transform.position = head.position - transform.up * neckLength;
        }
    }
}
