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
            //Broadcasting,   // 广播详细阶段，用于通知在线客户端一些须知信息
            GetReady,   // 等待其他玩家（此时房间内人数小于 4）
            Ready2Deal, // 准备开始到发牌的过渡
            Deal,   // 发牌阶段
            Deal2Bid,   // 发牌到抢底的缓冲阶段
            //Bid,    // 抢底阶段
            Touch,      // 抢底阶段，摸牌
            Touch2LastBid,  // 摸牌到最后抢底的过渡阶段
            LastBid,    // 最后抢底，为摸牌结束后，再给玩家几秒钟的思考时间，看看还需不需要抢底
            LastBid2BidBury,    // 最后抢底到埋底的过渡阶段
            BidBury,    // 抢底阶段结束后，庄家埋底
            Bid2Deal,   // 没人抢底，重新发牌，过渡阶段
            Bid2Fry,    // 抢底到炒底的缓冲阶段
            //Fry,    // 炒底阶段
            FryShow,    // 炒底阶段的亮牌
            FryShow2Bury,   // 亮牌到埋底的过渡阶段
            FryBury2Show,   // 埋底重新回到亮牌的过渡阶段
            FryBury,    // 炒底阶段的埋底
            Fry2FindFriend,     // 炒底阶段到寻友的过渡阶段
            FindFriend,     // 寻友阶段

            FindFriend2Linger,  // 寻友到停留的过渡
            FindFriendLinger,   // 寻友阶段之后的停留阶段

            FindFriend2Fight,   // 寻友到对战的过渡阶段
            //Fry2Fight,  // 炒底到对战的缓冲阶段
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
            //GoBroadcasting,     // 指示广播操作
            //DoneBroadcasting,   // 完成广播操作
            //NotReady,   // 游戏还没有准备好开始
            Ready,      // 有 4 个玩家进入房间，可以开始游戏
            DoneReady2Deal, // 完成准备到发牌过渡
            DoneDeal,   // 完成发牌
            DoneDeal2Bid,   // 完成发牌-抢底缓冲阶段的清理, 初始化
            DoneTouch,      // 完成摸牌
            DoneTouch2LastBid,  // 完成摸牌到最后抢底的过渡
            NoHigherBid,     // 不可能有更高出价者, 抢底结束
            EndLastBid,     // 最后抢底阶段也已经结束
            NoBidder,   // 没有抢底的人
            DoneLastBid2BidBury,    // 完成最后抢底到埋底的过渡
            DoneBidBury,        // 庄家完成埋底
            DoneBid2Deal,       // 完成抢底到重新发牌的过渡阶段
            DoneBid2Fry,    // 完成抢底-炒底缓冲阶段的清理, 初始化
            //NoHigherFry,     // 不可能有更高出价者, 炒底结束
            //AllSkipFry,     // 所有玩家跳过炒牌，炒底结束
            SuccessfulShow,     // 炒底阶段成功亮牌，则可以埋底
            DoneFryShow2Bury,   // 完成炒底阶段从亮牌到埋底的过渡流程
            DoneFryBury2Show,   // 完成炒底阶段从埋底重新回到亮牌的过渡流程
            FryContinue,    // 炒底继续
            FryEnd,     // 炒底结束
            //DoneFry2Fight,  // 完成炒底-对战缓冲阶段的清理, 初始化
            DoneFry2FindFriend, // 完成炒底到寻友的过渡
            //DoneFindFriend,     // 完成寻友

            IsFightAlone,   // 庄家选择单打
            HasFriend,      // 庄家有朋友
            DoneFindFriend2Linger,  // 完成寻友到停留的过渡
            DoneFindFriendLinger,   // 完成寻友停留

            DoneFindFriend2Fight,   // 完成寻友到对战的过渡
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
                //case Signal.NotReady:
                //    m_state = State.GetReady;
                //    break;

                case Signal.Ready:
                    m_state = State.Ready2Deal;
                    break;
                case Signal.DoneReady2Deal:
                    m_state = State.Deal;
                    break;
                // 如果已经完成发牌
                case Signal.DoneDeal:
                    // 将游戏状态设置为抢底
                    m_state = State.Deal2Bid;
                    break;
                case Signal.DoneDeal2Bid:
                    //m_state = State.Bid;
                    m_state = State.Touch;
                    break;
                case Signal.DoneTouch:
                    m_state = State.Touch2LastBid;
                    break;
                case Signal.DoneTouch2LastBid:
                    m_state = State.LastBid;
                    break;
                case Signal.EndLastBid:
                    m_state = State.LastBid2BidBury;
                    break;
                case Signal.NoHigherBid:
                    m_state = State.LastBid2BidBury;
                    break;
                case Signal.DoneLastBid2BidBury:
                    m_state = State.BidBury;
                    break;
                case Signal.NoBidder:
                    m_state = State.Bid2Deal;
                    break;
                case Signal.DoneBidBury:
                    m_state = State.Bid2Fry;
                    break;
                case Signal.DoneBid2Deal:
                    m_state = State.Deal;
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
                case Signal.FryEnd:// 炒底结束后
                    //m_state = State.Fry2Fight;
                    // 准备进入寻友阶段
                    m_state = State.Fry2FindFriend;
                    break;
                //case Signal.DoneFry2Fight:
                //    m_state = State.Fight;
                //    break;
                // TODO: 完善寻友阶段状态机的更新
                case Signal.DoneFry2FindFriend:
                    m_state = State.FindFriend;
                    break;
                //case Signal.DoneFindFriend:
                //    m_state = State.FindFriend2Fight;
                //    break;
                case Signal.IsFightAlone:
                    m_state = State.FindFriend2Fight;
                    break;
                case Signal.HasFriend:
                    m_state = State.FindFriend2Linger;
                    break;
                case Signal.DoneFindFriend2Linger:
                    m_state = State.FindFriendLinger;
                    break;
                case Signal.DoneFindFriendLinger:
                    m_state = State.FindFriend2Fight;
                    break;
                case Signal.DoneFindFriend2Fight:
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
