import json
import uuid

with open("Assets/prefabs/player.prefab", "r", encoding="utf-8") as f:
    data = json.load(f)

components = data["RootObject"]["Components"]

def add_comp(type_name):
    components.append({
        "__type": type_name,
        "__guid": str(uuid.uuid4()),
        "__enabled": True
    })

add_comp("UltimoBarrio.Combat.HealthComponent")
add_comp("UltimoBarrio.Inventory.InventoryComponent")
add_comp("UltimoBarrio.Economy.Wallet")
add_comp("UltimoBarrio.UI.PlayerMessageService")

# We assume InteractionPromptPanel is a UI component we want on the player
add_comp("UltimoBarrio.UI.InteractionPromptPanel")

with open("Assets/prefabs/player.prefab", "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2)

print("Added custom components to player.prefab")
