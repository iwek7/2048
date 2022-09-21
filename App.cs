using Godot;
using System;

public partial class App : Control
{
	private Button _startGameButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_startGameButton = FindChild("StartGameButton") as Button;
		_startGameButton.Pressed += OnStartButtonClicked;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		base._UnhandledInput(inputEvent);
		if (inputEvent is not InputEventKey { Pressed: true } keyboardEvent) return;
		
		switch (keyboardEvent.Keycode)
		{
			case Key.Left:
				GD.Print("Move left");
				break;
			case Key.Up:
				GD.Print("Move up");
				break;
			case Key.Right:
				GD.Print("Move right");
				break;
			case Key.Down:
				GD.Print("Move down");
				break;
		}
	}
	
	private void OnStartButtonClicked()
	{
		GD.Print("Starting game!");
	}

}
