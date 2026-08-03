
import json
scene_path = 'Assets/scenes/main.scene'
d = json.load(open(scene_path, 'r', encoding='utf-8'))

world = None
for go in d['GameObjects']:
    if go.get('Name') == 'World':
        world = go
        break

if world and 'Children' in world:
    original_len = len(world['Children'])
    world['Children'] = [c for c in world['Children'] if c.get('Name') != 'Prototype Apartment A01']
    print('Removed ' + str(original_len - len(world['Children'])) + ' raw A01 objects from World.')

with open(scene_path, 'w', encoding='utf-8') as f:
    json.dump(d, f, indent=2)

