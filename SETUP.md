# Gray Box Mansion

A first-person walk through a dark two-storey house. Four relics open the front door.

This project uses **Unity 6.6** (`6000.6.7f1`) and the **Input System** package only. Project Settings → Player → Active Input Handling is "Input System Package". Do not switch it back to the old Input Manager. `Input.GetAxis` will throw.

The mansion is not saved in the project until you build it. An editor script creates the whole scene, so you can run it again any time and it will overwrite the old one.

## Play

1. Open this folder in Unity `6000.6.7f1`.
2. Wait until the Package Manager finishes and the Console has no compile errors. The first open also downloads AI Navigation, which the scene uses for a NavMesh.
3. If you are in Play mode, click Play again to stop.
4. In the menu bar, choose **Tools → Gray Box Mansion → Build Mansion Scene**.
5. If Unity asks to save the scene you already have open, save it or choose not to. The builder then makes a new scene.
6. When the dialog says the mansion is built, you are looking at `Assets/Scenes/Mansion`. It is also added to **File → Build Settings**.
7. Click **Play**, then click the Game view once so the mouse and keyboard reach the game.

You start in the foyer, looking at the front door. The house is lit by a few warm lamps. Corners are dark on purpose.

Running the menu again replaces `Assets/Scenes/Mansion`. It does not delete the scripts.

## Controls

| Action | Keyboard and mouse | Gamepad |
| --- | --- | --- |
| Move | W A S D, or the arrow keys | Left stick |
| Look | Mouse, while the cursor is locked | Right stick |
| Jump | Space | South button (A on Xbox, Cross on PlayStation) |
| Sprint | Hold Left Shift | Click the left stick |
| Interact | E, or left click | North button (Y on Xbox, Triangle on PlayStation) |
| Hold an interaction | Hold E until the bar fills | Hold the north button |
| Flashlight | F, after you pick one up | Right bumper |
| Choose an item | Number keys 1–8, or the scroll wheel | D-pad left and right |
| Release the cursor | Escape | Start |
| Lock the cursor again | Click in the Game view | |

The cursor locks and hides when you press Play. Press Escape if you need the mouse back for the editor.

The flashlight beam stays on when you select something else. Only the little model in the corner of the view hides. Press F again to turn the beam off.

Things you can use light up warm when you look at them. The line under the crosshair is the prompt.

## Spoiler: how to get out

The front door lock has four slots. Each relic fills one slot, in any order. Nothing you need is locked behind the thing it opens.

1. The flashlight is on the console table to the right of where you start.
2. **Silver locket.** Living room, west of the foyer. A chest against the north wall. The drawer is stuck. Hold E.
3. **Bedroom key.** Kitchen, south-west, on the counter. It opens the master bedroom upstairs.
4. **Study key.** Upstairs, in the nursery (west end of the hall), on the floor in front of the crib. The bow is stamped with a book. It opens the study, east of the foyer.
5. **Music box.** On the desk in the study.
6. **Cracked portrait.** Master bedroom, upstairs on the east side, in front of the dresser. The vestibule door from the hall is open. The bedroom door is the locked one.
7. **Candelabra.** Storage room at the south-east of the upstairs hall. Hold E on the cupboard.
8. Stand at the front door and look at the lock. Select a relic (number keys or the scroll wheel) and press E. Do that for all four. The door opens, and the screen says **You escaped**. **Play Again** reloads the mansion. R does the same.

You can close a door you opened. It stays unlocked. Keys are not used up.

## The house

Ground floor, front of the house to the back:

- **Foyer** in the middle, with the front door and a double-height ceiling. The stairs start at the back of the foyer.
- **Living room** to the west, then **dining room**, then **kitchen**.
- **Study** to the east of the foyer. Locked. South of that is an open **gallery**, then the back of the house joins the kitchen through the center hall.

Upstairs:

- The stairs land in a **hallway** that runs the width of the house.
- **Nursery** and **bathroom** are on the west.
- **Master bedroom** and a small **storage** room are on the east. Storage has a hatch in the ceiling. It does not open.
- Low walls beside the stairwell keep you from walking off the upstairs floor into the foyer.

The stair you walk on is a ramp the CharacterController can climb (about 37 degrees, under the slope limit). The boxes that look like steps are only the look of the stair.

## Sound

Rebuild the mansion after pulling audio changes. An older saved `Mansion` scene does not contain the audio object.

Select **Audio** in the hierarchy. **Game Audio → Master Volume** scales every sound, including the room tone. It starts at `0.85`.

Where the files came from is in `Assets/Audio/CREDITS.md`. Unity creates the `.meta` files for those clips on import. You do not need to author them.

## If something is wrong

- **The menu item is missing.** The scripts did not compile. Read the Console. The usual cause is that AI Navigation has not finished importing.
- **WASD does nothing.** Click the Game view so it has focus. The cursor must be locked. Escape unlocks it on purpose.
- **The mouse does not look.** Click the Game view.
- **The player falls forever.** Build the scene again from the menu. The floor colliders are created by that script.
- **The Console says there are 2 audio listeners.** The built scene should have one, on the player's camera. Delete any extra camera you added by hand.
- **Play Again does nothing.** The scene has to be named `Mansion` and listed in Build Settings. Run the menu item again.
