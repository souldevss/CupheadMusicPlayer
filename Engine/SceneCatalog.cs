using System.Collections.Generic;

namespace CupheadMusicPlayer.Engine
{
    public static class SceneCatalog
    {
        public class SceneName
        {
            public string Name { get; set; }
            public List<string> SceneIds { get; set; } = new List<string>();

            public override string ToString() => Name;
        }

        public static readonly List<SceneName> All = new List<SceneName>
        {
            // Isles
            new SceneName { Name = "Inkwell Isle 1", SceneIds = { "scene_map_world_1" } },
            new SceneName { Name = "Inkwell Isle 2", SceneIds = { "scene_map_world_2" } },
            new SceneName { Name = "Inkwell Isle 3", SceneIds = { "scene_map_world_3" } },
            new SceneName { Name = "Inkwell Hell", SceneIds = { "scene_map_world_4" } },
            new SceneName { Name = "Inkwell Isle 4 (DLC)", SceneIds = { "scene_map_world_DLC" } },

            // Run & Gun / Platforming
            new SceneName { Name = "Forest Follies", SceneIds = { "scene_level_platforming_1_1F" } },
            new SceneName { Name = "Treetop Trouble", SceneIds = { "scene_level_platforming_1_2F" } },
            new SceneName { Name = "Funfair Fever", SceneIds = { "scene_level_platforming_2_1F" } },
            new SceneName { Name = "Funhouse Frazzle", SceneIds = { "scene_level_platforming_2_2F" } },
            new SceneName { Name = "Perilous Piers", SceneIds = { "scene_level_platforming_3_1F" } },
            new SceneName { Name = "Rugged Ridge", SceneIds = { "scene_level_platforming_3_2F" } },

            // Bosses - Isle 1
            new SceneName { Name = "The Root Pack", SceneIds = { "scene_level_veggies" } },
            new SceneName { Name = "Goopy le Grande", SceneIds = { "scene_level_slime" } },
            new SceneName { Name = "Ribby & Croaks", SceneIds = { "scene_level_frogs" } },
            new SceneName { Name = "Cagney Carnation", SceneIds = { "scene_level_flower" } },

            // Bosses - Isle 2
            new SceneName { Name = "Baroness Von Bon Bon", SceneIds = { "scene_level_baroness" } },
            new SceneName { Name = "Wally Warbles", SceneIds = { "scene_level_flying_bird" } },
            new SceneName { Name = "Djimmi the Great", SceneIds = { "scene_level_flying_genie" } },
            new SceneName { Name = "Beppi the Clown", SceneIds = { "scene_level_clown" } },
            new SceneName { Name = "Grim Matchstick", SceneIds = { "scene_level_dragon" } },

            // Bosses - Isle 3
            new SceneName { Name = "Rumor Honeybottoms", SceneIds = { "scene_level_bee" } },
            new SceneName { Name = "Dr. Kahl's Robot", SceneIds = { "scene_level_robot" } },
            new SceneName { Name = "Sally Stageplay", SceneIds = { "scene_level_sally_stage_play" } },
            new SceneName { Name = "Werner Werman", SceneIds = { "scene_level_mouse" } },
            new SceneName { Name = "Captain Brineybeard", SceneIds = { "scene_level_pirate" } },
            new SceneName { Name = "Cala Maria", SceneIds = { "scene_level_flying_,mermaid" } },
            new SceneName { Name = "Phantom Express", SceneIds = { "scene_level_train" } },

            // Isle 4 / King Dice
            new SceneName { Name = "King Dice", SceneIds = {
                "scene_level_dice_palace_main", "scene_level_dice_palace_domino",
                "scene_level_dice_palace_chips", "scene_level_dice_palace_cigar",
                "scene_level_dice_palace_booze", "scene_level_dice_palace_roulette",
                "scene_level_dice_palace_rabbit", "scene_level_dice_palace_flying_horse",
                "scene_level_dice_palace_memory", "scene_level_dice_palace_eight_ball" } },
            new SceneName { Name = "The Devil", SceneIds = { "scene_level_devil" } },

            // DLC bosses
            new SceneName { Name = "Glumstone the Giant", SceneIds = { "scene_level_old_man" } },
            new SceneName { Name = "Mortimer Freeze", SceneIds = { "scene_level_snow_cult" } },
            new SceneName { Name = "Howling Aces", SceneIds = { "scene_level_airplane" } },
            new SceneName { Name = "Esther Winchester", SceneIds = { "scene_level_flying_cowboy" } },
            new SceneName { Name = "Moonshine Mob", SceneIds = { "scene_level_rum_runners" } },
            new SceneName { Name = "Chef Saltbaker", SceneIds = { "scene_level_saltbaker" } },

            // Misc
            new SceneName { Name = "Title Screen", SceneIds = { "scene_slot_select" } },
            new SceneName { Name = "Scoreboard (Win)", SceneIds = { "scene_win" } },
            new SceneName { Name = "Shop", SceneIds = { "scene_shop" } },
            new SceneName { Name = "Shop (DLC)", SceneIds = { "scene_shop_dlc" } },
            new SceneName { Name = "Elder Kettle", SceneIds = { "scene_level_house_elder_kettle" } },
            new SceneName { Name = "Mausoleum", SceneIds = { "scene_level_mausoleum" } },
        };
    }
}
