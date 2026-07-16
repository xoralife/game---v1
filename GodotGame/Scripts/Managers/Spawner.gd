extends Node

var spawn_points: Array[Vector3] = []

func _ready():
	var arena_size := 22.0
	for i in range(8):
		var angle = (360.0 / 8) * i
		var radius = arena_size * 0.45
		var x = sin(deg_to_rad(angle)) * radius
		var z = cos(deg_to_rad(angle)) * radius
		spawn_points.append(Vector3(x, 0, z))

	for i in range(4):
		var x = randf_range(-arena_size * 0.3, arena_size * 0.3)
		var z = randf_range(-arena_size * 0.3, arena_size * 0.3)
		spawn_points.append(Vector3(x, 0, z))

func get_spawn_position() -> Vector3:
	if spawn_points.is_empty():
		return Vector3(randf_range(-8, 8), 0, randf_range(-8, 8))
	var pos = spawn_points[randi() % spawn_points.size()]
	pos += Vector3(randf_range(-1, 1), 0, randf_range(-1, 1))
	return pos
