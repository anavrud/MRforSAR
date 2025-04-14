# PC GPS Simulator with Target Location Support
#
# Inspired by/Based on: pythonclient.py
# Original author: jryebread (GitHub)
# Source: https://gist.github.com/jryebread/2bdf148313f40781f1f36d38ada85d47
# 
# Modified by: Aleksander Navrud
# Date: April 2025
# Modifications:
# - Added target location support
# - Implemented random GPS coordinate generation
# - Added interactive command interface
#
# This script runs on a PC to simulate GPS data
# It generates random coordinates and serves them to HoloLens clients

import socket
import threading
import time
import json
import random

# Global variable to store the target location
target_location = {
    "latitude": None,
    "longitude": None,
    "altitude": 0,
    "isSet": False
}

# Debug flag - set to False to disable verbose output
DEBUG_MODE = False

def generate_random_gps():
    """
    Generate random GPS data within the specified range
    Also includes any target location information
    Returns values with 4 decimal places precision
    """
    return {
        "latitude": round(random.uniform(63.3, 63.5), 4),
        "longitude": round(random.uniform(10.3, 10.7), 4),
        "altitude": round(random.uniform(0, 100), 1),
        "timestamp": int(time.time() * 1000),
        "valid": True,
        # Target location data
        "targetLatitude": target_location["latitude"],
        "targetLongitude": target_location["longitude"],
        "targetAltitude": target_location["altitude"],
        "hasTarget": target_location["isSet"]
    }

def handle_client(client_socket):
    """
    Handle a client connection by sending periodic GPS data
    """
    try:
        print("[*] Client connected, sending GPS data...")
        while True:
            # Generate random GPS data with target information
            gps_data = generate_random_gps()
            
            # Convert to JSON string
            json_data = json.dumps(gps_data)
            
            # Only print if debug mode is enabled
            if DEBUG_MODE:
                print(f"Sending: {json_data}")
            
            # Send data to client
            message = json_data.encode('utf-8')
            client_socket.send(message)
            
            # Send updates every 10 seconds
            time.sleep(10)
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
    Clear the target location
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
    Start the GPS simulator server
    Sets up socket and manages client connections
    """
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    
    # Bind to all interfaces
    server_ip = '0.0.0.0'
    server_port = 8085
    
    server.bind((server_ip, server_port))
    server.listen(5)
    
    # Get this PC's IP address to display
    import socket as sk
    hostname = sk.gethostname()
    ip_address = sk.gethostbyname(hostname)
    
    print(f"[*] GPS Simulator running")
    print(f"[*] IP Address: {ip_address}")
    print(f"[*] Port: {server_port}")
    print(f"[*] Use this IP in HoloLens: {ip_address}")
    
    # Start thread for handling user input
    input_thread = threading.Thread(target=handle_user_input)
    input_thread.daemon = True
    input_thread.start()
    
    print(f"[*] Target location interaction enabled")
    print(f"[*] Press Ctrl+C to stop the server")
    print(f"[*] Waiting for input...")
    
    try:
        while True:
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
    Handle user input for setting target location
    Runs in separate thread to not block server operation
    """
    while True:
        try:
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
    print("Starting GPS simulator server...")
    start_server()