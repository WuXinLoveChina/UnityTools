using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSBeatMatchData : FSDataBase
{
    private SpecialBlockType m_blockType = SpecialBlockType.none;
    private bool m_isOver = false;
    private EffectController m_ctrl;


    public EffectController EffCtrl { get; set; }
    public bool IsOver { get; set; }
    public SpecialBlockType MatchType { get; set; }

}
public class FSBeatMatch : FSBase
{
    private bool m_isOverExecute = false;
    FSBeatMatchData m_data;
    public override void Start(FSBase pre, FSDataBase data)
    {
        base.Start(pre, data);
        m_data = data as FSBeatMatchData;
        m_isOverExecute = false;

        if (m_data.IsOver)
        {
            //游戏胜利  弹出星星操作

        }
        else
        {
            //游戏未结束 将同色消与特殊元素交换产生的特殊元素消除

        }


        AppFacade.Instance.StartCoroutine(StepByStepExecute(m_data));

    }

    IEnumerator StepByStepExecute(FSBeatMatchData data)
    {
        Tiled tiled=null;
        FSFallingData fallingdata = new FSFallingData();
        List<Tiled> temp = LevelManager.Instance.GetSpecialBlockList(); ;
        EffectData effData = new EffectData();
        if (FSM.Instance.IsDebug)
            effData.fscheck = new FSCheck();
        foreach (var item in temp)
        {
            if (item != null)
            {
                data.EffCtrl.CreateEffect(EffectType.LineCrush, item, effData, item.CanMoveBlocker.SpType);
            }
        }
        data.EffCtrl.Execute();
        yield return new WaitForSeconds(0.25f);
    }
    public override void OnFinish()
    {
        base.OnFinish();
    }
    public override void BackMove(FSDataBase data)
    {
        base.BackMove(data);
    }
}
