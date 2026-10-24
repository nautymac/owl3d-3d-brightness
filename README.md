# Owl3D 3D Brightness

**English** · [한국어](README.ko.md)

A small tray tool that fixes the **picture getting darker when Live 3D is turned on** on the Owl3D Shift display.
It raises brightness, gamma, saturation and contrast only while Live 3D is running, and restores the original picture when
3D stops. The window is Korean on a Korean Windows and English otherwise.

It does not touch the Owl3D software itself. It is a personal tool, not made or supported by the Owl3D company.

## Why

With Live 3D on, the Shift keeps white as it is but shows midtones noticeably darker, so video and photos look dull even
with the monitor brightness at maximum. Raising brightness in Windows or the graphics driver instead makes the normal
desktop (3D off) too bright and washed out. This tool applies its correction **only while 3D is running**, so both look
right.

## Features

| Feature | Description |
|---|---|
| Automatic 3D detection | Applies the correction when Live 3D starts and restores the original picture when it stops. Nothing to switch by hand. |
| Gamma | Raises or lowers midtones; white and black stay where they are. |
| Gain | Multiplies the whole picture's brightness; black stays black, so it does not look washed out. |
| Saturation | More vivid or paler colours. Works on NVIDIA graphics. |
| Contrast | Adjusts the difference between light and dark areas. |
| Live adjustment | Move a slider and see the result on the 3D screen immediately. |
| Global hotkeys | Work while a video or game is fullscreen; the current values appear briefly on screen. |
| Your own default | Save the values you like as the default and recall them with one button or hotkey. |
| Remembers values | The values are saved and used again at the next start. |
| Tray app | Keeps running in the tray after the window is closed; can start at login. |
| Safe restore | If the PC shuts down or the tool exits while the correction is active, the original picture is restored at the next start. |
| Screen off / on | The Shift has no power button. A hotkey or button turns its screen off and back on. |
| Korean / English | Follows the Windows display language. |
| No installer | A single exe. Put it in any folder and run it. |

## Use

1. Download `Owl3DBrightness.exe` from [Releases](../../releases) and put it in any folder. There is no installer.
2. Run it: the adjustment window opens and an icon appears in the tray. Closing the window keeps it running in the tray.
3. Turn on Live 3D in Owl3D: the correction is applied 4–5 seconds later. Turn it off and the picture returns to normal
   within 2–3 seconds.

| Setting | Range | Off | First default |
|---|---|---|---|
| Gamma | 0.50 ~ 2.50 | 1.00 | 0.70 |
| Gain | 1.00 ~ 2.00 | 1.00 | 1.40 |
| Saturation | 0 ~ 100 | 50 | 70 |
| Contrast | 0.50 ~ 1.50 | 1.00 | 1.00 |

- **Default**: goes back to the saved default values.
- **Save current values as default**: saves the current slider values as the default. They are shown as `[default …]`
  next to each setting.
- **Correction off**: sets every value to "Off".
- **Start at login**: registers the exe's current location. If you move the exe, tick it again.

## Hotkeys

They work whichever program is in front; the current value appears briefly at the top of the screen.

| Keys | Action |
|---|---|
| `Ctrl`+`Alt`+`B` | Show / hide the window |
| `Ctrl`+`Alt`+`P` | Shift screen off / on |
| `Ctrl`+`Alt`+`]` / `[` | Gamma +0.05 / −0.05 |
| `Ctrl`+`Alt`+`=` / `-` | Gain +0.05 / −0.05 |
| `Ctrl`+`Alt`+`.` / `,` | Saturation +5 / −5 |
| `Ctrl`+`Alt`+`'` / `;` | Contrast +0.05 / −0.05 |
| `Ctrl`+`Alt`+`9` | Default |
| `Ctrl`+`Alt`+`0` | Correction off |

## Notes

- **Windows limit**: Windows refuses corrections that are too far from the original. If gamma, gain and contrast are all
  raised a lot, a red warning appears at the bottom of the window and nothing is applied; move one of them back towards 1.00.
- **Saturation works only on NVIDIA graphics** (it uses the driver's digital vibrance). On other graphics the slider is
  disabled.
- The correction covers the whole screen; it cannot brighten only the video area.
- Screen off sends a power command to the monitor (DDC/CI). While the screen is off the window cannot be seen, so use the
  hotkey to turn it back on. The tool must be running for the hotkey to work; if the screen does not come back, unplug and
  replug the cable.
- Settings: `%LOCALAPPDATA%\Owl3D\owl3d-3d-brightness.ini`, log: `owl3d-3d-brightness.log` in the same folder.
- Command-line options: `--hidden` (start in the tray only), `--lang en` / `--lang ko` (force the language).
- Requires Windows 10/11 (.NET Framework 4, included with Windows).

## How it works

Whether Live 3D is running is decided by whether the `display-mirror.exe` process exists. Brightness, gamma and contrast
are applied through the graphics output's gamma ramp, saturation through the NVIDIA driver. Owl3D resets the ramp when it
takes over the fullscreen output, so while 3D is on the tool checks the values every 2 seconds and applies them again. If
the PC shuts down with saturation applied, the original value is restored at the next start.

## Build

Only the C# compiler that comes with Windows is needed.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

The result is `dist\Owl3DBrightness.exe`. The icon (`src\app.ico`) is drawn by code; to change the design, edit
`tools\IconArt.cs` and run `tools\make-icon.ps1` to build it again.
