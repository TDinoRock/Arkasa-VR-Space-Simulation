using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Arkasa
{
    // Moving in zero gravity, two ways:
    //  1. Handrails: hand-over-hand. Handrails and walls only have Meta's Hand Grab Interactable (no Grabbable,
    //     so they can't move). This watches Meta's two Hand Grab Interactors: when a hand grabs something fixed
    //     like that, the body moves so the hand stays where it grabbed.
    //  2. Swimming: with nothing to hold, sweeping an open palm pushes the body the opposite way (like pushing
    //     water). Only the palm side pushes, so bringing the hand back edge-first or back-of-hand-first doesn't
    //     undo the stroke.
    // Letting go / stopping keeps the momentum (the Rigidbody's Linear Damping slows it down, and PlayerRig's
    // Max Speed caps it). The body is only moved, never rotated.
    public class ZeroGLocomotion : MonoBehaviour
    {
        public PlayerRig rig;
        [Tooltip("OVRComprehensiveInteractionRig > ComprehensiveInteractorsLeft > ... > HandGrabInteractor")]
        public HandGrabInteractor leftHandGrab;
        [Tooltip("OVRComprehensiveInteractionRig > ComprehensiveInteractorsRight > ... > HandGrabInteractor")]
        public HandGrabInteractor rightHandGrab;

        [Header("Handrails")]
        public float maxPullSpeed = 3f;
        [Range(0, 1)] public float keepMomentumOnRelease = 0.9f;

        [Header("Swimming (arm strokes, no handrail needed)")]
        public bool swimEnabled = true;
        [Tooltip("How strongly a palm stroke pushes the body. Higher = faster swimming.")]
        public float swimStrength = 2f;
        [Tooltip("Hand speed (m/s) below this does nothing, so small movements and tracking jitter don't move you")]
        public float minStrokeSpeed = 0.4f;
        [Tooltip("Hand speed (m/s) above this is a tracking glitch and is ignored")]
        public float maxStrokeSpeed = 5f;

        [Header("Live values (read only)")]
        public bool leftHolding;
        public bool rightHolding;
        public Vector3 grabPoint;           // world point the pulling hand is holding
        public float leftStrokeSpeed;       // how fast each hand is moving relative to the body
        public float rightStrokeSpeed;

        IHand holdingHand;                  // the hand currently pulling (null = floating)
        IHand otherHoldingHand;
        Vector3 recentVelocity;
        Vector3 lastLeftLocal, lastRightLocal;   // last hand positions relative to the body
        bool hadLeft, hadRight;

        void OnEnable()
        {
            leftHandGrab.WhenStateChanged += LeftGrabChanged;
            rightHandGrab.WhenStateChanged += RightGrabChanged;
        }

        void OnDisable()
        {
            leftHandGrab.WhenStateChanged -= LeftGrabChanged;
            rightHandGrab.WhenStateChanged -= RightGrabChanged;
        }

        void LeftGrabChanged(InteractorStateChangeArgs change) { GrabChanged(leftHandGrab, change); }
        void RightGrabChanged(InteractorStateChangeArgs change) { GrabChanged(rightHandGrab, change); }

        void GrabChanged(HandGrabInteractor grab, InteractorStateChangeArgs change)
        {
            if (change.NewState == InteractorState.Select && IsFixed(grab.SelectedInteractable))
                StartHolding(grab.Hand);
            else if (change.PreviousState == InteractorState.Select)
                StopHolding(grab.Hand);
        }

        // Something without a Grabbable can't be carried, so grabbing it means holding on to the station.
        static bool IsFixed(HandGrabInteractable interactable)
        {
            return interactable != null && interactable.GetComponentInParent<Grabbable>() == null;
        }

        void StartHolding(IHand hand)
        {
            if (holdingHand != null && holdingHand != hand) otherHoldingHand = holdingHand;
            holdingHand = hand;
            grabPoint = HandPosition(hand);
            UpdateLiveValues();
        }

        void StopHolding(IHand hand)
        {
            if (otherHoldingHand == hand) otherHoldingHand = null;
            if (holdingHand == hand)
            {
                if (otherHoldingHand != null)
                {
                    // The other hand is still holding: continue from it.
                    holdingHand = otherHoldingHand;
                    otherHoldingHand = null;
                    grabPoint = HandPosition(holdingHand);
                }
                else
                {
                    // Let go completely: keep (most of) the momentum.
                    holdingHand = null;
                    if (!rig.IsLocked())
                        rig.body.linearVelocity = Vector3.ClampMagnitude(recentVelocity * keepMomentumOnRelease, rig.maxSpeed);
                }
            }
            UpdateLiveValues();
        }

        // Swimming runs every frame, because that is when Meta updates the hand poses.
        void Update()
        {
            bool canSwim = swimEnabled && holdingHand == null && !rig.IsLocked() && Time.deltaTime > 0;
            leftStrokeSpeed = Stroke(rig.leftHand, leftHandGrab, canSwim, ref lastLeftLocal, ref hadLeft);
            rightStrokeSpeed = Stroke(rig.rightHand, rightHandGrab, canSwim, ref lastRightLocal, ref hadRight);
        }

        // Pushes the body for one hand's stroke this frame. Returns the hand's speed (for the live values).
        float Stroke(Hand hand, HandGrabInteractor grab, bool canSwim, ref Vector3 lastLocal, ref bool hadLast)
        {
            Pose wrist;
            if (hand == null || !hand.IsTrackedDataValid || !hand.GetRootPose(out wrist))
            {
                hadLast = false;
                return 0;
            }

            // Hand movement relative to the body (so drifting along doesn't count as a stroke).
            Vector3 local = rig.transform.InverseTransformPoint(wrist.position);
            Vector3 handVelocity = hadLast ? rig.transform.TransformDirection(local - lastLocal) / Time.deltaTime : Vector3.zero;
            lastLocal = local;
            hadLast = true;

            float speed = handVelocity.magnitude;
            bool holdingSomething = grab.State == InteractorState.Select;
            if (!canSwim || holdingSomething || speed < minStrokeSpeed || speed > maxStrokeSpeed) return speed;

            // Only the palm pushes: full push when the palm leads the movement, none when the back or edge leads.
            Vector3 palmDirection = wrist.rotation * (hand.Handedness == Handedness.Left ? Constants.LeftPalmar : Constants.RightPalmar);
            float palmLeading = Mathf.Clamp01(Vector3.Dot(palmDirection, handVelocity / speed));

            rig.body.linearVelocity -= handVelocity * palmLeading * swimStrength * Time.deltaTime;
            return speed;
        }

        void FixedUpdate()
        {
            if (holdingHand == null)
            {
                recentVelocity = rig.body.linearVelocity;
                return;
            }
            if (rig.IsLocked())
            {
                holdingHand = null;
                otherHoldingHand = null;
                UpdateLiveValues();
                return;
            }

            // Where must the body be so the hand is exactly at the grab point?
            Vector3 handOffset = HandPosition(holdingHand) - rig.body.position;
            Vector3 targetPosition = grabPoint - handOffset;
            Vector3 velocity = (targetPosition - rig.body.position) / Time.fixedDeltaTime;

            rig.body.linearVelocity = Vector3.ClampMagnitude(velocity, maxPullSpeed);
            recentVelocity = Vector3.Lerp(recentVelocity, rig.body.linearVelocity, 0.25f);
        }

        static Vector3 HandPosition(IHand hand)
        {
            Pose pose;
            hand.GetRootPose(out pose);
            return pose.position;
        }

        void UpdateLiveValues()
        {
            leftHolding = IsHeld(Handedness.Left);
            rightHolding = IsHeld(Handedness.Right);
        }

        bool IsHeld(Handedness side)
        {
            return (holdingHand != null && holdingHand.Handedness == side) ||
                   (otherHoldingHand != null && otherHoldingHand.Handedness == side);
        }
    }
}
