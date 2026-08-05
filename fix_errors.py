import re

with open('Code/UltimoBarrio/Items/WorldItemPickup.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = re.sub(r'Log\.Info\([^;]*pickup[^;]*\);', 'Log.Info("Pickup");', text)
with open('Code/UltimoBarrio/Items/WorldItemPickup.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('Code/UltimoBarrio/Crafting/CraftingStation.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = re.sub(r'Log\.Info\([^;]*interact[^;]*\);', 'Log.Info("Interact");', text)
text = re.sub(r'Log\.Info\([^;]*Switched[^;]*\);', 'Log.Info("Switched");', text)
text = re.sub(r'Log\.Info\([^;]*Missing[^;]*\);', 'Log.Info("Missing");', text)
with open('Code/UltimoBarrio/Crafting/CraftingStation.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('Code/UltimoBarrio/Players/PlayerCameraEffects.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('.IsDucked', ' != null')
text = text.replace('Transform.LocalPosition', 'LocalPosition')
text = text.replace('Transform.LocalRotation', 'LocalRotation')
with open('Code/UltimoBarrio/Players/PlayerCameraEffects.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('Code/UltimoBarrio/Players/PlayerMovementModifier.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('.IsDucked', ' != null')
text = text.replace('CurrentType == HeldItemType.', 'CurrentSlot == HeldItemSlot.')
text = text.replace('HeldItemType.Melee', 'HeldItemSlot.Melee')
text = text.replace('HeldItemType.Primary', 'HeldItemSlot.Primary')
with open('Code/UltimoBarrio/Players/PlayerMovementModifier.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('Code/UltimoBarrio/Combat/HeldItemController.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('GetBoneTransform(', 'GetBoneObject(')
text = text.replace('global::Sandbox.Transform.Zero', 'null')
text = text.replace('newWep.WorldTransform = boneT;', 'newWep.SetParent(boneT);')
with open('Code/UltimoBarrio/Combat/HeldItemController.cs', 'w', encoding='utf-8') as f:
    f.write(text)
