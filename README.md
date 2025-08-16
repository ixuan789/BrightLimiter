# BrightLimiter

A background service utility to limit the adjustable brightness range on Windows systems.
When the system brightness changes, it automatically overrides the brightness to stay within your defined range.

[中文](./Readme.zh-cn.md)

## Usage 

- Limit brightness between 30% and 80% and install as a background service (runs automatically on startup)
```
./BrightLimiter.exe 30 80 --install
```

- Uninstall the service
```
./BrightLimiter.exe --uninstall
```

- Run without install, limiting brightness to 50%
```
./BrightLimiter.exe 50 50
```

## About

I use a Surface Pro X, which switches to very low-frequency PWM dimming at brightness levels below 57%.

Coincidentally, 57% is the perfect minimum brightness for my needs.

Therefore, this small thing can prevent the brightness from being adjusted below 57%, thus avoiding PWM dimming.

I figured others might also have situations where they need to limit their laptop screen's brightness, so I made this tool to control both the upper and lower brightness limits.

For ease of use, I added the functionality to install it as a service.

The program's principle is simple: it receives a WMI event notification when the brightness changes and uses WMI to set the brightness back to within the specified range.

It is built using .NET 8 and the WmiLight library, with AOT support, which is why the file size in the Release download is very small.
