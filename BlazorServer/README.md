# WoW Classic Grind Bot - BlazorServer

This is the web interface for the WoW Classic Grind Bot that allows you to configure and monitor the bot.

## Setup Instructions

### Prerequisites
- World of Warcraft Classic client installed and running
- .NET 8 SDK installed
- Google Chrome browser

### First Time Setup

1. **Edit the batch script** in `run.bat` to point to where you have put the repo BlazorServer folder
   ```bat
   start "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" "http://localhost:5000"
   c:
   cd /D "%~dp0"
   dotnet run -c Release
   pause
   ```

2. **Execute the `run.bat`** - This will start the bot and Chrome. WoW client must be already running.

3. **If you get "Unable to find the Wow process is it running?"** in the console window then it can't find game executable.

### Configuration Process

When running the BlazorServer for the first time you will have to follow a setup process:

1. **Start the game and login with a character**
2. **Navigate to "2. Addon Configuration"**
   - Fill the **Author** input form with your character name
   - Fill the **Title** input form with a descriptive name
   - Then press **Save** button → Log should see "AddonConfigurator.Install successful"
   - Should see a loading screen
   - At the top left corner of the game window should see flashing pixels/cells

3. **Navigate to "5. Frame Configuration"**
   - **Guidance for good DataFrame**
   - Click on **Auto** → **Start** → **Validate FrameConfiguration**

### Addon Control Panel

The **Status** field shows:
- **Update Available** - if addon needs updating
- **Up to Date** - if addon is current
- **Not Installed** - if addon is not installed

Press **Save** button to install/update the addon.

### Important Notes

- The app reads the game state using small blocks of color shown at the top of the screen by an Addon
- This needs to be configured properly for the bot to work
- Chrome will open automatically and navigate to the configuration interface
- Follow the step-by-step process in the web interface
- The server will restart automatically after configuration changes

### Troubleshooting

- If the process is not detected, make sure WoW is running before starting the bot
- If addon is not visible, restart the game and ensure addons are enabled
- If configuration fails, use the "Delete" option to start over
- Check the log output for detailed error messages

### Navigation

The web interface provides different views:
- **Dashboard** - Main control panel (available after setup)
- **Addon Configuration** - Configure the game addon
- **Frame Configuration** - Configure screen capture
- **Screenshot** - View current game screenshot
- **Log** - View detailed logs
- **Restart** - Restart the server

The navigation will automatically guide you through the setup process on first run. 