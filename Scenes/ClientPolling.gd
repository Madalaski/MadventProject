extends Control

const UDP_PORT = 4242
var udp := PacketPeerUDP.new()
var udp_connected = false

static var host_ip_address : String

var spinner : ColorRect

func _ready():
	set_process(true)
# warning-ignore:return_value_discarded
	udp.set_dest_address("255.255.255.255", UDP_PORT)
	udp.set_broadcast_enabled(true)
	
	spinner = get_node("SpinningLoading") as ColorRect

func _process(_delta):
	spinner.rotation_degrees += _delta * 360.0 * 3.0
	
	if !udp_connected:
		# Try to contact server
# warning-ignore:return_value_discarded
		udp.put_packet("Contacted Host".to_utf8_buffer())

	if udp.get_available_packet_count() > 0:
		print(udp.get_packet().get_string_from_utf8())
		host_ip_address = udp.get_packet_ip()

		# Stop Broadcasting
		udp_connected = true
		set_process(false)
		
		get_tree().change_scene_to_file("res://main.tscn")
