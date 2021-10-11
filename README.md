# RainbowTaskbar
Customizable Windows taskbar color and transitions.
**Conflicts with TransparentTB! Close it before running RainbowTaskbar**


![image](https://user-images.githubusercontent.com/39013925/127749893-c171da6b-6dc3-4539-8ccb-9f54dc2675cf.png)

- [Usage](#usage)
   - [Editor](#editor)
   - [Tray icon](#systray)
- [Examples](#examples)

# Usage
## Editor
![image](https://user-images.githubusercontent.com/39013925/136743214-a4355570-f94e-41d1-b482-c5c87706ec77.png)

**1,2** - Add or remove config line

**3** - Config line explorer

**4** - Config line options with descriptions

**5** - Apply new config

**6** - Color effect preview


## SysTray
![image](https://user-images.githubusercontent.com/39013925/136702026-0333b00b-5af4-4014-9868-a092ef89acfd.png)

When left clicked, it will open the GUI config editor. Closing it will actually minimize it to tray again.

When right clicked, it will close RainbowTaskbar.

## Startup
If you chose No on the startup message box, or moved the executable, you can delete the config located at `%appdata%\rnbconf.txt` to get the prompt again.


# Examples
## Rainbow fading gradient (default config)
```
t 4
t 2 200

c 1 255 0 0 fgrd 255 154 0 500
c 1 255 154 0 fgrd 208 222 33 500
c 1 208 222 33 fgrd 79 220 74 500
c 1 79 220 74 fgrd 63 218 216 500
c 1 63 218 216 fgrd 47 201 226 500
c 1 47 201 226 fgrd 28 127 238 500
c 1 28 127 238 fgrd 95 21 242 500
c 1 95 21 242 fgrd 186 12 248 500
c 1 186 12 248 fgrd 251 7 217 500
c 1 251 7 217 fgrd 255 0 0 500
```

## Pulsing gradient with image
```
t 4
t 2 180
t 1 230

i 0 0 0 0 C:\Users\Ad2017\Image.bmp 150
c 750 255 0 180 fgrd 0 200 255 400
c 100 255 255 255 grad 255 255 255
c 750 0 200 255 fgrd 255 0 180 400
c 100 255 255 255 grad 255 255 255
```
### 
