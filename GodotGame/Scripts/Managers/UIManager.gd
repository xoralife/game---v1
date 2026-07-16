extends CanvasLayer

var hp_bar: ColorRect
var hp_fill: ColorRect
var hp_label: Label
var ammo_label: Label
var wave_label: Label
var enemies_label: Label
var score_label: Label
var combo_label: Label
var wave_bar_bg: ColorRect
var wave_bar_fill: ColorRect
var announce_label: Label
var game_over_panel: ColorRect
var final_score: Label
var high_score: Label
var restart_btn: Button

var combo := 0
var combo_timer := 0.0
var player: CharacterBody3D
var gun: Node3D
var wm: Node

func _ready():
	build_hud()
	player = get_node("/root/Game/Player") if get_node("/root/Game/Player") else null
	if player:
		player.health_changed.connect(_on_health_changed)
		player.player_damaged.connect(_on_damaged)
	gun = player.get_gun() if player else null
	wm = get_node("/root/Game/WaveManager") if get_node("/root/Game/WaveManager") else null
	if wm:
		wm.wave_started.connect(_on_wave_started)
		wm.wave_cleared.connect(_on_wave_cleared)

func build_hud():
	var font_size = 18

	# HP Bar
	hp_bar = ColorRect.new()
	hp_bar.name = "HPBarBG"
	hp_bar.color = Color(0.2, 0.2, 0.2, 0.7)
	hp_bar.position = Vector2(20, 20)
	hp_bar.size = Vector2(250, 25)
	add_child(hp_bar)

	hp_fill = ColorRect.new()
	hp_fill.name = "HPFill"
	hp_fill.color = Color.GREEN
	hp_fill.position = Vector2(20, 20)
	hp_fill.size = Vector2(250, 25)
	add_child(hp_fill)

	hp_label = Label.new()
	hp_label.position = Vector2(275, 20)
	hp_label.size = Vector2(50, 25)
	hp_label.text = "100"
	hp_label.add_theme_color_override("font_color", Color.WHITE)
	add_child(hp_label)

	# Ammo
	ammo_label = Label.new()
	ammo_label.position = Vector2(get_viewport().size.x / 2 - 60, get_viewport().size.y - 50)
	ammo_label.size = Vector2(120, 30)
	ammo_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	ammo_label.add_theme_color_override("font_color", Color.WHITE)
	add_child(ammo_label)

	# Wave
	wave_label = Label.new()
	wave_label.position = Vector2(get_viewport().size.x / 2 - 100, 10)
	wave_label.size = Vector2(200, 25)
	wave_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	wave_label.add_theme_color_override("font_color", Color.YELLOW)
	add_child(wave_label)

	# Enemies
	enemies_label = Label.new()
	enemies_label.position = Vector2(get_viewport().size.x / 2 - 80, 35)
	enemies_label.size = Vector2(160, 20)
	enemies_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	enemies_label.add_theme_color_override("font_color", Color.WHITE)
	add_child(enemies_label)

	# Wave progress bar
	wave_bar_bg = ColorRect.new()
	wave_bar_bg.color = Color(0.2, 0.2, 0.2, 0.6)
	wave_bar_bg.position = Vector2(get_viewport().size.x / 2 - 100, 55)
	wave_bar_bg.size = Vector2(200, 6)
	add_child(wave_bar_bg)

	wave_bar_fill = ColorRect.new()
	wave_bar_fill.color = Color.YELLOW
	wave_bar_fill.position = Vector2(get_viewport().size.x / 2 - 100, 55)
	wave_bar_fill.size = Vector2(0, 6)
	add_child(wave_bar_fill)

	# Score
	score_label = Label.new()
	score_label.position = Vector2(get_viewport().size.x - 200, 15)
	score_label.size = Vector2(180, 25)
	score_label.add_theme_color_override("font_color", Color.WHITE)
	add_child(score_label)

	# Combo
	combo_label = Label.new()
	combo_label.position = Vector2(get_viewport().size.x / 2 + 120, get_viewport().size.y / 2 - 20)
	combo_label.size = Vector2(150, 40)
	combo_label.add_theme_color_override("font_color", Color.CYAN)
	combo_label.visible = false
	add_child(combo_label)

	# Announce
	announce_label = Label.new()
	announce_label.position = Vector2(get_viewport().size.x / 2 - 200, get_viewport().size.y / 2 - 40)
	announce_label.size = Vector2(400, 80)
	announce_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	announce_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	announce_label.add_theme_color_override("font_color", Color.YELLOW)
	announce_label.visible = false
	add_child(announce_label)

	# Game Over Panel
	game_over_panel = ColorRect.new()
	game_over_panel.name = "GameOverPanel"
	game_over_panel.color = Color(0, 0, 0, 0.85)
	game_over_panel.position = Vector2(0, 0)
	game_over_panel.size = get_viewport().size
	game_over_panel.visible = false
	add_child(game_over_panel)

	var go_label = Label.new()
	go_label.position = Vector2(get_viewport().size.x / 2 - 200, get_viewport().size.y / 2 - 80)
	go_label.size = Vector2(400, 60)
	go_label.text = "GAME OVER"
	go_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	go_label.add_theme_color_override("font_color", Color.RED)
	go_label.add_theme_font_size_override("font_size", 48)
	game_over_panel.add_child(go_label)

	final_score = Label.new()
	final_score.position = Vector2(get_viewport().size.x / 2 - 200, get_viewport().size.y / 2 - 20)
	final_score.size = Vector2(400, 30)
	final_score.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	final_score.add_theme_color_override("font_color", Color.WHITE)
	final_score.add_theme_font_size_override("font_size", 28)
	game_over_panel.add_child(final_score)

	high_score = Label.new()
	high_score.position = Vector2(get_viewport().size.x / 2 - 200, get_viewport().size.y / 2 + 20)
	high_score.size = Vector2(400, 30)
	high_score.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	high_score.add_theme_color_override("font_color", Color.YELLOW)
	high_score.add_theme_font_size_override("font_size", 22)
	game_over_panel.add_child(high_score)

	restart_btn = Button.new()
	restart_btn.position = Vector2(get_viewport().size.x / 2 - 110, get_viewport().size.y / 2 + 70)
	restart_btn.size = Vector2(220, 50)
	restart_btn.text = "RESTART"
	restart_btn.pressed.connect(_on_restart)
	game_over_panel.add_child(restart_btn)

	# Connect GameManager
	GameManager.game_over.connect(_show_game_over)
	GameManager.score_changed.connect(_update_score)

