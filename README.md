## About

This is an implementation of an interface for the PST REST Server of [ps-tech](https://www.ps-tech.com/) for their optical tracking systems, specialised for usage with Unity3D. During development `PST SDK 5.0.1.0-acae3ae` was used. 
Specifically, the interface encapsulates everything related from connection establishment to teardown with the PST REST Server and getting the current poses of tracked targets. To use this interface drag'n'drop the `PstInterface` component on any GameObject and call `PstInterface.GetLatestTargetPoseOf(...)`. 

## Example

See the scene `SampleScene` and script `PstTest` for an example usage of the `PstInterface`.

## Integration via the Unity-Package-Manager

Any Unity version supporting the Unity-Package-Manager is able to add this PST interface as a package. Locate the `Packages/manifest.json` (near the `Assets/`) and add he following line:
> "com.simteam.pst": "https://github.com/fh-joanneum-research-and-design-lab/pst-integration.git#1.0.0"

Note that the version number at the end relates to any git tag. Every git tag in this repository will link to a Unity-Package-Manager compliant subtree of this repository. When adding this as a package be sure to check the git tags if a newer version is available. 

Note that newer versions of the Unity-Package-Manager should be able to add this package via the Packages UI in the Editor.

## Notes

* The PST SDK (5.0.1.0-acae3ae) currently only supports 32-bit applications, therefore 64-bit application must use the PST REST API.
* Note that PST uses [Server sent events](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#Event_stream_format) to expose `TrackerData` (see [PST docs](file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_start_tracker_data_stream.html); this is a local link to the local PST documentation and will only work if you have the PST SDK installed).

## License

This repository is under the `BSD 3-Clause License` (see the [license file](LICENSE)).