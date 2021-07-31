# RainbowTaskbar
Customizable Windows taskbar color.

# Default config
```
# format:
# c (ms) (r) (g) (b) (effect) (... effect options)

# effects:
# none (0) (0) (0)
# fade (ms) (steps) (0)
# do not use a high amount of steps! the color interpolation function is not really fast

c 10 28 118 201 fade 4000 50 0
c 10 214 28 235 fade 4000 50 0
c 10 80 28 235 fade 1000 50 0
```
Config can be found in: `%appdata%\rnbconf.txt`
