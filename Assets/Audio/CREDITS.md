# Audio credits

Clips live in this folder. Unity writes the `.meta` import settings the first time it imports them. Do not hand-author those metas.

Volumes in the game are set on the Audio object. This file only records where each clip came from.

These files were copied from the public project [grayboxgalore](https://github.com/Leemu645/grayboxgalore), which already tracked the CC0 sources below. They are not linked at runtime. The copies in this folder are the ones the game loads.

## CC0 recordings

Licence for every file in this section: [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/).

### Kenney — 50 RPG sound effects

Author: Kenney Vleugels (www.kenney.nl). Credit is appreciated and not required.

- Page: https://opengameart.org/content/50-rpg-sound-effects
- File: https://opengameart.org/sites/default/files/RPGsounds_Kenney.zip

| Clip in this folder | File inside the zip |
| --- | --- |
| `creak_01.ogg` | `OGG/creak1.ogg` |
| `creak_02.ogg` | `OGG/creak2.ogg` |
| `closet_open_01.ogg` | `OGG/doorOpen_1.ogg` |
| `closet_open_02.ogg` | `OGG/doorOpen_2.ogg` |
| `key_jingle.ogg` | `OGG/handleCoins.ogg` |

Door creaks use the closet opens and the creaks together.

### rubberduck — 100 CC0 SFX

Author: rubberduck.

- Page: https://opengameart.org/content/100-cc0-sfx
- File: https://opengameart.org/sites/default/files/100-CC0-SFX_0.zip

| Clip in this folder | File inside the zip |
| --- | --- |
| `land.ogg` | `wooden_02.ogg` |
| `flashlight_on.ogg` | `switch_01.ogg` |
| `flashlight_off.ogg` | `switch_02.ogg` |
| `lock_unlock_01.ogg` | `key_open_01.ogg` |

### TinyWorlds — steps on wood

Author: TinyWorlds.

- Page: https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud
- File: https://opengameart.org/sites/default/files/%5Bkdd%5DDifferentSteps_0.zip

| Clip in this folder | File inside the zip |
| --- | --- |
| `footstep_wood_01.ogg` | `wood01.ogg` |
| `footstep_wood_02.ogg` | `wood02.ogg` |
| `footstep_wood_03.ogg` | `wood03.ogg` |

### rubberduck — 100 CC0 SFX #2

Author: rubberduck.

- Page: https://opengameart.org/content/100-cc0-sfx-2
- File: https://opengameart.org/sites/default/files/sfx_100_v2.zip

| Clip in this folder | File inside the zip |
| --- | --- |
| `footstep_wood_04.ogg` | `sfx100v2_footstep_wood_01.ogg` |
| `jump.ogg` | `sfx100v2_air_03.ogg` |
| `pickup.ogg` | `sfx100v2_items_01.ogg` |
| `lock_unlock_02.ogg` | `sfx100v2_lock_open_01.ogg` |

Kenney.nl itself returned 403 from the machine that first downloaded these, so the Kenney clips are the OpenGameArt reupload.

## Synthesized

These were generated in Python (mono, 22050 Hz, 16-bit PCM). They are not recordings. Replace them when a suitable CC0 file is available.

| Clip | What it stands in for |
| --- | --- |
| `slot_fill.wav` | A short chime when a relic fills a slot on the front door. Pitch rises a little with each slot. |
| `ambient_mansion.wav` | A quiet drone with two dull ticks, looped in 2D on the Audio object. |
| `lock_rattle.wav` | A wrong item in a lock, or a locked door. Synthesized in grayboxgalore and copied here. |
