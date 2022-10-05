using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Logic;

[Tool]
public partial class BlockNode : ColorRect {
    // todo: make this palette nicer 
    private static readonly Dictionary<BlockValue, Color> ColorMap = new() {
        [Logic.BlockValue.Two] = new Color("#70f6de"),
        [Logic.BlockValue.Four] = new Color("#00bae0"),
        [Logic.BlockValue.Eight] = new Color("#0079d4"),
        [Logic.BlockValue.Sixteen] = new Color("#182b9b"),
        [Logic.BlockValue.ThirtyTwo] = new Color("#56279b"),
        [Logic.BlockValue.SixtyFour] = new Color("#7c2098"),
        [Logic.BlockValue.HundredAndTwentyEight] = new Color("#9b1893"),
        [Logic.BlockValue.TwoHundredFiftySix] = new Color("#da0c79"),
        [Logic.BlockValue.FiveHundredTwelve] = new Color("#ff4357"),
        [Logic.BlockValue.ThousandTwentyFour] = new Color("#ff7c32"),
        [Logic.BlockValue.TwoThousandFortyEight] = new Color("#ffb400")
    };

    private int _blockEnumValue = 0;
    // todo: add all missing values
    [Export(PropertyHint.Enum, "2,4,8,16,32,64,128,256,512,1024,2048")]
    private int BlockValue {
        get => _blockEnumValue;
        set {
            _blockEnumValue = value;
            Value = (BlockValue)Math.Pow(2, value + 1);
        } 
    } // just for export, dont use it for anything else


    private Label _label;

    private BlockValue _value;

    public BlockValue Value {
        get => _value;
        set {
            _value = value;
            Color = ColorMap[value];
            _label.Text = ((int)value).ToString();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _label = (Label)FindChild("Label");
    }
}