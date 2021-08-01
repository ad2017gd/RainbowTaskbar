# RainbowTaskbar
Customizable Windows taskbar color and transitions.
**Conflicts with TransparentTB! Close it before running RainbowTaskbar**


![image](https://user-images.githubusercontent.com/39013925/127749893-c171da6b-6dc3-4539-8ccb-9f54dc2675cf.png)

- [Usage](#usage)
   - [Config](#config)
   - [Options](#options)
   - [Effects](#effects)
   - [Examples](#examples)


# Usage
## Config
### Location
Config can be found in: `%appdata%\rnbconf.txt`
## Options
### Color (`c`)
> c (ms) (r) (g) (b) (effect) (... effect options) 

Apply color effect on taskbar.


### Transparency (`t`)
> t (1=taskbar; 2=rainbowtaskbar; 3=both; 4=blur taskbar, ignores alpha) (alpha, 0-255)

Set transparency for underlay or taskbar, or set taskbar blur.


### Wait (`w`)
> w (ms)

Wait for (ms) miliseconds. Useful for options that have no duration paramemter.


### Image (`i`)
> i (x) (y) (width, full=0) (height, full=0) (full path, without spaces) (alpha, optional) (image width, optional) (image height, optional) 

Show bitmap (*.bmp) on underlay. To apply other color effects, set lower opacity.


## Effects
### None (`none`)
> none

Solid color.


### Fade in (`fade`)
> fade (ms) (steps, optional)

Fade solid color. Default steps are (ms)/20. **Do not use a high amount of steps!**


### Gradient (`grad`)
> grad (r2) (g2) (b2)

2 color gradient.


### Fade in gradient (`fgrd`)
> fgrd (r2) (g2) (b2) (ms) (steps, optional)

Fade 2 color gradient. Default steps are (ms)/20. **Do not use a high amount of steps!**

## Examples
### Rainbow fading gradient (default config)
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

### Pulsing gradient with image
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
