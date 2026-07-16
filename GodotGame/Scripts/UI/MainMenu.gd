extends Control

func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE

	# Background
	var bg = ColorRect.new()
	bg.color = Color(0.1, 0.1, 0.12)
	bg.position = Vector2(0, 0)
	bg.size = get_viewport().size
	add_child(bg)

	# Title
	var title = Label.new()
	title.text = "WAVE SURVIVAL"
	title.position = Vector2(get_viewport().size.x / 2 - 250, 150)
	title.size = Vector2(500, 80)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_color_override("font_color", Color.RED)
	title.add_theme_font_size_override("font_size", 56)
	add_child(title)

	# High score
	var hs = Label.new()
	hs.name = "HighScore"
	hs.text = "High Score: " + str(GameManager.get_high_score())
	hs.position = Vector2(get_viewport().size.x / 2 - 200, 230)
	hs.size = Vector2(400, 30)
	hs.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hs.add_theme_color_override("font_color", Color.YELLOW)
	hs.add_theme_font_size_override("font_size", 22)
	add_child(hs)

	# Controls
	var ctrl = Label.new()
	ctrl.text = "WASD - Move | Shift - Sprint | Space - Jump\nLeft Click - Shoot | R - Reload | Mouse - Look\n\nSurvive waves! Boss every 5 waves!"
	ctrl.position = Vector2(get_viewport().size.x / 2 - 250, 300)
	ctrl.size = Vector2(500, 120)
	ctrl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	ctrl.add_theme_color_override("font_color", Color.WHITE)
	ctrl.add_theme_font_size_override("font_size", 16)
	add_child(ctrl)

	# Play button
	make_button("PLAY", Vector2(0, -180), Color(0.15, 0.5, 0.15), func():
		get_tree().change_scene_to_file("res://Scenes/Game.tscn")
	)

	# Quit button
	make_button("QUIT", Vector2(0, -250), Color(0.5, 0.15, 0.15), func():
		get_tree().quit()
	)

func make_button(text: String, offset: Vector2, color: Color, callback: Callable):
	var btn = Button.new()
	btn.text = text
	btn.position = Vector2(get_viewport().size.x / 2 - 110, get_viewport().size.y / 2 + offset.y)
	btn.size = Vector2(220, 55)
	btn.pressed.connect(callback)
	add_child(btn)
