## PST Integration Package for Unity3D

Attach the `PstInterface` component to a GameObject and it will handle the rest. The component can be used to access cached pose data from the PST REST Server. Using existing Unity callbacks like `Awake`, `Start` and `OnApplicationQuit`, the connection to the PST REST Server is established and teared down.