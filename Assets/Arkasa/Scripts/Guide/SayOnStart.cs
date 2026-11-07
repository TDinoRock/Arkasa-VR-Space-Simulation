using UnityEngine;

namespace Arkasa
{
    // Has the guide say one line shortly after the scene starts (e.g. the Playground welcome).
    public class SayOnStart : MonoBehaviour
    {
        public string lineId = "pg_welcome";
        public float delay = 1.5f;

        void Start()
        {
            Invoke("SayIt", delay);
        }

        void SayIt()
        {
            GuideService.Instance.Say(lineId, GuidePriority.Step);
        }
    }
}
