extends Node

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_server_button_pressed():
	get_tree().change_scene_to_file("res://BigPlayer.tscn")
	pass # Replace with function body.


func _on_client_button_pressed():
	get_tree().change_scene_to_file("res://main.tscn")
	pass # Replace with function body.
