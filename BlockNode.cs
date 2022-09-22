using Godot;
using System;
using System.Collections.Generic;

public partial class BlockNode : Node2D {

    private static Dictionary<int, Color> _colorMap = new() {
        [2] = Colors.Red,
        [4] = Colors.Blue
    };

    private ColorRect _colorRect;

    // todo: make this not int but concrete type that allows only certain values
    private int _value;
    public int Value {
        get => _value;
        set {
            _value = value;
            _colorRect.Color = _colorMap[value];
        }
    }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _colorRect = FindChild("ColorRect") as ColorRect;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}