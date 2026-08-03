
import json
scene_path = 'Assets/scenes/main.scene'
d = json.load(open(scene_path, 'r', encoding='utf-8'))

# Clean root
new_root = []
for go in d['GameObjects']:
    if go.get('__Prefab') == 'prefabs/world/pf_apartment_unit.prefab':
        continue
    if go.get('Name') == 'apartment-a01' or go.get('Name') == 'apartment-a02':
        continue
    if go.get('Name') == 'World':
        # Clean world children
        if 'Children' in go:
            go['Children'] = [c for c in go['Children'] if 'Apartment' not in c.get('Name', '')]
    new_root.append(go)
d['GameObjects'] = new_root

with open(scene_path, 'w', encoding='utf-8') as f:
    json.dump(d, f, indent=2)
print('Scene cleaned.')

