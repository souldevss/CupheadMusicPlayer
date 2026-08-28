using System;
using System.Collections.Generic;

namespace CupheadMusicPlayer.Engine
{
    public static class MusicMappings
    {
        public static readonly Dictionary<string, string> Mappings =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "scene_level_veggies", "root.mp3" },
            { "scene_level_slime", "goopy.mp3" },
            { "scene_level_frogs", "ribby.mp3" },
            { "scene_level_flower", "cagney.mp3" },
            { "scene_level_baroness", "baroness.mp3" },
            { "scene_level_flying_bird", "wally.mp3" },
            { "scene_level_flying_genie", "djimmi.mp3" },
            { "scene_level_clown", "beppi.mp3" },
            { "scene_level_dragon", "grim.mp3" },
            { "scene_level_bee", "rumor.mp3" },
            { "scene_level_robot", "kahl.mp3" },
            { "scene_level_sally_stage_play", "sally.mp3" },
            { "scene_level_mouse", "werner.mp3" },
            { "scene_level_pirate", "captain.mp3" },
            { "scene_level_flying_,mermaid", "cala.mp3" },
            { "scene_level_train", "phantom.mp3" },
            { "scene_level_devil", "devil.mp3" },
            { "scene_map_world_1", "isle1.mp3" },
            { "scene_map_world_2", "isle2.mp3" },
            { "scene_map_world_3", "isle3.mp3" },
            { "scene_map_world_4", "islehell.mp3" },
            { "scene_map_world_DLC", "isleDLC.mp3" },
            { "scene_level_dice_palace_main", "king.mp3" },
            { "scene_level_dice_palace_domino", "king.mp3" },
            { "scene_level_dice_palace_chips", "king.mp3" },
            { "scene_level_dice_palace_cigar", "king.mp3" },
            { "scene_level_dice_palace_booze", "king.mp3" },
            { "scene_level_dice_palace_roulette", "king.mp3" },
            { "scene_level_dice_palace_rabbit", "king.mp3" },
            { "scene_level_dice_palace_flying_horse", "king.mp3" },
            { "scene_level_dice_palace_memory", "king.mp3" },
            { "scene_level_dice_palace_eight_ball", "king.mp3" },
            { "scene_level_old_man", "glumstone.mp3" },
            { "scene_level_snow_cult", "mortimer.mp3" },
            { "scene_level_airplane", "howling.mp3" },
            { "scene_level_flying_cowboy", "esther.mp3" },
            { "scene_level_rum_runners", "moonshine.mp3" },
            { "scene_level_saltbaker", "chef.mp3" },
            { "scene_win", "scoreboard.mp3" },
            { "scene_shop", "shop.mp3" },
            { "scene_slot_select", "title.mp3" },
            { "scene_shop_dlc", "shopdlc.mp3" },
            { "scene_level_house_elder_kettle", "elder.mp3" },
            { "scene_level_platforming_1_1F", "forest.mp3" },
            { "scene_level_platforming_1_2F", "treetop.mp3" },
            { "scene_level_platforming_2_1F", "funfair.mp3" },
            { "scene_level_platforming_2_2F", "funhouse.mp3" },
            { "scene_level_platforming_3_1F", "perilous.mp3" },
            { "scene_level_platforming_3_2F", "rugged.mp3" },
            { "scene_level_mausoleum", "mausoleum.mp3" }
        };
    }
}
