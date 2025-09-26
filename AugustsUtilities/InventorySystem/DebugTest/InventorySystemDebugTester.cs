//using System.Linq;
//using AugustsUtility.InventorySystem;
//using AugustsUtility.ItemSystem;

//using AugustsUtility.Testing;
//using UnityEngine;

//public class InventorySystemDebugTester : MonoBehaviour
//{
//    private InventoryComponent _playerInv;
//    private InventoryComponent _chestInv1;
//    private InventoryComponent _chestInv2;
//    private GameObject _playerGO, _chestGO1, _chestGO2;
//    private ItemDatabase _database; // Changed to class-level field

//    private const string HEALTH_POTION = "potion_health";
//    private const string MANA_POTION = "potion_mana";
//    private const string SWORD = "sword_test";

//    private void Start()
//    {
//        if (InventoryManager.Instance == null)
//        {
//            Debug.LogError("InventoryManager singleton not found. Add it to a GameObject in the scene.");
//            enabled = false;
//            return;
//        }

//        // Assign to the class-level field
//        _database = Resources.Load<ItemDatabase>("ItemDatabase");
//        if (_database == null)
//        {
//            Debug.LogError("ItemDatabase not found in Resources folder.");
//            enabled = false;
//            return;
//        }

//        RunAllTests();
//    }

//    private void RunAllTests()
//    {
//        Debug.Log("--- Running Automated Inventory System Test Suite ---");
//        try
//        {
//            SetupTestInventories();
//            Test_AddItem();
//            Test_RemoveItem();
//            Test_HasAndCountItem();
//            Test_CapabilityQueries();
//            Test_Manager_GetByCategory();
//            Test_Manager_SwapItems();
//            Test_Manager_TransferItem();
//            Debug.Log("<color=green>--- ALL TESTS PASSED ---</color>");
//        }
//        catch (AssertionException e)
//        {
//            Debug.LogError($"<color=red>--- TEST SUITE FAILED ---</color>\n{e.Message}");
//        }
//        finally
//        {
//            CleanupTestInventories();
//        }
//    }

//    #region Test Implementation

//    private void Test_AddItem()
//    {
//        ResetAllInventories();
//        var p = _playerInv;

//        Assert.AreEqual(0, p.AddItem(HEALTH_POTION, 1), "T1.1: Add single item.");
//        Assert.AreEqual(1, p.GetItemCount(HEALTH_POTION));

//        int stackSize = p.Slots[0].ItemInstance.Definition.StackSize;
//        Assert.AreEqual(0, p.AddItem(HEALTH_POTION, stackSize), $"T1.2: Fill one stack and start another.");
//        Assert.AreEqual(stackSize + 1, p.GetItemCount(HEALTH_POTION));

//        int capacity = p.Size * stackSize;
//        p.AddItem(HEALTH_POTION, capacity); // Fill inventory completely
//        Assert.AreEqual(5, p.AddItem(HEALTH_POTION, 5), "T1.3: Add to full inventory should return remainder.");

//        LogTestPassed("AddItem");
//    }

//    private void Test_RemoveItem()
//    {
//        ResetAllInventories();
//        var p = _playerInv;
//        p.AddItem(HEALTH_POTION, 10);

//        Assert.IsTrue(p.RemoveItem(HEALTH_POTION, 5), "T2.1: Remove partial stack.");
//        Assert.AreEqual(5, p.GetItemCount(HEALTH_POTION));

//        Assert.IsFalse(p.RemoveItem(HEALTH_POTION, 10), "T2.2: Remove more than available should fail.");
//        Assert.AreEqual(5, p.GetItemCount(HEALTH_POTION));

//        Assert.IsTrue(p.RemoveItem(HEALTH_POTION, 5), "T2.3: Remove exact amount.");
//        Assert.AreEqual(0, p.GetItemCount(HEALTH_POTION));

//        LogTestPassed("RemoveItem");
//    }

//    private void Test_HasAndCountItem()
//    {
//        ResetAllInventories();
//        var p = _playerInv;
//        p.AddItem(HEALTH_POTION, 5);
//        p.AddItem(MANA_POTION, 1);

//        Assert.AreEqual(5, p.GetItemCount(HEALTH_POTION), "T3.1: GetItemCount check.");
//        Assert.IsTrue(p.HasItem(HEALTH_POTION, 5), "T3.2: HasItem exact amount check.");
//        Assert.IsFalse(p.HasItem(HEALTH_POTION, 6), "T3.3: HasItem insufficient amount check.");
//        Assert.IsFalse(p.HasItem(SWORD), "T3.4: HasItem for non-existent item check.");

//        LogTestPassed("HasAndCountItem");
//    }

//    private void Test_CapabilityQueries()
//    {
//        ResetAllInventories();
//        var p = _playerInv;
//        p.AddItem(HEALTH_POTION, 5);
//        p.AddItem(MANA_POTION, 3);
//        p.AddItem(SWORD, 1);

//        var firstConsumable = p.GetFirstItemWithCapability<ConsumableCapability>();
//        Assert.IsNotNull(firstConsumable, "T4.1: GetFirstItemWithCapability should find a potion.");

