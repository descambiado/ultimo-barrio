
import json
import uuid

with open('Assets/prefabs/world/pf_apartment_unit.prefab', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Find Stash Anchor
def find_go(gos, name):
    for go in gos:
        if go.get('Name') == name: return go
        if 'Children' in go:
            found = find_go(go['Children'], name)
            if found: return found
    return None

root = data.get('RootObject', data.get('GameObjects', [data])[0] if 'GameObjects' in data else data)
stash = find_go([root], 'Stash Anchor')

if stash:
    components = stash.get('Components', [])
    has_collider = any(c.get('__type') == 'Sandbox.BoxCollider' for c in components)
    if not has_collider:
        components.append({
            '__type': 'Sandbox.BoxCollider',
            '__guid': str(uuid.uuid4()),
            '__enabled': True,
            'Center': '0,0,0',
            'Scale': '50,50,50'
        })
        stash['Components'] = components
        with open('Assets/prefabs/world/pf_apartment_unit.prefab', 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2)
        print('Added BoxCollider to Stash Anchor')
    else:
        print('BoxCollider already exists on Stash Anchor')
else:
    print('Stash Anchor not found!')

