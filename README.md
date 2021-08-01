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
As of July 2021 Unity Cloud Builds are no longer running. Previously Cloud Builds were created from the `master` branch for Mac and Windows. Instead builds are done locally and then shared via S3. Furthermore, Cloud Builds were not much use beyond Mac and Windows build targets, since Unity Cloud Builds do not support the Quest or Hololens.

### Building for WebGL
To build for `WebGL`:
* in the Unity editor, change the build target to `WebGL`
* Rename the `Oculus` folder to `Oculus~` to hide the folder from Unity
* Create the WebGL build in a folder named `CEASAR`
* When the build has completed, Rename the `Oculus~` folder to `Oculus` to return the folder to normal
* By default, the Unity WebGL build uses static 960 x 600 pixel dimensions for the content frame. CSS adjustments can be made to the `index.html` file located in the build folder to change how the CEASAR content frame is shown in the browser. The `index.html` file located in `Assets\HTML` contains CSS that allows the CEASAR content frame to fill the browser window. You can copy this file over the built `index.html` file located in the WebGL build root folder if you have made the WebGL build in a folder named `CEASAR` (otherwise the copied `index.html` file will contain references to file names that do not exsit in the WebGL build folder).

### Building for Oculus Quest
To build for Oculus Quest, you need:
* a Windows PC set up with Android Build Support enabled
* you must install subcomponents: Android SDK & NDK Tools, and the OpenJDK as part of installation

While this build process may work on Mac (untested) on the PC you gain the ability to use the Oculus Quest in `Link` mode for live in-editor debugging while wearing the headset, if your headset is set up correctly (Developer mode needs to be enabled on the device, which can only be done via the Oculus iOS / Android application for administering your device).

### Building for Hololens2
Make sure you are using the `hl2-2019` branch when working with the HoloLens. For full information on Hololens build configuration consult [the MRTK installation guide](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html).

#### Prerequisites
The following prerequisites are required to build and deploy to a HoloLens as of July 2021.
- Install Unity 2019.4.18f1.
- Install the Unity 2019.4 Universal Windows Platform module.
- Install Visual Studio 2019.
- Using the Visual Studio Installer:
  - install the Visual Studio Universal Platform development Workload.
  - install the Visual Studio Game development with Unity Workload.
  - install the Visual Studio Universal Platform development Workload.
  - install the USB Device Connectivity Individual Component.
  - ensure that the Windows 10 SDK Individual Component is installed (this should be installed with the Universal Platform development Workload).

#### Unity Settings
Unity is used to create a Visual Studio solution from the Unity CEASAR project. Ensure that the following settings and configurations are enabled before building.
- Ensure that Unity is pointing at your current installation of Visual Studio.
  - From the Unity menu select Edit > Preferences.
  - Set the installed version of Visual Studio 2019 as the External Script Editor (you may need to browse to the installed Visual Studio executable in your Program Files folder).
- In the Unity Build Settings, change the platform to Universal Windows Platform.
- Select all of the required HoloLens scenes under Scenes in Build (note that the HoloLens uses different scenes than the other platforms - the HoloLens scenes are located in `Assets/Scenes/Hololens`).
- Set the Target Device to HoloLens.
- Set the Architecture to ARM64 (this will cause a warning on some systems indicating that the local machine does not support running projects compiled with the ARM64 architecture - this warning can be ignored).
- Set the Build Type to D3D Project.
- Set the Target SDK version to the Latest Installed.
- Set the Build Configuration to Release (change this to Debug if you plan to debug the CEASAR application in the HoloLens emulator or if you plan to use remote debugging on the HoloLens itself. Note that the Debug version will run at a much lower frame rate. You may also see a warning in the Build Settings when setting this to Debug which can be ignored).
- Turn off Copy PDB Files (turn this on if you plan to debug using either of the methods above).

