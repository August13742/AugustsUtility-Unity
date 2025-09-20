using System.Linq;
using AugustsUtility.ItemSystem;
using AugustsUtility.ItemSystem.Capabilities;
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
        // Add required components for testing (using corrected names)
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
        Debug.Log("Press 3 to use Omni Potion");
        Debug.Log("Press 4 to use Sword (should do nothing)");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseItemByID(healthPotionID);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseItemByID(manaPotionID);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UseItemByID(omniPotionID);
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

        var allActionableCaps = instance.Definition.GetCapabilities<ActionableCapability>();

        int executedActions = 0;
        foreach (var cap in allActionableCaps)
        {
            HandlerRegistry.Execute(instance, cap, this.gameObject);
            executedActions++;
        }

        if (executedActions == 0)
        {
            Debug.Log($"Item '{instance.Definition.DisplayName}' has no actionable capabilities to execute.");
        }
    }

    private void RunAutomatedTests()
    {
        Debug.Log("--- Running Automated Item System Tests ---");
        try
        {
            // Test 1: Query for all consumables
            var consumables = _database.GetAllItemsWithCapability<ConsumableCapability>().ToList();
            Assert(consumables.Count >= 3, $"Test 1.1 FAILED: Expected at least 3 consumables, but found {consumables.Count}.");
            Assert(consumables.Any(d => d.ID == omniPotionID), "Test 1.2 FAILED: Omni Potion not found in consumables query.");
            LogTestResult("Test 1: GetAllItemsWithCapability<ConsumableCapability>", true);

            // Test 2: Query for all equippables
            var equipment = _database.GetAllItemsWithCapability<EquipmentCapability>().ToList();
            Assert(equipment.Count >= 1, $"Test 2.1 FAILED: Expected at least 1 piece of equipment, but found {equipment.Count}.");
            Assert(equipment.Any(d => d.ID == swordID), "Test 2.2 FAILED: Sword not found in equipment query.");
            LogTestResult("Test 2: GetAllItemsWithCapability<EquipmentCapability>", true);

            // Test 3: Query for all actionable items
            var actionables = _database.GetAllActionableItems().ToList();
            Assert(actionables.Count >= 3, $"Test 3.1 FAILED: Expected at least 3 actionable items, but found {actionables.Count}.");
            Assert(!actionables.Any(d => d.ID == swordID), "Test 3.2 FAILED: Sword was incorrectly found in actionable items query.");
            LogTestResult("Test 3: GetAllActionableItems", true);

            // Test 4: Verify Omni Potion's composition
            var omniDef = _database.GetByID(omniPotionID);
            Assert(omniDef != null, "Test 4.0 FAILED: Omni Potion definition not found.");
            var omniConsumableCaps = omniDef.GetCapabilities<ConsumableCapability>().ToList();
            Assert(omniConsumableCaps.Count == 2, $"Test 4.1 FAILED: Omni Potion should have 2 ConsumableCapabilities, but has {omniConsumableCaps.Count}.");
            Assert(omniConsumableCaps.Any(c => c.TypeToRestore == ConsumableCapability.ResourceType.Health), "Test 4.2 FAILED: Omni Potion is missing Health capability.");
            Assert(omniConsumableCaps.Any(c => c.TypeToRestore == ConsumableCapability.ResourceType.Mana), "Test 4.3 FAILED: Omni Potion is missing Mana capability.");
            LogTestResult("Test 4: Omni Potion Composition", true);

            // Test 5: Edge case - getting non-existent item
            var nonExistent = _database.GetByID("item_that_does_not_exist");
            Assert(nonExistent == null, "Test 5.1 FAILED: GetByID should return null for non-existent ID.");
            LogTestResult("Test 5: Non-Existent Item Query", true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>AUTOMATED TEST SUITE FAILED WITH AN EXCEPTION:</color> {e.Message}");
        }

        Debug.Log("--- Automated Tests Complete ---");
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
