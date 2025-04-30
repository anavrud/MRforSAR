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

### Unity Plugin for HoloLens 2 Research Mode
- **Original Source**: [HoloLens2-ResearchMode-Unity](https://github.com/petergu684/HoloLens2-ResearchMode-Unity/tree/master?tab=MIT-1-ov-file) by petergu684
- **Used For**: Unity Plugin for using research mode functionality in HoloLens 2.
- **Modifications**: Adapted for our specific needs and integrated into the project.
