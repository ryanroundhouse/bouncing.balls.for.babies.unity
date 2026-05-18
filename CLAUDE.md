# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A small Unity game ("Bouncing Balls for Babies") shipped to the Windows Store and Android Play Store. Three planets (Earth / Jupiter / Moon) are flicked around an arena to squish spawning "bug" cylinders; first ball to 5 squishes wins.

## Unity version and tooling

- Unity Editor **6000.0.75f1** (Unity 6) — see `ProjectSettings/ProjectVersion.txt`. Project was migrated from Unity 4.5 → 2017.4 → Unity 6, so expect API churn (e.g. `Rigidbody.linearVelocity` is used in place of the old `.velocity`).
- There is **no CLI build / test / lint workflow**. All build, play-mode testing, and platform builds (Android `.apk`/`.aab`, Windows Store) happen inside the Unity Editor. Don't invent shell commands for these.
- `.csproj` / `.sln` files at the repo root are Editor-regenerated artifacts and are gitignored — never hand-edit them.
- Build outputs (`.apk`, `.aab`, `Build/`, `Library/`, `Logs/`, `UserSettings/`) and signing material (`Keystore/`, `*.pfx`, `*.keystore`) are gitignored. Don't commit them.
- Editor utility: **Tools → Dump FBX Sub-Asset IDs** (`Assets/Editor/DumpSubAssetIds.cs`) writes `subasset_dump.txt` at the repo root — used to recover fileIDs for the `BugAnimated.fbx` / `goo.fbx` sub-assets after the 4.5→2017.4 migration.

## Scene + script architecture

Three scenes, loaded by name via `SceneManager.LoadScene`:

1. **SplashScene** — `SplashScreenCameraScript` shows `Resources/splash screen.png` for ~3s then loads `MainScene`.
2. **MainScene** — gameplay.
3. **VictoryScene** — `VictoryCameraScript` reads the winner and waits for a tap to reload `MainScene`.

Cross-scene state is carried by **`WinningInfoScript`** (two `static` fields: `WinningPlanet` name + `WinningPlanetIndex`). `BallScript.squishSomething()` writes them before loading `VictoryScene`, and `VictoryBallScript` reads `WinningPlanetIndex` to pick the winning material.

Music persists across scenes via **`MusicManagerScript`** on a `DontDestroyOnLoad` GameObject named `"MusicManager"`. Other scripts find it with `GameObject.Find("MusicManager")` and call `PlayMusic(Music.Gameplay | Music.Victory)`. The enum value is used as an **index into the AudioSource list** on that GameObject, so the order of `AudioSource` components in the inspector is load-bearing.

### Input + ball flicking (CameraScript)

`CameraScript` (attached to the main camera in MainScene) owns input. It raycasts mouse/touch input against objects tagged **`"Ball"`** and dispatches three `SendMessage` events that ball-like scripts listen for: **`Touched(Vector3 hitPoint)`**, **`MouseTouched(Vector3 hitPoint)`**, **`TouchEnded(bool push)`**. Because dispatch is by name, **renaming any of those three methods on `BallScript` / `ScoreBallScript` will silently break input.** `TouchEnded(true)` means "push from where the user clicked"; `false` means "pull toward the last touch point".

Shake-to-scatter is also in `CameraScript.Update` using `Input.acceleration` + a low-pass filter; it finds every `"Ball"`-tagged object and applies a random horizontal impulse.

### Squish / spawning (SquishCylinderScript + Cylinderscript)

`SquishCylinderScript` (a manager in MainScene) keeps the arena topped up at 3 `BugPrefab` instances, spawning at random positions inside a designer-configured `TopLeftSpawnLocation` / `BottomRightSpawnLocation` rect (note: the code samples those vectors as `(x..z, _, x..z)` — that's intentional, not a bug). Holding **Space** force-spawns extras.

Each `BugPrefab` runs `Cylinderscript`, which on `OnTriggerEnter`:
- plays a random squish sound,
- flattens itself (`localScale = (1.6, 0.1, 1.6)`) and calls `BallScript.squishSomething()` on the colliding ball,
- spawns a `GooPrefab` and randomly tints it using one of `green slime / blue slime / orange slime / yellow slime` materials,
- schedules removal 5s later via `ParentScript.RemoveCylinder`.

### HUD overlays (ScoreCameraScript)

`ScoreCameraScript` is a second camera (`"GuiWriter"`) rendering legacy IMGUI (`OnGUI`). It draws the three planet scores plus two fading instructional overlays. Other scripts cancel the instructions overlay by setting `((ScoreCameraScript)GameObject.Find("GuiWriter").GetComponent("ScoreCameraScript")).AbortInstructions = true` — so the GUI camera GameObject **must be named `"GuiWriter"`**.

`VictoryCameraScript` and `ScoreCameraScript` both mutate `GUI.skin.GetStyle("label")` — they share the same `GUIStyle` instance, so font/size assignments race. Be careful changing one without considering the other.

## Tags, names, and Resources — known fragile coupling

Because the scripts use string lookups, the following names are **load-bearing** and must not be renamed without updating code:

- **Tags:** `Ball`, `Floor`, `MainCamera`.
- **GameObject names:** `MusicManager`, `GuiWriter`.
- **`Resources/` lookups** (case-sensitive on Android/iOS): `ObjectiveOverlay`, `InstructionOverlay`, `ScoreText`, `Splash Screen` (note space), `AlegreyaBold`, `AlegreyaBoldVictory`, `GooPrefab`, `BugPrefab`, and the four slime materials (`green slime`, `blue slime`, `orange slime`, `yellow slime`).
- **Scene names** passed to `SceneManager.LoadScene`: `MainScene`, `VictoryScene`. They must also be in **Build Settings → Scenes In Build** or the load will fail at runtime.

## Conventions worth following

- Scripts use the `Assets.Scripts` namespace only for `Utility` (helper static class with `GetRandomInt` / `GetRandomFloat` / `PauseGame`). `MonoBehaviour` scripts deliberately stay in the global namespace because they're attached via the inspector — keep that pattern unless you also update the prefab/scene serialized references.
- Random numbers throughout use `Assets.Scripts.Utility` (which wraps `System.Random`), **not** `UnityEngine.Random`. Match that when adding code.
- There is no test project, no CI, and no automated lint. Validate changes by playing the scene in the Editor.
