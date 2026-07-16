extends Node

@export var spawn_chance := 0.4
@export var health_restore := 30.0
@export var ammo_restore := 15

func spawn_pickup(pos: Vector3):
	if randf() > spawn_chance:
		return

	var is_health = randf() < 0.5
	var pickup = Area3D.new()
	pickup.name = "Pickup"

	var mesh = MeshInstance3D.new()
	mesh.mesh = SphereMesh.new()
	mesh.mesh.radius = 0.15
	mesh.mesh.height = 0.3
	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color.GREEN if is_health else Color.CYAN
	mat.emission_enabled = true
	mat.emission = Color.GREEN if is_health else Color.CYAN
	mesh.material_override = mat
	pickup.add_child(mesh)

	var col = CollisionShape3D.new()
	col.shape = SphereShape3D.new()
	col.shape.radius = 0.3
	pickup.add_child(col)

	pickup.position = pos + Vector3(0, 0.5, 0)
	get_parent().add_child(pickup)

	# Float animation
	var tween = create_tween()
	tween.set_loops()
	tween.tween_property(pickup, "position:y", pos.y + 0.8, 1.0)
	tween.tween_property(pickup, "position:y", pos.y + 0.5, 1.0)

	# Rotation
	var rtween = create_tween()
	rtween.set_loops()
	rtween.tween_property(pickup, "rotation:y", PI * 2, 2.0).as_relative()

	# Pickup script
	pickup.body_entered.connect(_on_pickup_collected.bind(pickup, is_health))

	# Auto remove
	get_tree().create_timer(8.0).timeout.connect(func():
		if is_instance_valid(pickup):
			pickup.queue_free()
	)

func _on_pickup_collected(body: Node, pickup: Area3D, is_health: bool):
	if not body.has_method("take_damage"):  # Not player
		return

	var player = body as CharacterBody3D
	if is_health and player.has_method("heal"):
		player.heal(health_restore)
	elif not is_health:
		var gun = player.get_gun()
		if gun and "reserve" in gun:
			gun.reserve += ammo_restore

	if is_instance_valid(pickup):
		pickup.queue_free()
