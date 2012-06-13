# Disentanglement

This is a solver for [Thinkfun's Gordian's Knot puzzle](http://www.thinkfun.com/gordiansknot) which runs on
Windows and Windows Phone.  It uses a [Portable Class Library][1] to share the solver code between the different
platforms.

Channel 9's [Visual Studio Toolbox: Portable Class Libraries](http://channel9.msdn.com/Shows/Visual-Studio-Toolbox/Visual-Studio-ToolboxPortable-Class-Libraries)
show covers an overview of Portable Class Libraries, how to use them, and a demo of this and several other apps.

When run, the program will start trying to solve the puzzle.  While this is happening the "next" command will display
the current solver state.  When it is finished solving the background will change from black to blue, and the "next"
command will advance through the steps in the solution to the puzzle it found.

  [1]: http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx

# Windows Controls

- Spacebar - Show next solution step if solver has finished finding solution, otherwise show current solver state
- B - Show previous solution step (or stop showing current solver state)
- Arrow keys - Rotate view
- R - Reset view
- 1 to 6 - Show / hide individual puzzle pieces
- ESC - Exit
- F11 - Full screen
- Page up / Page down - Zoom in / out

# Windows Phone Controls

Drag around on the screen to rotate the puzzle.  The bottom of the screen is divided into 3 "invisible buttons".
The one on the right advances to the next solution step or shows the current solver state.  The middle one
resets the view, and the one on the left shows the previous step or stops showing the current solver state.