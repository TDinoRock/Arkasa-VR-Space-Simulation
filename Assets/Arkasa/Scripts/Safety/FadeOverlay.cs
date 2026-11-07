using UnityEngine;
using UnityEngine.UI;

namespace Arkasa
{
    // A full-view colour fade with a message, attached to the head camera.
    // Used by the emergency stop (priority 100) and tracking loss (priority 10); the higher priority wins.
    public class FadeOverlay : MonoBehaviour
    {
        public Image fadeImage;
        public Text messageText;

        [Header("Live values (read only)")]
        public float alpha;
        public int shownBy = -1;   // priority of whoever is showing it (-1 = hidden)

        Color color = Color.black;
        float fadeFrom, fadeTo, fadeStart, fadeDuration = 0.01f;

        void Awake()
        {
            Apply(0);
        }

        public void Show(Color newColor, string message, float duration, int priority)
        {
            if (priority < shownBy) return;
            shownBy = priority;
            color = newColor;
            messageText.text = message;
            StartFade(newColor.a, duration);
        }

        public void Hide(int priority, float duration)
        {
            if (priority < shownBy) return;
            shownBy = -1;
            messageText.text = "";
            StartFade(0f, duration);
        }

        void StartFade(float target, float duration)
        {
            fadeFrom = alpha;
            fadeTo = target;
            fadeStart = Time.unscaledTime;
            fadeDuration = Mathf.Max(0.01f, duration);
        }

        void Update()
        {
            float t = Mathf.Clamp01((Time.unscaledTime - fadeStart) / fadeDuration);
            Apply(Mathf.Lerp(fadeFrom, fadeTo, t));
        }

        void Apply(float a)
        {
            alpha = a;
            Color c = color;
            c.a = a;
            fadeImage.color = c;
            fadeImage.enabled = a > 0.001f;
            messageText.enabled = a > 0.3f;
        }
    }
}
