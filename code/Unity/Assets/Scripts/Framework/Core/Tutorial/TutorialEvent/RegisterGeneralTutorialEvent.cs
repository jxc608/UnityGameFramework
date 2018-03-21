using UnityEngine;
using Snaplingo.Tutorial;
using System;

public class RegisterGeneralTutorialEvent : TutorialEvent
{
    public static Action<bool> GeneralShoot = null;

    public override void Init (TutorialManager manager, System.Action<TutorialEvent> shootEvent)
    {
        base.Init (manager, shootEvent);
        GeneralShoot = delegate(bool obj) {
			Shoot (obj ? 0 : 1);  
        };
    }

    public override void Cancel ()
    {
        GeneralShoot = null;
    }

    public override string[] GetTagLabels ()
    {
        return new string[]{ "成功标签", "失败标签" };
    }
}
