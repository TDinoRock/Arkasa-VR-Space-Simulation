using UnityEngine;

namespace Arkasa
{
    // Gives an object a name and a guide description. Looking at it (GazeIdentifier) highlights it
    // and the guide reads out the line with the matching id from GuideService.
    public class Identifiable : MonoBehaviour
    {
        public string displayName = "Object";
        [Tooltip("Id of the guide line to read, e.g. obj_toolkit")]
        public string guideLineId = "obj_cube";

        [Header("Highlight")]
        public bool canHighlight = true;
        public Renderer[] renderers;
        public Material highlightMaterial;

        [Header("Live values (read only)")]
        public bool highlighted;

        Material[] normalMaterials;

        void Awake()
        {
            normalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) normalMaterials[i] = renderers[i].sharedMaterial;
        }

        public void SetHighlight(bool on)
        {
            if (!canHighlight || highlightMaterial == null || on == highlighted) return;
            highlighted = on;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].sharedMaterial = on ? highlightMaterial : normalMaterials[i];
        }

        public void Describe()
        {
            if (GuideService.Instance != null) GuideService.Instance.Say(guideLineId, GuidePriority.Info);
        }
    }
}
