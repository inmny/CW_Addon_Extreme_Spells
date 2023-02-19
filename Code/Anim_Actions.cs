using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cultivation_Way;
using Cultivation_Way.Utils;
using Cultivation_Way.Animation;
using UnityEngine;
using Cultivation_Way.Extensions;
namespace Extreme_Spells.Code
{
    internal static class Anim_Actions
    {
        public static void extreme_void_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            if (cur_frame_idx == 17) anim.cur_frame_idx = 6;
            WorldTile center = MapBox.instance.GetTile((int)src_vec.x, (int)src_vec.y);
            if (center == null || anim.src_object == null || !anim.src_object.base_data.alive) { anim.force_stop(false); return; }


            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(center, (int)Mathf.Log10(anim.cost_for_spell / 1000) * 12);

            List<BaseSimObject> enemies = CW_SpellHelper.find_enemies_in_tiles(tiles, anim.src_object.kingdom);
            float dist;
            foreach (BaseSimObject enemy in enemies)
            {
                dist = Toolbox.DistVec2Float(dst_vec, enemy.currentPosition);
                CW_SpellHelper.cause_damage_to_target(anim.src_object, enemy, anim.cost_for_spell / ((1 + dist)*10000f));
                if (enemy.objectType != MapObjectType.Actor || enemy==  anim.src_object) continue;
                if (!enemy.base_data.alive)
                {
                    // 夺取灵气
                    anim.end_froze_time += CW_Utils_Others.get_raw_wakan(((CW_Actor)enemy).cw_status.wakan, ((CW_Actor)enemy).cw_status.wakan_level);
                    ((CW_Actor)enemy).cw_status.wakan = 0;
                }
                else
                {
                    // 拖拽
                    ((CW_Actor)enemy).add_force((dst_vec.x-enemy.currentPosition.x) * 0.1f, (dst_vec.y-enemy.currentPosition.y) * 0.1f, 0.05f);
                }
                

            }
            foreach(WorldTile tile in tiles)
            {
                CW_MapChunk chunk = tile.get_cw_chunk();
                if(Toolbox.randomChance(0.01f))MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);
                if (chunk == null) continue;
                anim.end_froze_time += CW_Utils_Others.get_raw_wakan(chunk.wakan * 0.05f, chunk.wakan_level);
                chunk.wakan *= 0.95f;
                chunk.update(true);
            }
            Cultivation_Way.World_Data.instance.map_chunk_manager.force_update_mapmode();
        }
        public static void extreme_void_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            WorldTile center = MapBox.instance.GetTile((int)src_vec.x, (int)src_vec.y);
            if (center == null || anim.src_object == null || !anim.src_object.base_data.alive) return;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(center, (int)Mathf.Log10(anim.cost_for_spell / 1000) * 12);
            foreach(WorldTile tile in tiles)
            {
                MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);
            }
            CW_Actor actor = (CW_Actor)anim.src_object;
            actor.cw_status.wakan += CW_Utils_Others.compress_raw_wakan(anim.end_froze_time, actor.cw_status.wakan_level);
            Debug.Log(CW_Utils_Others.compress_raw_wakan(anim.end_froze_time, actor.cw_status.wakan_level));
            for(int i=0;i<20;i++)actor.check_level_up();
        }

        internal static void extreme_meteorolite_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            throw new NotImplementedException();
        }

        internal static void extreme_meteorolite_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            throw new NotImplementedException();
        }
    }
}