//        var allConsumables = p.GetAllItemsWithCapability<ConsumableCapability>();
//        Assert.AreEqual(2, allConsumables.Count, "T4.2: GetAllItemsWithCapability should find 2 types of potions.");
//        Assert.AreEqual(8, allConsumables.Sum(kvp => kvp.Value), "T4.3: Total count of consumables should be correct.");

//        var equipment = p.GetAllItemsWithCapability<EquipmentCapability>();
//        Assert.AreEqual(1, equipment.Count, "T4.4: Query should find the sword.");
//        Assert.AreEqual(1, equipment[SWORD]);

//        LogTestPassed("CapabilityQueries");
//    }

//    private void Test_Manager_GetByCategory()
//    {
//        var chests = InventoryManager.Instance.GetInventoriesByCategory(Category.StorageContainer).ToList();
//        Assert.AreEqual(2, chests.Count, "T5.1: Should find two StorageContainer inventories.");

//        var players = InventoryManager.Instance.GetInventoriesByCategory(Category.Player).ToList();
//        Assert.AreEqual(1, players.Count, "T5.2: Should find one Player inventory.");

//        LogTestPassed("Manager_GetByCategory");
//    }

//    private void Test_Manager_SwapItems()
//    {
//        ResetAllInventories();
//        _playerInv.AddItem(HEALTH_POTION, 1);
//        _chestInv1.AddItem(SWORD, 1);

//        var slotA = _playerInv.Slots[0];
//        var slotB = _chestInv1.Slots[0];

//        Assert.IsTrue(InventoryManager.Instance.SwapItems(slotA, slotB), "T6.1: Swap between two inventories.");
//        Assert.AreEqual(SWORD, _playerInv.Slots[0].ItemInstance.Definition.ID);
//        Assert.AreEqual(HEALTH_POTION, _chestInv1.Slots[0].ItemInstance.Definition.ID);

//        var slotC = _chestInv1.Slots[1]; // Empty slot
//        Assert.IsTrue(InventoryManager.Instance.SwapItems(slotB, slotC), "T6.2: Swap with an empty slot.");
//        Assert.IsTrue(_chestInv1.Slots[0].IsEmpty());
//        Assert.IsFalse(_chestInv1.Slots[1].IsEmpty());

//        LogTestPassed("Manager_SwapItems");
//    }

//    private void Test_Manager_TransferItem()
//    {
//        ResetAllInventories();
//        _playerInv.AddItem(HEALTH_POTION, 10);
//        // Use the Initialize method to change the size for the test
//        _chestInv1.Initialize(Category.StorageContainer, 1);

//        Assert.IsTrue(InventoryManager.Instance.TransferItem(_playerInv, _chestInv1, HEALTH_POTION, 5), "T7.1: Successful transfer.");
//        Assert.AreEqual(5, _playerInv.GetItemCount(HEALTH_POTION));
//        Assert.AreEqual(5, _chestInv1.GetItemCount(HEALTH_POTION));

//        int stackSize = _database.GetByID(HEALTH_POTION).StackSize;
//        _chestInv1.Slots[0].ItemInstance.Count = stackSize;

//        Assert.IsFalse(InventoryManager.Instance.TransferItem(_playerInv, _chestInv1, HEALTH_POTION, 1), "T7.2: Transfer to full inventory should fail.");
//        Assert.AreEqual(5, _playerInv.GetItemCount(HEALTH_POTION), "T7.3: Player item count should not change on failed transfer (rollback).");

//        LogTestPassed("Manager_TransferItem");
//    }

//    #endregion

//    #region Setup & Teardown

//    private void SetupTestInventories()
//    {
//        _playerGO = new GameObject("Test_Player") { tag = "Player" };
//        _playerInv = _playerGO.AddComponent<InventoryComponent>();
//        _playerInv.Initialize(Category.Player, 10); // Use Initialize

//        _chestGO1 = new GameObject("Test_Chest1");
//        _chestInv1 = _chestGO1.AddComponent<InventoryComponent>();
//        _chestInv1.Initialize(Category.StorageContainer, 10); // Use Initialize

//        _chestGO2 = new GameObject("Test_Chest2");
//        _chestInv2 = _chestGO2.AddComponent<InventoryComponent>();
//        _chestInv2.Initialize(Category.StorageContainer, 10); // Use Initialize
//    }

//    private void CleanupTestInventories()
//    {
//        Destroy(_playerGO);
//        Destroy(_chestGO1);
//        Destroy(_chestGO2);
//    }

//    private void ResetAllInventories()
//    {
//        // Re-initialize to reset size for tests that modify it
//        _playerInv.Initialize(Category.Player, 10);
//        _chestInv1.Initialize(Category.StorageContainer, 10);
//        _chestInv2.Initialize(Category.StorageContainer, 10);
//    }

//    private void LogTestPassed(string testName)
//    {
//        Debug.Log($"<color=green>PASSED:</color> {testName}");
//    }

//    #endregion
//}
