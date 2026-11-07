using UnityEngine;

namespace Arkasa
{
    // Shows a bad/good status: text, lamp material and light colour (e.g. "ANTENNA SIGNAL: LOST" -> "OK").
    // Call SetGood() from an event (the cable's "On Connected"). Optionally spins something when good (the dish).
    public class StatusDisplay : MonoBehaviour
    {
        public TextMesh label;
        public string badText = "ANTENNA SIGNAL: LOST";
        public string goodText = "ANTENNA SIGNAL: OK";
        public Color badColor = new Color(1f, 0.2f, 0.15f);
        public Color goodColor = new Color(0.2f, 1f, 0.35f);

        [Header("Lamp (optional)")]
        public Renderer lamp;
        public Material badMaterial;
        public Material goodMaterial;
        public Light lampLight;
        public bool blinkWhenBad = true;

        [Header("Spin when good (optional)")]
        public Transform spinObject;
        public float spinSpeed = 8f;

        [Header("Live values (read only)")]
        public bool isGood;

        void Start()
        {
            Show(isGood);
        }

        public void SetGood()
        {
            Show(true);
        }

        void Show(bool good)
        {
            isGood = good;
            if (label != null)
            {
                label.text = good ? goodText : badText;
                label.color = good ? goodColor : badColor;
            }
            if (lamp != null) lamp.sharedMaterial = good ? goodMaterial : badMaterial;
            if (lampLight != null) lampLight.color = good ? goodColor : badColor;
        }

        void Update()
        {
            if (lampLight != null)
            {
                bool blinkOff = !isGood && blinkWhenBad && Mathf.Sin(Time.time * 6f) < 0;
                lampLight.intensity = blinkOff ? 0.2f : 1.2f;
            }
            if (isGood && spinObject != null) spinObject.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.Self);
        }
    }
}
