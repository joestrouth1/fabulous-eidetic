# fabulous-eidetic

A work-in-progress memory game written in F# with Fabulous & Avalonia.

For now, it's reproduces an issue where:

- Given the app renders a VStack of content while running
- and the app is running on Windows on ARM
- When I hide the last button in a row
- Then the row takes up less height
- And the height of the VStack shrinks
- And the dot of the 'i' in the top TextBlock is visible in multiple locations

![]("/idot-issue.mp4")

Inspired by [Eidetic](https://github.com/hathibelagal-dev/Eidetic-Memory-Trainer). From the original app's description on F-Droid:

> The chimp test, also known as the Ayumu memory test, is a test designed to assess working memory.
>
> It was popularized by a study conducted at the Primate Research Institute of Kyoto University in Japan, where chimpanzees demonstrated remarkable short-term memory abilities.
>
> Chimps seem to, effectively, have a photographic memory (eidetic memory).
>
> Untrained humans generally perform significantly worse than chimpanzees on the chimp test. So, this app trains you. The hope is that we can eventually match or beat chimp records.

## Gameplay

- Numbers from 1 to 9 are displayed randomly

- You must memorize the positions of the numbers

- Once you press 1, all buttons display a ?

  - _not implemented_

- You need to then use your memory to press 2, 3,..., 9

- It's game over if you press the wrong button thrice
  - _not implemented_
