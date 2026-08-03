import os

def replace_in_file(path, replacements):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    for old, new in replacements:
        content = content.replace(old, new)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

base_dir = 'Code/UltimoBarrio'

# AIBase
replace_in_file(os.path.join(base_dir, 'AI/AIBase.cs'), [
    ('public virtual void TakeDamage(float amount, Vector3 position, Vector3 force, Guid attackerId)', 'public virtual void TakeDamage(DamageEvent damageEvent)')
])

# BaseInventoryComponent
replace_in_file(os.path.join(base_dir, 'Combat/BaseInventoryComponent.cs'), [
    ('public class BaseInventoryComponent : Component, IInventory\n    {', 'public class BaseInventoryComponent : Component, IInventory\n    {\n        [Property] public string InventoryId { get; set; } = System.Guid.NewGuid().ToString();\n        [Property] public int MaxSlots { get; set; } = 24;')
])

# StashComponent
replace_in_file(os.path.join(base_dir, 'Inventory/StashComponent.cs'), [
    ('public class StashComponent : Component, IInventory, IInteractable\n    {', 'public class StashComponent : Component, IInventory, IInteractable\n    {\n        [Property] public string InventoryId { get; set; } = System.Guid.NewGuid().ToString();\n        [Property] public int MaxSlots { get; set; } = 24;')
])

# ItemDefinition
replace_in_file(os.path.join(base_dir, 'Items/ItemDefinition.cs'), [
    ('[AssetType(', '[GameResource(')
])

print('Compile fix done.')
