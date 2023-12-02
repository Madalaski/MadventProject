extends Control

const UDP_PORT = 4242
var server := UDPServer.new()

var spinner : ColorRect

func _ready():
	set_process(true)
	server.listen(UDP_PORT)
	spinner = get_node("SpinningLoading") as ColorRect


func _process(_delta):
	spinner.rotation_degrees += _delta * 360.0 * 3.0
	
# warning-ignore:return_value_discarded
	server.poll()
	if server.is_connection_available():
		var peer : PacketPeerUDP = server.take_connection()
		var pkt = peer.get_packet()
		print("Accepted peer: %s:%s" % [peer.get_packet_ip(),                    peer.get_packet_port()])
		print("Received data: %s" % [pkt.get_string_from_utf8()])
		# Reply so it knows we received the message.
# warning-ignore:return_value_discarded
		peer.put_packet(pkt)
		server.stop()
		var handler = ResourceLoader.load("res://MainMenuHandler.gd")
		if handler.IsVREnabled:
			get_tree().change_scene_to_file("res://BigPlayerVR.tscn")
		else:
			get_tree().change_scene_to_file("res://BigPlayer.tscn")
