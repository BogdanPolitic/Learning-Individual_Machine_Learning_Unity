# Learning-Individual

<h3> General description </h3>

This is a prototype, part of a huge Unity 3D project that I will be working on this year's span. It consists of a humanoid character which learns to do things being operated by Machine Learning techniques. I decided I would do the most aspects from scratch, and thus the character (+ animations) and apartment's structure are completely self-made (in Blender).

Actions the character is currently capable of:
- walking
- standing
- grabbing objects
- recognizing objects after a supervised training session

Short highlight of walking and grabbing objects:

![Demo](https://github.com/BogdanPolitic/Demos/blob/main/Learning_Individual_demo_0.gif?raw=true)

<h3> Implementation details </h3>

The apartment and character are made in Blender, as stated above. 

The apartment is nothing more than a symetrical placement of walls and floors, with self-applied textures. 

The character's body is made using mesh extrudes with a subdivision surface modifier permanently on and some final sculpting after applying the modifier. The hair and glasses were joined into the body mesh afterwards. I had manually applied the body weights by a long and boring session of weight painting, and then I set Inverse Kinematics (IK) for each hand and each leg. There's a few basic animations I made for this character: idle, walking and grabbing objects. I made those using Blender timeline. Because in the process of exporting to Unity, the object loses its inverse kinematics connections, the grabbing animation is done by linearly interpolating some IK grabbing animations (from Blender) using a Blend Tree.

