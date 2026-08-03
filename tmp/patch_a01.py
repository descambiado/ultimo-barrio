
import json
scene_path = 'Assets/scenes/main.scene'
d = json.load(open(scene_path, 'r', encoding='utf-8'))
for go in d['GameObjects']:
    if go.get('__guid') == 'b416f7b7-2009-45d9-bac6-78314448c93f':
        patch = go.setdefault('__PrefabInstancePatch', {})
        overrides = patch.setdefault('PropertyOverrides', [])
        
        has_override = any(o.get('Property') == 'ApartmentId' for o in overrides)
        if not has_override:
            overrides.append({
                'Target': {
                    'Type': 'Component',
                    'IdValue': '0f58841d-b584-4da9-a783-a5e34917e36c'
                },
                'Property': 'ApartmentId',
                'Value': 'apartment-a01'
            })
            print('Added override to A01.')

with open(scene_path, 'w', encoding='utf-8') as f:
    json.dump(d, f, indent=2)

