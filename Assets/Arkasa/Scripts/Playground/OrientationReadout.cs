using UnityEngine;

namespace Arkasa
{
    // Playground station 1: shows the participant's orientation compared to the station's "up",
    // and turns an arrow to point along the participant's own up.
    public class OrientationReadout : MonoBehaviour
    {
        public PlayerRig rig;
        public TextMesh text;
        public Transform yourUpArrow;

        [Header("Live values (read only)")]
        public string state;
        public float tiltFromUpright;
        public float pitch;
        public float roll;
        public float maxTiltReached;

        void Update()
        {
            Transform head = rig.head;

            tiltFromUpright = Vector3.Angle(head.up, Vector3.up);
            pitch = Mathf.Asin(Mathf.Clamp(Vector3.Dot(head.forward, Vector3.up), -1, 1)) * Mathf.Rad2Deg;
            roll = Vector3.SignedAngle(Vector3.ProjectOnPlane(Vector3.up, head.forward), head.up, head.forward);
            maxTiltReached = Mathf.Max(maxTiltReached, tiltFromUpright);

            if (tiltFromUpright < 30) state = "UPRIGHT";
            else if (tiltFromUpright > 150) state = "UPSIDE DOWN";
            else if (tiltFromUpright > 60 && tiltFromUpright < 120) state = "SIDEWAYS";
            else state = "TILTED";

            string tracking = !rig.headsetPresent ? "NO HEADSET" : rig.headsetTracked ? "OK" : "LOST";
            text.text = "BODY ORIENTATION\n" + state +
                        "\ntilt from upright " + Mathf.RoundToInt(tiltFromUpright) + "°" +
                        "\npitch " + Mathf.RoundToInt(pitch) + "°  roll " + Mathf.RoundToInt(roll) + "°" +
                        "\nmax tilt reached " + Mathf.RoundToInt(maxTiltReached) + "°" +
                        "\ntracking " + tracking;

            yourUpArrow.rotation = Quaternion.FromToRotation(Vector3.up, head.up);
        }
    }
}
