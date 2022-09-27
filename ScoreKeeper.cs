using Godot;

public partial class ScoreKeeper : GridContainer {

    private RichTextLabel _scoreLabel;
    public int Score { get; private set; }
    
    public override void _Ready() {
        base._Ready();
        _scoreLabel = (RichTextLabel) FindChild("ScoreValueLabel");
    }

    public void UpdateScore(int scoreToAdd) {
        Score += scoreToAdd;
        _scoreLabel.Text = Score.ToString();
    }

    public void ResetScore() {
        Score = 0;
        _scoreLabel.Text = Score.ToString();
    }
}