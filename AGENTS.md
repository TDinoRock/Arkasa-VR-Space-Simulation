# Arkasa Gyroscope-Based VR

Short VR space-astronaut training simulation (zero-gravity) for Meta Quest.

## Stack
- Unity **6000.5.7f1** (Unity 6), URP
- Meta XR All-in-One SDK 207.x (Core, Interaction, Platform, MRUK, Haptics, Audio, Voice)
- XR provider: OpenXR with the Meta XR feature group on (Android + Standalone). Do not use the deprecated Oculus XR plugin.
- Input System (new). `com.unity.xr.legacyinputhelpers` is a required dependency; leave it.
- Targets: Android/Quest (min SDK 32, target 34) and Standalone PC VR
- App ID: `com.arkasa.gyroscopevr`, company `Arkasa`

## Rules
- Check Meta's current docs before relying on SDK APIs; they change between releases (e.g. OVRHaptics, OVR skeleton deprecated).
- Performance budget: at least 72 FPS on Quest (13.9 ms/frame); aim for roughly 200–300 draw calls.
- Comfort: player-controlled movement only, damped acceleration, vignette option, seated mode.
