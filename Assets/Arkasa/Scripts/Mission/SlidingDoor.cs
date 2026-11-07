using UnityEngine;

namespace Arkasa
{
    // A hatch that slides sideways when open. Place it in the closed position in the scene.
    public class SlidingDoor : MonoBehaviour
    {
        public Vector3 openOffset = new Vector3(1.45f, 0, 0);
        public float speed = 1.2f;
        public bool isOpen;

        Vector3 closedPosition;

        void Awake()
        {
            closedPosition = transform.localPosition;
        }

        public void Open() { isOpen = true; }
        public void Close() { isOpen = false; }

        void Update()
        {
            Vector3 target = isOpen ? closedPosition + openOffset : closedPosition;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
        }
    }
}
