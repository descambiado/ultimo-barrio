import json
import uuid

with open("Assets/prefabs/player.prefab", "r", encoding="utf-8") as f:
    data = json.load(f)

comps = data.get("RootObject", {}).get("Components", [])
has_hud = any(c.get("__type") == "UltimoBarrio.UI.PlayerHud" for c in comps)

if not has_hud:
    comps.append({
        "__type": "UltimoBarrio.UI.PlayerHud",
        "__guid": str(uuid.uuid4()),
        "__enabled": True
    })
    with open("Assets/prefabs/player.prefab", "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
    print("Added PlayerHud to player.prefab")
else:
    print("PlayerHud already exists")
