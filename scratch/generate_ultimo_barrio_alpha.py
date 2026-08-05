import json
import uuid

def g():
    return str(uuid.uuid4())

scene = {
    "__guid": g(),
    "GameObjects": []
}

# 1. MapInstance
scene["GameObjects"].append({
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "MapInstance",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "1,1,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [
        {
            "__type": "Sandbox.MapInstance",
            "__guid": g(),
            "__enabled": True,
            "MapName": "thieves.rpdowntown3t",
            "UseMapFromLaunch": True
        }
    ],
    "Children": []
})

# 2. UltimoBarrioWorldOverlay Structured by Sectors
overlay = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "UltimoBarrioWorldOverlay",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "1,1,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [],
    "Children": []
}

sector_res = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "Sector_Residential",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "1,1,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [],
    "Children": []
}

apts_config = [
    ("apartment-a01", "-500, -300, 20"),
    ("apartment-a02", "-500, 300, 20"),
    ("apartment-a03", "0, -600, 20"),
    ("apartment-a04", "0, 600, 20"),
    ("apartment-a05", "500, -300, 20"),
    ("apartment-a06", "500, 300, 20")
]

for apt_id, pos_str in apts_config:
    px, py, pz = [float(c) for c in pos_str.split(",")]
    stash_guid = g()
    door_guid = g()
    spawn_guid = g()
    raid_guid = g()

    apt_go = {
        "__guid": g(),
        "__version": 2,
        "Flags": 0,
        "Name": apt_id,
        "Position": pos_str,
        "Rotation": "0,0,0,1",
        "Scale": "1,1,1",
        "Tags": "",
        "Enabled": True,
        "NetworkMode": 2,
        "NetworkFlags": 0,
        "NetworkOrphaned": 0,
        "NetworkTransmit": True,
        "OwnerTransfer": 1,
        "Components": [
            {
                "__type": "UltimoBarrio.Apartments.ApartmentComponent",
                "__guid": g(),
                "__enabled": True,
                "ApartmentId": apt_id,
                "ClaimState": 0,
                "OwnerId": "",
                "DoorReference": {"_type": "gameobject", "id": door_guid},
                "StashReference": {"_type": "gameobject", "id": stash_guid},
                "SpawnReference": {"_type": "gameobject", "id": spawn_guid},
                "RaidTargetReference": {"_type": "gameobject", "id": raid_guid}
            }
        ],
        "Children": [
            {
                "__guid": door_guid,
                "__version": 2,
                "Flags": 0,
                "Name": "Claim Portal",
                "Position": f"{px + 40},{py},{pz + 40}",
                "Rotation": "0,0,0,1",
                "Scale": "0.5,0.5,1",
                "Tags": "",
                "Enabled": True,
                "NetworkMode": 2,
                "NetworkFlags": 0,
                "NetworkOrphaned": 0,
                "NetworkTransmit": True,
                "OwnerTransfer": 1,
                "Components": [
                    {
                        "__type": "UltimoBarrio.Apartments.ApartmentClaimInteractable",
                        "__guid": g(),
                        "__enabled": True,
                        "ApartmentId": apt_id
                    },
                    {
                        "__type": "UltimoBarrio.Apartments.ApartmentDoorPolicy",
                        "__guid": g(),
                        "__enabled": True,
                        "ApartmentId": apt_id,
                        "IsLocked": True
                    },
                    {
                        "__type": "Sandbox.BoxCollider",
                        "__guid": g(),
                        "__enabled": True,
                        "Center": "0,0,0",
                        "Scale": "40,40,80",
                        "Static": True
                    },
                    {
                        "__type": "Sandbox.ModelRenderer",
                        "__guid": g(),
                        "__enabled": True,
                        "Model": "models/sbox_props/wooden_door/wooden_door.vmdl"
                    }
                ],
                "Children": []
            },
            {
                "__guid": stash_guid,
                "__version": 2,
                "Flags": 0,
                "Name": "Stash Anchor",
                "Position": f"{px - 40},{py + 30},{pz + 20}",
                "Rotation": "0,0,0,1",
                "Scale": "0.5,0.5,0.5",
                "Tags": "",
                "Enabled": True,
                "NetworkMode": 2,
                "NetworkFlags": 0,
                "NetworkOrphaned": 0,
                "NetworkTransmit": True,
                "OwnerTransfer": 1,
                "Components": [
                    {
                        "__type": "UltimoBarrio.StashComponent",
                        "__guid": g(),
                        "__enabled": True,
                        "ApartmentId": apt_id,
                        "MaxSlots": 24
                    },
                    {
                        "__type": "UltimoBarrio.InventoryComponent",
                        "__guid": g(),
                        "__enabled": True,
                        "InventoryId": f"{apt_id}:stash",
                        "MaxSlots": 24
                    },
                    {
                        "__type": "Sandbox.BoxCollider",
                        "__guid": g(),
                        "__enabled": True,
                        "Center": "0,0,0",
                        "Scale": "50,50,50",
                        "Static": True
                    },
                    {
                        "__type": "Sandbox.ModelRenderer",
                        "__guid": g(),
                        "__enabled": True,
                        "Model": "models/sbox_props/plastic_crate/plastic_crate.vmdl"
                    }
                ],
                "Children": []
            },
            {
                "__guid": spawn_guid,
                "__version": 2,
                "Flags": 0,
                "Name": "Owner Spawn Anchor",
                "Position": f"{px},{py},{pz + 10}",
                "Rotation": "0,0,0,1",
                "Scale": "1,1,1",
                "Tags": "",
                "Enabled": True,
                "NetworkMode": 2,
                "NetworkFlags": 0,
                "NetworkOrphaned": 0,
                "NetworkTransmit": True,
                "OwnerTransfer": 1,
                "Components": [],
                "Children": []
            },
            {
                "__guid": raid_guid,
                "__version": 2,
                "Flags": 0,
                "Name": "Raid Target Anchor",
                "Position": f"{px + 20},{py},{pz + 10}",
                "Rotation": "0,0,0,1",
                "Scale": "1,1,1",
                "Tags": "",
                "Enabled": True,
                "NetworkMode": 2,
                "NetworkFlags": 0,
                "NetworkOrphaned": 0,
                "NetworkTransmit": True,
                "OwnerTransfer": 1,
                "Components": [],
                "Children": []
            }
        ]
    }
    sector_res["Children"].append(apt_go)

