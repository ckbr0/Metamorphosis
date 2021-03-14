# Metamorphosis
Among Us Mod, which transforms Impostors into Metamorphs.

### What can Metamorphs do?

Metamorphs can take on appearance of any other non Metamorph player.

### How do they do it?

By touching other players, Metamorph automaticaly collects their DNA samples.
Using their DNA, they can tako on their appearance.
Metamorph can only hold the DNA of the last player touched.
After touching another player, a new button will become active.
Upon pressing the new button (or pressing F key on your keyboard), Metamorphosis will take place.
Metamorphosis is only active for a duration (duration is set in the lobby game options menu).
After Metamorphosis expires a new cooldown is started, which prevents the Metamorph from collecting new DNA samples.

## Install from pre-built release

TODO.

## Install from source

#### Build Requirements

- .NET version 5.0 ([download](https://dotnet.microsoft.com/download/dotnet))
- Among Us Version v2021.3.5s

#### Steps

- Clone or download project's source code.
- Download latest BepInEx bleeding edge build ([download](https://builds.bepis.io/projects/bepinex_be/350/BepInEx_UnityIL2CPP_x86_07a69cf_6.0.0-be.350.zip)).
- Extract the contents of BepInEx_IL2CPP_x86.zip into your games's root directory.
- Run the game once to compleate BepInEx configuration process.
- Copy all .dll libraries except **netstandard.dll** from *BepInEx/core* and *BepInEx/unhollowed* into project's *libs* directory.
- Build the project with eather Visual Studio or by calling the command *dotnet build*.
- Copy *Metamorphosis.dll* from build directory to *BepInEx/plugins* in your game's root directory.

## Resources

[https://github.com/Woodi-dev/Among-Us-Sheriff-Mod](https://github.com/Woodi-dev/Among-Us-Sheriff-Mod) For code snippets.  
[https://github.com/DorCoMaNdO/Reactor-Essentials](https://github.com/DorCoMaNdO/Reactor-Essentials) For custom button creation.

## Liecense

This software is distributed under the GNU GPLv2 License.
