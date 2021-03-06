﻿using System;
using System.IO;
using System.Linq;
using kOS.Safe;
using kOS.Safe.Utilities;
using kOS.Safe.Persistence;
using System.Collections.Generic;

namespace kOS.Cli.IO
{
    /// <summary>
    /// CLI volume directory to be used for a custom Kerboscript compilation and deployment.
    /// </summary>
    [KOSNomenclature("CliVolumeDirectory")]
    public class CliVolumeDirectory : VolumeDirectory
    {
        private readonly CliVolume volume;
        private readonly string volumePath;

        public CliVolumeDirectory(CliVolume volume, VolumePath path) : base(volume, path)
        {
            this.volume = volume;
            this.volumePath = volume.GetArchivePath(path);
        }

        public override IDictionary<string, VolumeItem> List()
        {
            string[] files = Directory.GetFiles(volumePath);
            var filterHid = files.Where(f => (File.GetAttributes(f) & FileAttributes.Hidden) != 0);
            var filterSys = files.Where(f => (File.GetAttributes(f) & FileAttributes.System) != 0);
            var visFiles = files.Except(filterSys).Except(filterHid).ToArray();
            string[] directories = Directory.GetDirectories(volumePath);

            Array.Sort(directories);
            Array.Sort(visFiles);

            var result = new Dictionary<string, VolumeItem>();

            foreach (string directory in directories)
            {
                string directoryName = System.IO.Path.GetFileName(directory);
                result.Add(directoryName, new CliVolumeDirectory(volume, VolumePath.FromString(directoryName, Path)));
            }

            foreach (string file in visFiles)
            {
                string fileName = System.IO.Path.GetFileName(file);
                result.Add(fileName, new CliVolumeFile(volume, new FileInfo(file), VolumePath.FromString(fileName, Path)));
            }

            return result;
        }

        public override int Size {
            get {
                return List().Values.Aggregate(0, (acc, x) => acc + x.Size);
            }
        }
    }
}
