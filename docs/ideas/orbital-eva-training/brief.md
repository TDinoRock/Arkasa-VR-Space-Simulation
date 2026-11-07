# Orbital EVA Training — Brief

## Goal
Let participants feel orientation changes in an orbital environment (driven by a physical multi-axis gyroscope rig) while exploring a virtual space station and completing one simple maintenance task: find a communication antenna fault and reconnect a loose cable to restore the signal. Total experience: 5–10 minutes.

## Player/user experience
1. **Interior (orientation):** start inside a modern orbital station: windows showing Earth, equipment cabinets, control panels, tool storage, airlock. The participant looks around and floats through the confined space.
2. **Fault:** a control panel shows the antenna signal is lost, and the AI guide explains the task.
3. **Tool kit:** collect the repair tool kit from tool storage.
4. **Airlock:** cycle the airlock to the exterior.
5. **EVA:** move along the station exterior by handrails (Earth in the background) to the antenna, open the maintenance panel, and plug the loose cable into the connection point.
6. **Restore:** the signal comes back with confirmation from the AI guide.
7. **Return:** go back through the airlock to the interior, which ends the experience.

Throughout: full-body orientation follows the rig (sideways and upside down are shown correctly, not just head look), and a pause / emergency stop is always reachable.

**Playground scene** for testing each feature in isolation:
- body orientation
- grabbing objects
- look at an object to identify it
- zero-g handrail "walking"
- floating in a confined space
- AI guide responses
- pause / e-stop

## Scope
**Must-have (v1, functional prototype):**
- Playground scene with test stations for every feature above.
- Interior and exterior scenes with the full task loop, built from greybox/placeholder art (primitives plus simple materials, Earth skybox/sphere).
- Orientation: driven by Quest headset tracking only. The world stays inertial; the camera rig is never auto-rotated. Physical body/rig rotation appears 1:1 in VR. A body/torso frame is estimated from head + hands for the avatar/hand presentation.
- Input: **hand tracking** (Meta Interaction SDK building blocks, hands only). Grab objects, grab handrails and pull to move, or swim with arm strokes when there is nothing to hold (added 2026-11-07, user request), release keeps damped momentum, poke buttons/panels.
- Look-to-identify: gaze/head ray highlights an object and the AI guide names it.
- AI guide: **scripted, event-driven lines** with text-to-speech and subtitles on a world-locked/body-locked panel. Triggers: next step, action confirmed, object description on grab/press/look. Built behind an interface so a live LLM can replace it later.
- Safety:
  - Participant pause: a dedicated hand gesture/menu (e.g. palm-up menu) that pauses movement and the guide.
  - Participant **emergency stop**: an immediate fade to a neutral, static, comfort scene, and all motion stops.
  - Operator stop: a second control outside the headset (see open questions).
- Target: **Quest standalone** (Android) build. Unity 6000.5.7f1, URP, OpenXR + Meta XR SDK 207.

**Out of scope (v1):**
- Final art, audio design, and live LLM conversation.
- Rig hardware sensor integration (kept possible via an orientation-source interface).
- Multiplayer, scoring/analytics backend, and controller-based input (may be added later).

## Constraints
- Unity 6000.5.7f1, URP, OpenXR with the Meta XR feature group; Meta XR SDK 207.x (confirm 6000.5 support).
- Quest standalone: at least 72 FPS (13.9 ms), roughly 200–300 draw calls, mobile-friendly lighting.
- Comfort: no forced camera rotation, damped acceleration, an optional vignette, and all motion under player control except what the physical rig produces.
- Hand tracking must keep working at odd body angles; provide a fallback if hands are lost (the guide prompts the participant to hold position).
- Headset tracking risk: inside-out tracking/Guardian may lose tracking upside down or while spinning. The prototype must detect tracking loss and handle it gracefully (freeze plus message, no snapping).
- Total experience 5–10 minutes.

