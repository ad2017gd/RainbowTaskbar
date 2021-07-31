# RainbowTaskbar
Customizable Windows taskbar color and transitions.

![image](https://user-images.githubusercontent.com/39013925/127749893-c171da6b-6dc3-4539-8ccb-9f54dc2675cf.png)

# Default config
```
# format:
# c (ms) (r) (g) (b) (effect) (... effect options) - change taskbar color
# t (1=taskbar; 2=rainbowtaskbar; 3=both; 4=blur taskbar, ignores alpha) (alpha, 0-255) - set transparency
# w (ms) - wait N ms
# i (x) (y) (width, full=0) (height, full=0) (full path, without spaces) (alpha, opaque=255) (image crop width, optional) (image crop height, optional) 

# effects:
# none - solid color
# fade (ms) (steps, optional) - fade solid color
# grad (r2) (g2) (b2) - gradient
# grad (r2) (g2) (b2) (ms) (steps, optional) - fade gradient

# fade : do not use a high amount of steps! the color interpolation function is not optimized,
# and the Win32 Sleep function is inaccurate at low values

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
Config can be found in: `%appdata%\rnbconf.txt`
