using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using JukeboxLibrary.Interfaces;
using JukeboxLibrary.MachineParts;

namespace JukeboxLibrary.Helpers
{
    public static class FileSystemParser
    {
        //public static void

        public static List<ISong> ParseFileSystem(ISongSources mediaDrives, string huntString)
        {
            var retVal = new List<ISong>();

            //const string root = @"e:\";

            //if (string.IsNullOrEmpty(ParseLine)) return;
            // TODO: Maybe do this elsewhere
            huntString = PreparePattern(huntString);

            foreach (var drive in mediaDrives.Sources)
            {

                foreach (var possibleSongDirectory in Directory.GetDirectories(drive))
                {
                    try
                    {
                        foreach (var possibleSongFile in Directory.GetFiles(possibleSongDirectory, huntString, SearchOption.AllDirectories))
                        {
                            if (ExtensionsOk(possibleSongFile))
                            //if (ExtensionsOk(dfile) && VersionRequired(dfile))
                            {
                                //FileName = dfile;
                                //break;
                                // TODO: Do something
                                retVal.Add(new Song() { FullPath = possibleSongFile});
                            }
                        }

                        //if (!string.IsNullOrEmpty(FileName))
                        //{
                        //    Next();
                        //}
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Standard...
                    }
                }
            }

            return retVal;
            //NothingDoing();
        }

        private static bool ExtensionsOk(string candidate)
        {
            var extensions = new List<string> { ".mp3", ".m4a" };

            foreach (var s in extensions)
            {
                if (candidate.Contains(s)) return true;
            }

            return false;
        }

        private static string PreparePattern(string initialPattern)
        {
            return "*" + initialPattern.Replace(' ', '*') + "*";
        }
    }
}
