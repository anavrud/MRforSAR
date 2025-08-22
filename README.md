
## Important Notice: Map Content Removed

All map images and coordinate data previously included in this project have been deleted due to copyright restrictions from "Norge i bilder" (Statens kartverk, Geovekst, and associated partners). The repository does not contain any copyrighted map data. If you wish to use map content, you must obtain appropriate licenses or use your own open data sources.

## Video Demonstration

https://github.com/user-attachments/assets/3e5eaad2-4373-40d7-a28b-723a7aa53dba


## Acknowledgments and Credits

This project incorporates code based on or inspired by:

### GPS Data Communication Components
- **Original Source**: [GPS Socket Implementation](https://gist.github.com/jryebread/2bdf148313f40781f1f36d38ada85d47) by jryebread
- **Used For**: Foundation for GPS server and client communication components
- **Specific Files**:
  - `gps_pc.py` - PC-based GPS simulator (based on `pythonclient.py`)
  - `gps_android.py` - Android GPS server (based on `pythonclient.py`) 
  - `GPSSocketClient.cs` - HoloLens client (based on `server.cs`)

### Enhancements
Our implementation extends the original code with:
- Target location functionality
- Device-specific adaptations (Android, PC, HoloLens)
- Interactive command interface
- Reconnection logic and error handling

### Unity Plugin for HoloLens 2 Research Mode
- **Original Source**: [HoloLens2-ResearchMode-Unity](https://github.com/petergu684/HoloLens2-ResearchMode-Unity/tree/master?tab=MIT-1-ov-file) by petergu684
- **Used For**: Unity Plugin for using research mode functionality in HoloLens 2.
- **Modifications**: Adapted for our specific needs and integrated into the project.
