import json
import uuid

scene = {
  "__guid": str(uuid.uuid4()),
  "GameObjects": [
    {
      "__guid": str(uuid.uuid4()),
      "Name": "Screen",
      "Position": "0,0,0",
      "Rotation": "0,0,0,1",
      "Scale": "1,1,1",
      "Enabled": True,
      "Components": [
        {
          "__type": "Sandbox.ScreenPanel",
          "__guid": str(uuid.uuid4()),
          "Opacity": 1,
          "Scale": 1,
          "ZIndex": 100
        },
        {
          "__type": "UltimoBarrio.QA.TestSpanishUI",
          "__guid": str(uuid.uuid4())
        }
      ]
    }
  ],
  "SceneProperties": {
    "FixedUpdateFrequency": 50,
    "MaxFixedUpdates": 5,
    "NetworkFrequency": 30,
    "NetworkInterpolation": True,
    "ThreadedAnimation": True,
    "TimeScale": 1,
    "UseFixedUpdate": True,
    "NavMesh": {
      "Enabled": False
    }
  }
}

with open('Assets/scenes/test_spanish_text.scene', 'w', encoding='utf-8') as f:
    json.dump(scene, f, indent=2)
