using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Arkasa
{
    public enum GuidePriority { Info = 0, Step = 1, Safety = 2 }

    // One scripted line the guide can say. Edit these in the Inspector on the Guide object.
    [System.Serializable]
    public class GuideLine
    {
        public string id;
        [TextArea(2, 4)] public string text;
        public AudioClip voice;
    }

    // The scripted AI guide: plays a voice clip and shows a subtitle for each line.
    // Other scripts call Say("line_id"). Safety lines interrupt everything; step lines interrupt
    // descriptions; descriptions wait their turn. (A live AI could replace Say() later.)
    public class GuideService : MonoBehaviour
    {
        public static GuideService Instance;

        public AudioSource voiceSource;
        public GameObject subtitlePanel;
        public Text subtitleText;
        public float subtitleHoldTime = 1.5f;

        [Header("Script")]
        public List<GuideLine> lines = new List<GuideLine>();

        [Header("Live values (read only)")]
        public string currentLineId;
        public List<string> history = new List<string>();   // most recent last

        GuidePriority currentPriority;
        float busyUntil;
        Queue<GuideLine> waiting = new Queue<GuideLine>();

        void Awake()
        {
            Instance = this;
            subtitlePanel.SetActive(false);
        }

        public GuideLine FindLine(string id)
        {
            foreach (GuideLine line in lines)
                if (line.id == id) return line;
            return null;
        }

        public void Say(string id, GuidePriority priority)
        {
            GuideLine line = FindLine(id);
            if (line == null)
            {
                Debug.LogWarning("Guide line not found: " + id);
                return;
            }

            bool busy = Time.unscaledTime < busyUntil;
            if (busy && id == currentLineId) return;   // already saying it

            if (priority == GuidePriority.Safety) waiting.Clear();

            if (!busy || priority > currentPriority || priority == GuidePriority.Step)
            {
                Play(line, priority);
            }
            else if (waiting.Count < 2)
            {
                waiting.Enqueue(line);   // descriptions wait, but don't pile up
            }
        }

        // Convenience for UnityEvents (buttons): say a step line.
        public void SayStep(string id)
        {
            Say(id, GuidePriority.Step);
        }

        void Play(GuideLine line, GuidePriority priority)
        {
            currentLineId = line.id;
            currentPriority = priority;

            float duration = Mathf.Max(2f, line.text.Split(' ').Length * 0.38f);
            voiceSource.Stop();
            if (line.voice != null)
            {
                voiceSource.clip = line.voice;
                voiceSource.Play();
                duration = line.voice.length;
            }
            busyUntil = Time.unscaledTime + duration + 0.3f;

            subtitleText.text = line.text;
            subtitlePanel.SetActive(true);

            history.Add(line.id);
            if (history.Count > 20) history.RemoveAt(0);
        }

        void Update()
        {
            if (Time.unscaledTime < busyUntil) return;

            if (waiting.Count > 0)
            {
                Play(waiting.Dequeue(), GuidePriority.Info);
            }
            else if (subtitlePanel.activeSelf && Time.unscaledTime > busyUntil + subtitleHoldTime)
            {
                subtitlePanel.SetActive(false);
                currentLineId = "";
            }
        }
    }
}
