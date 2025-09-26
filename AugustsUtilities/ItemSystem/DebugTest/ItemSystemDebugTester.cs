using System.Linq;
using AugustsUtility.ItemSystem;
using AugustsUtility.ItemSystem.Example;
using UnityEngine;

public class ItemSystemDebugTester : MonoBehaviour
{
    [Header("Item IDs To Test")]
    [SerializeField] private string healthPotionID = "potion_health";
    [SerializeField] private string manaPotionID = "potion_mana";
    [SerializeField] private string omniPotionID = "potion_omni";
    [SerializeField] private string swordID = "sword_test";

    private ItemDatabase _database;

    private void Start()
    {
        // Add required components for testing
        if (GetComponent<HealthComponent>() == null)
            gameObject.AddComponent<HealthComponent>();
        if (GetComponent<ManaComponent>() == null)
            gameObject.AddComponent<ManaComponent>();

        _database = Resources.Load<ItemDatabase>("ItemDatabase");
        if (_database == null)
        {
            Debug.LogError("[DebugTester] Could not find the ItemDatabase in Resources. The test cannot run.");
            enabled = false;
            return;
        }

        RunAutomatedTests();

        Debug.Log("--- Manual Testing Ready ---");
        Debug.Log("Press 1 to use Health Potion");
        Debug.Log("Press 2 to use Mana Potion");
        Debug.Log("Press 3 to show Omni Potion debug info");
        Debug.Log("Press 4 to use Sword (should do nothing)");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseItemByID(healthPotionID);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseItemByID(manaPotionID);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            ShowOmniPotionDebug();
        if (Input.GetKeyDown(KeyCode.Alpha4))
            UseItemByID(swordID);
    }

    private void UseItemByID(string itemID)
    {
        var definition = _database.GetByID(itemID);
        if (definition == null)
        {
            Debug.LogWarning($"Tried to use an item with ID '{itemID}' that doesn't exist in the database.");
            return;
        }

        var instance = new ItemInstance(definition);
        Debug.Log($"--- Using {instance.Definition.DisplayName} ---");

        // Get all consumable capabilities (our new polymorphic system)
        var consumableCapabilities = instance.Definition.GetCapabilitiesOfType<ConsumableCapability>();

        int executedActions = 0;
        foreach (var cap in consumableCapabilities)
        {
            Debug.Log($"Executing {cap.GetType().Name}:");

            // Show capability details before execution
            LogCapabilityDetails(cap);

            // Execute through the handler system
            HandlerRegistry.Execute(instance, cap, this.gameObject);
            executedActions++;
        }

        // Also check for other actionable capabilities
        var otherActionables = instance.Definition.GetCapabilitiesOfType<ActionableCapability>()
            .Where(cap => !(cap is ConsumableCapability));

        foreach (var cap in otherActionables)
        {
            Debug.Log($"Executing {cap.GetType().Name} (Non-consumable actionable):");
            HandlerRegistry.Execute(instance, cap, this.gameObject);
            executedActions++;
        }

        if (executedActions == 0)
        {
            Debug.Log($"Item '{instance.Definition.DisplayName}' has no actionable capabilities to execute.");
        }

        Debug.Log($"--- Finished using {instance.Definition.DisplayName} ({executedActions} capabilities executed) ---");
    }

    private void ShowOmniPotionDebug()
    {
        var definition = _database.GetByID(omniPotionID);
        if (definition == null)
        {
            Debug.LogWarning($"Omni Potion with ID '{omniPotionID}' not found in database.");
            return;
        }

        Debug.Log($"=== OMNI POTION DEBUG INFO ===");
        Debug.Log($"Item: {definition.DisplayName} (ID: {definition.ID})");
        Debug.Log($"Description: {definition.Description}");
        Debug.Log($"Stack Size: {definition.StackSize}");

        var consumableCapabilities = definition.GetCapabilitiesOfType<ConsumableCapability>().ToList();
        Debug.Log($"Total Consumable Capabilities: {consumableCapabilities.Count}");

        for (int i = 0; i < consumableCapabilities.Count; i++)
        {
            var cap = consumableCapabilities[i];
            Debug.Log($"--- Capability {i + 1}: {cap.GetType().Name} ---");
            LogCapabilityDetails(cap);
        }

        // Show crafting requirements if any
        var craftingCap = definition.GetFirstCapabilityOfType<CraftingCapability>();
        foreach (var recipe in craftingCap.Recipes)
        {
            Debug.Log($"--- Crafting Requirements ---");
            Debug.Log($"Ingredients: {recipe.Ingredients}");
            Debug.Log($"Outputs: {recipe.Outputs}");
        }

        Debug.Log($"=== END OMNI POTION DEBUG ===");
    }

    private void LogCapabilityDetails(ConsumableCapability cap)
    {
        Debug.Log($"  Cooldown: {cap.Cooldown}s");

        // Use pattern matching to show specific details for different capability types
        switch (cap)
        {
            case HealthEffectCapability healthCap:
                Debug.Log($"  Heal Amount: {healthCap.Amount}");
                Debug.Log($"  Instant Heal: {healthCap.IsInstant}");
                break;

            case ManaEffectCapability manaCap:
                Debug.Log($"  Mana Amount: {manaCap.Amount}");
                Debug.Log($"  Mana Regen Multiplier: {manaCap.IsInstant}x");
                break;

            default:
                Debug.Log($"  (No specific details available for {cap.GetType().Name})");
                break;
        }
    }

