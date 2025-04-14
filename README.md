## Acknowledgments

This project incorporates code based on or inspired by:

### GPS Data Communication Components
- **Original Source**: [GPS Socket Implementation](https://gist.github.com/jryebread/2bdf148313f40781f1f36d38ada85d47) by jryebread
- **Used For**: Foundation for GPS server and client communication components
- **Specific Files**:
  - `gps_pc.py` - PC-based GPS simulator (based on `pythonclient.py`)
  - `gps_android.py` - Android GPS server (based on `pythonclient.py`) 
  - `GPSSocketClient.cs` - HoloLens client (based on `server.cs`)

### Modifications
Our implementation extends the original code with:
- Target location functionality
- Device-specific adaptations (Android, PC, HoloLens)
- Interactive command interface
- Reconnection logic and error handling
