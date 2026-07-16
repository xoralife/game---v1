extends Node

@export var base_enemy_count := 3
@export var enemies_per_wave := 2
@export var spawn_delay := 1.0
@export var wave_delay := 3.0
@export var boss_interval := 5

var current_wave := 0
var enemies_alive := 0
var enemies_to_spawn := 0
var total_enemies := 0
var spawn_timer := 0.0
var wave_active := false

signal wave_started(wave: int)
signal wave_cleared()

func _process(delta):
	if not wave_active or enemies_to_spawn <= 0:
		return
	spawn_timer -= delta
	if spawn_timer <= 0:
		spawn_enemy()
		spawn_timer = max(spawn_delay - current_wave * 0.03, 0.3)

func start_next_wave():
	current_wave += 1
	total_enemies = base_enemy_count + (current_wave - 1) * enemies_per_wave
	enemies_to_spawn = total_enemies
	enemies_alive = total_enemies
	wave_active = true
	spawn_timer = 0
	wave_started.emit(current_wave)

func spawn_enemy():
	var spawner = get_node("../Spawner")
	if not spawner or not spawner.has_method("get_spawn_position"):
		enemies_to_spawn = 0
		return

	var pos = spawner.get_spawn_position()
	var is_boss = current_wave % boss_interval == 0 and enemies_alive == total_enemies

	# Create enemy scene
	var enemy = CharacterBody3D.new()
	enemy.name = "Enemy"

	var mesh = MeshInstance3D.new()
	mesh.name = "Mesh"
	mesh.mesh = CapsuleMesh.new()
	mesh.mesh.height = 1.2
	mesh.mesh.radius = 0.4
	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color.RED
	mesh.material_override = mat
	mesh.position.y = 0.6
	enemy.add_child(mesh)

	var col = CollisionShape3D.new()
	col.name = "Collision"
	col.shape = CapsuleShape3D.new()
	col.shape.height = 1.2
	col.shape.radius = 0.4
	col.position.y = 0.6
	enemy.add_child(col)

	enemy.position = pos
	enemy.set_script(preload("res://Scripts/Enemies/Enemy.gd"))

	# Configure
	var hp_mult = pow(1.1, current_wave - 1)
	var spd_mult = pow(1.05, current_wave - 1)

	if is_boss:
		enemy.enemy_type = 2  # BOSS
		enemy.max_health = 200.0 * hp_mult
		enemy.health = enemy.max_health
		enemy.damage = 40.0
		enemy.score_value = 50
	elif current_wave >= 3 and randf() < 0.3 + current_wave * 0.02:
		enemy.enemy_type = 1  # FAST
	else:
		enemy.enemy_type = 0  # NORMAL

	enemy.max_health *= hp_mult
	enemy.health = enemy.max_health
	enemy.move_speed *= spd_mult

	get_node("/root/Game").add_child(enemy)
	enemies_to_spawn -= 1

func on_enemy_killed():
	enemies_alive -= 1
	if enemies_alive <= 0 and enemies_to_spawn <= 0:
		wave_active = false
		wave_cleared.emit()
		get_tree().create_timer(wave_delay).timeout.connect(start_next_wave)

func get_enemies_remaining() -> int:
	return enemies_alive

func get_wave_progress() -> float:
	if total_enemies == 0:
		return 0
	return 1.0 - float(enemies_alive) / total_enemies
