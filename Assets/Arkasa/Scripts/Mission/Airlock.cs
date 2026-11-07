using System.Collections;
using UnityEngine;

namespace Arkasa
{
    // Two-door airlock. Press Cycle while inside the chamber: both doors close, the light turns red while it
    // cycles, then the other door opens. Going outside needs the tool kit. Turns the safety tether on outside.
    public class Airlock : MonoBehaviour
    {
        public SlidingDoor innerDoor;
        public SlidingDoor outerDoor;
        public ZoneTrigger chamber;
        public Light statusLight;
        public SafetyTether tether;
        [Tooltip("Mission event needed before going outside (empty = no requirement)")]
        public string requiredEvent = "toolkit_collected";
        public float cycleTime = 4f;

        public Color readyColor = new Color(0.4f, 1f, 0.5f);
        public Color cyclingColor = new Color(1f, 0.25f, 0.15f);

        [Header("Live values (read only)")]
        public bool playerOutside;
        public bool cycling;

        void Start()
        {
            innerDoor.Open();
            outerDoor.Close();
            statusLight.color = readyColor;
            tether.enabled = false;
        }

        public void Cycle()
        {
            if (cycling) return;
            if (!chamber.playerInside)
            {
                Say("airlock_occupied");
                return;
            }
            if (!playerOutside && !TaskSequencer.HasEventHappened(requiredEvent))
            {
                Say("toolkit_needed");
                return;
            }
            StartCoroutine(CycleRoutine());
        }

        IEnumerator CycleRoutine()
        {
            cycling = true;
            innerDoor.Close();
            outerDoor.Close();
            statusLight.color = cyclingColor;
            Say("airlock_cycling");

            yield return new WaitForSeconds(cycleTime);

            playerOutside = !playerOutside;
            if (playerOutside) outerDoor.Open();
            else innerDoor.Open();
            statusLight.color = readyColor;
            tether.enabled = playerOutside;
            cycling = false;

            TaskSequencer.Report(playerOutside ? "airlock_exterior" : "airlock_interior");
        }

        void Say(string id)
        {
            if (GuideService.Instance != null) GuideService.Instance.Say(id, GuidePriority.Step);
        }
    }
}
