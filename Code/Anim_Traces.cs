using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cultivation_Way.Animation;
namespace Extreme_Spells.Code
{
    internal static class Anim_Traces
    {
        public static void trace_gold_sword_b(ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim, ref float delta_x, ref float delta_y)
        {
            float cur_to_dst_dist = Toolbox.DistVec2Float(anim.gameObject.transform.localPosition, dst_vec);

            CW_AnimationSetting setting = anim.get_setting(false);

            float trace_grad = setting.trace_grad * 10 / (10 + cur_to_dst_dist);


            delta_x = Mathf.Sign(dst_vec.x - anim.gameObject.transform.localPosition.x) * trace_grad;

            delta_y = Mathf.Sign(dst_vec.y - anim.gameObject.transform.localPosition.y) * trace_grad;

            if (Mathf.Abs(delta_x) * anim.cur_elapsed > Mathf.Abs(dst_vec.x - anim.gameObject.transform.localPosition.x)) delta_x = (dst_vec.x - anim.gameObject.transform.localPosition.x) / anim.cur_elapsed;
            else { anim.change_scale(1.005f); }

            if (Mathf.Abs(delta_y) * anim.cur_elapsed > Mathf.Abs(dst_vec.y - anim.gameObject.transform.localPosition.y)) delta_y = (dst_vec.y - anim.gameObject.transform.localPosition.y) / anim.cur_elapsed;
            else { anim.change_scale(1.005f); }

            return;
        }

        internal static void trace_gold_sword_a(ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim, ref float delta_x, ref float delta_y)
        {
            float cur_to_dst_dist = Toolbox.DistVec2Float(anim.gameObject.transform.localPosition, dst_vec);

            CW_AnimationSetting setting = anim.get_setting(false);

            float trace_grad = setting.trace_grad * 10 / (10 + cur_to_dst_dist) * Toolbox.randomFloat(0.3f,1.5f) / cur_to_dst_dist;


            delta_x = (dst_vec.x - anim.gameObject.transform.localPosition.x) * trace_grad;

            delta_y = (dst_vec.y - anim.gameObject.transform.localPosition.y) * trace_grad;

            if ((delta_x * delta_x + delta_y * delta_y) * anim.cur_elapsed * anim.cur_elapsed > (cur_to_dst_dist+Cultivation_Way.Others.CW_Constants.anim_dst_error) * (cur_to_dst_dist + Cultivation_Way.Others.CW_Constants.anim_dst_error))
            {
                delta_x = (dst_vec.x - anim.gameObject.transform.localPosition.x) / anim.cur_elapsed;
                delta_y = (dst_vec.y - anim.gameObject.transform.localPosition.y) / anim.cur_elapsed;

                //anim.trace_length = 0;
                (dst_vec, src_vec) = (src_vec, dst_vec);
            }

            return;
        }
    }
}
