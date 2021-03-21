# GPU-Line-of-Sight
GPU Line of Sight / Field of View visualization for Unity

## Description

This asset allows you to easily add **line of sight / field of view visualization** to your Unity project. 

All rendering is done on the GPU, making this system incredibly fast and allowing an unlimited amount of line of sights sources. It is perfectly suited for top-down stealth or action games, but also works from other viewpoints and in other type of games. The effect can be tweaked to achieve any desired visual style.


![GPU_LOS_Screen01](https://user-images.githubusercontent.com/45169553/111900700-542b7a00-8a34-11eb-8c5e-72eceddc8557.png)

![GPU_LOS_Screen02](https://user-images.githubusercontent.com/45169553/111900709-64dbf000-8a34-11eb-8237-3c995bc50366.png)

![GPU_LOS_Screen03](https://user-images.githubusercontent.com/45169553/111900762-9f458d00-8a34-11eb-8f89-4c0abb81d397.png)

![GPU_LOS_Screen04](https://user-images.githubusercontent.com/45169553/111900766-a40a4100-8a34-11eb-8714-23e2d3a14709.png)


A line of sight system visualizes which parts of the game world can be seen from the standpoint of for example a 3rd person in-game character. The area outside of the line of sight is obscured and remains hidden from the player.

This system was developed as an alternative to CPU based line of sight visualizations. CPU based systems use a combination of ray casting and dynamic meshes to compute the line of sight and are often CPU intensive, resulting in a serious impact on performance.

This new GPU based line of sight system uses a technique very similar to shadow mapping and is significantly faster than any CPU based system, freeing up valuable CPU time. It has been designed from the ground up with performance, ease-of-use and customizability in mind. The user can control the look of the effect by selecting which image effects are applied to the area outside of the line of sight.

### Main Features

- **Performance:** Designed with performance in mind, zero allocation during run-time
- **Customization:** Customize the effect so that it matches the visual style of your project seamlessly
- **Editor preview:** The effect is previewed inside the scene editor, so tweaking is easy and straightforward
- **Visibility checking:** Includes scripts to check if objects are inside the line of sight
- **Documentation:** Includes full documentation and an extensive example scene

### Supported

- **Unity version 5.2** and above
- Forward and deferred rendering pipelines
- DX11

### NOT Supported

- Scriptable Render Pipelines (URP, HDRP)
- Sprite or 2D toolkit based projects
- Mobile and WebGL platforms
