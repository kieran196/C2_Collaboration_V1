# C2_Collaboration_V1
VR for prototyping and designing new Collaborative Command and Control environments

# How to setup the user-study (AR/VR Block-selection task)

**Configuring the Virtual Reality side**

Hardware used: HTC Vive (Standard), (Optional) Zephyr Bioharness (With a computer that has bluetooth).

1. Download/Clone the master repo on GitHub and open the project in Unity.
Note: Make sure to use a recent version since Unity recently changed their prefab system. (I set it up using 2018.3.0.f2)

2. Open the 'NetworkingTestVR' scene. Locate the debugger GameObject and ensure LOCAL_COLLABORATION is set.
![Alt Text](https://i.gyazo.com/d95a6eeb6acb4928e94b330052533d0a.png)

3. Locate the VRMultiRigAttempt prefab and under Block_Selection_Task set the desired TASK_TYPE. If changes have been made to the prefab make sure to apply the changes.
Note: The VRMultiRigAttempt prefab is the player-prefab which contains the core networking functionality.
![Alt Text](https://i.gyazo.com/7f7f7232c079f5962317c34179f112ff.png)
SPAWN_SPEED = The rate at which the cubes spawn on the table, and DESTROY_SPEED = The speed at which the cube disappears. These are the two variables the AR user manipulates to change the difficulty of the task. By default blocks spawn in every 1s, and disappear every 2s.

4. If setting up the Zephyr Heart Rate monitor on the host PC, ensure the HR_DATA_ENABLED variable in the ReadPythonData script is set to true. If the HR monitor is being setup on the client (Hololens) then make sure HR_DATA_ENABLED is set to false.
![Alt Text](https://i.gyazo.com/7ac1a35b527b61cef319f4faebcdfe1f.png)

5. The correct settings should now be configured, run the VR application as the LAN host (H), press '2' to enable the VR rig.
![Alt Text](https://i.gyazo.com/0a214fade8593a0d51e79d28d9b98050.png)

**VR HOST IS NOW SUCCESSFULLY PREPARED FOR THE USER-STUDY**

Also make sure to record the IP address of the PC hosting, as we'll need this in the next part: Configuring the AR side.
![Alt Text](https://i.gyazo.com/87b326ec026f2e3714cca42625d45017.png)

**Configuring the Mixed Reality side**

Hardware used: Microsoft Hololens, (Optional) Zephyr Bioharness (With a computer that has bluetooth).

1. Download/Clone the HololensBranch repo on GitHub and open the project in Unity. (Using a recent version of Unity)

2. Open the 'NetworkingTestVR' scene. Locate the NetworkManager GameObkect, under NetworkHandler open the 'Network Info' dropdown and set the Network Address to the IP Address of the host PC.
![Alt Text](https://i.gyazo.com/aa36d668953e68f61291276731492ef5.png)

3. Locate the Debugger GameObject and set COLLAB_TYPE to LOCAL_COLLABORATION.
![Alt Text](https://i.gyazo.com/6da88ae2a59d120bd7516ab61bf5b561.png)

4. On the Hololens run the Holographic emulation player. (This is much easier than having to build) In Unity go Windows -> XR -> Holographic Emulation. Set Emulation Mode: Remote to Device, Remote Machine = The IP Address shown on the Hololens. Set Max BitRate and connect. If unable to connect, ensure the Hololens is connected to WiFi.
![Alt Text](https://i.gyazo.com/59db020c53776eab8face424fbcfce0c.png)

5. The correct settings should now be configured. The run application and the Hololens should automatically connect to the host.

**Part 6. Calibration setup:** The AR user should now take the controller labelled 'AR' (labelled inside of the VR scene). The AR user uses the Vive controller inside the local environment as their input device.
Note: (The air-tap gesture for the Hololens is also implemented but isn't as precise as the Vive controller)

7. Next, align the Vive Controller with the ghost AR controller floating in the air and pull the trigger. The Hololens will now be calibrated to the Vive coordinate space.

![Alt Text](https://i.gyazo.com/afa67b0d7d732089ff471d5425d62dd4.png)

![Alt Text](https://i.gyazo.com/6e1cc0a6986001c6cdfe3f59669cac07.gif)
