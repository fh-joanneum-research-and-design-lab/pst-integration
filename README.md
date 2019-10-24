## PST Integration Package for Unity3D

The UPM package of the PST integration for Unity3D.

### Usage

Attach the `PstInterface` component to a GameObject and it will handle the rest. The component can be used to access cached pose data gathered from the PST REST Server. Using existing Unity callbacks like `Awake`, `Start` and `OnApplicationQuit`, the connection to the PST REST Server is established and teared down.

`PstInterface.GetLatestTargetPoseOf(...)` returns the current pose of the given tracked target.