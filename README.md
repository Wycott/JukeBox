# Jukebox

<a href="https://docs.microsoft.com/en-us/dotnet/csharp/"><img src="https://raw.githubusercontent.com/Wycott/RepositoryResources/main/Graphics/language-csharp.svg" title="Language C#" alt="Language C#"></a>
<a href="https://en.wikipedia.org/wiki/BSD_licenses#3-clause_license_(%22BSD_License_2.0%22,_%22Revised_BSD_License%22,_%22New_BSD_License%22,_or_%22Modified_BSD_License%22)"><img src="https://raw.githubusercontent.com/Wycott/RepositoryResources/main/Graphics/license-BSD--3-green.svg" title="BSD-3" alt="BSD-3"></a>
<a href="https://github.com/Wycott/RepositoryResources/blob/main/REPOTYPE.md"><img src="https://raw.githubusercontent.com/Wycott/RepositoryResources/main/Graphics/repo%20type-Application-yellow.svg" title="Application" alt="Application"></a>

## Operation

The Jukebox will parse selected directories (currently hardcoded in **SongSources.cs**) looking for music files (.mp3 & .m4a) of the given pattern. If one is found, it gives the user the opportunity to play it or skip to the next file that matches the pattern.

An example of a pattern would be:

>ivi pra

This pattern would match, say: "Living on a prayer.mp3".

N.B. the patterns have to be in order. This wouldn't match:

>pra ivi

## History

The original version of this program was written in 2014.

_Last updated: 30 June 2022_