    private void RunAutomatedTests()
    {
        Debug.Log("--- Running Automated Item System Tests ---");
        try
        {
            // Test 1: Query for all consumables (now includes polymorphic capabilities)
            var consumables = _database.GetAllItemsWithCapability<ConsumableCapability>().ToList();
            Assert(consumables.Count >= 3, $"Test 1.1 FAILED: Expected at least 3 consumables, but found {consumables.Count}.");
            Assert(consumables.Any(d => d.ID == omniPotionID), "Test 1.2 FAILED: Omni Potion not found in consumables query.");
            LogTestResult("Test 1: GetAllItemsWithCapability<ConsumableCapability>", true);

            // Test 2: Query for all equippables
            var equipment = _database.GetAllItemsWithCapability<EquipmentCapability>().ToList();
            Assert(equipment.Count >= 1, $"Test 2.1 FAILED: Expected at least 1 piece of equipment, but found {equipment.Count}.");
            Assert(equipment.Any(d => d.ID == swordID), "Test 2.2 FAILED: Sword not found in equipment query.");
            LogTestResult("Test 2: GetAllItemsWithCapability<EquipmentCapability>", true);

            // Test 3: Query for all actionable items (consumables are actionable now)
            var actionables = _database.GetAllActionableItems().ToList();
            Assert(actionables.Count >= 3, $"Test 3.1 FAILED: Expected at least 3 actionable items, but found {actionables.Count}.");
            Assert(actionables.Any(d => d.ID == healthPotionID), "Test 3.2 FAILED: Health Potion should be actionable.");
            LogTestResult("Test 3: GetAllActionableItems", true);

            // Test 4: Verify Omni Potion's polymorphic composition
            var omniDef = _database.GetByID(omniPotionID);
            Assert(omniDef != null, "Test 4.0 FAILED: Omni Potion definition not found.");
            var omniConsumableCaps = omniDef.GetCapabilitiesOfType<ConsumableCapability>().ToList();
            Assert(omniConsumableCaps.Count >= 2, $"Test 4.1 FAILED: Omni Potion should have at least 2 ConsumableCapabilities, but has {omniConsumableCaps.Count}.");

            // Check for specific capability types
            bool hasHealthCap = omniConsumableCaps.Any(c => c is HealthEffectCapability);
            bool hasManaCap = omniConsumableCaps.Any(c => c is ManaEffectCapability);
            Assert(hasHealthCap, "Test 4.2 FAILED: Omni Potion is missing HealthEffectCapability.");
            Assert(hasManaCap, "Test 4.3 FAILED: Omni Potion is missing ManaPotionCapability.");
            LogTestResult("Test 4: Omni Potion Polymorphic Composition", true);

            // Test 5: Verify specific capability types exist
            var healthPotionDef = _database.GetByID(healthPotionID);
            Assert(healthPotionDef != null, "Test 5.0 FAILED: Health Potion definition not found.");
            var healthCaps = healthPotionDef.GetCapabilitiesOfType<HealthEffectCapability>().ToList();
            Assert(healthCaps.Count == 1, $"Test 5.1 FAILED: Health Potion should have exactly 1 HealthPotionCapability, but has {healthCaps.Count}.");
            LogTestResult("Test 5: Specific Capability Types", true);

            // Test 6: Edge case - getting non-existent item
            var nonExistent = _database.GetByID("item_that_does_not_exist");
            Assert(nonExistent == null, "Test 6.1 FAILED: GetByID should return null for non-existent ID.");
            LogTestResult("Test 6: Non-Existent Item Query", true);

            // Test 7: Crafting capability test (if items have crafting requirements)
            TestCraftingRequirements();

        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>AUTOMATED TEST SUITE FAILED WITH AN EXCEPTION:</color> {e.Message}");
        }

        Debug.Log("--- Automated Tests Complete ---");
    }

    private void TestCraftingRequirements()
    {
        // Test if any items have crafting capabilities
        var craftableItems = _database.GetAllItemsWithCapability<CraftingCapability>().ToList();

        if (craftableItems.Count > 0)
        {
            Debug.Log($"Found {craftableItems.Count} craftable items:");
            foreach (var item in craftableItems)
            {
                var craftingCap = item.GetFirstCapabilityOfType<CraftingCapability>();
                foreach (var recipe in craftingCap.Recipes)
                {
                    Debug.Log($"--- Crafting Requirements ---");
                    Debug.Log($"Ingredients: {recipe.Ingredients}");
                    Debug.Log($"Outputs: {recipe.Outputs}");
                }
            }
        }
        else
        {
            Debug.Log("No items with crafting capabilities found in database.");
        }

        LogTestResult("Test 7: Crafting Capabilities Query", true);
    }

    private void Assert(bool condition, string failMessage)
    {
        if (!condition)
            throw new System.Exception(failMessage);
    }

    private void LogTestResult(string testName, bool success, string message = "")
    {
        if (success)
            Debug.Log($"<color=green>PASSED:</color> {testName}");
        else
            Debug.LogError($"<color=red>FAILED:</color> {testName}. {message}");
    }
}