overlay["Children"].append(sector_res)

# Sector Plaza (Trader Kiosk)
sector_plaza = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "Sector_Plaza",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "1,1,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [],
    "Children": [
        {
            "__guid": g(),
            "__version": 2,
            "Flags": 0,
            "Name": "Kiosko Comerciante",
            "Position": "0,0,25",
            "Rotation": "0,0,0,1",
            "Scale": "1,1,1",
            "Tags": "",
            "Enabled": True,
            "NetworkMode": 2,
            "NetworkFlags": 0,
            "NetworkOrphaned": 0,
            "NetworkTransmit": True,
            "OwnerTransfer": 1,
            "Components": [
                {
                    "__type": "UltimoBarrio.Trading.Trader",
                    "__guid": g(),
                    "__enabled": True,
                    "WaterPrice": 10,
                    "MedicinePrice": 20,
                    "AmmoPrice": 5,
                    "ScrapSellPrice": 2
                },
                {
                    "__type": "Sandbox.BoxCollider",
                    "__guid": g(),
                    "__enabled": True,
                    "Center": "0,0,25",
                    "Scale": "60,60,50",
                    "Static": True
                },
                {
                    "__type": "Sandbox.ModelRenderer",
                    "__guid": g(),
                    "__enabled": True,
                    "Model": "models/sbox_props/cash_register/cash_register.vmdl"
                }
            ],
            "Children": []
        }
    ]
}
overlay["Children"].append(sector_plaza)

# Sector Scrapyard & Resources
sector_scrap = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "Sector_Scrapyard",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "1,1,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [],
    "Children": []
}

import math
for i in range(20):
    angle = (i / 20.0) * 2 * math.pi
    r = 180 + (i % 3) * 60
    x = r * math.cos(angle)
    y = r * math.sin(angle)
    sector_scrap["Children"].append({
        "__guid": g(),
        "__version": 2,
        "Flags": 0,
        "Name": f"Chatarra {i+1}",
        "Position": f"{x:.1f},{y:.1f},15",
        "Rotation": "0,0,0,1",
        "Scale": "0.5,0.5,0.5",
        "Tags": "",
        "Enabled": True,
        "NetworkMode": 2,
        "NetworkFlags": 0,
        "NetworkOrphaned": 0,
        "NetworkTransmit": True,
        "OwnerTransfer": 1,
        "Components": [
            {
                "__type": "UltimoBarrio.WorldItemPickup",
                "__guid": g(),
                "__enabled": True,
                "ItemId": "chatarra",
                "Amount": (i % 3) + 1
            },
            {
                "__type": "UltimoBarrio.World.ResourceNode",
                "__guid": g(),
                "__enabled": True,
                "ItemId": "chatarra",
                "Amount": (i % 3) + 1,
                "RespawnTime": 30
            },
            {
                "__type": "Sandbox.BoxCollider",
                "__guid": g(),
                "__enabled": True,
                "Center": "0,0,0",
                "Scale": "20,20,20"
            },
            {
                "__type": "Sandbox.ModelRenderer",
                "__guid": g(),
                "__enabled": True,
                "Model": "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl"
            }
        ],
        "Children": []
    })

overlay["Children"].append(sector_scrap)

scene["GameObjects"].append(overlay)

