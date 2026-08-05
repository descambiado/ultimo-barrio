import json
import uuid

def g():
    return str(uuid.uuid4())

scene = {
    "__guid": g(),
    "GameObjects": []
}

# World Root
world = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "World",
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

# Floor
floor = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "MainStreetFloor",
    "Position": "0,0,0",
    "Rotation": "0,0,0,1",
    "Scale": "20,20,1",
    "Tags": "",
    "Enabled": True,
    "NetworkMode": 2,
    "NetworkFlags": 0,
    "NetworkOrphaned": 0,
    "NetworkTransmit": True,
    "OwnerTransfer": 1,
    "Components": [
        {
            "__type": "Sandbox.ModelRenderer",
            "__guid": g(),
            "__enabled": True,
            "Model": "models/dev/plane.vmdl",
            "MaterialOverride": "materials/default.vmat"
        },
        {
            "__type": "Sandbox.PlaneCollider",
            "__guid": g(),
            "__enabled": True,
            "Center": "0,0,0",
            "Normal": "0,0,1"
        }
    ],
    "Children": []
}
world["Children"].append(floor)

# Sun
sun = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "Sun",
    "Position": "0,0,500",
    "Rotation": "-0.3826834,0,0,0.9238795",
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
            "__type": "Sandbox.DirectionalLight",
            "__guid": g(),
            "__enabled": True,
            "LightColor": "0.98,0.92,0.84,1",
            "Shadows": True
        }
    ],
    "Children": []
}
world["Children"].append(sun)

# Sky
sky = {
    "__guid": g(),
    "__version": 2,
    "Flags": 0,
    "Name": "Sky",
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
            "__type": "Sandbox.SkyBox2D",
            "__guid": g(),
            "__enabled": True,
            "SkyMaterial": "materials/skybox/skybox_day_01.vmat"
        }
    ],
    "Children": []
}
world["Children"].append(sky)

# Trader Kiosk
trader_kiosk = {
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
            "Model": "models/dev/box.vmdl",
            "Tint": "0.8,0.6,0.2,1"
        }
    ],
    "Children": []
}
world["Children"].append(trader_kiosk)

# 6 Apartments
apartment_positions = [
    ("-400, -200, 0", "apartment-a01"),
    ("-400, 200, 0", "apartment-a02"),
    ("0, -400, 0", "apartment-a03"),
    ("0, 400, 0", "apartment-a04"),
    ("400, -200, 0", "apartment-a05"),
    ("400, 200, 0", "apartment-a06")
]

for pos_str, apt_id in apartment_positions:
    px, py, pz = [float(c) for c in pos_str.split(",")]
    
    stash_guid = g()
    door_guid = g()
    
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
                "StashReference": {"_type": "gameobject", "id": stash_guid}
            }
        ],
        "Children": [
            {
                "__guid": g(),
                "__version": 2,
                "Flags": 0,
                "Name": "Claim Portal",
                "Position": f"{px + 80},{py},{pz + 40}",
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
                        "Model": "models/dev/box.vmdl",
                        "Tint": "0.2,0.6,0.9,1"
                    }
                ],
                "Children": []
            },
            {
                "__guid": stash_guid,
                "__version": 2,
                "Flags": 0,
                "Name": "Stash Anchor",
                "Position": f"{px - 50},{py + 50},{pz + 20}",
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
                        "Scale": "60,60,60",
                        "Static": True
                    },
                    {
                        "__type": "Sandbox.ModelRenderer",
                        "__guid": g(),
                        "__enabled": True,
                        "Model": "models/dev/box.vmdl",
                        "Tint": "0.2,0.7,0.3,1"
                    }
                ],
                "Children": []
            },
            {
                "__guid": door_guid,
                "__version": 2,
                "Flags": 0,
                "Name": "Door",
                "Position": f"{px + 80},{py},{pz + 50}",
                "Rotation": "0,0,0,1",
                "Scale": "0.2,1,2",
                "Tags": "",
                "Enabled": True,
                "NetworkMode": 2,
                "NetworkFlags": 0,
                "NetworkOrphaned": 0,
                "NetworkTransmit": True,
                "OwnerTransfer": 1,
                "Components": [
                    {
                        "__type": "Sandbox.BoxCollider",
                        "__guid": g(),
                        "__enabled": True,
                        "Center": "0,0,0",
                        "Scale": "20,80,160",
                        "Static": True
                    },
                    {
                        "__type": "Sandbox.ModelRenderer",
                        "__guid": g(),
                        "__enabled": True,
                        "Model": "models/dev/box.vmdl",
                        "Tint": "0.5,0.3,0.1,1"
                    }
                ],
                "Children": []
            },
            {
                "__guid": g(),
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
            }
        ]
    }
    world["Children"].append(apt_go)

