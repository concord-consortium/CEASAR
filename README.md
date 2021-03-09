# CEASAR
Connections of Earth And Sky using AR/VR

Traditional ways of learning about the stars and motions of the sun, moon, and planets employ elements common to augmented reality: collaboration and embodiment. The CEASAR project (Connections of Earth and Sky Using Augmented Reality) is researching methods of collaborative problem-solving using augmented reality (AR). With AR, users see their surroundings superimposed with digital objects, making collaboration possible because learners can see and communicate with those around them. But there are few educational examples of AR. Our goal is to demonstrate the benefits of an immersive augmented reality platform for collaborative learning and problem-solving.

## Project Configuration

Project uses Unity 2019.4 and is set up to be built either locally or using the Unity Cloud Build feature for desktop builds.

For VR, the project currently uses the Oculus Quest, and hence the Oculus libraries are included for VR builds. The Oculus libraries have very few conflicts with desktop configurations, so for desktop, web client, and VR client builds, the master branch can be used.

For AR, this project uses the Hololens2, and this work is currently in the [hl2-2019 branch](https://github.com/concord-consortium/CEASAR/tree/hl2-2019). The Hololens 2 is well-supported via the inclusion of the MRTK (Mixed Reality Toolkit).

All clients on different platforms connect to the same multiplayer server, enabling students to collaborate cross-platform and share perspectives on the sky.

## Network Components
This project relies on a central network server built from [Colyseus](https://github.com/colyseus/colyseus-unity3d) hosted on Heroku for multiplayer functionality (currently early in development). The network server software is available [here](https://github.com/concord-consortium/CEASAR-server)

## Builds
Current setup: Unity 2019.4.18f1 (or 2019.4.x since the 2019.4 is the LTS version). At time of writing, 2020 is in Tech release (unstable) mode, and frequently introduces breaking changes and incompatibilities with Oculus and MRTK libraries.

## IMPORTANT
The Oculus Quest uses the new XR plugin system, since the project was supposed to be more future-proof. At time of development, the MRTK was not working well with this new system. Rather than back-port the Quest to match the MRTK version, the decision was made to keep two branches for now until a stable 2020 release from Unity emerges with a compatible MTRK version for full editor support.

### Editor and General Information
In-Editor testing: The network panel is set to show extra entries in Editor mode - this proved useful in UI design, and is a useful feature (using conditional compilation) should this be required elsewhere. Much of the `SceneLoader` component uses conditional compilation to set up cameras and UI for different build targets.

### Unity Cloud Build
Unity Cloud Build is currently used from `Master` branch for most builds. You can build standalone desktop builds at any time on either Mac or Windows if required. This is not much use beyond those build targets, since it cannot build for Quest or Hololens.

### Building for WebGL
To build for `WebGL`:
* in the Unity editor, change the build target to `WebGL`
* Rename the `Oculus` folder to `Oculus~` to hide the folder from Unity
* Do the build to WebGL
* When the build has completed, Rename the `Oculus~` folder to `Oculus` to return the folder to normal

### Building for Oculus Quest
To build for Oculus Quest, you need:
* a Windows PC set up with Android Build Support enabled
* you must install subcomponents: Android SDK & NDK Tools, and the OpenJDK as part of installation

While this build process may work on Mac (untested) on the PC you gain the ability to use the Oculus Quest in `Link` mode for live in-editor debugging while wearing the headset, if your headset is set up correctly (Developer mode needs to be enabled on the device, which can only be done via the Oculus iOS / Android application for administering your device).

### Building for Hololens2
To build for Hololens2, you need:
* a Windows PC set up with the Windows SDK installed
* a recent version of Visual Studio 2019 **with support for USB debugging** (an optional installation component not installed by default). Full information on Hololens build configuration [the MRTK installation guide](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html).

At time of writing, this process requires MRTK 2.4, Unity 2019.4, and uses the soon-to-be-deprecated Legacy XR pipeline for Unity.

### Recommended Development & Build Process
Because the switching of build targets is time-intensive, you can set up **multiple copies of the project locally** and configure each copy to target a different build target. This works well if you are also on different branches, since you won't generate conflicts from opening one or the other.

### Folder structure


**CEASAR_QUEST** Build target: Android, Git: Master or a branch off Master as base

**CEASAR_DESKTOP** Build target: Standalone (Windows or Mac, you can switch easily), Git: `Master` or a branch off `Master` as a base

**CEASAR_HOLOLENS** Build target: Universal Windows Platform, Git: `hl-2019` or a branch off `hl-2019` as a base


## Data Attribution
Star data sourced from the [HYG Database](https://astronexus.com/hyg)
https://github.com/astronexus/HYG-Database
Constellation Lines from [Stellarium](https://github.com/Stellarium/stellarium/blob/master/skycultures/western/constellationship.fab)