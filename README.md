# AugustsUtility-Unity

A collection of data-driven utility systems for Unity, ported and adapted from my Godot Projects, tested via automated runners; not battle-scarred in productions because I rarely use Unity.



---

## Table of Contents

1.  [Core Systems](#core-systems)
    * [Item System](#item-system)
    * [Inventory System](#inventory-system)
    * [Audio System](#audio-system)
2.  [Setup Guide](#setup-guide)
3.  [Usage Notes](#usage-notes)
4.  [Testing](#testing)

## Core Systems

### Item System

A capability-oriented item definition system using `ScriptableObject`s as the data source.

* **Data-Driven:** `ItemDatabase` (`ScriptableObject`) is auto-initialized at runtime.
* **Component-Based:** Item behaviors are defined via composable capabilities (e.g., `ConsumableCapability`, `EquipmentCapability`).
* **Editor-Friendly:** Includes custom editor tools for database population.

**API Sketch:**  
```csharp
// Load the database (cached)  
var db = Resources.Load<ItemDatabase>("ItemDatabase");

// Query for item definitions  
var itemDef = db.GetByID("potion_health");  
var allEquipment = db.GetAllItemsWithCapability<EquipmentCapability>();

// Execute item actions  
HandlerRegistry.Execute(itemInstance, capability, userGameObject);
```
### **Inventory System**

A component-based, slot-based inventory system with support for categories, stacking, and event-driven updates.

* **Component Architecture:** Attach InventoryComponent to any GameObject.  
* **Global Management:** InventoryManager singleton tracks all inventories and handles inter-inventory operations.  
* **Categorization:** Filter inventories by type (Player, Storage, Merchant, etc.).  
* **Event-Driven:** A single static event notifies external systems (like UI) of any slot change.

**Core Event:**

* `InventoryComponent.OnSlotUpdated(InventorySlot updatedSlot)`

**API Sketch:**  
```csharp
// Manager-level operations  
InventoryManager.Instance.SwapItems(slotA, slotB);  
InventoryManager.Instance.TransferItem(sourceInv, targetInv, "item_id", amount);

// Component-level queries  
var containers = InventoryManager.Instance.GetInventoriesByCategory(Category.StorageContainer);  
bool hasItem = playerInventory.HasItem("potion_health", 5);
```
### **Audio System**

A robust audio manager for music crossfades, SFX pooling, and playlist management.

* **Music Player:** A/B music player with configurable crossfades.  
* **SFX Pooling:** Efficiently plays one-shot and looping sound effects from a pre-warmed pool.  
* **Playlists:** Manages sequential track playback with events on track change.  
* **Positional Audio:** Supports playing SFX at a specific world position.

**API Sketch:**  
```csharp
AudioManager.Instance.PlayMusic(musicTrack, 2.5f); // 2.5s crossfade  
AudioManager.Instance.PlayPlaylist(combatPlaylist);  
AudioManager.Instance.PlaySFX(explosionSfx);  
AudioManager.Instance.PlaySFXAtPosition(impactSfx, transform.position);
```
## **Setup Guide**

1. Drop the folder in, remove what you don't need, put Singletons into Scene Tree.

## **Usage Notes**

* **Singletons:** Managers are designed as scene-level singletons. Ensure exactly one instance exists. For persistence across scenes, use DontDestroyOnLoad on their host GameObject.  
* **Item IDs:** All item lookups are based on the string ID field in the ItemDefinition. These must be unique.  
* **Database Path:** The system loads the database from Resources/ItemDatabase.asset. This path is hardcoded in DatabaseLoader.cs (Automatically Generated)
* **Database Update:** Database update is manual. Use `Tools -> Item System -> Repopulate Database`

## **Testing**

Each system includes a debug tester script for validation.

* ItemSystemDebugTester: Validates database loading and capability queries.  
* InventorySystemDebugTester: Validates adding, removing, swapping, and transferring items.  
* AudioManagerTester: Validates music fades, SFX playback, and playlists.

To use, attach the desired tester component to a GameObject in a sandbox scene and run. Console output is verbose by design.
