<div align="center">
   <a href="../../stargazers"><img src="https://img.shields.io/github/stars/ad2017gd/RainbowTaskbar?style=for-the-badge"></a>
   <a href="LICENSE"><img src="https://img.shields.io/github/license/ad2017gd/RainbowTaskbar?style=for-the-badge"></a>
   <a href="../../issues"><img src="https://img.shields.io/github/issues/ad2017gd/RainbowTaskbar?style=for-the-badge"></a>
   <a href="../../releases"><img src="https://img.shields.io/github/v/release/ad2017gd/RainbowTaskbar?style=for-the-badge"></a>
   
   <h1>RainbowTaskbar</h1>
<p>Lightweight utility for Windows taskbar customization. Supports color effects, transitions, blur, images, rounded corners and transparency. Tested on Windows 11, 10, 7 and even Vista!</p>
   <p><strong>Conflicts with TranslucentTB! Close it before running RainbowTaskbar</strong></p>


   <img style="margin: 0 auto; width: 75%;" src="https://user-images.githubusercontent.com/39013925/139337700-3216b58a-230b-4b47-a305-1de7525bafd5.png">
<br>
<br>
<br>
</div>

# Table of contents
- [Getting started](#getting-started)
- [Usage](#usage)
   - [Tray icon](#tray-icon)
   - [Editor](#editor)
      - [Color effects](#color)
      - [Transparency options](#transparency)
      - [Images](#bitmaps)
      - [Delays](#delays)
      - [Randomizer](#randomizer)
      - [Rounded corners](#rounded-corners)
- [Examples](#examples)
- [License](#license)

# Getting started
First off, grab yourself the hottest new release at https://github.com/ad2017gd/RainbowTaskbar/releases.

RainbowTaskbar is a portable app, so there's no installing required.

![image](https://user-images.githubusercontent.com/39013925/139337115-ca6d6721-ba52-472b-ac32-8a96412995fe.png)

This is an **one-time** prompt, only visible if there is no config file present. You can always toggle it under the system tray menu. If you change the location of the executable, make sure to **toggle the option in the menu**, and set it back on.

![image](https://user-images.githubusercontent.com/39013925/139337440-856fe46e-0068-439c-9702-d7798f879729.png)

This will pop up when you start RainbowTaskbar if there is an update available. If you click Cancel, but change your mind, you have to **manually** remove the registry key located at `HKEY_CURRENT_USER\SOFTWARE\RainbowTaskbarNoUpdate`.

# Usage
## Tray icon
![image](https://user-images.githubusercontent.com/39013925/139339987-bd5501bc-45dc-4573-a5dc-1e7cd082f948.png)

This is what you first see when you start up RainbowTaskbar, apart from the taskbar itself. Used to access the editor, examples, and different options.

You can click on it to bring the editor up, or right click to see the options.

![image](https://user-images.githubusercontent.com/39013925/139340136-e09317bd-a4d3-4e79-9503-2bd67a41bccd.png)


## Editor
![image](https://user-images.githubusercontent.com/39013925/139339695-dce24946-1092-4565-81fd-c0cbdbccfb14.png)

This is the editor. It works by showing you all the config lines, and a helper GUI for them.

You can use the `+ -` buttons to add or remove config lines.

There are currently **6** basic options, listed below.
<hr>



### Color
**Color** effects are a basic option for RainbowTaskbar.
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
The **transparency** of the underlay, or the actual taskbar, can be altered. You can also toggle blur on the taskbar.

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
**Bitmap** (.bmp) images can be added on RainbowTaskbar. They can also blend with other color effects by altering the transparency.
If your bitmap doesn't work, try opening it in Paint and saving it again.
```
i 0 0 0 0 C:\troll.bmp 128
c 200 32 128 64 fgrd 80 60 230 500
```

![image](https://user-images.githubusercontent.com/39013925/137605838-b97c7abe-0beb-4525-8724-cdd67e355cda.png)

### Delays
**Delays** can be used to sleep for an amount of time. They can be used along with bitmaps, to replicate animated images.
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
The **randomizer** can be used to create random color effects. It obsoletes the color parameters of the next color effect and replaces it with random ones. It has no parameters.
```
r
c 1 0 0 0 fgrd 0 0 0 1000
```

![image](https://user-images.githubusercontent.com/39013925/139097253-0e438d0e-f6e3-45e2-a5b0-d473c0b751cc.png)

### Rounded corners
**Rounded corners** are defined using a border radius. The GUI border radius maximum value is set to 64 pixels. **The corners are not antialiased!**
```
b 20
```
![image](https://user-images.githubusercontent.com/39013925/139352702-8cd26f28-3eee-49b9-92ad-277ee2909558.png)



<hr>

# Examples
Examples can now be found in the system tray menu.
![image](https://user-images.githubusercontent.com/39013925/139342259-00fcfccc-ec94-449a-a047-9fea4f811e93.png)


# License
The app is distributed under the MIT License. See `LICENSE` for more information.
