# AugustsUtility-Unity

A grab-bag of data-driven systems and runtime utilities for Unity, ported out of my actual projects and cleaned up enough to be reusable.  
Battle-tested in prototypes, not in commercial releases. Use like any other sharp tool: deliberately.

---

## Table of Contents

1. Overview
2. Installation
3. Usage Notes
4. License

---

## 1. Overview

Everything lives under the `AugustsUtilities` folder and is intended to be:

- **Data-driven**: ScriptableObjects and plain config where possible.
- **Decoupled**: Systems talk through IDs, events, and registries rather than hard references.
- **Minimal ceremony**: Drop the folder in, wire the few singletons you care about, delete the rest.

Core “systems” are:

- Item / capability system
- Slot-based inventory
- Audio manager

On top of that sits a growing set of **runtime utilities** used across my games:

- Camera shake helpers
- Generic tween runner
- Telegraph / hitbox visualization helpers  
and so on, all under `AugustsUtility.*` namespaces.

---


## 2. Installation

1. **Copy the folder**
   Drop `AugustsUtilities/` into your Unity project (anywhere under `Assets/`).

2. **Delete what you don’t need**
   This is not meant to be a monolithic framework. If you don’t need a system, delete its folder.

3. **Wire singletons**
   For systems that expect a single instance (audio manager, inventory manager, tween runner, camera shake, etc.):

   * Put the prefab / GameObject into a bootstrap scene.
   * Optionally mark as `DontDestroyOnLoad` if you want it across scenes.

---

## 3. Usage Notes

* **Singleton assumptions**
  Audio, inventory manager, tween runner, camera shake, etc. are written assuming *one* instance.
  If you want multiple, you’ll have to adjust the static access pattern.

* **IDs are strings**
  Item lookups are string ID–based; keep them unique and centralized in the database.

* **Resources path**
  The item database is loaded from `Resources/ItemDatabase.asset` by default
  (see the generated loader if you need to change this).

---

## 4. License

MIT. Do whatever

