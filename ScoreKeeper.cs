using Godot;

public partial class ScoreKeeper : GridContainer {

    private RichTextLabel _scoreLabel;
    private int score = 0;
    
    public override void _Ready() {
        base._Ready();
        _scoreLabel = (RichTextLabel) FindChild("ScoreValueLabel");
    }

    public void UpdateScore(int scoreToAdd) {
        score += scoreToAdd;
        _scoreLabel.Text = score.ToString();
    }
}