# Jukebox

<a href="https://docs.microsoft.com/en-us/dotnet/csharp/"><img src="https://raw.githubusercontent.com/Wycott/RepositoryResources/main/Graphics/language-csharp.svg" title="Language C#" alt="Language C#"></a>
<a href="https://github.com/Wycott/RepositoryResources/blob/main/REPOTYPE.md"><img src="https://raw.githubusercontent.com/Wycott/RepositoryResources/main/Graphics/repo%20type-Application-yellow.svg" title="Application" alt="Application"></a>
<img src="https://img.shields.io/badge/.NET_Core-8-red">

## Operation

The Jukebox will parse selected directories (currently hardcoded in **SongSources.cs**) looking for music files (.mp3) of the given pattern. If one is found, it gives the user the opportunity to play it or skip to the next file that matches the pattern.

An example of a pattern would be:

>ivi pra

This pattern would match, say: "Living on a prayer.mp3".

N.B. the patterns have to be in order. This wouldn't match:

>pra ivi

## Play Any Song By Artist

If you just want to play a song by given artist you can use a wild card followed by the artist name stub separated by @ so this:

>*@Jovi

Would match  any song by Bon Jovi. N.B. the results returned each time are randomised.

## History

The original version of this program was written in 2014.

---

*Last updated: 4 December 2023*
