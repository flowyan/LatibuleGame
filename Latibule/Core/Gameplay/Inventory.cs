namespace Latibule.Core.Gameplay;

public class Inventory
{
    public List<IItem?> Items = [];
    public int SelectedItemIndex = -1;

    public IItem? SelectedItem()
    {
        if (SelectedItemIndex > Items.Count - 1 || SelectedItemIndex < 0) return null;
        return Items[SelectedItemIndex];
    }
}