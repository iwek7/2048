using Godot;
using Logic;

[Tool]
public partial class BlockNode : ColorRect {
    private static readonly Dictionary<BlockValue, Color> ColorMap = new() {
        [Logic.BlockValue.Two] = new Color("#EEF1FF"),
        [Logic.BlockValue.Four] = new Color("#D2DAFF"),
        [Logic.BlockValue.Eight] = new Color("#AAC4FF"),
        [Logic.BlockValue.Sixteen] = new Color("#B1B2FF"),
        
        [Logic.BlockValue.ThirtyTwo] = new Color("#FFE6BC"),
        [Logic.BlockValue.SixtyFour] = new Color("#C3B091"),
        [Logic.BlockValue.HundredAndTwentyEight] = new Color("#8E806A"),
        [Logic.BlockValue.TwoHundredFiftySix] = new Color("#615748"),
        
        [Logic.BlockValue.FiveHundredTwelve] = new Color("#ffc4c0"),
        [Logic.BlockValue.ThousandTwentyFour] = new Color("#f28282"),
        [Logic.BlockValue.TwoThousandFortyEight] = new Color("#eb4848")
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
            if (Engine.IsEditorHint() && _label == null) {
                // I think when running in tool mode this setter is called before this object is _ready
                // so that _label is not assigned.
                // Therefore this early return avoids ugly error in editor output when opening this scene.
                // Issue fixes itself when _ready is called eventually.
                // This situation should not happen in non-tool mode.
                return;
            }

            // for some weird reason when running game editor keeps on setting exported value of node that is not in scene
            // but is used in editor to edit scene
            // since node in never readied _label is never assigned and thus is null
            // it looks like engine bug
            if (_label != null) {
                _label.Text = ((int)value).ToString();
            }
            
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _label = (Label)FindChild("Label");
    }
}