# AugustsUtility-Unity

A grab-bag of data-driven systems and runtime utilities for Unity, ported out of my actual projects and cleaned up enough to be reusable.  
Battle-tested in prototypes, not in commercial releases. Use like any other sharp tool: deliberately.

---

## Table of Contents

1. Overview
2. Packages
   - Item System
   - Inventory System
   - Audio System
   - Camera Shake (`AugustsUtility.CameraShake`)
   - Tween System (`AugustsUtility.Tween`)
   - Telegraph / Hitbox Utilities (`AugustsUtility.Telegraph`)
3. Installation
4. Quick Start Snippets
   - Item / Inventory
   - Audio
   - Camera Shake
   - Tween
   - Telegraph
5. Usage Notes
6. Testing
7. License

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

## 2. Packages

### 2.1 Item System

Capability-oriented item definitions using `ScriptableObject`s as the data source.

- **Data-driven**: `ItemDatabase` is a ScriptableObject index of all item definitions.
- **Component-based**: Item behavior is built out of “capabilities”
  (`ConsumableCapability`, `EquipmentCapability`, custom ones, etc.).
- **Editor-friendly**: You define item assets; the database is auto-built or rebuilt via a menu command.

Conceptual flow:

1. Create `ItemDefinition` assets (ID + stats + capability list).
2. Build / refresh the `ItemDatabase` asset.
3. At runtime, query by ID and execute capabilities through a handler registry.

**API sketch:**

```csharp
// Load the database (cached internally)
var db = Resources.Load<ItemDatabase>("ItemDatabase");

// Query for definitions
var potion = db.GetByID("potion_health");
var allEquipment = db.GetAllItemsWithCapability<EquipmentCapability>();

// Execute an item capability
HandlerRegistry.Execute(potionInstance, capability, userGameObject);
