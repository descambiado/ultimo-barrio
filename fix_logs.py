import re

with open('Code/UltimoBarrio/Crafting/CraftingStation.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()
with open('Code/UltimoBarrio/Crafting/CraftingStation.cs', 'w', encoding='utf-8') as f:
    for line in lines:
        if 'Log.Info(' in line and '\"' not in line and '$' not in line:
            line = '// ' + line
        f.write(line)

with open('Code/UltimoBarrio/Combat/MeleeWeapon.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('DamageEvent', 'Sandbox.DamageInfo')
text = text.replace('Transform.Position', 'WorldPosition')
with open('Code/UltimoBarrio/Combat/MeleeWeapon.cs', 'w', encoding='utf-8') as f:
    f.write(text)
