using UnityEngine;
using UnityEngine.Events;

namespace Arkasa
{
    // The hinged maintenance panel at the antenna base. The latch button calls PressLatch().
    // It only opens once the tool kit has been collected (the "toolkit_collected" mission event).
    // "On Opened" turns on the cable socket (its Meta Snap Interactable).
    public class MaintenancePanel : MonoBehaviour
    {
        public Transform doorHinge;
        [Tooltip("Mission event that must have happened first (empty = no requirement)")]
        public string requiredEvent = "toolkit_collected";
        public float openAngle = -115f;
        public float swingSpeed = 90f;
        public UnityEvent onOpened;

        [Header("Live values (read only)")]
        public bool isOpen;
        public float angle;

        public void PressLatch()
        {
            if (isOpen) return;
            if (!TaskSequencer.HasEventHappened(requiredEvent))
            {
                if (GuideService.Instance != null) GuideService.Instance.Say("toolkit_needed", GuidePriority.Step);
                return;
            }
            isOpen = true;
            TaskSequencer.Report("panel_opened");
            onOpened.Invoke();
        }

        void Update()
        {
            angle = Mathf.MoveTowards(angle, isOpen ? openAngle : 0f, swingSpeed * Time.deltaTime);
            doorHinge.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }
}
