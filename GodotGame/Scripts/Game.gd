extends Node3D

var player_scene: PackedScene
var player_instance: Node3D

func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	build_arena()
	spawn_player()
	spawn_managers()
	spawn_ui()

func build_arena():
	var ground = MeshInstance3D.new()
	ground.name = "Ground"
	var plane = PlaneMesh.new()
	plane.size = Vector2(50, 50)
	ground.mesh = plane
	ground.position = Vector3(0, -0.5, 0)
	ground.rotation.x = -PI / 2
	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color(0.25, 0.25, 0.3)
	ground.material_override = mat
	add_child(ground)

	for i in range(-4, 5):
		var l = MeshInstance3D.new()
		l.mesh = BoxMesh.new()
		l.mesh.size = Vector3(0.05, 0.02, 22)
		l.position = Vector3(i * 2.5, 0.01, 0)
		add_child(l)
		var l2 = MeshInstance3D.new()
		l2.mesh = BoxMesh.new()
		l2.mesh.size = Vector3(22, 0.02, 0.05)
		l2.position = Vector3(0, 0.01, i * 2.5)
		add_child(l2)

	var wc = Color(0.35, 0.35, 0.4)
	make_wall(Vector3(0, 2, -11), Vector3(24, 4, 0.5), wc)
	make_wall(Vector3(0, 2, 11), Vector3(24, 4, 0.5), wc)
	make_wall(Vector3(-11, 2, 0), Vector3(0.5, 4, 24), wc)
	make_wall(Vector3(11, 2, 0), Vector3(0.5, 4, 24), wc)

	var cc = Color(0.5, 0.35, 0.2)
	make_wall(Vector3(-4, 0.75, -3), Vector3(2, 1.5, 0.3), cc)
	make_wall(Vector3(4, 0.75, -3), Vector3(2, 1.5, 0.3), cc)
	make_wall(Vector3(-3, 1, 5), Vector3(1.5, 2, 0.3), Color(0.4, 0.3, 0.2))
	make_wall(Vector3(3, 0.5, 5), Vector3(3, 1, 0.3), cc)
	make_wall(Vector3(0, 0.5, -6), Vector3(4, 1, 0.3), Color(0.4, 0.3, 0.2))

	var light = DirectionalLight3D.new()
	light.rotation = Vector3(deg_to_rad(50), deg_to_rad(-30), 0)
	light.light_energy = 1.2
	light.shadow_enabled = true
	add_child(light)

	var env = WorldEnvironment.new()
	env.environment = Environment.new()
	env.environment.ambient_light_color = Color(0.35, 0.35, 0.4)
	add_child(env)

func make_wall(pos: Vector3, size: Vector3, color: Color):
	var m = MeshInstance3D.new()
	m.mesh = BoxMesh.new()
	m.mesh.size = size
	m.position = pos
	var mat = StandardMaterial3D.new()
	mat.albedo_color = color
	m.material_override = mat
	add_child(m)
	var col = StaticBody3D.new()
	col.position = pos
	var s = CollisionShape3D.new()
	s.shape = BoxShape3D.new()
	s.shape.size = size
	col.add_child(s)
	add_child(col)

func spawn_player():
	var p = CharacterBody3D.new()
	p.name = "Player"
	var col = CollisionShape3D.new()
	col.shape = CapsuleShape3D.new()
	col.shape.height = 1.8
	col.shape.radius = 0.4
	col.position.y = 0.9
	p.add_child(col)

	var cam = Camera3D.new()
	cam.name = "Camera3D"
	cam.position.y = 0.7
	cam.fov = 60
	p.add_child(cam)

	var gun = Node3D.new()
	gun.name = "Gun"

	var gb = MeshInstance3D.new()
	gb.mesh = BoxMesh.new()
	gb.mesh.size = Vector3(0.04, 0.04, 0.25)
	var gm = StandardMaterial3D.new()
	gm.albedo_color = Color(0.2, 0.2, 0.22)
	gb.material_override = gm
	gun.add_child(gb)

	var bar = MeshInstance3D.new()
	bar.mesh = CylinderMesh.new()
	bar.mesh.top_radius = 0.015
	bar.mesh.bottom_radius = 0.015
	bar.mesh.height = 0.12
	bar.position.z = 0.22
	var bm = StandardMaterial3D.new()
	bm.albedo_color = Color(0.15, 0.15, 0.17)
	bar.material_override = bm
	gun.add_child(bar)

	var h = MeshInstance3D.new()
	h.mesh = BoxMesh.new()
	h.mesh.size = Vector3(0.03, 0.04, 0.04)
	h.position = Vector3(0, -0.03, -0.05)
	var hm = StandardMaterial3D.new()
	hm.albedo_color = Color(0.3, 0.2, 0.1)
	h.material_override = hm
	gun.add_child(h)

	cam.add_child(gun)
	add_child(p)
	player_instance = p

	# Attach scripts
	p.set_script(preload("res://Scripts/Player/Player.gd"))
	gun.set_script(preload("res://Scripts/Player/Gun.gd"))

func spawn_managers():
	var wm = Node.new()
	wm.name = "WaveManager"
	wm.set_script(preload("res://Scripts/Managers/WaveManager.gd"))
	add_child(wm)

	var sp = Node.new()
	sp.name = "Spawner"
	sp.set_script(preload("res://Scripts/Managers/Spawner.gd"))
	add_child(sp)

	var pk = Node.new()
	pk.name = "PickupManager"
	pk.set_script(preload("res://Scripts/Managers/PickupManager.gd"))
	add_child(pk)

func spawn_ui():
	var ui = CanvasLayer.new()
	ui.name = "UI"
	ui.set_script(preload("res://Scripts/Managers/UIManager.gd"))
	add_child(ui)

func _input(event):
	if event.is_action_pressed("ui_cancel"):
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED else Input.MOUSE_MODE_CAPTURED
