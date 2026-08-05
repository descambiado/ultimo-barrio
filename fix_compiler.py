with open('Code/UltimoBarrio/Combat/MeleeWeapon.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('DamageEvent', 'Sandbox.DamageInfo')
with open('Code/UltimoBarrio/Combat/MeleeWeapon.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('Code/UltimoBarrio/Players/PlayerMovementModifier.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('Controller.IsDucked', 'Input.Down(\"duck\")')
text = text.replace('HeldItemType', 'HeldItemSlot')
text = text.replace('CurrentType', 'CurrentSlot')
with open('Code/UltimoBarrio/Players/PlayerMovementModifier.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('Code/UltimoBarrio/Players/PlayerCameraEffects.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('controller.IsDucked', 'Input.Down(\"duck\")')
with open('Code/UltimoBarrio/Players/PlayerCameraEffects.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('Code/UltimoBarrio/Combat/HeldItemController.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('GetBoneTransform(', 'GetBoneObject(')
text = text.replace('global::Sandbox.Transform.Zero', 'null')
text = text.replace('newWep.WorldTransform = boneT;', 'newWep.SetParent(boneT);')
with open('Code/UltimoBarrio/Combat/HeldItemController.cs', 'w', encoding='utf-8') as f:
    f.write(text)
