# Android GPS Server with Target Location Support
#
# This script runs on an Android device with Termux installed
# It captures real GPS data and serves it to HoloLens clients
# along with optional target location information

import socket
import threading
import time
import json
import subprocess
import os

# Global variable to store the target location
target_location = {
    "latitude": None,
    "longitude": None,
    "altitude": 0,
    "isSet": False
}

# Debug flag - set to False to disable verbose output
DEBUG_MODE = False

def get_android_gps():
    """
    Get real GPS data from Android via termux-api
    Returns a dictionary with both current location and target info
    """
    try:
        # Call termux-location command to access Android GPS
        output = subprocess.check_output(['termux-location']).decode('utf-8')
        
        # Parse the JSON output
        location_data = json.loads(output)
        
        # Build GPS data with both current location and target info
        return {
            "latitude": location_data['latitude'],
            "longitude": location_data['longitude'],
            "altitude": location_data.get('altitude', 0),
            "timestamp": int(time.time() * 1000),
            "valid": True,
            # Include target location in the data packet
            "targetLatitude": target_location["latitude"],
            "targetLongitude": target_location["longitude"],
            "targetAltitude": target_location["altitude"],
            "hasTarget": target_location["isSet"]
        }
    except Exception as e:
        print(f"Error getting GPS data: {e}")
        # Return fallback data with target info
        return {
            "latitude": 0.0,
            "longitude": 0.0,
            "altitude": 0.0,
            "timestamp": int(time.time() * 1000),
            "valid": False,
            "targetLatitude": target_location["latitude"],
            "targetLongitude": target_location["longitude"],
            "targetAltitude": target_location["altitude"],
            "hasTarget": target_location["isSet"]
        }

def handle_client(client_socket):
    """
    Handle a client connection by sending periodic GPS data
    Runs in a separate thread for each connected client
    """
    try:
        print("[*] Client connected, sending GPS data...")
        while True:
            # Get GPS data from Android
            gps_data = get_android_gps()
            
            # Convert to JSON string
            json_data = json.dumps(gps_data)
            
            # Only print if debug mode is enabled
            if DEBUG_MODE:
                print(f"Sending: {json_data}")
            
            # Send data to client
            message = json_data.encode('utf-8')
            client_socket.send(message)
            
            # Sleep for 5 seconds between updates (can be adjusted)
            time.sleep(5)
    except Exception as e:
        print(f"Connection closed: {e}")
    finally:
        client_socket.close()

def set_target_location(lat, lon, alt=0):
    """
    Set the target location that will be sent to clients
    
    Parameters:
        lat (float): Target latitude
        lon (float): Target longitude
        alt (float): Target altitude (optional)
    """
    global target_location
    target_location = {
        "latitude": lat,
        "longitude": lon,
        "altitude": alt,
        "isSet": True
    }
    print(f"[*] Target location set to: Lat={lat}, Lon={lon}, Alt={alt}")

def clear_target_location():
    """
    Clear the target location data
    """
    global target_location
    target_location = {
        "latitude": None,
        "longitude": None,
        "altitude": 0,
        "isSet": False
    }
    print("[*] Target location cleared")

def start_server():
    """
    Start the GPS server on Android
    Sets up socket, handles client connections, and manages user input
    """
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    
    # Bind to all interfaces to allow connections from any device
    server_ip = '0.0.0.0'
    server_port = 8085
    
    server.bind((server_ip, server_port))
    server.listen(5)
    
    # Get this device's IP address to display
    ip_info = subprocess.check_output(['ifconfig', 'wlan0']).decode('utf-8')
    ip_address = "unknown"
    for line in ip_info.split('\n'):
        if "inet " in line:
            ip_address = line.split('inet ')[1].split(' ')[0]
    
    print(f"[*] GPS Server running")
    print(f"[*] IP Address: {ip_address}")
    print(f"[*] Port: {server_port}")
    print(f"[*] Use this in Unity: {ip_address}:{server_port}")
    
    # Start a thread for handling user input
    input_thread = threading.Thread(target=handle_user_input)
    input_thread.daemon = True
    input_thread.start()
    
    print(f"[*] Target location interaction enabled")
    print(f"[*] Press Ctrl+C to stop the server")
    print(f"[*] Waiting for input...")
    
    try:
        while True:
            # Accept incoming connections
            client, addr = server.accept()
            print(f"[*] Accepted connection from {addr[0]}:{addr[1]}")
            
            # Create a thread to handle the client
            client_handler = threading.Thread(target=handle_client, args=(client,))
            client_handler.daemon = True
            client_handler.start()
    except KeyboardInterrupt:
        print("[*] Shutting down server")
        server.close()

def handle_user_input():
    """
    Handle user input for setting and managing target locations
    Runs in a separate thread to not block server operation
    """
    while True:
        try:
            # Simplified prompt for user interaction
            print("\n[?] Enter command (y/clear/help): ", end="")
            response = input().lower()

            # Process the response
            if response in ["y", "yes"]:
                try:
                    # Get target coordinates
                    print("[?] Enter target coordinates (lat lon [alt]): ", end="")
                    coords = input().strip()
                    parts = coords.split()
                    
                    if len(parts) >= 2:
                        lat = float(parts[0])
                        lon = float(parts[1])
                        alt = float(parts[2]) if len(parts) > 2 else 0
                        set_target_location(lat, lon, alt)
                    else:
                        print("[!] Invalid format. Expected: latitude longitude [altitude]")
                except ValueError:
                    print("[!] Invalid coordinates. Please enter numeric values.")
            
            elif response == "clear":
                clear_target_location()
                print("[*] Target location cleared")
            
            elif response == "help":
                print("\nCommands:")
                print("  y, yes          - Set a target location")
                print("  clear           - Remove existing target")
                print("  help            - Show this help")
                print("  Ctrl+C          - Exit the server")
            
            else:
                print("[!] Unknown command. Type 'help' for available commands.")
                
        except Exception as e:
            print(f"[!] Error: {e}")

if __name__ == "__main__":
    # Request location permissions first by invoking termux-location
    os.system('termux-location')
    print("Starting GPS server with target location support...")
    start_server()