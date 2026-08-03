
import json

# 1. Patch main.scene ClaimDistance
with open('Assets/scenes/main.scene', 'r', encoding='utf-8') as f:
    main_scene = json.load(f)

def fix_claim(node):
    if 'Components' in node:
        for c in node['Components']:
            if c.get('__type') == 'UltimoBarrio.Apartments.ApartmentClaimService':
                c['ClaimDistance'] = 5000
    for child in node.get('Children', []):
        fix_claim(child)

for go in main_scene.get('GameObjects', []):
    fix_claim(go)

with open('Assets/scenes/main.scene', 'w', encoding='utf-8') as f:
    json.dump(main_scene, f, indent=2)

# 2. Patch pf_apartment_unit.prefab Stash Anchor Collider
with open('Assets/prefabs/world/pf_apartment_unit.prefab', 'r', encoding='utf-8') as f:
    apt = json.load(f)

def fix_stash(node):
    if node.get('Name') == 'Stash Anchor':
        if 'Components' in node:
            for c in node['Components']:
                if c.get('__type') == 'Sandbox.BoxCollider':
                    c['__type'] = 'Sandbox.ModelCollider'
                    c.pop('Center', None)
                    c.pop('Scale', None)
                    c['Model'] = 'models/dev/box.vmdl'
    for child in node.get('Children', []):
        fix_stash(child)

root = apt.get('RootObject', apt.get('GameObjects', [apt])[0] if 'GameObjects' in apt else apt)
fix_stash(root)

with open('Assets/prefabs/world/pf_apartment_unit.prefab', 'w', encoding='utf-8') as f:
    json.dump(apt, f, indent=2)

print('Patched ClaimDistance and ModelCollider')

