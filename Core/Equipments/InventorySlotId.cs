namespace Core;

public enum InventorySlotId
{
    Ammo = 0,
    Head = 1,
    Neck = 2,
    Shoulder = 3,
    Shirt = 4,
    Chest = 5,
    Waist = 6,
    Legs = 7,
    Feet = 8,
    Wrists = 9,
    Hands = 10,
    Finger_1 = 11,
    Finger_2 = 12,
    Trinket_1 = 13,
    Trinket_2 = 14,
    Back = 15,
    Mainhand = 16,
    Offhand = 17,
    Ranged = 18,
    Tabard = 19,
    Bag_0 = 20,
    Bag_1 = 21,
    Bag_2 = 22,
    Bag_3 = 23
}

public static class InventorySlotId_Extension
{
    public static string ToStringF(this InventorySlotId value) => value switch
    {
        InventorySlotId.Ammo => nameof(InventorySlotId.Ammo),
        InventorySlotId.Head => nameof(InventorySlotId.Head),
        InventorySlotId.Neck => nameof(InventorySlotId.Neck),
        InventorySlotId.Shoulder => nameof(InventorySlotId.Shoulder),
        InventorySlotId.Shirt => nameof(InventorySlotId.Shirt),
        InventorySlotId.Chest => nameof(InventorySlotId.Chest),
        InventorySlotId.Waist => nameof(InventorySlotId.Waist),
        InventorySlotId.Legs => nameof(InventorySlotId.Legs),
        InventorySlotId.Feet => nameof(InventorySlotId.Feet),
        InventorySlotId.Wrists => nameof(InventorySlotId.Wrists),
        InventorySlotId.Hands => nameof(InventorySlotId.Hands),
        InventorySlotId.Finger_1 => nameof(InventorySlotId.Finger_1),
        InventorySlotId.Finger_2 => nameof(InventorySlotId.Finger_2),
        InventorySlotId.Trinket_1 => nameof(InventorySlotId.Trinket_1),
        InventorySlotId.Trinket_2 => nameof(InventorySlotId.Trinket_2),
        InventorySlotId.Back => nameof(InventorySlotId.Back),
        InventorySlotId.Mainhand => nameof(InventorySlotId.Mainhand),
        InventorySlotId.Offhand => nameof(InventorySlotId.Offhand),
        InventorySlotId.Ranged => nameof(InventorySlotId.Ranged),
        InventorySlotId.Tabard => nameof(InventorySlotId.Tabard),
        InventorySlotId.Bag_0 => nameof(InventorySlotId.Bag_0),
        InventorySlotId.Bag_1 => nameof(InventorySlotId.Bag_1),
        InventorySlotId.Bag_2 => nameof(InventorySlotId.Bag_2),
        InventorySlotId.Bag_3 => nameof(InventorySlotId.Bag_3),
        _ => throw new System.NotImplementedException(),
    };
}
