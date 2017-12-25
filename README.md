# NaiveViableLookingNBody2DSystemGenerator
A 2D n-body system generator that looks and feels viable.

It was made with Unity 2017.
You can import the files directly into an existing Unity project.
Launch the scene to test it out.

## Naive
This generator is based on the idea that the center of mass of a two-body system is at a shared focus point of the two bodies.
An ellipse is made of a center, two foci and an eccentricy that is responsible for the flatness of the ellipse and the positions of the foci. An elliptical orbit means that one of the foci is actually the center of mass of the orbit. And in a two-body system, the bodies share one of their foci, which is placed at the center of mass of the system.

The naive approach I chose here is to apply this property to n-body systems, knowing this is wrong as there is no analytical solution for n-body systems. But I think this is actually not invalid but just way too restrictive. This approach probably prevents certain configurations when n is greater than two.

## Viable looking
The system generated usually looks like it is viable but I have not yet tested the generated systems under the use of the universal law of gravitation. The planets have their own mass and their own orbit.

## N-body
This generator can be used to create a planetary system with one, two, three or more bodies.

## 2D
It is a 2D planetary system generator but adding the 3rd dimension should not be a problem I think.

## Algorithm
- Generate n bodies by randomly assigning them a mass.
- Distribute the n bodies on the screen (all bodies are at the apoapsis).
- Find the center of mass of the system formed by the n bodies.
- Generate an elliptical orbit for each body.
- Move each ellipse's center so that all foci are at the center of mass of the system.