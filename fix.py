import os
path = r'C:\Users\davyd\.gemini\antigravity\brain\ab476ef3-b11e-42ff-9976-249fc387e4b5\.system_generated\worktrees\subagent-Economy-Developer-self-4422be3a\Code\UltimoBarrio\PlayerInteractor.cs'
with open(path, 'r', encoding='utf-8') as f: content = f.read()
target = '''                var pickup = tr.GameObject.Components.Get<WorldItemPickup>();
                if (pickup != null)
                {
                    _hud?.ShowPrompt(pickup.GetComponent<IInteractable>().GetInteractionPrompt(new InteractionRequest { InteractorId = player.GameObject.Network.OwnerId.ToString(), InteractorObject = player.GameObject }), \"Pulsa E\");
                    if (Input.Pressed(\"Use\"))
                    {
                        var interactable = pickup.GetComponent<IInteractable>();
                        if (interactable != null && interactable.CanInteract(GameObject.Id))
                        {
                            interactable.OnInteract(GameObject.Id);
                            _hud?.ShowMessage(\"Objeto recogido\");
                        }
                    }
                    return;
                }'''
replacement = '''                var interactable = tr.GameObject.Components.Get<IInteractable>();
                if (interactable != null)
                {
                    var req = new InteractionRequest { InteractorId = GameObject.Network.OwnerId.ToString(), InteractorObject = GameObject };
                    _hud?.ShowPrompt(interactable.GetInteractionPrompt(req), \"Pulsa E\");
                    if (Input.Pressed(\"Use\"))
                    {
                        if (interactable.CanInteract(req))
                        {
                            interactable.OnInteract(req);
                        }
                    }
                    return;
                }'''
content = content.replace(target, replacement)
with open(path, 'w', encoding='utf-8') as f: f.write(content)