func _process(delta):
	if combo > 0:
		combo_timer -= delta
		if combo_timer <= 0:
			combo = 0
			combo_label.visible = false

	# Update HUD
	if gun and gun.has_method("get_ammo_text"):
		ammo_label.text = gun.get_ammo_text()
		if gun.is_reloading:
			ammo_label.text += " [RELOADING]"

	if wm and wm.has_method("get_enemies_remaining"):
		enemies_label.text = "Enemies: " + str(wm.get_enemies_remaining())
		wave_bar_fill.size.x = wm.get_wave_progress() * 200

	if GameManager:
		score_label.text = "Score: " + str(GameManager.score)

func _on_health_changed(percent: float):
	hp_fill.size.x = percent * 250
	hp_fill.color = Color.GREEN if percent > 0.5 else (Color.YELLOW if percent > 0.25 else Color.RED)
	hp_label.text = str(int(percent * 100))

func _on_damaged():
	# Visual feedback handled by player script
	pass

func _on_wave_started(wave: int):
	wave_label.text = "WAVE " + str(wave)
	announce_label.visible = true
	announce_label.text = "WAVE " + str(wave)
	announce_label.add_theme_color_override("font_color", Color.RED if wave % 5 == 0 else Color.YELLOW)
	if wave % 5 == 0:
		announce_label.text = "!!! BOSS WAVE " + str(wave) + " !!!"
	get_tree().create_timer(2.0).timeout.connect(func(): announce_label.visible = false)

func _on_wave_cleared():
	announce_label.visible = true
	announce_label.text = "WAVE CLEARED!"
	announce_label.add_theme_color_override("font_color", Color.GREEN)
	get_tree().create_timer(2.0).timeout.connect(func(): announce_label.visible = false)

func _show_game_over():
	game_over_panel.visible = true
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	final_score.text = "Score: " + str(GameManager.score)
	high_score.text = "High Score: " + str(GameManager.high_score)

func _on_restart():
	game_over_panel.visible = false
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	GameManager.restart()
	get_tree().reload_current_scene()

func _update_score(new_score: int):
	score_label.text = "Score: " + str(new_score)
