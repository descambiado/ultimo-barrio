
import json

with open('Assets/prefabs/world/pf_apartment_unit.prefab', 'r', encoding='utf-8') as f:
    data = json.load(f)

def fix_comp(node):
    if node.get('Name') == 'Stash Anchor':
        if 'Components' in node:
            for c in node['Components']:
                if c.get('__type') == 'Sandbox.BoxCollider':
                    c['Static'] = True
    for child in node.get('Children', []):
        fix_comp(child)

root = data.get('RootObject', data.get('GameObjects', [data])[0] if 'GameObjects' in data else data)
fix_comp(root)

with open('Assets/prefabs/world/pf_apartment_unit.prefab', 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)
print('Patched Stash BoxCollider to Static')

