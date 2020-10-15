# CEASAR
Connections of Earth And Sky using AR/VR

Traditional ways of learning about the stars and motions of the sun, moon, and planets employ elements common to augmented reality: collaboration and embodiment. The CEASAR project (Connections of Earth and Sky Using Augmented Reality) is researching methods of collaborative problem-solving using augmented reality (AR). With AR, users see their surroundings superimposed with digital objects, making collaboration possible because learners can see and communicate with those around them. But there are few educational examples of AR. Our goal is to demonstrate the benefits of an immersive augmented reality platform for collaborative learning and problem-solving.

## Project Configuration

Project uses Unity 2020.1.8f1 and is built using the Unity Cloud Build feature. AR and VR features are currently in-development. For AR, this project will use the Hololens2, though this work has not yet been completed. For VR, the project currently uses the Oculus Quest, and hence the Oculus libraries are included for VR builds.

All clients on different platforms connect to the same multiplayer server, enabling students to collaborate cross-platform and share perspectives on the sky.

## Network Components
This project relies on a central network server built from [Colyseus](https://github.com/colyseus/colyseus-unity3d) hosted on Heroku for multiplayer functionality (currently early in development). The network server software is available [here](https://github.com/concord-consortium/CEASAR-server)

## Builds
Unity Cloud Build is currently used from Master branch for most builds. WebGL builds currently require manual intervention to ignore all the Oculus libraries.
To build for WebGL, in the Unity editor, rename the `Oculus` folder to `Oculus~` to hide the folder from Unity. When the build has completed, the folder name can be changed back.

## Data Attribution
Star data sourced from the [HYG Database](https://astronexus.com/hyg)
https://github.com/astronexus/HYG-Database
