extends Node

var score := 0
var high_score := 0
var is_game_over := false

signal game_over()
signal score_changed(new_score: int)

func _ready():
	high_score = get_high_score()

func add_score(amount: int):
	score += amount
	score_changed.emit(score)

func game_over_trigger():
	is_game_over = true
	if score > high_score:
		high_score = score
		save_high_score(high_score)
	game_over.emit()

func restart():
	score = 0
	is_game_over = false

func save_high_score(val: int):
	var f = FileAccess.open("user://highscore.save", FileAccess.WRITE)
	if f:
		f.store_32(val)
		f.close()

func get_high_score() -> int:
	var f = FileAccess.open("user://highscore.save", FileAccess.READ)
	if f:
		var val = f.get_32()
		f.close()
		return val
	return 0
