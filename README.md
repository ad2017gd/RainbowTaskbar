# RainbowTaskbar
Lightweight utility for Windows taskbar customization. Supports color effects, transitions, blur, images and transparency.
**Conflicts with TranslucentTB! Close it before running RainbowTaskbar**


![image](https://user-images.githubusercontent.com/39013925/127749893-c171da6b-6dc3-4539-8ccb-9f54dc2675cf.png)

It works on Windows 7 and 11 too!
![image](https://user-images.githubusercontent.com/39013925/138772234-88fce6bb-785f-4496-a281-16c28e6f976e.png)

**WINDOWS VISTA**
![image](https://user-images.githubusercontent.com/39013925/138771919-a9f3080d-82a5-486f-b3ac-d4c21bf42037.png)


- [Usage](#usage)
   - [Editor](#editor)
      - [Color effects](#color)
      - [Transparency options](#transparency)
      - [Images](#bitmaps)
      - [Delays](#delays)
      - [Randomizer](#randomizer)
   - [Tray icon](#tray-icon)
- [Examples](#examples)

# Usage
## Editor
![image](https://user-images.githubusercontent.com/39013925/136743214-a4355570-f94e-41d1-b482-c5c87706ec77.png)

**1,2** - Add or remove config line

**3** - Config line explorer

**4** - Config line options with descriptions

**5** - Apply new config

**6** - Color effect preview



### Color
Color effects are a basic option for RainbowTaskbar.
In the editor, they are previewed on the top.

Supported color effects are:
- Solid (none) - solid color
- Fade (fade) - fade in color
- Gradient (grad) - 2 color gradient, along the width
- Fade gradient (fgrd) - fade in gradient

```
c 750 0 200 255 fgrd 255 0 180 400
c 100 255 255 255 grad 255 255 255
```

![image](https://user-images.githubusercontent.com/39013925/137605831-1e4b7caa-61d0-4a7a-9609-a8b6891e6c53.png)


### Transparency
The transparency of the underlay, or the actual taskbar, can be altered. You can also toggle blur on the taskbar.

Supported transparency effects are:
- Taskbar (1) - change taskbar alpha
- RnbTsk (2) - change underlay alpha
- Both (3) - change taskbar and underlay alpha
- Blur (4) - toggle taskbar blur

```
t 4
t 2 180
t 1 230
```

![image](https://user-images.githubusercontent.com/39013925/137605833-33f2da34-a24d-429e-8ef3-4d2d09145210.png)


### Bitmaps
Bitmap (.bmp) images can be added on RainbowTaskbar. They can also blend with other color effects by altering the transparency.
If your bitmap doesn't work, try opening it in Paint and saving it again.
```
i 0 0 0 0 C:\troll.bmp 128
c 200 32 128 64 fgrd 80 60 230 500
```

![image](https://user-images.githubusercontent.com/39013925/137605838-b97c7abe-0beb-4525-8724-cdd67e355cda.png)

### Delays
Delays can be used to sleep for an amount of time. They can be used along with bitmaps, to replicate animated images.
```
i 0 0 0 0 C:\penguins_1.bmp 255
w 15
i 0 0 0 0 C:\penguins_2.bmp 255
w 15
...
i 0 0 0 0 C:\penguins_63.bmp 255
w 15
```

![image](https://user-images.githubusercontent.com/39013925/137605869-ac5a574a-f8fb-4aac-9db4-d21d86d995f6.png)

### Randomizer
The randomizer can be used to create random color effects. It obsoletes the color parameters of the next color effect and replaces it with random ones. It has no parameters.
```
r
c 1 0 0 0 fgrd 0 0 0 1000
```

![image](https://user-images.githubusercontent.com/39013925/139097253-0e438d0e-f6e3-45e2-a5b0-d473c0b751cc.png)



## Tray icon
![image](https://user-images.githubusercontent.com/39013925/136702026-0333b00b-5af4-4014-9868-a092ef89acfd.png)

When left clicked, it will open the GUI config editor. Closing it will actually minimize it to tray again.

When right clicked, it will open a menu with different options.

![image](https://user-images.githubusercontent.com/39013925/138571165-1ef66965-abe9-4159-b024-96218393e8a1.png)


# Examples
Examples can now be found in the system tray menu.

