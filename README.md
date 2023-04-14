# CMGT-Project-Innovation

## Asset Attribution
Unless otherwise mentioned, everything in this project is made by us.

### Fonts
- [Cinzel](https://fonts.google.com/specimen/Cinzel) by Natanael Gama
- [Footlight MT Light](https://learn.microsoft.com/en-us/typography/font-list/footlight-mt) by Monotype Corporation

### Libraries
- [websocket-sharp](https://github.com/sta/websocket-sharp) (MIT)
- [QRCoder](https://github.com/codebude/QRCoder) (MIT)
- [QRCoder.Unity](https://github.com/codebude/QRCoder.Unity) (MIT)
- [ZXing.Net](https://github.com/micjahn/ZXing.Net) (Apache 2.0)
- [ByteMe](https://github.com/japajoe/ByteMe) (MIT)

## Cloning instructions
This project uses Symlinks to share assets between the Host and the Client projects, so make sure your OS supports them.

### Windows
Windows 10 should have support for them these days, but make sure that you have this git setting set:
```powershell
git config --global core.symlinks true
```
This will enable git to set up the symlinks correctly when you clone this project.

Alternatively, you can also clone the project using this command:
```powershell
git clone -c core.symlinks=true <URL>
```
This makes it so you only clone this specific repository with symlinks enabled, and you don't modify your whole global git setup.

If neither of these two methods work, run the `setup_shared.ps1` script (with admin privileges)

### Linux
Should just work out of the box.

## Networking on Windows
Unity editor does not ask for Windows' permission regarding networking. Any phone-to-host networking has to be done via building the host.
