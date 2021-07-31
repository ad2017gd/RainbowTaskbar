# RainbowTaskbar
Customizable Windows taskbar color.

# Default config
```
# format:
# c (ms) (r) (g) (b) (effect) (... effect options) - change taskbar color
# t (1=taskbar, 2=rainbowtaskbar, 3=both, 4=blur taskbar, ignores alpha) (alpha, 0-255) - set transparency
# d (ms) - wait N ms

# effects:
# none - solid color
# fade (ms) (steps) - fade solid color
# grad (r2) (g2) (b2) - gradient

# fade : do not use a high amount of steps! the color interpolation function is not optimized,
# and the Win32 Sleep function is inaccurate at low values

t 4
t 1 200
c 1000 140 0 185 fgrd 0 189 208 5000 100
c 1000 0 189 208 fgrd 140 0 185 5000 100
```
Config can be found in: `%appdata%\rnbconf.txt`
