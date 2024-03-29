﻿using JukeboxLibrary.Helpers;
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary.MachineParts;

public class SongList : ISongList
{
    public List<ISong> SongCollection { get; set; } = new();

    public void Build(ISongSources sources, string selectedPattern)
    {
        SongCollection = FileSystemParser.ParseFileSystem(sources, selectedPattern);
    }
}