## Integration
- Existing project: Meta XR All-in-One SDK 207 (Core, Interaction SDK, Audio, Haptics, Voice), XR Hands, Input System, URP.
- New systems (C#, namespace `Arkasa`):
  - `OrientationSource` interface (headset now, rig sensor later)
  - `ZeroGLocomotion` (handrail pull plus momentum)
  - `ZeroGBody` (Rigidbody, no gravity, damping)
  - `Grabbable`/identifiable object metadata
  - `GazeIdentifier`
  - `GuideService` (scripted lines + TTS + subtitles)
  - `TaskSequencer` (step state machine)
  - `SafetyManager` (pause / e-stop)
  - `Airlock`
  - `CableConnector`/`AntennaSignal`
- Scenes: `Playground`, `StationInterior`, `StationExterior` (or one scene with an airlock transition).

## Done when
- [ ] Playground: each feature station works on Quest with hand tracking:
  - rolling or turning the body sideways or upside down in real life shows the same orientation in VR, with no auto-correction
  - grab, inspect and release an object
  - looking at an object names it
  - pulling along handrails moves the player, and momentum decays smoothly
  - floating in a confined room collides softly with walls
- [ ] The AI guide speaks and subtitles a line for: each step start, each confirmed action, and each object grabbed/pressed/looked at.
- [ ] Full loop is completable start to finish in 5–10 minutes: look around, take the tool kit, airlock out, reach the antenna, reconnect the cable, signal restored, airlock in, end.
- [ ] Participant pause and e-stop are reachable at any time and any orientation. E-stop fades to a static safe view within 0.5 s and stops all motion.
- [ ] The operator stop works without the participant's input.
- [ ] Tracking loss is detected and handled without camera snapping.
- [ ] Holds 72 FPS on Quest 3 in both environments.

## Open questions
- **Operator stop on standalone:** build it as a LAN companion (phone/PC web page or small app sending UDP to the headset), or as an operator-held controller with a hold-to-stop button? Default plan: LAN UDP listener + a simple PC/phone sender.
- **TTS engine:** Meta Voice SDK TTS needs Wit.ai (online). An offline alternative is Android native TTS or pre-recorded/pre-generated clips. Default plan: pre-generated audio clips + subtitles (offline, reliable).
- **Which Quest model** for testing (3 / 3S / Pro)?
- **Art:** greybox for now; source final assets later (Asset Store, AI-generated, or custom)?
- ~~One scene vs two~~ **Decided (build):** one `Mission` scene containing interior, airlock and exterior, which avoids carrying the player rig and mission state across a scene load. The airlock cycle is the transition. Plus a separate `Playground` scene.
- ~~Operator stop~~ **Decided:** LAN UDP listener (port 47770) plus `tools/operator.ps1` on the PC.
- ~~TTS engine~~ **Decided:** offline clips pre-generated with Windows TTS (`tools/generate_guide_audio.ps1`), shown with subtitles.
- **Scenes are built out in the hierarchy** (no runtime generation); scripts stay simple with Inspector-visible fields and UnityEvents for wiring (user request, 2026-11-06).
- ~~Custom hand layer on XR Hands~~ **Changed (user request, 2026-11-06):**
  - The player is Meta's building blocks, the same as SampleScene: Camera Rig plus OVRComprehensiveInteractionRig.
  - Hands only, with no Locomotor.
  - Grab, poke and handrail grabbing use Interaction SDK components (Grab quick action, Poke Interactable, Hand Grab Interactable).
  - Arkasa scripts only add the zero-g body, mission, guide and safety logic.
- **No extra tools:** the test runner menu and the automated tests were removed (user request, 2026-11-06). The operator script and voice generator stay.
- **Meta scripts first (user request, 2026-11-06):** wherever Meta's SDK has a component, use it instead of a custom script:
  - grab: Grab Interaction
  - handrails: plain Hand Grab Interactable
  - holster and cable socket: Snap Interactor/Interactable
  - wrist menu: HandJoint, pose recognizer and Active State event wrapper
  - head-locked panels: HmdOffset
  - editor look: OVRHeadsetEmulator

  Removed as a result: Climbable, ToolKit, CableConnector, WristMenu, BodyLockedPanel and DesktopSimulator.
- Does the physical rig obstruct the Quest's tracking cameras or the hands' visibility?
