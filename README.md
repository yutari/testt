ví dụ file json test draw:
{
  "lines": [
    { "start": { "X": 0, "Y": 0, "Z": 0 }, "end": { "X": 100, "Y": 0, "Z": 0 } }
  ],
  "circles": [
    { "center": { "X": 150, "Y": 50, "Z": 0 }, "radius": 30.0 }
  ],
  "arcs": [
    { "center": { "X": 250, "Y": 50, "Z": 0 }, "radius": 40.0, "startAngle": 0.0, "endAngle": 1.5708 }
  ],
  "polylines": [
    {
      "vertices": [
        { "Point": { "X": 0, "Y": 100 }, "Bulge": 0.0 },
        { "Point": { "X": 100, "Y": 100 }, "Bulge": 1.0 }, 
        { "Point": { "X": 100, "Y": 150 }, "Bulge": 0.0 }
      ],
      "isClosed": false
    }
  ],
  "leaders": [
    {
      "points": [
        { "X": 200, "Y": 100, "Z": 0 },
        { "X": 220, "Y": 120, "Z": 0 },
        { "X": 240, "Y": 120, "Z": 0 }
      ],
      "text": "Leader demo"
    }
  ],
  "texts": [
    { "position": { "X": 0, "Y": 200, "Z": 0 }, "rotation": 0.0, "width": 0.0, "content": "DBText demo", "type": "DBText" },
    { "position": { "X": 0, "Y": 220, "Z": 0 }, "rotation": 0.0, "width": 50.0, "content": "MText demo\nDòng 2", "type": "MText" }
  ]
}
ví dụ file test select:
{
  "scope": "all",
  "filters": {
    "entityTypes": ["LINE", "CIRCLE"],
    "layers": [],
    "blocks": [],
    "colors": ["rbg(255,0,0)", "rbg(255,255,255)"]
  }
}

