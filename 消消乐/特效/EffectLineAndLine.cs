using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLineAndLine : EffectBase
{
    Tiled firstPointStart;
    Tiled firstPointEnd;
    List<Blocker> firstLst = new List<Blocker>();
    Tiled secondPointStart;
    Tiled secondPointEnd;
    List<Blocker> secondLst = new List<Blocker>();
    public EffectLineAndLine(Tiled orign, EffectCallback callback, EffectData check, object args)
        : base(orign, callback, check, args)
    {
        m_ctrl = new EffectController(this.OnEffectCallback);
        m_orign.CanMoveBlocker.MarkMatch = true;
        m_orign.CanMoveBlocker.CrushState = true;
    }
    public override void Start()
    {
        base.Start();
        var spType = m_orign.CanMoveBlocker.SpType;
        CheckBlocker(spType, out firstPointStart, out firstPointEnd, ref firstLst);
        CheckBlocker(spType == SpecialBlockType.horizontal ? SpecialBlockType.vertical : SpecialBlockType.horizontal, out secondPointStart, out secondPointEnd, ref secondLst);
    }
    public override void Play()
    {
        base.Play();
        m_matchSuccess = true;
        EventDispatcher.Notify(GCEventType.OneStepHasDone);
        int lastScore = LevelManager.Instance.Score;
        firstLst.Add(m_orign.CanMoveBlocker);
        LevelManager.Instance.CurMultiple = GCBlockerScore.LineAndLineCrush;
        LevelManager.Instance.Score += GCBlockerScore.LineAndLineEffect;

        //EventDispatcher.Notify(GCEventType.OnShowScore,LevelManager.Instance.Score - lastScore,);
        EventDispatcher.Notify(GCEventType.OnShowScore,m_orign.Guid, LevelManager.Instance.Score - lastScore, (ColorType)(m_orign.CanMoveBlocker.BaseID - BlockerID.baseredid), m_orign.CanMoveBlocker.position);


        var particle = ResourcePool.Instance.Pop(GCResPath.LineCrushPath);
        var mono = particle.GetComponent<LineCrushMoveEffect>();
        particle.SetActive(true);
        mono.ShowEffects(m_orign.position, firstPointStart == null ? m_orign.position : firstPointStart.position, firstPointEnd == null ? m_orign.position : firstPointEnd.position, firstLst, (obj)=>
        {
            ResourcePool.Instance.Push(GCResPath.LineCrushPath, obj, 0.1f);
        });

        var particle1 = ResourcePool.Instance.Pop(GCResPath.LineCrushPath);
        var mono1 = particle1.GetComponent<LineCrushMoveEffect>();
        particle1.SetActive(true);
        mono1.ShowEffects(m_orign.position, secondPointStart == null ? m_orign.position : secondPointStart.position, secondPointEnd == null ? m_orign.position : secondPointEnd.position, secondLst, (obj)=>
        {
            ResourcePool.Instance.Push(GCResPath.LineCrushPath, obj, 0.1f);
        });
        
        
        if (m_matchSuccess)
        {
            EventDispatcher.Notify(GCEventType.OneStepHasDone);
        }
    }
    public override void Finish(Action action)
    {
        //base.Finish();
        m_Data.OutData.isSucced = true;
        AgainTryMatch();
        m_matchItems.Add(m_orign.CanMoveBlocker);
        m_orign.CanMoveBlocker.CrushState = false;
        LevelManager.Instance.DelayDestroyBlockers(m_matchItems);
        m_ctrl.Execute();
        action.Invoke();
    }
    public override void AgainTryMatch()
    {
        Tiled src = (Tiled)m_args;
        for (int i = 0; i < m_matchItems.Count; i++)
        {
            if (src.Guid != m_matchItems[i].SelfTiled.Guid)
                TryMatch(m_matchItems[i]);
        }
        RemoveAreaBlocker();
    }
    bool CheckMatchBlocker(Tiled tiled, ref List<Blocker> blockers)
    {
        var destroyblocker = tiled.MatchBlocker;
        if (destroyblocker.SpType != SpecialBlockType.package)
            blockers.Add(destroyblocker);
        m_matchItems.Add(destroyblocker);
        destroyblocker.MarkMatch = true;
        destroyblocker.MatchEffectType = EffectType.LineLine;
        if (destroyblocker.CanBlock())
        {
            return true;
        }
        return false;
    }
    void CheckBlocker(SpecialBlockType SpType, out Tiled pointStart, out Tiled pointEnd, ref List<Blocker> blockers)
    {
        //SpecialBlockType type = SpType != SpecialBlockType.none ? SpType : m_orign.CanMoveBlocker.SpType;
        pointStart = null;
        pointEnd = null;
        if (SpType == SpecialBlockType.horizontal)
        {
            for (int j = m_orign.Col; j < LevelManager.Instance.Map.MaxCol; j++)
            {
                var tiled = LevelManager.Instance.Map.GetTiled(m_orign.Row, j);
                if (null != tiled && tiled.IsValidTiled())
                {
                    pointStart = tiled;
                }
                if (CheckMatch(tiled))
                {
                    if (CheckMatchBlocker(tiled, ref blockers))
                    {
                        break;
                    }
                }
            }
            for (int j = m_orign.Col - 1; j >= 0; j--)
            {
                var tiled = LevelManager.Instance.Map.GetTiled(m_orign.Row, j);
                if (null != tiled && tiled.IsValidTiled())
                {
                    pointEnd = tiled;
                }
                if (CheckMatch(tiled))
                {
                    if (CheckMatchBlocker(tiled, ref blockers))
                    {
                        break;
                    }
                }
            }
        }
        else if (SpType == SpecialBlockType.vertical)
        {
            for (int j = m_orign.Row; j < LevelManager.Instance.Map.MaxRow; j++)
            {
                var tiled = LevelManager.Instance.Map.GetTiled(j, m_orign.Col);
                if (null != tiled && tiled.IsValidTiled())
                {
                    pointStart = tiled;
                }
                if (CheckMatch(tiled))
                {
                    if (CheckMatchBlocker(tiled, ref blockers))
                    {
                        break;
                    }
                }
            }
            for (int j = m_orign.Row - 1; j >= 0; j--)
            {
                var tiled = LevelManager.Instance.Map.GetTiled(j, m_orign.Col);
                if (null != tiled && tiled.IsValidTiled())
                {
                    pointEnd = tiled;
                }
                if (CheckMatch(tiled))
                {
                    if (CheckMatchBlocker(tiled, ref blockers))
                    {
                        break;
                    }
                }
            }
        }
    }
}
