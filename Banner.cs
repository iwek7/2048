using Godot;
using System;

public delegate void RestartButtonClickedDelegate();

public delegate void QuitButtonClickedDelegate();

public partial class Banner : Control {
    private const string WinningMessage = "You won!";
    private const string LosingMessage = "Game over...";

    public event RestartButtonClickedDelegate RestartButtonClicked;
    public event QuitButtonClickedDelegate QuitButtonClicked;

    private Label _resultLabel;
    private Label _scoreLabel;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _resultLabel = GetNode<Label>("Panel/CenterContainer/GridContainer/ResultLabel");
        _scoreLabel = GetNode<Label>("Panel/CenterContainer/GridContainer/MarginContainer/ScoreLabel");
        
        GetNode<Button>("Panel/CenterContainer/GridContainer/GridContainer/RestartButton").Pressed += () => {
            RestartButtonClicked?.Invoke();
        };
        GetNode<Button>("Panel/CenterContainer/GridContainer/GridContainer/QuitGameButton").Pressed += () => {
            QuitButtonClicked?.Invoke();
        };
    }

    public void Show(bool won, int score) {
        Visible = true;
        _resultLabel.Text = won ? WinningMessage : LosingMessage;
        _scoreLabel.Text = $"Your score was: {score}";
    }
}