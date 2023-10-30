# Sea Spelunking

[![youtube](https://github.com/TechnicJelle/CMGT-Project-Innovation/assets/22576047/d1bde682-9779-4123-b75d-8726e8a891d3)](https://youtu.be/VY_CouT-l0w)

## Soundtrack
Stream or buy the soundtrack of this game at [redotter.bandcamp.com/track/sea-spelunking](https://redotter.bandcamp.com/track/sea-spelunking)

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

### Sound
Made with Ableton Live

#### VSTs Used
- [Harmonus](https://lostin70s.com/shop/keys/harmonus-300) by Lostin70s
- [BBC Symphony Orchestra](https://www.spitfireaudio.com/bbc-symphony-orchestra-discover) by Spitfire
- [HurdyGurdy](http://samcycle.blogspot.com/2016/06/samsara-hurdy-gurdy-free-vsti.html) by Samsara

#### SFX Used
| Host                                                                                                                          | Client                                                                                                                                                    |
|-------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Gun shot](https://freesound.org/people/schots/sounds/382735/) (CC0) by schots                                                | [Ships bell.wav](https://freesound.org/people/CGEffex/sounds/97795/) (CC BY 4.0) by CGEffex                                                               |
| [Waves » Big waves hit land.wav](https://freesound.org/people/straget/sounds/412308/) (CC0) by straget                        | [Wind » Medium Wind](https://freesound.org/people/kangaroovindaloo/sounds/205966/) (CC BY 4.0) by kangaroovindaloo)                                       |
| [seagulls....waves. » Flock of seagulls.wav](https://freesound.org/people/juskiddink/sounds/98479/) (CC BY 4.0) by juskiddink | [Pack - Footsteps » Footsteps_Walk.wav](https://freesound.org/people/Nox_Sound/sounds/490951/) (CC0) by Nox_Sound                                         |
| [Hit - Wooden » hit - wooden 09.wav](https://freesound.org/people/Anthousai/sounds/406278/) (CC0) by Anthousai                | [160049 » Shoveling Sand](https://freesound.org/people/Piggimon/sounds/366387/) (CC BY-NC 4.0) by Piggimon                                                |
|                                                                                                                               | [Soundsnap sample pack » Utensils drawer - rummaging and searching.](https://freesound.org/people/SoundsnapFX/sounds/584202/) (CC BY 3.0) by SoundsnapFX  |
|                                                                                                                               | [Chest Opening.wav](https://freesound.org/people/spookymodem/sounds/202092/) (CC0) by spookymodem)                                                        |
|                                                                                                                               | [Gold Coins » Gold Coins X2 Drop on a Heep of Others.wav](https://freesound.org/people/The_Frisbee_of_Peace/sounds/575574/) (CC0) by The_Frisbee_of_Peace |
|                                                                                                                               | [Counting Me Shillings.mp3](https://freesound.org/people/husky70/sounds/161315/) (CC0) by husky70                                                         |
|                                                                                                                               | [Sound Recordings » Bottle Caps.wav](https://freesound.org/people/MikeRat/sounds/188226/) (CC0) by MikeRat                                                |


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
