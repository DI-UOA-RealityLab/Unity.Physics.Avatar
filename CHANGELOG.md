## 0.0.6 (2022-09-15)

#### Fixes

* **fix:** Fix StrongRotationDrives
  > There was a bug in the way the StrongRotationsDrives were updated in real-time when the configurations was changing. Now this is fixed and the respective code has been improved.

* **fix:** Fix physics button functionality
  > The button after the update to the latest version has stopped working properly. This was due to some changes in Unity PhysX and how it handles the properties of ConfigurableJoints. The parameters have been tweaked to fix the button behavior. For the same reasons, the reset on physics objects upon pressing the button was broken and this has also been fixed by changing the respective script.

## 0.0.5 (2022-07-12)

#### Features

* **feature:** Add more options to the configuration Scriptable Object of the Articulation Bodies Hand
  > The option to add the missing colliders of the hand on a layer that doesn't collide with anything else was moved from the ArticulationBodiesHand class to the ArticulationBodiesConfiguration Scriptable Object. Additionally, on the configuration was added a different section for the ArticulationDrives of the finger to make the ones that have long chains (>3) of bones in order to make them stronger and hold the rest of the structure.

## 0.0.4 (2022-07-05)

#### Fixes

* **fix:** fix rotation lag on fingers
  > There were a lot of issues when the force limit of the wrist was higher than the fingers resulting into a lag on the finger movement. This is now fixed by adding colliders to the Articulation Bodies that didn't have an immediate one. The only requirement is to add a layer to the project that doesn't collide with anything so these colliders can be set to work like this and select it on the ArticulationBodiesHand script.

## 0.0.3 (2022-06-13)

#### Fixes

* **structure:** change the name of the samples folder
  > Due to unforseen problems when changing the samples folder name, the samples folder was named back "Samples~".

## 0.0.2 (2022-06-13)

#### Features

* **fix:** create new materials and fix missing references
  > There were references on materials and other assets that they weren't included in the package. This has been fixed by adding new materials on the package and fixing all the missing references.

* **structure:** change the name of the samples folder
  > Due to problems maintaing the samples folder, and since there is no known way of keeping the proposed name by Unity and avoiding issues, the sample folder was named "Samples" insted of "Samples~".

## 0.0.1 (2022-05-03)

#### Features

* **structure:** create initial package structure
  > The structure of the repository has been created with all the required files for the package.
