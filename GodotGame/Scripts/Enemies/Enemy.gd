extends CharacterBody3D

enum Type { NORMAL, FAST, BOSS }

@export var enemy_type := Type.NORMAL
@export var max_health := 50.0
@export var move_speed := 4.0
@export var damage := 20.0
@export var score_value := 10

var health := 50.0
var attack_cooldown := 1.0
var attack_timer := 0.0
var hit_flash_timer := 0.0
var player_ref: CharacterBody3D = null
var is_dead := false

var original_color := Color.RED

func _ready():
	health = max_health
	match enemy_type:
		Type.NORMAL:
			original_color = Color.RED
		Type.FAST:
			original_color = Color.YELLOW
			move_speed = 6.0
			health = 30.0
			max_health = 30.0
			score_value = 15
			scale = Vector3(0.7, 0.7, 0.7)
		Type.BOSS:
			original_color = Color(0.5, 0, 0.5)
			move_speed = 3.0
			health = 200.0
			max_health = 200.0
			damage = 40.0
			score_value = 50
			scale = Vector3(1.5, 1.5, 1.5)

	# Set color
	var mesh = get_node("Mesh")
	if mesh:
		var mat = mesh.material_override if mesh.material_override else StandardMaterial3D.new()
		mat.albedo_color = original_color
		mesh.material_override = mat

	# Collision
	var col = get_node("Collision")
	if col and col.shape:
		var s = col.shape
		s.height = 1.0 * (scale.y if enemy_type == Type.BOSS else 1.0)
		s.radius = 0.4 * (scale.x if enemy_type == Type.BOSS else 1.0)

func _physics_process(delta):
	if is_dead or GameManager.is_game_over:
		return

	if attack_timer > 0:
		attack_timer -= delta
	if hit_flash_timer > 0:
		hit_flash_timer -= delta
	else:
		reset_color()

	# Find player
	if not player_ref:
		player_ref = get_node("/root/Game/Player") if has_node("/root/Game/Player") else null
		return

	var dist = global_position.distance_to(player_ref.global_position)
	var dir = (player_ref.global_position - global_position).normalized()

	if dist > 2.0:
		velocity.x = dir.x * move_speed
		velocity.z = dir.z * move_speed
		look_at(Vector3(player_ref.global_position.x, global_position.y, player_ref.global_position.z))
	else:
		velocity.x = 0
		velocity.z = 0

	move_and_slide()

	if dist <= 2.0 and attack_timer <= 0:
		attack()

func attack():
	attack_timer = attack_cooldown
	if player_ref and player_ref.has_method("take_damage"):
		player_ref.take_damage(damage)

func take_damage(amount: float):
	if is_dead:
		return
	health -= amount
	hit_flash()
	if health <= 0:
		die()

func hit_flash():
	hit_flash_timer = 0.08
	var mesh = get_node("Mesh")
	if mesh:
		var mat = mesh.material_override
		if mat:
			mat.albedo_color = Color.WHITE
			mesh.material_override = mat

func reset_color():
	hit_flash_timer = 0
	var mesh = get_node("Mesh")
	if mesh:
		var mat = mesh.material_override
		if mat:
			mat.albedo_color = original_color
			mesh.material_override = mat

func die():
	is_dead = true
	is_safe_to_delete = true

	# Death particles
	for i in range(5):
		var frag = MeshInstance3D.new()
		frag.mesh = SphereMesh.new()
		frag.mesh.radius = 0.1
		var fm = StandardMaterial3D.new()
		fm.albedo_color = original_color
		frag.material_override = fm
		frag.position = global_position + Vector3(randf_range(-0.3, 0.3), randf_range(0, 0.5), randf_range(-0.3, 0.3))
		get_parent().add_child(frag)
		var rb = RigidBody3D.new()
		rb.position = frag.position
		frag.get_parent().remove_child(frag)
		rb.add_child(frag)
		get_parent().add_child(rb)
		rb.apply_impulse(Vector3(randf_range(-3, 3), randf_range(2, 5), randf_range(-3, 3)))
		var t = create_tween()
		t.tween_interval(1.5)
		t.tween_callback(func(): rb.queue_free())

	# Score
	GameManager.add_score(score_value)

	# Drop pickup
	var pm = get_node("/root/Game/PickupManager") if has_node("/root/Game/PickupManager") else null
	if pm and pm.has_method("spawn_pickup"):
		pm.spawn_pickup(global_position)

	# Notify wave manager
	var wm = get_node("/root/Game/WaveManager") if has_node("/root/Game/WaveManager") else null
	if wm and wm.has_method("on_enemy_killed"):
		wm.on_enemy_killed()

	queue_free()

var is_safe_to_delete := false
