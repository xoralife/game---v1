# FPS Training Room

**Educational Unity project** for learning how FPS shooting mechanics work — recoil patterns, spray spread, aim assist, target lock, and heat-seeking projectiles.

Built with **Unity 2022.3 LTS** and **C#**.

---

## Purpose

This is a **learning sandbox**, not a game or a cheat tool. Every mechanic is exposed with real-time controls (sliders, toggles) so you can see exactly how they work under the hood:

- **Recoil Patterns** — How predefined (x,y) curves create weapon recoil, how spread blooms, and how recovery works
- **Recoil Compensation** — Visualization of what "zero recoil" looks like (purely educational)
- **Aim Assist** — Sticky crosshair, bullet magnetism, and slowdown mechanics
- **Target Lock** — Lock-on and snap-to-target mechanics
- **Heat-Seeking Projectiles** — Homing bullets with lead prediction and proximity detonation

All mechanics are adjustable in real-time via the **Control Panel** (press Tab).

---

## How to Use

### Setup
1. Open project in Unity 2022.3 LTS
2. Go to **FPS Training > Setup Training Scene** (menu bar)
3. Go to **FPS Training > Generate Default Assets**
4. Press **Play**

### Controls

| Key | Action |
|-----|--------|
| WASD | Move |
| Shift | Sprint |
| Ctrl | Crouch |
| Space | Jump |
| Mouse | Look |
| Left Click | Fire |
| Right Click | Aim Down Sights |
| R | Reload |
| 1-3 / Scroll | Switch Weapon |
| Tab / Esc | Toggle Control Panel |
| F1 | Toggle Controls Guide |

### Control Panel

The control panel lets you adjust:

- **Recoil** — Intensity multiplier, compensation toggle, pattern visualization
- **Targeting** — Aim assist, target lock, bullet magnetism, heat-seeking (on/off + strength)
- **Training** — Infinite ammo, god mode, time speed, target groups
- **Stats** — Accuracy, kills, damage, combo, score

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Weapons/       — SprayPattern, WeaponBase, RecoilController, Projectile, etc.
│   ├── Targeting/     — AimAssist, TargetLock, HeatSeekingProjectile, BulletMagnetism, etc.
│   ├── Training/      — TargetDummy, MovingTarget, TrainingUI, StatsTracker, FPSController, etc.
│   └── Debug/         — RecoilDebugOverlay, ShotImpactDisplay, HitMarkerUI, etc.
├── Editor/            — AssetGenerator, SceneSetupEditor
├── ScriptableObjects/ — Recoil patterns (generated via menu)
├── Prefabs/           — (to be set up in Unity)
└── Resources/         — Default training settings
```

---

## Educational Features

| Mechanic | Script | What to Learn |
|---|---|---|
| **Recoil Pattern** | `SprayPattern.cs` + `RecoilController.cs` | Curves, per-shot offsets, pattern looping |
| **Spray / Bloom** | `SprayPattern.cs` | Accuracy degradation, recovery over time |
| **Recoil Comp** | `RecoilController.cs` | How compensation hacks work (shown for education) |
| **Aim Assist** | `AimAssist.cs` | Sticky zones, angle-based detection |
| **Target Lock** | `TargetLock.cs` | Lock-on timers, snap curves |
| **Heat Seeker** | `HeatSeekingProjectile.cs` | Homing, lead prediction, proximity fuse |
| **Bullet Magnetism** | `BulletMagnetism.cs` | Magnetic aim bending toward targets |
| **Range Finding** | `RangeFinder.cs` | Distance measurement, optimal range display |

---

## 50 Commit Workflow

This project was built incrementally across 50 git commits to demonstrate iterative development:

| Phase | Commits | Files |
|-------|---------|-------|
| 1. Project Setup | 1-5 | .gitignore, manifest, settings, asmdef |
| 2. Core Shooting | 6-15 | WeaponBase, RecoilController, Projectile, etc. |
| 3. Targeting | 16-25 | AimAssist, TargetLock, HeatSeekingProjectile, etc. |
| 4. Training | 26-35 | TargetDummy, TrainingUI, StatsTracker, FPSController |
| 5. Debug/Vis | 36-42 | RecoilDebugOverlay, HitMarkerUI, PerformanceMonitor |
| 6. Assets/Scene | 43-48 | Asset generators, scene setup, input guide |
| 7. Polish | 49-50 | README, final config, GitHub push |

---

## License

Educational use only. Not affiliated with PUBG or any game studio.
