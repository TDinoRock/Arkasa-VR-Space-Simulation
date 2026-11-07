using System.Collections.Generic;
using UnityEngine;

namespace Arkasa
{
    // Sits on the head. Looking at an Identifiable for a moment highlights it and the guide names it.
    public class GazeIdentifier : MonoBehaviour
    {
        public float range = 3000f;
        public float aimRadius = 0.06f;
        public float lookTime = 0.8f;
        public float repeatAfter = 25f;
        [Tooltip("Layers that can be looked at (leave out the Player layer so your own hands don't block the view)")]
        public LayerMask layers = ~0;

        [Header("Live values (read only)")]
        public Identifiable lookingAt;
        public float lookTimer;
        public List<string> identified = new List<string>();   // names of everything identified so far

        Dictionary<Identifiable, float> lastSpoken = new Dictionary<Identifiable, float>();

        void Update()
        {
            bool running = SafetyManager.Instance == null || SafetyManager.Instance.state == SessionState.Running;

            Identifiable target = null;
            RaycastHit hit;
            if (running && Physics.SphereCast(transform.position, aimRadius, transform.forward, out hit, range, layers, QueryTriggerInteraction.Collide))
                target = hit.collider.GetComponentInParent<Identifiable>();

            if (target != lookingAt)
            {
                if (lookingAt != null) lookingAt.SetHighlight(false);
                lookingAt = target;
                lookTimer = 0;
                if (lookingAt != null) lookingAt.SetHighlight(true);
            }
            if (lookingAt == null) return;

            lookTimer += Time.deltaTime;
            if (lookTimer < lookTime) return;

            float last;
            bool spokenRecently = lastSpoken.TryGetValue(lookingAt, out last) && Time.time - last < repeatAfter;
            if (spokenRecently) return;

            lastSpoken[lookingAt] = Time.time;
            if (!identified.Contains(lookingAt.displayName)) identified.Add(lookingAt.displayName);
            lookingAt.Describe();
        }
    }
}
