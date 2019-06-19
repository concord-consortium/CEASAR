# CEASAR
Connections of Earth And Sky using AR/VR

Project uses Unity 2019.1.7f1 and is built using the Unity Cloud Build feature.

## Network Components
This project relies on a central network server on Heroku for multiplayer functionality (currently early in development). The network server software is available [here](https://github.com/concord-consortium/CEASAR-server)

## Vuforia
---
Although Vuforia AR support is enabled in the CEASAR project, the required Vuforia assets have not been imported, scenes have not been structured to support Vuforia, and build settings have not been configured to build with Vuforia.

### Import Assets and Configure Scene
* Add an AR Camera and import Vuforia assets.  In an existing scene or a new scene add an AR Camera by choosing GameObject > Vuforia Engine > AR Camera from the Unity menu.  Unity will prompt you to import required Vuforia assets.  Import these assets.  After adding the AR Camera to the scene, delete or disable the default Main Camera from the scene hierarchy view.  At this point, you should be able to run the scene from the Unity debugger and the AR Camera will display content from your computer's web camera.
* Add an Image Target and import the default Vuforia image database. In the above scene, add an Image Target by choosing GameObject > Vuforia Engine > Image from the Unity menu. Unity will prompt you to import a default image database.  Import this database.  
* Configure Image Target.  Select the Image Target object and view the Image Target Behaviour in the Inspector.  The default database (`VuforiaMars_Images`) should be selected under Database.  Choose an image from the Image Target dropdown (you will find print-friendly versions of these images in `Editor\Vuforia\ForPrint\ImageTargets`). This selected image will be used as the tracked Image Target.
* Add content to Image Target.  Add GameObjects as children of the newly created Image Target object.  These child GameObjects will appear when the Target Image is detected by the AR Camera.
* Add App License Key.  Select Window > Vuforia Configuration from the Unity menu.  Enter a valid license in App License Key section (licenses are created and retrieved from the [Vuforia Developer Portal](https://developer.vuforia.com/) under the License Manager page and require a Vuforia account).

### Build for Android
* Enable ARCore support Vuforia engine.  Add `core-1.4.0.aar` to `Assets/Plugins/Android/`.  Once imported, select `core-1.4.0.aar` in the Project View and ensure Android is checked under Platforms.  For more information, visit [Using ARCore with Vuforia Engine](https://library.vuforia.com/content/vuforia-library/en/articles/Solution/arcore-with-vuforia.html).
* In Player Settings > Other Settings, make sure a valid package name is set.
* In Player Settings > Other Settings, set the Minimum API Level to 4.3.
* In Player Settings > Other Settings, set the Target Achitectures to ARMv7.
* In Player Settings > Other Settings, make sure Vulcan is not in the list of Graphics APIs (if it is, remove it).
* In Player Settings > XR Settings, check Vuforia Augmented Reality Supported.
* Add Vuforia enabled scene to Build Settings (use the Add Open Scenes button under Scenes in Build).
* After building an APK and deploying to an Android device, you might need to manually give the CEASAR app access to the device camera.
* If the CEASAR app has access to the camera and the app shows up as a black screen, try closing and re-opening the app.
