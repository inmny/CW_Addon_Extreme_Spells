using NCMS;
using UnityEngine;
using Cultivation_Way;
using Cultivation_Way.Actions;
using Cultivation_Way.Library;
using Cultivation_Way.Animation;
using System;

namespace Extreme_Spells.Code
{
	[ModEntry]
	public class Addon_Main_Class : CW_Addon
	{
		public override void awake(){
			// 不要在此处添加代码，除非你知道你在做什么
			// DO NOT code here.
			load_mod_info(System.Type.GetType("Mod"));
		}
		public override void initialize(){
			Log("添加超级法术!");
			// 在这里初始化模组内容
			// Initalize your mod content here
			add_spells();
		}

        private void add_spells()
        {
            /**
			gold_sword_a();
			gold_sword_b();
			extreme_fire();
			extreme_water();
			extreme_tornado();
            extreme_meteorolite();
            */
            extreme_void();
        }
        // 吞噬天地
        private void extreme_void()
        {
            CW_AnimationSetting anim_setting = new CW_AnimationSetting();
            anim_setting.frame_interval = 0.05f;
            anim_setting.loop_limit_type = AnimationLoopLimitType.TIME_LIMIT;
            anim_setting.loop_time_limit = 6f;
            anim_setting.layer_name = "EffectsBack";
            anim_setting.point_to_dst = false;
            anim_setting.anim_froze_frame_idx = -1;
            anim_setting.trace_grad = 20;
            anim_setting.frame_action = Anim_Actions.extreme_void_frame;
            anim_setting.end_action = Anim_Actions.extreme_void_end;
            anim_setting.set_trace(AnimationTraceType.NONE);
            CW_EffectManager.instance.load_as_controller("extreme_void_anim", "effects/anti_matter/", anim_limit: 10, controller_setting: anim_setting, base_scale: 0.5f);

            CW_Asset_Spell spell = new CW_Asset_Spell(
                id: "extreme_void", anim_id: "extreme_void_anim",
                new CW_Element(new int[] { 20, 20, 20, 20, 20 }), element_type_limit: null,
                rarity: 1, free_val: 1, cost: 0.98f, min_cost: 1000,
                learn_level: 295, cast_level: 10, can_get_by_random: true,
                cultisys_black_or_white_list: true, cultisys_list: null,
                banned_races: null,
                target_type: CW_Spell_Target_Type.ACTOR,
                target_camp: CW_Spell_Target_Camp.ENEMY,
                triger_type: CW_Spell_Triger_Type.ATTACK,
                anim_type: CW_Spell_Animation_Type.CUSTOM,
                damage_action: null,
                anim_action: null,
                spell_action: Code.Spell_Actions.extreme_void_spell_action,
                check_and_cost_action: Cultivation_Way.Actions.CW_SpellAction_Cost.default_check_and_cost
                );
            spell.add_tag(CW_Spell_Tag.IMMORTAL);
            spell.add_tag(CW_Spell_Tag.ATTACK);
            add_spell(spell);
        }
        // 地爆天星
        private void extreme_meteorolite()
        {
            CW_AnimationSetting anim_setting = new CW_AnimationSetting();
            anim_setting.frame_interval = 0.05f;
            anim_setting.loop_limit_type = AnimationLoopLimitType.TIME_LIMIT;
            anim_setting.loop_time_limit = 6f;
            anim_setting.layer_name = "Objects";
            //anim_setting.always_roll = true;
            anim_setting.point_to_dst = false;
            anim_setting.anim_froze_frame_idx = -1;
            anim_setting.trace_grad = 20;
            anim_setting.frame_action = Anim_Actions.extreme_meteorolite_frame;
            anim_setting.end_action = Anim_Actions.extreme_meteorolite_end;
            anim_setting.set_trace(AnimationTraceType.TRACK);
            //CW_EffectManager.instance.load_as_controller("")
        }
        // 飓风领域
        private void extreme_tornado()
        {
            throw new NotImplementedException();
        }
        // 天河之水
        private void extreme_water()
        {
            throw new NotImplementedException();
        }
        // 天火焚世
        private void extreme_fire()
        {
            throw new NotImplementedException();
        }
        // 金光剑阵
        private void gold_sword_b()
        {
            throw new NotImplementedException();
        }
        // 万剑归宗
        private void gold_sword_a()
        {
            throw new NotImplementedException();
        }

        private void add_spell(CW_Asset_Spell spell)
        {
            try
            {
				CW_Library_Manager.instance.spells.add(spell);
				Log("成功添加法术'{0}'", LocalizedTextManager.getText("spell_" + spell.id));
			}
			catch(Exception e)
            {
				Error("添加法术'{0}'失败，错误信息如下:", spell.id);
				Error(e.Message);
				Error(e.Source);
            }
        }
    }
}