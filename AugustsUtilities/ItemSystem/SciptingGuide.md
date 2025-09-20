Item System Scripting Guide

The system's core programming loop is: Query the ItemDatabase to get an ItemDefinition, create an ItemInstance from it, and execute its actions via the HandlerRegistry.

## 1. Accessing the Database

Initialization is handled automatically at startup. To get a reference to the database in any script's Awake() or Start() method, load it from Resources. It's recommended to cache this reference.


private ItemDatabase _database;

void Start()
{
    _database = Resources.Load<ItemDatabase>("ItemDatabase");
    if (_database == null)
    {
        Debug.LogError("ItemDatabase not found! The system cannot function.");
    }
}

## 2. Querying for Item Blueprints (ItemDefinition)

The ItemDatabase provides several methods to find the static data for items.

A. Get a specific item by its unique ID:

Use this when you know exactly which item you need.
C#

// Get the blueprint for a health potion
ItemDefinition healthPotionDef = _database.GetByID("potion_health");

if (healthPotionDef != null)
{
    Debug.Log($"Found item: {healthPotionDef.DisplayName}");
}

B. Get all items that have a specific feature:

Use this to find all items that share a common capability, perfect for populating UI or for AI decision-making.


// Find all items that can be equipped
IEnumerable<ItemDefinition> allEquipment = _database.GetAllItemsWithCapability<EquipmentCapability>();

// Find all items that can be consumed
IEnumerable<ItemDefinition> allConsumables = _database.GetAllItemsWithCapability<ConsumableCapability>();

C. Get all "usable" items:

This is a shortcut to find any item that has at least one ActionableCapability (i.e., any capability that has a corresponding handler).


// Get all items that a player can "use" in some way
IEnumerable<ItemDefinition> allUsableItems = _database.GetAllActionableItems();

## 3. Working with Items

Once you have an ItemDefinition, you can inspect its properties or create a runtime instance.

A. Creating an ItemInstance:

An ItemInstance represents an item in the game world, like in a player's inventory. It holds a reference to its definition and a stack count.


ItemDefinition swordDef = _database.GetByID("weapon_sword");
ItemInstance mySword = new ItemInstance(swordDef, 1); // A single sword

B. Checking for and reading capability data:

You can query an ItemDefinition to see what it can do and access the data associated with that behavior.


ItemDefinition swordDef = _database.GetByID("weapon_sword");

// Check if the item is equipment
if (swordDef.HasCapability<EquipmentCapability>())
{
    // Get the capability's data to read its properties
    var equipCap = swordDef.GetCapability<EquipmentCapability>();
    Debug.Log($"Damage: {equipCap.Damage}");
}

## 4. Executing Item Actions

To make an item do something, you execute its ActionableCapability via the HandlerRegistry.

The Execute method requires three things:

    The ItemInstance being used.

    The specific ActionableCapability to execute.

    A context object, which is typically the GameObject using the item (the player, an NPC, etc.).

A. Executing a single, known action:


// Assume 'player' is the GameObject of the user and 'potionInstance' is a valid ItemInstance
var consumableCap = potionInstance.Definition.GetCapability<ConsumableCapability>();

if (consumableCap != null)
{
    HandlerRegistry.Execute(potionInstance, consumableCap, player);
}

B. Executing ALL actions on an item:

This is the most powerful and common pattern. It allows items like the "Omni Potion" to work without any special logic. You simply find all actionable capabilities and execute each one.


// Assume 'itemInstance' is any ItemInstance
// Assume 'user' is the GameObject of the user

Debug.Log($"--- Using {itemInstance.Definition.DisplayName} ---");

var allActionableCaps = itemInstance.Definition.GetCapabilities<ActionableCapability>();

int actionsFound = 0;
foreach (var cap in allActionableCaps)
{
    HandlerRegistry.Execute(itemInstance, cap, user);
    actionsFound++;
}

if (actionsFound == 0)
{
    Debug.Log("Item has no executable actions.");
}