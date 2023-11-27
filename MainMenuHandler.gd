extends Node

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

static var IsVREnabled = false

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_server_button_pressed():
	IsVREnabled = false
	get_tree().change_scene_to_file("res://Scenes/ServerPolling.tscn")
	pass # Replace with function body.


func _on_client_button_pressed():
	get_tree().change_scene_to_file("res://Scenes/ClientPolling.tscn")
	pass # Replace with function body.


func _on_server_button_vr_pressed():
	IsVREnabled = true
	get_tree().change_scene_to_file("res://Scenes/ServerPolling.tscn")
	pass # Replace with function body.
