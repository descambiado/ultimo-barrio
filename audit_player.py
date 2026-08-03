import json

with open("Assets/prefabs/player.prefab", "r", encoding="utf-8") as f:
    data = json.load(f)

comps = data.get("RootObject", {}).get("Components", [])
types = [c.get("__type") for c in comps]

print("PLAYER COMPONENTS:")
from collections import Counter
for k, v in Counter(types).items():
    print(f"{k}: {v}")
