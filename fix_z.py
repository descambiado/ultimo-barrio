import json

scene_file = 'Assets/scenes/ultimo_barrio_alpha.scene'

with open(scene_file, 'r', encoding='utf-8') as f:
    data = json.load(f)

def fix_z(node):
    if 'Position' in node:
        parts = node['Position'].split(',')
        if len(parts) == 3:
            z = float(parts[2])
            if z > 100:
                parts[2] = '25.0'  # force to ground level approximately
                node['Position'] = ','.join(parts)
                print(f"Fixed {node.get('Name')} Z from {z} to 25.0")
    
    for c in node.get('Children', []):
        fix_z(c)

if 'GameObjects' in data:
    for go in data['GameObjects']: fix_z(go)

with open(scene_file, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)
