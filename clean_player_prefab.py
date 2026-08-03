import json

with open("Assets/prefabs/player.prefab", "r", encoding="utf-8") as f:
    data = json.load(f)

comps = data.get("RootObject", {}).get("Components", [])
new_comps = []
for c in comps:
    t = c.get("__type")
    if t not in ["UltimoBarrio.UI.PlayerMessageService", "UltimoBarrio.UI.InteractionPromptPanel"]:
        new_comps.append(c)

data["RootObject"]["Components"] = new_comps

with open("Assets/prefabs/player.prefab", "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2)

print("Removed static/UI classes from player.prefab Components.")
