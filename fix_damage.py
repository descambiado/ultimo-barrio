with open('Code/UltimoBarrio/Combat/MeleeWeapon.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('Sandbox.DamageInfo', 'DamageEvent')

if 'using UltimoBarrio.Core;' not in text:
    text = 'using UltimoBarrio.Core;\n' + text

with open('Code/UltimoBarrio/Combat/MeleeWeapon.cs', 'w', encoding='utf-8') as f:
    f.write(text)
