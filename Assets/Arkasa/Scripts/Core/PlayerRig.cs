using Oculus.Interaction.Input;
using UnityEngine;

namespace Arkasa
{
    // Sits on Meta's Camera Rig building block ([BuildingBlock] Camera Rig).
    // The Camera Rig shows the headset pose 1:1, so sideways and upside down in the gyroscope rig look right.
    // This script adds the zero-g body: a gravity-free Rigidbody on the same object.
    // The rig is only ever MOVED, never ROTATED, so the game never changes the participant's real orientation.
    public class PlayerRig : MonoBehaviour
    {
        public static PlayerRig Instance;

        [Header("Meta rig parts")]
        public OVRCameraRig cameraRig;
        [Tooltip("CenterEyeAnchor")]
        public Transform head;
        [Tooltip("Interaction SDK hand (OVRComprehensiveInteractionRig > ComprehensiveInteractorsLeft)")]
        public Hand leftHand;
        [Tooltip("Interaction SDK hand (OVRComprehensiveInteractionRig > ComprehensiveInteractorsRight)")]
        public Hand rightHand;

        [Header("Zero-g body (on this object)")]
        public Rigidbody body;
        public SphereCollider headCollider;
        public Transform bodyFrame;
        public float maxSpeed = 1.6f;

        [Header("Live values (read only)")]
        public bool headsetPresent;
        public bool headsetTracked = true;
        public bool lockedBySafety;
        public bool lockedByTracking;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            OVRManager.TrackingLost += OnTrackingLost;
            OVRManager.TrackingAcquired += OnTrackingAcquired;
        }

        void OnDisable()
        {
            OVRManager.TrackingLost -= OnTrackingLost;
            OVRManager.TrackingAcquired -= OnTrackingAcquired;
        }

        void OnTrackingLost() { headsetTracked = false; }
        void OnTrackingAcquired() { headsetTracked = true; }

        void Update()
        {
            headsetPresent = OVRManager.isHmdPresent;
        }

        void FixedUpdate()
        {
            // Keep the body collider around the head.
            headCollider.center = transform.InverseTransformPoint(head.position);

            if (IsLocked())
            {
                body.linearVelocity = Vector3.zero;
                return;
            }

            if (body.linearVelocity.magnitude > maxSpeed)
                body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
        }

        public bool IsLocked()
        {
            return lockedBySafety || lockedByTracking;
        }

        public void Stop()
        {
            body.linearVelocity = Vector3.zero;
        }
    }
}
