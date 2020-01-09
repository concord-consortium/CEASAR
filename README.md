# CEASAR
Connections of Earth And Sky using AR/VR

Traditional ways of learning about the stars and motions of the sun, moon, and planets employ elements common to augmented reality: collaboration and embodiment. The CEASAR project (Connections of Earth and Sky Using Augmented Reality) is researching methods of collaborative problem-solving using augmented reality (AR). With AR, users see their surroundings superimposed with digital objects, making collaboration possible because learners can see and communicate with those around them. But there are few educational examples of AR. Our goal is to demonstrate the benefits of an immersive augmented reality platform for collaborative learning and problem-solving.

## Project Configuration

Project uses Unity 2019.2.13f1 and is built using the Unity Cloud Build feature. AR and VR features are currently in-development. For AR, using the Unity AR Foundation toolkit, enabling builds for both Android and iOS with image targets. Previous experiments used the Vuforia library for this purpose, but due to license restrictions this is no longer used. For VR, the project currently uses the Oculus Quest, and hence the Oculus libraries are included for VR builds.

TThe AR and VR experiences are connected to the same multiplayer server as desktop clients, enabling students to collaborate cross-platform and share perspectives on the sky.

## Network Components
This project relies on a central network server built from [Colyseus](https://github.com/colyseus/colyseus-unity3d) hosted on Heroku for multiplayer functionality (currently early in development). The network server software is available [here](https://github.com/concord-consortium/CEASAR-server)

## Builds
Unity Cloud Build is currently used from Master branch for most builds. WebGL builds currently require manual intervention to remove all the Oculus libraries. Even though we don't use the microphone in the project, unused Oculus VR code that assumes access to the microphone will cause builds to fail, so the simple solution is to locally remove Oculus pieces and generate a WebGL build from the required version of the project.
