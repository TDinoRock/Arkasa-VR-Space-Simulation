using UnityEngine;

namespace Arkasa
{
    // EVA safety tether. If the participant drifts further than maxLength from the anchor, it gently pulls them back.
    // The Airlock turns this on when going outside and off when back inside.
    public class SafetyTether : MonoBehaviour
    {
        public PlayerRig rig;
        public Transform anchor;
        public LineRenderer line;
        public float maxLength = 15f;
        public float pullSpeed = 0.8f;

        [Header("Live values (read only)")]
        public float distance;

        float lastWarning = -100f;

        void OnEnable() { line.enabled = true; }
        void OnDisable() { line.enabled = false; }

        void FixedUpdate()
        {
            if (rig.IsLocked()) return;

            Vector3 toAnchor = anchor.position - rig.head.position;
            distance = toAnchor.magnitude;
            float tooFar = distance - maxLength;
            if (tooFar <= 0) return;

            Vector3 pull = toAnchor.normalized * Mathf.Min(pullSpeed + tooFar * 0.2f, 1.2f);
            rig.body.linearVelocity = Vector3.Lerp(rig.body.linearVelocity, pull, 0.05f);

            if (Time.time - lastWarning > 20f)
            {
                lastWarning = Time.time;
                if (GuideService.Instance != null) GuideService.Instance.Say("tether", GuidePriority.Step);
            }
        }

        void LateUpdate()
        {
            line.SetPosition(0, anchor.position);
            line.SetPosition(1, rig.head.position - rig.head.up * 0.45f);
        }
    }
}
