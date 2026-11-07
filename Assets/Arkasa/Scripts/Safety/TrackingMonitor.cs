using UnityEngine;

namespace Arkasa
{
    // Watches headset tracking (it can drop out when the participant is upside down or spinning in the rig).
    // When lost: freeze movement, keep the last view and dim it with a message. When back: fade in again.
    // Also reminds the participant when both hands have been out of view for a while.
    public class TrackingMonitor : MonoBehaviour
    {
        public PlayerRig rig;
        public FadeOverlay overlay;

        [Header("Timing (seconds)")]
        public float lostAfter = 0.15f;
        public float regainedAfter = 0.5f;
        public float handsLostAfter = 5f;
        public float handsReminderEvery = 30f;

        [Header("Message")]
        public Color dimColor = new Color(0.05f, 0.07f, 0.1f, 0.85f);
        public string lostMessage = "TRACKING LOST\nPlease hold still";

        [Header("Live values (read only)")]
        public bool trackingLost;
        public float lostTimer;
        public float okTimer;
        public float handsLostTimer;

        float lastHandsReminder = -100f;

        void Update()
        {
            if (!rig.headsetPresent) return;
            float dt = Time.unscaledDeltaTime;

            if (rig.headsetTracked) { okTimer += dt; lostTimer = 0; }
            else { lostTimer += dt; okTimer = 0; }

            if (!trackingLost && lostTimer > lostAfter)
            {
                trackingLost = true;
                rig.lockedByTracking = true;
                rig.Stop();
                overlay.Show(dimColor, lostMessage, 0.25f, 10);
                Say("tracking_lost", GuidePriority.Safety);
            }
            else if (trackingLost && okTimer > regainedAfter)
            {
                trackingLost = false;
                rig.lockedByTracking = false;
                overlay.Hide(10, 0.6f);
                Say("tracking_ok", GuidePriority.Info);
            }

            bool running = SafetyManager.Instance == null || SafetyManager.Instance.state == SessionState.Running;
            bool handsLost = !rig.leftHand.IsTrackedDataValid && !rig.rightHand.IsTrackedDataValid;
            if (handsLost && running && !trackingLost) handsLostTimer += dt;
            else handsLostTimer = 0;

            if (handsLostTimer > handsLostAfter && Time.unscaledTime - lastHandsReminder > handsReminderEvery)
            {
                lastHandsReminder = Time.unscaledTime;
                Say("hands_lost", GuidePriority.Step);
            }
        }

        void Say(string id, GuidePriority priority)
        {
            if (GuideService.Instance != null) GuideService.Instance.Say(id, priority);
        }
    }
}
