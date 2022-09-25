using Godot;
using System;
using System.Collections.Generic;

public partial class BlockNode : ColorRect {
    private static Dictionary<int, Color> _colorMap = new() {
        [2] = Colors.Red,
        [4] = Colors.Blue,
        [8] = Colors.Green,
        [16] = Colors.Beige
    };

    private Label _label;

    // todo: make this not int but concrete type that allows only certain values
    private int _value;

    public int Value {
        get => _value;
        set {
            _value = value;
            Color = _colorMap[value];
            _label.Text = value.ToString();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _label = (Label)FindChild("Label");
    }

    // todo: this is useless
    public void Resize(Vector2 newSize) {
        Size = newSize;
    }
}