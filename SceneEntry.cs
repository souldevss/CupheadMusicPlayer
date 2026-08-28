using System.Collections.Generic;

namespace CupheadMusicPlayer
{
    /// <summary>
    /// A user-defined scene entry: pick a friendly name (which carries the raw
    /// scene IDs), then optionally override the file and volume.
    /// </summary>
    public class SceneEntry
    {
        /// <summary>Friendly display name from <see cref="Engine.SceneCatalog"/>.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Raw scene IDs attached to this entry (from the catalog).</summary>
        public List<string> SceneIds { get; set; } = new List<string>();

        /// <summary>Full path to the music file for this scene.</summary>
        public string File { get; set; } = string.Empty;

        /// <summary>0-100 per-scene volume. -1 = use global volume.</summary>
        public int Volume { get; set; } = -1;
    }
}
