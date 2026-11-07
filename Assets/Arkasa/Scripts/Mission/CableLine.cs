using UnityEngine;

namespace Arkasa
{
    // Draws the loose cable from the antenna to the plug as a smooth curve.
    // Everything else about the plug is Meta / Unity components on the Cable Plug:
    //   grabbing      Meta Grab Interaction (Grabbable + Hand Grab Interactable)
    //   plugging in   Meta Snap Interactor, snapping to the Snap Interactable on the socket
    //   cable length  Unity Configurable Joint (Linear Limit)
    public class CableLine : MonoBehaviour
    {
        public Transform cableStart;        // where the cable comes out of the antenna
        public Transform plug;
        public LineRenderer line;
        public int points = 17;

        void Start()
        {
            line.positionCount = points;
        }

        // Cubic Bezier curve from the cable start to the plug.
        void LateUpdate()
        {
            Vector3 a = cableStart.position;
            Vector3 b = plug.position;
            Vector3 handleA = a + cableStart.forward * 0.4f;
            Vector3 handleB = b - plug.forward * 0.3f;
            for (int i = 0; i < points; i++)
            {
                float t = i / (float)(points - 1);
                float u = 1 - t;
                line.SetPosition(i, u * u * u * a + 3 * u * u * t * handleA + 3 * u * t * t * handleB + t * t * t * b);
            }
        }
    }
}
