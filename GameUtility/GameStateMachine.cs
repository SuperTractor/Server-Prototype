using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtility
{
    public class GameStateMachine
    {
        // 游戏的状态
        public enum State
        {
            Deal,   // 发牌阶段
            Deal2Bid,   // 发牌到抢底的缓冲阶段
            Bid,    // 抢底阶段
            Bid2Fry,    // 抢底到炒底的缓冲阶段
            //Fry,    // 炒底阶段
            FryShow,    // 炒底阶段的亮牌
            FryShow2Bury,   // 亮牌到埋底的过渡阶段
            FryBury2Show,   // 埋底重新回到亮牌的过渡阶段
            FryBury,    // 炒底阶段的埋底
            Fry2Fight,  // 炒底到对战的缓冲阶段
            Fight,   // 对战阶段
            Fight2Score,    // 对战到计分的缓冲阶段
            Score,   // 计分
            Score2Deal  // 计分到下一次发牌的缓冲阶段
        };
        State m_state;
        public State state { get { return m_state; } }
        // 可以向游戏状态机发送的信号
        public enum Signal
        {
            DoneDeal,   // 完成发牌
            DoneDeal2Bid,   // 完成发牌-抢底缓冲阶段的清理, 初始化
            NoHigherBid,     // 不可能有更高出价者, 抢底结束
            DoneBid2Fry,    // 完成抢底-炒底缓冲阶段的清理, 初始化
            //NoHigherFry,     // 不可能有更高出价者, 炒底结束
            //AllSkipFry,     // 所有玩家跳过炒牌，炒底结束
            SuccessfulShow,     // 炒底阶段成功亮牌，则可以埋底
            DoneFryShow2Bury,   // 完成炒底阶段从亮牌到埋底的过渡流程
            DoneFryBury2Show,   // 完成炒底阶段从埋底重新回到亮牌的过渡流程
            FryContinue,    // 炒底继续
            FryEnd,     // 炒底结束
            DoneFry2Fight,  // 完成炒底-对战缓冲阶段的清理, 初始化
            PlayerHandCardAllEmpty, // 所有玩家的手牌为空
            DoneFight2Score,    // 完成对战-计分缓冲阶段的清理, 初始化
            DoneScore,  // 完成计分
            DoneScore2Deal // 完成计分-发牌缓冲阶段的清理, 初始化

        };

        // 根据信号更新游戏状态机
        public void Update(Signal signal)
        {
            switch (signal)
            {
                // 如果已经完成发牌
                case Signal.DoneDeal:
                    // 将游戏状态设置为抢底
                    m_state = State.Deal2Bid;
                    break;
                case Signal.DoneDeal2Bid:
                    m_state = State.Bid;
                    break;
                case Signal.NoHigherBid:
                    m_state = State.Bid2Fry;
                    break;
                case Signal.DoneBid2Fry:
                    //m_state = State.Fry;
                    m_state = State.FryShow;
                    break;
                //case Signal.NoHigherFry:
                //    m_state = State.Fry2Fight;
                //    break;
                //case Signal.AllSkipFry:
                //    m_state = State.Fry2Fight;
                //    break;
                case Signal.SuccessfulShow:
                    m_state = State.FryShow2Bury;
                    break;
                case Signal.DoneFryShow2Bury:
                    m_state = State.FryBury;
                    break;
                case Signal.DoneFryBury2Show:
                    m_state = State.FryShow;
                    break;
                case Signal.FryContinue:
                    m_state = State.FryBury2Show;
                    break;
                case Signal.FryEnd:
                    m_state = State.Fry2Fight;
                    break;
                case Signal.DoneFry2Fight:
                    m_state = State.Fight;
                    break;
                // 如果所有玩家的手牌都空了
                case Signal.PlayerHandCardAllEmpty:
                    // 将游戏状态重新设置为对战-计分缓冲阶段
                    m_state = State.Fight2Score;
                    break;
                case Signal.DoneFight2Score:
                    m_state = State.Score;
                    break;
                case Signal.DoneScore:
                    m_state = State.Score2Deal;
                    break;
                case Signal.DoneScore2Deal:
                    m_state = State.Deal;
                    break;
            }
        }
        // 直接更新游戏状态
        public void Update(State state)
        {
            m_state = state;
        }
    }
}
