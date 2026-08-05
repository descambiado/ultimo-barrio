import json
import uuid

scene_file = 'Assets/scenes/ultimo_barrio_alpha.scene'
with open(scene_file, 'r', encoding='utf-8') as f:
    data = json.load(f)

new_guid = str(uuid.uuid4())
station = {
  "__guid": new_guid,
  "Flags": 0,
  "Name": "Crafting Station",
  "Position": "0,200,25",
  "Enabled": True,
  "Components": [
    {
      "__type": "Sandbox.PrefabScene",
      "__guid": str(uuid.uuid4()),
      "__enabled": True,
      "Prefab": "prefabs/world/ub_crafting_station.prefab"
    }
  ]
}

if 'GameObjects' in data:
    data['GameObjects'].append(station)
else:
    print("Could not find GameObjects array")

with open(scene_file, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print("Injected Crafting Station to scene")
