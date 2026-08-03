
import json
import uuid

with open('Assets/scenes/main.scene', 'r', encoding='utf-8') as f:
    data = json.load(f)

def fix_stash(node):
    if node.get('Name') == 'Stash Anchor':
        if 'Components' in node:
            # check if it already has a collider
            has_collider = any(c.get('__type') in ('Sandbox.BoxCollider', 'Sandbox.ModelCollider') for c in node['Components'])
            if not has_collider:
                node['Components'].append({
                    '__type': 'Sandbox.BoxCollider',
                    '__guid': str(uuid.uuid4()),
                    '__enabled': True,
                    'Center': '0,0,0',
                    'Scale': '60,60,60',
                    'Static': True
                })
    for child in node.get('Children', []):
        fix_stash(child)

for go in data.get('GameObjects', []):
    fix_stash(go)

with open('Assets/scenes/main.scene', 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)
print('Patched Stash Anchors in main.scene with BoxCollider')

