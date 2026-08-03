
import json

with open('Assets/scenes/main.scene', 'r', encoding='utf-8') as f:
    data = json.load(f)

for go in data.get('GameObjects', []):
    if go.get('Name') == 'apartment-a02':
        def fix_comp(node):
            if 'Components' in node:
                for c in node['Components']:
                    if c.get('__type') == 'UltimoBarrio.Apartments.ApartmentClaimInteractable':
                        c['ApartmentId'] = 'apartment-a02'
            for child in node.get('Children', []):
                fix_comp(child)
        fix_comp(go)

with open('Assets/scenes/main.scene', 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)
print('Patched A02')

