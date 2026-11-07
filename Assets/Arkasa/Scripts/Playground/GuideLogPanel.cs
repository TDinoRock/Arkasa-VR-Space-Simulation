using UnityEngine;

namespace Arkasa
{
    // Playground station 6: lists the guide's most recent lines and the session state, to check the guide works.
    public class GuideLogPanel : MonoBehaviour
    {
        public GuideService guide;
        public SafetyManager safety;
        public TextMesh text;
        public int linesShown = 8;

        void Update()
        {
            string log = "GUIDE LOG   [session " + safety.state + "]";
            int first = Mathf.Max(0, guide.history.Count - linesShown);
            for (int i = first; i < guide.history.Count; i++)
            {
                GuideLine line = guide.FindLine(guide.history[i]);
                string words = line != null ? line.text : "";
                if (words.Length > 60) words = words.Substring(0, 57) + "...";
                log += "\n[" + guide.history[i] + "] " + words;
            }
            text.text = log;
        }
    }
}
