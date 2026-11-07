using Oculus.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Arkasa
{
    public enum SessionState { Running, Paused, Stopped }

    // Pause and emergency stop. Always reachable:
    //   Participant: wrist menu (look at the left palm), the pause panel, or both palms toward the face for 1.5 s (pause)
    //   Operator:    tools/operator.ps1 over Wi-Fi (OperatorLink), or the keyboard: P pause, Esc emergency stop, F5 reset
    // Emergency stop freezes all motion and fades to a still grey screen.
    public class SafetyManager : MonoBehaviour
    {
        public static SafetyManager Instance;

        public PlayerRig rig;
        public GameObject pausePanel;
        public FadeOverlay overlay;

        [Header("Emergency stop screen")]
        public Color stopColor = new Color(0.32f, 0.34f, 0.36f, 1f);
        public string stopMessage = "SESSION STOPPED\nStay still. The operator will assist you.";
        public float stopFadeTime = 0.35f;

        [Header("Palms-to-face pause gesture")]
        public bool palmsGestureEnabled = true;
        [Tooltip("Meta Active State Group that is active while both palms face the face (Systems > Safety > Hand Poses)")]
        public ActiveStateGroup bothPalmsTowardFace;
        public float palmsHoldTime = 1.5f;

        [Header("Live values (read only)")]
        public SessionState state = SessionState.Running;
        public float palmsTimer;

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        void Start()
        {
            pausePanel.SetActive(false);
        }

        public void Pause()
        {
            if (state != SessionState.Running) return;
            state = SessionState.Paused;
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            Say("paused");
        }

        public void Resume()
        {
            if (state != SessionState.Paused) return;
            state = SessionState.Running;
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            Say("resumed");
        }

        public void TogglePause()
        {
            if (state == SessionState.Running) Pause();
            else if (state == SessionState.Paused) Resume();
        }

        public void EmergencyStop()
        {
            if (state == SessionState.Stopped) return;
            state = SessionState.Stopped;
            Time.timeScale = 0f;

            rig.lockedBySafety = true;
            rig.Stop();
            foreach (Rigidbody body in FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude))
            {
                if (body.isKinematic) continue;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            pausePanel.SetActive(false);
            overlay.Show(stopColor, stopMessage, stopFadeTime, 100);
            Say("estop");
        }

        public void ResetSession()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void Update()
        {
            Keyboard keys = Keyboard.current;
            if (keys != null)
            {
                if (keys.pKey.wasPressedThisFrame) TogglePause();
                if (keys.escapeKey.wasPressedThisFrame) EmergencyStop();
                if (keys.f5Key.wasPressedThisFrame) ResetSession();
            }

            CheckPalmsGesture();
        }

        // Both open palms facing the face, in view, held for a moment = pause.
        void CheckPalmsGesture()
        {
            if (!palmsGestureEnabled || !rig.headsetPresent || state != SessionState.Running)
            {
                palmsTimer = 0;
                return;
            }

            if (bothPalmsTowardFace.Active) palmsTimer += Time.unscaledDeltaTime;
            else palmsTimer = 0;

            if (palmsTimer > palmsHoldTime)
            {
                palmsTimer = 0;
                Pause();
            }
        }

        void Say(string id)
        {
            if (GuideService.Instance != null) GuideService.Instance.Say(id, GuidePriority.Safety);
        }
    }
}
