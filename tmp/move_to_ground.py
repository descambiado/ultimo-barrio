
import json

scene_path = 'Assets/scenes/main.scene'
with open(scene_path, 'r', encoding='utf-8') as f:
    d = json.load(f)

for go in d.get('GameObjects', []):
    if go.get('Name') == 'apartment-a02':
        go['Position'] = '300,0,0'  # Put it on the ground next to A01
    
    if go.get('Name') == 'World':
        for child in go.get('Children', []):
            if child.get('Name') == 'Chatarra A02':
                child['Position'] = '150,150,50' # Put it on the ground between them

with open(scene_path, 'w', encoding='utf-8') as f:
    json.dump(d, f, indent=2)

print('Moved A02 and Chatarra to the ground level.')

