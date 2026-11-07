# Orbital EVA Training — Prototype Guide

Greybox functional prototype built from [brief.md](brief.md).

**Rule of thumb:** if Meta's SDK has a component for it, the scene uses Meta's component. Arkasa scripts only cover what Meta doesn't: zero-g body movement, mission steps, the guide, safety, and the station's doors and displays.

## Scenes (`Assets/Arkasa/Scenes`)
Everything is placed in the scene hierarchy. Nothing is created when you press Play. To change the experience, move or edit objects and Inspector values directly in the scene.

### The player: Meta building blocks
Both scenes use the same rig as `Assets/Scenes/SampleScene`:
- **[BuildingBlock] Camera Rig:** OVRCameraRig, OVRManager and OVRHeadsetEmulator.
- **[BuildingBlock] OVRComprehensiveInteractionRig:** Meta's Interaction SDK hands, with hand grab, poke, ray and distance grab interactors.

Changes from the sample, for a hands-only zero-g rig:
- Controller Tracking blocks, controller interactors, controller visuals and the **Locomotor** were removed.
- Microgesture locomotion is switched off.
- Passthrough is off.
- Hand tracking support is set to **Hands Only**.

The rig root also carries the zero-g body: a gravity-free **Rigidbody**, a head **SphereCollider**, **PlayerRig** and **ZeroGLocomotion**. **BodyFrame** sits under it, holding the **Tool Kit Holster** in the Mission scene. **CenterEyeAnchor** holds the guide's voice AudioSource and **GazeIdentifier**.

### Playground (build index 0): one station per feature
- **Systems:**
  - **Guide:** GuideService, its script lines and the subtitle panel.
  - **Safety:** SafetyManager, OperatorLink, TrackingMonitor, Hand Poses, the fade overlay, the pause panel and the wrist menu.
- **Playground:** Room plus the stations:
  1. Body Orientation
  2. Grab and Release
  3. Look To Identify
  4. Handrail Movement
  5. Confined Float
  6. AI Guide
  7. Safety
  8. Cable Plug

### Mission (build index 1): the full exercise
- **Space:** Earth and the stars.
- **Station:**
  - **Interior:** module shell, observation windows, handrails, lights, equipment, communications panel, tool storage with the Repair Tool Kit.
  - **Airlock:** chamber walls, inner and outer hatches, chamber zone, CYCLE button.
  - **Exterior:** solar arrays, truss, EVA handrails, safety tether, and the communication antenna with its maintenance panel, cable and antenna zone.
- **Mission:** TaskSequencer with the **Steps** list and the Objective Marker.

## What does what
| Feature | Built from |
|---|---|
| Picking up items (tool kit, cable plug, wrench...) | Meta **Add Grab Interaction** quick action: **Grabbable** (throws on release, so items drift) plus **Hand Grab Interactable**. **Interactable Unity Event Wrapper** → *When Select* runs on grab. |
| Handrails and walls | Meta **Hand Grab Interactable** and a kinematic Rigidbody, nothing else. **ZeroGLocomotion** (on the rig) listens to Meta's two Hand Grab Interactors: grabbing something without a Grabbable pulls the body. |
| Swimming (no handrail needed) | **ZeroGLocomotion**: sweeping a hand with the palm leading pushes the body the opposite way. Bringing it back edge-first or with the back of the hand leading does nothing, so strokes add up. It uses the Meta hand pose and palm direction. |
| Buttons | Meta **Poke Interactable**, **Surface** (Plane, Clipped Plane, Bounds Clipper), **Poke Interactable Visual** on the Cap, and **Interactable Unity Event Wrapper** → *When Select*. |
| Tool kit stowing | Grabbing it (*When Select*) reports `toolkit_collected` and switches on its Meta **Snap Interactor**. 1.2 s after you let go, it snaps to the **Tool Kit Holster**, a Meta **Snap Interactable** on BodyFrame. |
| Plugging in the cable | Meta **Snap Interactor** on the plug, which snaps to the **Snap Interactable** on *Snap Point*. Its **Interactor Unity Event Wrapper** → *When Select* turns the lamps green, reports `cable_connected` and turns off the plug's grab so it stays in. In the Mission, *Snap Point* is switched on by MaintenancePanel → *On Opened*. |
| Cable length | Unity **Configurable Joint** on the plug (Linear Limit 1.6 m). **CableLine** only draws the curve. |
| Wrist menu | Meta **HandJoint** (follows the left palm) plus **Active State Unity Event Wrapper**, which shows *Menu* while *Hand Poses > Left Palm Towards Face* is active. |
| Palms-to-face pause | Meta **Transform Recognizer Active State** per hand (Palm Towards Face), combined in an **Active State Group** (AND). SafetyManager pauses after it has been active for 1.5 s. |
| Subtitle and pause panels | Meta **HmdOffset**, which keeps them in front of the head. |
| Editor head look | Meta **OVRHeadsetEmulator** on the Camera Rig. |

Layers:
- **Player** is the rig. It is left out of gaze raycasts.
- **Grabbable** is for held items. Collisions between Player and Grabbable are off in Project Settings > Physics, so a held item can't shove you.

