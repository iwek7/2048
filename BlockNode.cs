using Godot;
using System;
using System.Collections.Generic;

public partial class BlockNode : ColorRect {
    // todo: make this palette nicer 
    private static Dictionary<int, Color> _colorMap = new() {
        [2] = new Color("#70f6de"),
        [4] = new Color("#00bae0"),
        [8] = new Color("#0079d4"),
        [16] = new Color("#182b9b"),
        [32] = new Color("#56279b"),
        [64] = new Color("#7c2098"),
        [128] = new Color("#9b1893"),
        [256] = new Color("#da0c79"),
        [512] = new Color("#ff4357"),
        [1024] = new Color("#ff7c32"),
        [2048] = new Color("#ffb400")
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
}