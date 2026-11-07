using System.Collections.Generic;
using UnityEngine;

namespace Arkasa
{
    // One step of the mission. Edit the steps in the Inspector on the Mission object.
    [System.Serializable]
    public class MissionStep
    {
        public string id;
        [Tooltip("Guide line read when the step starts (and repeated as a hint)")]
        public string instructionLine;
        [Tooltip("The step is done when this event is reported, e.g. toolkit_collected")]
        public string completeWhen;
        [Tooltip("Guide line read when the step is done")]
        public string doneLine;
        [Tooltip("What the objective marker points at")]
        public Transform objective;
    }

    // Runs the mission one step at a time: say the instruction, wait for the step's event, confirm, next.
    // Objects report events with TaskSequencer.Report("event_name") (e.g. the airlock reports "airlock_exterior").
    public class TaskSequencer : MonoBehaviour
    {
        public static TaskSequencer Instance;

        public ObjectiveMarker marker;
        public GazeIdentifier gaze;
        public List<MissionStep> steps = new List<MissionStep>();

        [Header("Timing (seconds)")]
        public float firstStepDelay = 1.5f;
        public float delayBetweenSteps = 3.5f;
        public float repeatHintAfter = 40f;

        [Header("Look-around step")]
        public int objectsToIdentify = 3;
        public float lookAroundMaxTime = 45f;

        [Header("Live values (read only)")]
        public int currentStep = -1;
        public bool finished;
        public float elapsedTime;
        public float stepTime;
        public List<string> reportedEvents = new List<string>();

        bool waitingForNextStep;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            waitingForNextStep = true;
            Invoke("NextStep", firstStepDelay);
        }

        // Called by mission objects. Safe to call even when there is no mission (e.g. in the Playground).
        public static void Report(string eventName)
        {
            if (Instance != null) Instance.ReportEvent(eventName);
        }

        public void ReportEvent(string eventName)
        {
            if (!reportedEvents.Contains(eventName)) reportedEvents.Add(eventName);
            if (waitingForNextStep || finished || currentStep < 0) return;
            if (eventName == steps[currentStep].completeWhen) CompleteStep();
        }

        // True if the event has been reported (or no event / no mission running, e.g. in the Playground).
        public static bool HasEventHappened(string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || Instance == null) return true;
            return Instance.HasHappened(eventName);
        }

        public bool HasHappened(string eventName)
        {
            return reportedEvents.Contains(eventName);
        }

        public string CurrentStepId()
        {
            if (finished) return "complete";
            if (currentStep < 0 || currentStep >= steps.Count) return "-";
            return steps[currentStep].id;
        }

        void NextStep()
        {
            waitingForNextStep = false;
            currentStep++;
            stepTime = 0;

            if (currentStep >= steps.Count)
            {
                finished = true;
                marker.SetTarget(null);
                Debug.Log("Mission complete in " + Mathf.RoundToInt(elapsedTime) + " s");
                return;
            }

            MissionStep step = steps[currentStep];
            Say(step.instructionLine);
            marker.SetTarget(step.objective);

            // Already done earlier (out of order)? Then complete straight away.
            if (HasHappened(step.completeWhen)) CompleteStep();
        }

        void CompleteStep()
        {
            waitingForNextStep = true;
            Say(steps[currentStep].doneLine);
            Invoke("NextStep", delayBetweenSteps);
        }

        void Update()
        {
            if (finished) return;
            elapsedTime += Time.deltaTime;
            if (waitingForNextStep || currentStep < 0) return;

            MissionStep step = steps[currentStep];
            stepTime += Time.deltaTime;

            // The look-around step finishes after identifying a few objects (or after a while).
            if (step.completeWhen == "looked_around")
                if (gaze.identified.Count >= objectsToIdentify || stepTime > lookAroundMaxTime)
                    ReportEvent("looked_around");

            if (stepTime > repeatHintAfter && !waitingForNextStep)
            {
                stepTime = 0;
                Say(step.instructionLine);
            }
        }

        void Say(string id)
        {
            if (!string.IsNullOrEmpty(id) && GuideService.Instance != null) GuideService.Instance.Say(id, GuidePriority.Step);
        }
    }
}
