## 0.0.11 (2023-09-15)

#### Improvements

* **improvement:** Improve hand scaling functionality
  > The previous version expected the scale change only on the top level. This now has changed and scale change on any level of the hierarchy is supported.

## 0.0.10 (2023-09-15)

#### Fixes

* **fix:** Fix hand scaling functionality
  > When the hands were using a different scale than the original, the physics hand didn't adjust appropriately. This is now fixed and the hand can be scaled on rutime to match the target hand by adjusting the parent anchors of the joints as needed. There is an issue when there is a transform in the hierarchy of joints that doesn't have an arrticulation body. Make changes to the sample scenes to reflect the aforementioned.

## 0.0.9 (2023-06-08)

#### Features

* **feature:** Add option to disable hand's finger collisions
  > The option to disable hand's finger collisions was added, similarily to how the hand's fingers ignored collisions with the palm. Now both of these two options are optional and can be set using the configuration settings. The basic configuration was updated appropriately.

## 0.0.8 (2023-06-01)

#### Fixes

* **fix:** Fix finger rotation issue
  > There was a bug related with finger rotations. The problem was that the finger bone rotations that stay unchanged, such as the Y and X axes of the the distal and intermidiate phalanges, if changed during runtime after the initialization of the physics hand, the changes weren't reflected on the physics hand. This resulted to wrong finger rotations. To solve this a check was added to the ArticulationBodyFollower script so that the parent anchor rotation gets tweaked appropriately to matcht the change.

* **fix:** Refactor and improve code
  > The code of the ArticulationBodyFollower scripted was refactored to be more readable and maintanable. Also some unecessary instantiations of objects were removed and replaced by the value itself (e.g. new Vector3).

## 0.0.7 (2023-01-17)

#### Features

* **feature:** Add scale updating of the hand on runtime
  > Until now the scale of the hand remained the same during runtime so changes to it introduced from the hand tracking input device weren't considered. This has changed and on Update every bone as well as the whole clonde hand itself are getting checked for changes in the scale.

#### Improvements

* **improvement:** Change joint properties to make it easier to be pushed
  > The sample physics button included with the library got updated with some changes in its properties to achieve a more realistic behavior.

* **improvement:** Update the proposed sample configuration
  > The configuration contained for the physics hand has been updated to provide a better starting point.

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