#### Build Visual Studio Solution in Unity
After setting up all of the required prerequisites and configuring Unity, you can build the Visual Studio solution.
- In the Unity Build Settings, press Build and select a location for Unity to build your project. Unity does not build a finished HoloLens application. Instead it builds a .sln solution file (and all required supporting files) that can be opened in Visual Studio.
- Once the build is complete, open the solution in Visual Studio (double-click the .sln file).

#### Build and Deploy to HoloLens
With the Visual Studio solution, you can build and deploy the CEASAR application to the HoloLens.
- To connect a PC to a HoloLens, you must first enable Developer Mode on the PC. This step is only required once. Open Settings on your PC. Select Update and Security. Select For developers. Enable Developer Mode and select Yes to accept the change (this might require downloading a Windows update before this step is complete).
- Connect the HoloLens to your PC using USB-C to USB-A cable.
- Open the solution that you built using Unity in Visual Studio (double-click the .sln file).
- Set the Solution Configuration to Release (set to Debug if you are going to debug the CEASAR application).
- Set the Solution Platform to ARM64.
- Set the Device to Device.
- From the Build menu select Deploy CEASAR. This will build CEASAR and deploy it to the HoloLens. During this process the HoloLens may power down or go into sleep mode. This may cause the build and deployment to fail. For best results wear the HoloLens during the deployment process so the HoloLens does not power down or enter sleep mode.
- Once the deployment is complete, CEASAR will appear in your list of HoloLens applications and can be run on your HoloLens.
- The first time that you connect the HoloLens to your PC and attempt to deploy to it, you will need to use a Pairing Pin. In the HoloLens, select Settings > Update > For developers and enable Developer Mode. Enable the Device Portal. Select Pair and the HoloLens will generate a Pairing Pin that can be entered in Visual Studio. Before selecting the HoloLens device from the menu, you will need to enter the Pairing Pin.

#### Remote Debugging
You can use remote debugging in Visual Studio to connect to an installed CEASAR application running on the HoloLens and see errors, exceptions, and debug statements in the Visual Studio Output window.
- Open the CEASAR solution that you built using Unity in Visual Studio (double-click the .sln file).
- Set the Solution Configuration to Debug.
- Build and deploy CEASAR to your Hololens using the above steps.
- From the Debug menu in Visual Studio, select Other Debug Targets > Debug Installed App Package.
- Set the Connection Type to Device.
- Select CEASAR from the list of App Packages and press Start.
- The CEASAR application will run on your HoloLens. As you use the application, Unity debug statements will appear in the Output window. The frame rate will be significantly lower in this debug mode.

#### Debug Using Emulator
Windows 10 Home Edition does not support the HoloLens Emulator. The HoloLens 2 Emulator requires the Windows 10 October 2018 update or later. Details on using the HoloLens Emulator can be found [here](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/using-the-hololens-emulator).

#### Create App Packages
- Open the solution that you built using Unity in Visual Studio (double-click the .sln file).
- Set the Solution Configuration to Release.
- Set the Solution Platform to ARM64.
- In the Solution Explorer, right click on the CEASAR (Universal Windows) node. From the menu, choose Publish > Create App Packages.
- In the Create App Packages dialog select to distribute the application using Sideloading.
- Choose to sign the application with the current certificate.
- Set an output location.
- Set a version number (if needed).
- Set the desired architectures (only need ARM64).

### Uploading Builds

Builds are typically shared in a "ceasar" folder inside of the "models-resources" bucket on S3. Create a new folder inside of the "ceasar" folder with an appropriate name (e.g., "v1.1"). Put the contents of the Mac, Windows, and HoloLens builds into individual .zip files and upload each file to the new folder. Upload the Quest .apk file to the new folder. Create a subfolder for the WebGL build and upload the contents of the WebGL build to this folder. Once the upload to S3 is complete, the build files can be downloaded from http://ceasar.concord.org (e.g., http://ceasar.concord.org/v1.1/ceasar_mac.zip or http://ceasar.concord.org/v1.1/ceasar_win.zip).

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