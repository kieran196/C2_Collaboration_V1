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
