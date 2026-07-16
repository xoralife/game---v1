extends CharacterBody3D

@export var walk_speed := 6.0
@export var sprint_speed := 9.0
@export var jump_velocity := 4.5
@export var mouse_sensitivity := 0.002
@export var max_health := 100.0

var health := 100.0
var gravity := 20.0
var invuln_timer := 0.0
var regen_timer := 0.0
var bob_timer := 0.0
var current_speed := 6.0

@onready var camera: Camera3D = $Camera3D
@onready var gun: Node3D = $Camera3D/Gun

signal health_changed(percent: float)
signal player_damaged()
signal player_died()

func _ready():
	health = max_health

func _input(event):
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		rotate_y(-event.relative.x * mouse_sensitivity)
		camera.rotate_x(-event.relative.y * mouse_sensitivity)
		camera.rotation.x = clamp(camera.rotation.x, deg_to_rad(-80), deg_to_rad(80))

func _physics_process(delta):
	if GameManager.is_game_over:
		return

	if invuln_timer > 0:
		invuln_timer -= delta

	# Regen
	if health < max_health and health > 0:
		regen_timer -= delta
		if regen_timer <= 0:
			health = min(health + 8 * delta, max_health)
			health_changed.emit(health / max_health)

	# Movement
	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var dir = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	var sprint = Input.is_action_pressed("sprint")
	current_speed = sprint_speed if sprint and dir.length() > 0 else walk_speed
	velocity.x = dir.x * current_speed
	velocity.z = dir.z * current_speed

	# Jump
	if is_on_floor() and Input.is_action_just_pressed("jump"):
		velocity.y = jump_velocity
	if not is_on_floor():
		velocity.y -= gravity * delta

	move_and_slide()

	# Head bob
	var moving = input_dir.length() > 0.1 and is_on_floor()
	if moving:
		bob_timer += delta * 10
		var bx = sin(bob_timer) * 0.02
		var by = sin(bob_timer * 2) * 0.01
		camera.position.x = bx
		camera.position.y = 0.7 + by
	else:
		bob_timer = 0
		camera.position.x = lerp(camera.position.x, 0.0, delta * 6)
		camera.position.y = lerp(camera.position.y, 0.7, delta * 6)

func take_damage(amount: float):
	if invuln_timer > 0 or health <= 0:
		return
	health -= amount
	invuln_timer = 0.5
	regen_timer = 3.0
	health_changed.emit(health / max_health)
	player_damaged.emit()
	if health <= 0:
		die()

func die():
	player_died.emit()
	GameManager.game_over_trigger()

func heal(amount: float):
	health = min(health + amount, max_health)
	health_changed.emit(health / max_health)

func get_gun() -> Node3D:
	return gun
