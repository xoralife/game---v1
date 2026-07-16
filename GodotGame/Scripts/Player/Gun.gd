extends Node3D

@export var damage := 30.0
@export var fire_rate := 0.12
@export var range := 100.0
@export var magazine_size := 30
@export var reload_time := 1.5

var ammo := 30
var reserve := 90
var next_fire := 0.0
var is_reloading := false
var reload_timer := 0.0
var recoil_offset := Vector3.ZERO

@onready var player: CharacterBody3D = get_parent().get_parent()
@onready var camera: Camera3D = get_parent()

signal shot_fired()
signal reload_started()
signal reload_ended()

func _ready():
	ammo = magazine_size

func _process(delta):
	if GameManager.is_game_over:
		return

	if is_reloading:
		reload_timer -= delta
		if reload_timer <= 0:
			finish_reload()
		return

	if Input.is_action_pressed("shoot") and Time.get_ticks_msec() / 1000.0 >= next_fire and ammo > 0:
		shoot()

	if Input.is_action_just_pressed("reload") and ammo < magazine_size and reserve > 0:
		start_reload()

	recoil_offset = recoil_offset.lerp(Vector3.ZERO, delta * 5)
	position = Vector3(0.3, -0.2, 0.4) + recoil_offset

func shoot():
	ammo -= 1
	next_fire = Time.get_ticks_msec() / 1000.0 + fire_rate

	# Recoil
	recoil_offset += Vector3(randf_range(-0.003, 0.003), -0.015, randf_range(-0.008, -0.003))
	recoil_offset = recoil_offset.limit_length(0.06)

	# Muzzle flash
	var flash = MeshInstance3D.new()
	flash.mesh = SphereMesh.new()
	flash.mesh.radius = 0.03
	flash.mesh.height = 0.06
	var fm = StandardMaterial3D.new()
	fm.albedo_color = Color.YELLOW
	fm.emission_enabled = true
	fm.emission = Color.YELLOW
	flash.material_override = fm
	flash.position.z = 0.35
	add_child(flash)
	var tween = create_tween()
	tween.tween_property(flash, "scale", Vector3.ZERO, 0.05)
	tween.tween_callback(func(): flash.queue_free())

	# Raycast
	var space = get_world_3d().direct_space_state
	var from = camera.global_position
	var to = from + -camera.global_transform.basis.z * range
	var query = PhysicsRayQueryParameters3D.create(from, to)
	query.collision_mask = 1
	var result = space.intersect_ray(query)

	if result:
		var enemy = result.collider.get_parent()
		if enemy.has_method("take_damage"):
			enemy.take_damage(damage)
			GameManager.add_score(10)

		# Hit effect
		var fx = MeshInstance3D.new()
		fx.mesh = SphereMesh.new()
		fx.mesh.radius = 0.04
		var fxm = StandardMaterial3D.new()
		fxm.albedo_color = Color.RED if enemy.has_method("take_damage") else Color.GRAY
		fx.material_override = fxm
		fx.position = result.position
		get_parent().add_child(fx)
		var t2 = create_tween()
		t2.tween_property(fx, "scale", Vector3.ZERO, 0.2)
		t2.tween_callback(func(): fx.queue_free())

	shot_fired.emit()

func start_reload():
	is_reloading = true
	reload_timer = reload_time
	reload_started.emit()

func finish_reload():
	var needed = magazine_size - ammo
	var give = min(needed, reserve)
	ammo += give
	reserve -= give
	is_reloading = false
	reload_ended.emit()

func get_ammo_text() -> String:
	return str(ammo) + " / " + str(reserve)