## Where to change things (Inspector)
| What | Where |
|---|---|
| Mission steps and their order | Mission > TaskSequencer > Steps |
| Guide lines, subtitles and voice clips | Systems > Guide > GuideService > Lines |
| What a button does | the button's Interactable Unity Event Wrapper > When Select |
| What happens when an item is grabbed | the item's Interactable Unity Event Wrapper > When Select |
| What happens when the cable is plugged in | Cable Plug > ISDK_SnapInteractor > Interactor Unity Event Wrapper > When Select |
| Cable length | Cable Plug > Configurable Joint > Linear Limit |
| Holster position, and the time before the kit returns to the hip | Camera Rig > BodyFrame > Tool Kit Holster position; Repair Tool Kit > ISDK_SnapInteractor > Time Out |
| Wrist menu position and angle | Systems > Safety > Wrist Menu > HandJoint > Offset / Rotation |
| Subtitle and pause panel distance | the panel's HmdOffset > Offset Translation |
| Grab feel (pinch/palm, throw) | the item's Hand Grab Interactable and Grabbable |
| Movement feel | Camera Rig > Rigidbody > Linear Damping, PlayerRig > Max Speed, ZeroGLocomotion |
| Swimming strength and dead zone | Camera Rig > ZeroGLocomotion > Swim Strength / Min Stroke Speed (Swim Enabled turns it off) |
| Look-to-identify timing | CenterEyeAnchor > GazeIdentifier > Look Time |
| E-stop colour, message and fade time | Systems > Safety > SafetyManager |
| Tether length | Station > Exterior > Safety Tether > Max Length |

### Mission events
Objects report plain-text events with `TaskSequencer.Report("name")`, or from a UnityEvent with TaskSequencer.ReportEvent. A step finishes when its **Complete When** event is reported.

| Event | Reported by |
|---|---|
| `looked_around` | 3 objects identified, or 45 s passed |
| `comms_checked` | DIAGNOSTICS button |
| `toolkit_collected` | Repair Tool Kit grab (When Select) |
| `airlock_exterior` / `airlock_interior` | Airlock, after cycling out or back in (going out needs `toolkit_collected`) |
| `reached_antenna` | Antenna Zone |
| `panel_opened` | MaintenancePanel (LATCH button, needs `toolkit_collected`) |
| `cable_connected` | Cable Plug snap (When Select) |

## Controls
**Quest (hand tracking):**
- **Grab:** pinch or close your hand on an item or handrail.
- **Move:** pull on handrails or walls, hand over hand. Or **swim**: sweep your open palms backward to go forward, then bring your hands back edge-first. When you stop you keep drifting, slowly slowing down.
- **Press:** push a button with your index finger.
- **Identify:** look at an object for about 0.8 s.

**Safety (always available):**
- **Wrist menu:** turn your left palm toward your face to get PAUSE and STOP buttons.
- **Gesture:** hold both palms toward your face for 1.5 s to pause.
- **Pause panel:** has RESUME and E-STOP buttons.
- **Operator:** run `tools/operator.ps1 -HeadsetIp <quest-ip>` and press S or Space to e-stop, P to pause, R to resume, X to reset (UDP 47770, same Wi-Fi).
- **Tracking lost:** the view dims and freezes instead of snapping.
- **Tether:** outside, a safety tether pulls you back beyond 15 m from the hatch.

**Editor (no headset):**
- **Look:** Meta's OVRHeadsetEmulator. Hold Ctrl and move the mouse to look, or Alt to pitch.
- **Safety keys:** P pause, Esc e-stop, F5 reset.
- **Hands:** grabbing, handrails, buttons and the cable plug need hands. Use the headset over Link, or the Meta XR Simulator.
- **Testing over Quest Link:** the OpenXR **Mock Runtime** feature must stay off (Project Settings > XR Plug-in Management > OpenXR). If it is on, Unity ignores the headset, Meta features switch off, and Play Mode stops after a few seconds.

## Orientation model
The world never rotates. The Camera Rig is the tracking origin and is only ever moved, by its Rigidbody (rotation frozen). OVRCameraRig applies the headset pose 1:1 (floor-level tracking origin), so a participant who is sideways or upside down in the gyroscope rig sees exactly that. BodyFrame follows the full head rotation and carries the tool kit holster.

## Guide voice
`tools/guide_lines.txt` is the voice script. Run `tools/generate_guide_audio.ps1` to make offline WAV clips with Windows text-to-speech into `Assets/Arkasa/Audio/Guide`. Then drag a clip into the matching line's **Voice** slot on the Guide object. Keep the text in the scene and the txt file the same.

## Building to Quest
1. File > Build Profiles, choose Android, then Switch Platform.
2. Meta > Tools > Project Setup Tool and click Fix All.
3. Build And Run with the Quest connected over USB.
4. Make sure hand tracking is on in the Quest settings.

## Known limits
- Greybox art. No automated tests.
- These need tuning on a headset:
  - **Wrist menu:** offset and angle. The palm is assumed to face -Y in Meta's OpenXR joint space.
  - **Subtitle and pause panels:** they are now locked rigidly to the head (HmdOffset) instead of smoothly following it.
- Not yet verified on a headset:
  - Meta hand grab on handrails while the rig moves
  - Snap holster and socket behaviour
  - Tracking while inverted in the rig
  - Hand tracking at odd angles
  - The 72 FPS target, with about 100 handrail interactables in the Mission scene