# Pickups: 20 Scrap, 6 Medicine, 6 Water, 4 Ammo, 2 Starter Weapons
pickups_data = []

# 20 Scrap
import math
for i in range(20):
    angle = (i / 20.0) * 2 * math.pi
    r = 150 + (i % 3) * 80
    x = r * math.cos(angle)
    y = r * math.sin(angle)
    pickups_data.append((f"Chatarra {i+1}", "chatarra", (i % 3) + 1, f"{x:.1f},{y:.1f},15", "0.6,0.6,0.2,1"))

# 6 Medicine
for i in range(6):
    angle = (i / 6.0) * 2 * math.pi + 0.3
    x = 250 * math.cos(angle)
    y = 250 * math.sin(angle)
    pickups_data.append((f"Medicina {i+1}", "medicine", 1, f"{x:.1f},{y:.1f},15", "0.9,0.2,0.2,1"))

# 6 Water
for i in range(6):
    angle = (i / 6.0) * 2 * math.pi + 0.6
    x = 300 * math.cos(angle)
    y = 300 * math.sin(angle)
    pickups_data.append((f"Agua {i+1}", "water", 1, f"{x:.1f},{y:.1f},15", "0.2,0.4,0.9,1"))

# 4 Ammo
for i in range(4):
    angle = (i / 4.0) * 2 * math.pi + 0.8
    x = 180 * math.cos(angle)
    y = 180 * math.sin(angle)
    pickups_data.append((f"Municion {i+1}", "ammo", 10, f"{x:.1f},{y:.1f},15", "0.3,0.8,0.3,1"))

# 2 Starter Weapons
pickups_data.append(("Pistola Inicial A", "weapon_pistol", 1, "-100, -100, 15", "0.1,0.1,0.1,1"))
pickups_data.append(("Pistola Inicial B", "weapon_pistol", 1, "100, 100, 15", "0.1,0.1,0.1,1"))

for name, item_id, amt, pos_str, tint in pickups_data:
    pickup_go = {
        "__guid": g(),
        "__version": 2,
        "Flags": 0,
        "Name": name,
        "Position": pos_str,
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
                "ItemId": item_id,
                "Amount": amt
            },
            {
                "__type": "Sandbox.BoxCollider",
                "__guid": g(),
                "__enabled": True,
                "Center": "0,0,0",
                "Scale": "20,20,20",
                "Static": False
            },
            {
                "__type": "Sandbox.ModelRenderer",
                "__guid": g(),
                "__enabled": True,
                "Model": "models/dev/box.vmdl",
                "Tint": tint
            }
        ],
        "Children": []
    }
    world["Children"].append(pickup_go)

scene["GameObjects"].append(world)

# Systems Root
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
                    "SaveSlotId": "barrio_alpha_01"
                }
            ],
            "Children": []
        }
    ]
}
scene["GameObjects"].append(systems)

# Spawn Points
spawn_points = {
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
            "Position": "-150,-150,10",
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
            "Position": "150,150,10",
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
scene["GameObjects"].append(spawn_points)

with open("Assets/scenes/barrio_alpha_01.scene", "w", encoding="utf-8") as f:
    json.dump(scene, f, indent=2)

print("Created Assets/scenes/barrio_alpha_01.scene successfully!")
