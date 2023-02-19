using Cultivation_Way.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cultivation_Way.Animation;
using UnityEngine;

namespace Extreme_Spells.Code
{
    internal static class Spell_Actions
    {
        public static void extreme_void_spell_action(CW_Asset_Spell spell_asset, BaseSimObject pUser, BaseSimObject pTarget, WorldTile pTargetTile, float cost)
        {
            if (pUser == null || !pUser.base_data.alive) return;
            CW_SpriteAnimation anim = CW_EffectManager.instance.spawn_anim(spell_asset.anim_id, pTargetTile, pTargetTile, pUser, pTarget, Mathf.Log10(cost / 1000));
            if (anim == null) return;
            anim.cost_for_spell = cost;
        }
    }
}
