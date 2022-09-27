using Godot;

public partial class ScoreKeeper : GridContainer {

    private RichTextLabel _scoreLabel;
    private int _score = 0;
    
    public override void _Ready() {
        base._Ready();
        _scoreLabel = (RichTextLabel) FindChild("ScoreValueLabel");
    }

    public void UpdateScore(int scoreToAdd) {
        _score += scoreToAdd;
        _scoreLabel.Text = _score.ToString();
    }

    public void ResetScore() {
        _score = 0;
        _scoreLabel.Text = _score.ToString();
    }
}