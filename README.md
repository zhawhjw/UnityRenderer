
<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h3 align="center">Unity Renderer</h3>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>

  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project


This is a Unity rendering pipline for trajecotry data dumped from Threejs


<p align="right">(<a href="#readme-top">back to top</a>)</p>



### Built With

* Windows 11 
* Unity 2021.3.27f1

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

1. Clone this project to local
2. Open project with Unity 2021.3.27f1
3. Follow the image to install the FFmpeg for Windows Build

[![Install FFmpeg][FFmpeg]]()



<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

### 1. Pre-configuration for scenario 
After open the project, find and double click to load 'SampleScene' under the 'Scene' folder in Unity file navigator window. Then, click the 'AgentManager' game object. On the right side of 'Inspector' window, you should see:  

[![Install FFmpeg][AgentManager-Parameters]]()

#### Red Area - Input Data

**Source Path**: A file path to the all data files

**Source File**: A file name that contains converted Threejs agent trajectory data 
 
**Source Wall File**: A file name that contains converted Threejs wall data

**Source Orientation File**: A file name that contains converted Threejs agent orientation data 
 
**Extension**: suffix for all data files

#### Green Area - Model

**Agent Prefab**: agent model

_note_:

* All valid agent models udner "Assets/Agent" folder
* **Agent CapsulePBD Variant** is tuned for threejs capsule rendering only
* **Agent Variant 1** is mix-amo agent model
* **Agent Variant** is used for RL paper rendering

**Obstacle Prefab**: obstacle (sphere) model
 
**Obstacle Wall Prefab**: wall (cube) model

#### Orange Area - Parameter for Animation and Simulation

**A**: lower-bound of normalized velocity value. Default should be 0.

**B**: upper-bound of normalized velocity value. Default should be 4. 
 
**T**: Threshold value to filter out normalized velocity value (i.e. [A, B] -> [T, B]). No default value and need fine-tuned.

_Note_:
* **A**, **B**, **T** are special variables that tied to make leg animation better. All velocity data will be: 
  1. normalized between **A** ~ **B**
  2. filter out any values below **T**
* **A** represent the blend value for standstill animation
* **B** represent the blend value for walking/running animation
* The default setting in animation blend tree is **0** for standstill, **4** for walking, **8** for running
* If value **3.6** appears, it means the animation is 90% of original walking speed: **3.6** / (**4** - **0** )
* In most of case, walking is good enough so we don't need value go beyond then **4**.
* **A**, **B**, **T** have to be fin-tuned for each scenario.

[![Blend Tree][Blend-Tree]]()

**Delta**: How many frames you want to cut-off at the end of simulation. For avoiding agent shift to target location.

**Frame Index**： Indicator of current frame. Change it leads to change the timestamp of simulation.

**Frame Step**: Change the speed of simulation.

**Frame Count**: Total Frame of simulation.




#### Blue Area - Object Rendering 

**Use Capsules**: enable capsule object instead of humanoid.

**Use Shadows**: enable shadow for object.

**Use Arrow**: enable facing direction indicator. Only works on old prefabs.

**Color Code**: enable the **Color Overrides**.

**Color Overrides**: click to expand the color palette for agent groups. Right side is the number of groups.

#### Purple Area - Trajectory Rendering 

**Render Trajectory**: enable the visualization of trajectory

**Trajectory Thickness**: if **Render Trajectory** enabled, adjust thickness of trajectory

**Trajectory Length**: if **Render Trajectory** enabled, adjust maximum length trajectory displayed

**Full Trajectory**: if **Render Trajectory** enabled, enable it will ignore **Trajectory Length** to show full trajectory

**Gradient Flag**: if **Render Trajectory** enabled, enable it will make trajectory color has gradient effect

#### Yellow Area - Special Condition

**Bilas**: if the data (before and after convert) from Bilas

**Orientation Flag**: if orientation data is needed (bottleneck scenario)
 
**Side Stepping Flag**: if no rotation for agent when move to side (bottleneck scenario)

**Shift Degree**: if **Side Stepping Flag** enabled, this should be 90

#### Other Areas

Trival or you should not touch

#### Floor
After open the project, find and double click to load 'SampleScene' under the 'Scene' folder in Unity file navigator window. Then, click the 'GridGenerationManager' game object. On the right side of 'Inspector' window, you should see:  

[![Grid Manager][Grid]]()

**Size**: control the size of each grid

**Margin**: control the size of margin between each grid

**Row**: width of floor

**Column**: length of floor

**Start**: shift of generated floor

### 2. Run Time

After click play button, you will see:

[![Run Time][Run-Time]]()

**Start Capture**: allows you to start recording. After click, it becomes **Stop Capture** and **Cancel Capture** buttons.

**Stop Capture**: finish recording and start to rendering

**Cancel Capture**: discard current recording

**Browse**: allows you to to open the finished renderred recordings

_Note_:
* The small bar in red area is the progress bar which tied to **Frame Index**.

* Too many times recording will lead graphic card memory error. You need to restart the whole unity to clean the cache. 

* Better to use **Stop Capture** instead of **Cancel Capture** even if you want to discard current recording, because **Cancel Capture** is buggy.

* You could use arrow keys to move camera, hold right-click to rotate camera, use wheel to zoom in/out camera

* F8 allows you to capture the screenshot of current view (store under "Assets/Backgrounds")



<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap
- [X] Added comments in code
- [ ] upcoming...

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[FFmpeg]: demo1.png
[AgentManager-Parameters]: demo.png
[Blend-Tree]: demo2.png
[Run-Time]: demo3.png
[Grid]: demo4.png