# 3. Systems Root
systems = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "Systems",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "1,1,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [
        {
            "__type": "UltimoBarrio.WorldTime.WorldClock",
            "__guid": g(),
            "__enabled": True
        },
        {
            "__type": "UltimoBarrio.Raids.RaidManager",
            "__guid": g(),
            "__enabled": True
        },
        {
            "__type": "UltimoBarrio.World.LootRespawner",
            "__guid": g(),
            "__enabled": True,
            "IntervalSeconds": 45
        },
        {
            "__type": "UltimoBarrio.Audio.UltimoBarrioAudioCatalog",
            "__guid": g(),
            "__enabled": True
        }
    ],
    "Children": [
        {
            "__guid": g(),
            "__version": 2,
            "Flags": 0,
            "Name": "Main Camera",
            "Position": "-300,0,100",
            "Rotation": "0,0,0,1",
            "Scale": "1,1,1",
            "Tags": "",
            "Enabled": True,
            "NetworkMode": 2,
            "NetworkFlags": 0,
            "NetworkOrphaned": 0,
            "NetworkTransmit": True,
            "OwnerTransfer": 1,
            "Components": [
                {
                    "__type": "Sandbox.CameraComponent",
                    "__guid": g(),
                    "__enabled": True,
                    "BackgroundColor": "0.33333,0.46275,0.52157,1",
                    "ClearFlags": "All",
                    "EnablePostProcessing": True,
                    "FieldOfView": 60,
                    "FovAxis": "Horizontal",
                    "IsMainCamera": True,
                    "Priority": 1,
                    "ZFar": 5000,
                    "ZNear": 5
                }
            ],
            "Children": []
        },
        {
            "__guid": g(),
            "__version": 2,
            "Flags": 0,
            "Name": "Network",
            "Position": "0,0,0",
            "Rotation": "0,0,0,1",
            "Scale": "1,1,1",
            "Tags": "",
            "Enabled": True,
            "NetworkMode": 2,
            "NetworkFlags": 0,
            "NetworkOrphaned": 0,
            "NetworkTransmit": True,
            "OwnerTransfer": 1,
            "Components": [
                {
                    "__type": "Sandbox.NetworkHelper",
                    "__guid": g(),
                    "__enabled": True,
                    "PlayerPrefab": {"_type": "gameobject", "prefab": "prefabs/player.prefab"},
                    "StartServer": True
                }
            ],
            "Children": []
        },
        {
            "__guid": g(),
            "__version": 2,
            "Flags": 0,
            "Name": "Apartment Claims",
            "Position": "0,0,0",
            "Rotation": "0,0,0,1",
            "Scale": "1,1,1",
            "Tags": "",
            "Enabled": True,
            "NetworkMode": 2,
            "NetworkFlags": 0,
            "NetworkOrphaned": 0,
            "NetworkTransmit": True,
            "OwnerTransfer": 1,
            "Components": [
                {
                    "__type": "UltimoBarrio.Apartments.ApartmentClaimService",
                    "__guid": g(),
                    "__enabled": True,
                    "ClaimDistance": 5000,
                    "SaveSlotId": "ultimo_barrio_alpha"
                }
            ],
            "Children": []
        }
    ]
}
scene["GameObjects"].append(systems)

# 4. SpawnPoints
spawns = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "SpawnPoints",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "1,1,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [],
    "Children": [
        {
            "__guid": g(),
            "__version": 2,
            "Flags": 0,
            "Name": "Primary Spawn A",
            "Position": "-100,-100,10",
            "Rotation": "0,0,0,1",
            "Scale": "1,1,1",
            "Tags": "",
            "Enabled": True,
            "NetworkMode": 2,
            "NetworkFlags": 0,
            "NetworkOrphaned": 0,
            "NetworkTransmit": True,
            "OwnerTransfer": 1,
            "Components": [
                {
                    "__type": "Sandbox.SpawnPoint",
                    "__guid": g(),
                    "__enabled": True
                }
            ],
            "Children": []
        },
        {
            "__guid": g(),
            "__version": 2,
            "Flags": 0,
            "Name": "Primary Spawn B",
            "Position": "100,100,10",
            "Rotation": "0,0,0,1",
            "Scale": "1,1,1",
            "Tags": "",
            "Enabled": True,
            "NetworkMode": 2,
            "NetworkFlags": 0,
            "NetworkOrphaned": 0,
            "NetworkTransmit": True,
            "OwnerTransfer": 1,
            "Components": [
                {
                    "__type": "Sandbox.SpawnPoint",
                    "__guid": g(),
                    "__enabled": True
                }
            ],
            "Children": []
        }
    ]
}
scene["GameObjects"].append(spawns)

with open("Assets/scenes/ultimo_barrio_alpha.scene", "w", encoding="utf-8") as f:
    json.dump(scene, f, indent=2)

print("Regenerated Assets/scenes/ultimo_barrio_alpha.scene with Cloud models and sector hierarchy!")
