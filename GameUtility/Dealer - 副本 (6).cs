// 各阶段规则的开关
#define RULE
//#undef RULE


#if (RULE)
// 发牌
#define DEAL
//#undef DEAL
// 抢底
#define BID
//#undef BID

// 调试抢底
//#define DEBUG_BID_1
#undef DEBUG_BID_1

// 炒底
#define FRY
//#undef FRY
// 找朋友
#define FINDFRIEND
//#undef FINDFRIEND
// 对战
#define FIGHT
//#undef FIGHT
// 升级
#define LEVEL
//#undef LEVEL
// 积分
#define SCORE
//#undef SCORE

#else
// 发牌
#undef DEAL
// 抢底
#undef BID
// 炒底
#undef FRY
// 找朋友
#undef FINDFRIEND
// 对战
#undef FIGHT
// 升级
#undef LEVEL
// 积分
#undef SCORE

#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GameUtility
{
    public class cardComb
    {
        public
          bool valid;
        public
          bool thrown;
        public
          int mainNumber, mainColor; // 花色编号01234对应方块 黑桃 梅花 红桃 无主 mainNumber对应CardList编码方式
        public
          int thisColor; // 有主：花色编号0123对应方块 黑桃 梅花 红桃 其他主牌与主花色牌编在一起 无主：4对应主牌
        public
          int thisSame; // 记录几同
        public
          int thisType; // 0 拖拉机 1 单张 2 对子 3 三同 4 四同
        public
          int Count; // 记录总牌数
        public
          List<int> data; // 记录内容
        public
          CardList raw; // 原始数据
        public
          List<int> maxtractor;  //最长拖拉机，仅在甩牌时有值
        public
          int TransFromCardList(int a)
        {
            return a % 13 == 12 ? 1 : a % 13 + 2;
        }
        public
          cardComb(CardList init, int mn, int mc)
        {
            Count = 0;
            for (int i = 0; i < 54; i++)
                Count += init.data[i];

            maxtractor = null;
            raw = init;
            valid = true;
            thrown = false;
            mainNumber = mn;
            mainColor = mc; // 初始化主级数与主花色
            data = new List<int>();

            // 判断花色在同一区间 合法性来源
            bool mainOccur = false;
            int count = 0;
            if (mainColor == 4)
            {
                for (int i = 0; i < 4; i++)
                    for (int j = i * 13; j < (i + 1) * 13; j++)
                    {
                        if (init.data[j] > 0)
                        {
                            if (j - i * 13 != mainNumber)
                            {
                                count++;
                                thisColor = i;
                                break;
                            }
                            else
                            {
                                mainOccur = true;
                                thisColor = 4;
                            }
                        }
                    }

                if (init.data[52] > 0 || init.data[53] > 0)
                {
                    mainOccur = true;
                    thisColor = 4;
                }
                if ((mainOccur && count > 0) || (!mainOccur && count >= 2))
                {
                    valid = false;
                    return;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    for (int j = i * 13; j < (i + 1) * 13; j++)
                        if (init.data[j] > 0)
                        {
                            if (i == mainColor)
                            {
                                mainOccur = true;
                                thisColor = mainColor;
                            }
                            else
                            {
                                if (j - i * 13 != mainNumber)
                                {
                                    count++;
                                    thisColor = i;
                                    break;
                                }
                                else
                                {
                                    mainOccur = true;
                                    thisColor = mainColor;
                                }
                            }
                        }

                if (init.data[52] > 0 || init.data[53] > 0)
                {
                    mainOccur = true;
                    thisColor = mainColor;
                }
                if ((mainOccur && count > 0) || (!mainOccur && count >= 2))
                {
                    valid = false;
                    return;
                }
            }

            // 整齐
            count = 0;
            bool[] oc = { false, false, false, false, false };
            for (int i = 0; i < 54; i++)
                if (init.data[i] > 0)
                {
                    if (oc[init.data[i]])
                    {
                        if (init.data[i] == 1)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                    }
                    else
                    {
                        oc[init.data[i]] = true;
                        count++;
                        if (count == 2)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                    }
                }
            if (oc[1])
            {
                thisSame = 1;
                thisType = 1;
                return;
            }
            for (int i = 2; i <= 4; i++)
                if (oc[i])
                    thisSame = i;

            // 相连
            bool flag = false;
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (init.data[i * 13 + mainNumber] > 0)
                        {
                            data.Add(i * 13 + mainNumber);
                            flag = true;
                        }
                    // 大小王
                    if (flag && init.data[52] == 0 && init.data[53] > 0)
                    {
                        thrown = true;
                        goto isThrown;
                    }
                    if (init.data[52] > 0)
                        data.Add(52);
                    if (init.data[53] > 0)
                        data.Add(53);
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (init.data[i] > 0)
                            {
                                if (flag)
                                {
                                    int temp = i - 1;
                                    if (temp % 13 == mainNumber)
                                        temp--;
                                    if (temp > 0 && init.data[temp] == 0)
                                    {
                                        thrown = true;
                                        goto isThrown;
                                    }
                                }
                                data.Add(i);
                                flag = true;
                            }
                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (init.data[i] > 0)
                            {
                                int temp = i - 1;
                                if (temp % 13 == mainNumber)
                                    temp--;
                                if (temp > 0 && init.data[temp] == 0 && flag)
                                {
                                    thrown = true;
                                    goto isThrown;
                                }
                                data.Add(i);
                                flag = true;
                            }
                    // 副主级牌
                    int count1 = 0;
                    bool viceOccurFlag = false;
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && init.data[i * 13 + mainNumber] > 0)
                        {
                            viceOccurFlag = true;
                            if (flag)
                            {
                                int temp = data[data.Count() - 1] % 13;
                                if ((temp != 11 && mainNumber == 12) || (temp != 12 && mainNumber != 12))
                                {
                                    thrown = true;
                                    goto isThrown;
                                }
                            }
                            data.Add(i * 13 + mainNumber);
                            count1++;
                            if (count1 > 1)
                            {
                                thrown = true;
                                goto isThrown;
                            }
                            flag = true;
                        }
                    // 主级牌
                    bool mainOccurFlag = false;
                    if (init.data[mainColor * 13 + mainNumber] > 0)
                    {
                        mainOccurFlag = true;
                        if (flag && !viceOccurFlag)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                        data.Add(mainColor * 13 + mainNumber);
                        flag = true;
                    }
                    // 小王
                    bool blackJokerOccurFlag = false;
                    if (init.data[52] > 0)
                    {
                        blackJokerOccurFlag = true;
                        if (flag && !mainOccurFlag)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                        data.Add(52);
                        flag = true;
                    };
                    // 大王
                    if (init.data[53] > 0)
                    {
                        if (flag && !blackJokerOccurFlag)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                        data.Add(53);
                        flag = true;
                    };
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (init.data[i] > 0)
                            {
                                if (flag)
                                {
                                    int temp = i - 1;
                                    if (temp % 13 == mainNumber)
                                        temp--;
                                    if (temp > 0 && init.data[temp] == 0)
                                    {
                                        thrown = true;
                                        goto isThrown;
                                    }
                                }
                                data.Add(i);
                                flag = true;
                            }
                }
            }

            // 确定类型 拖拉机 单张 对子 三同 四同
            if (thisSame == 2 && data.Count > 1)
                thisType = 0;
            else
                thisType = thisSame;
            // 纠正thisColor
            for (int i = 0; i < 4; i++)
                if (init.data[i * 13 + mainNumber] > 0)
                    thisColor = mainColor;
            return;

            // 甩牌 记录最长拖拉机 TODO
            isThrown:
            {
                thisSame = 0;
                thisType = 5;
                maxtractor = longtractor(init, thisColor, mainColor, mainNumber);
            }
            // 重新初始化
        }

        /// <summary>
        /// 找出长度为k的最大的拖拉机
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        List<int> findtractor(CardList temp, int k, int thisColor, int mainColor, int mainNumber)
        {
            List<int> maxtractor = new List<int>();
            List<int> currenttractor = new List<int>();
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (temp.data[i * 13 + mainNumber] > 1)
                        {
                            currenttractor.Add(i);
                        }
                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            //如果拖拉机长度大于等于k且最大牌较大,则将最大的k张牌赋值到maxtractor中
                            maxtractor.Clear();
                            if (maxtractor.Count() > 0)
                            {
                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }
                                }
                            }
                            else
                            {
                                if (currenttractor.Count() >= k)
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }

                                }
                            }
                            currenttractor.Clear();
                        }

                    }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);
                            }
                            else
                            {
                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();
                            }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }

                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();

                            }
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && temp.data[i * 13 + mainNumber] > 0)
                        {
                            int j = i * 13 + mainNumber;
                            if (temp.data[j] > 1)
                            {
                                currenttractor.Add(j);

                            }
                            else
                            {

                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();

                            }
                        }
                    // 主级牌
                    if (temp.data[mainColor * 13 + mainNumber] > 1)
                    {
                        currenttractor.Add(mainColor * 13 + mainNumber);

                    }
                    else
                    {
                        maxtractor.Clear();
                        if (maxtractor.Count() > 0)
                        {
                            if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                            {
                                for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                {
                                    maxtractor.Add(currenttractor.ElementAt(t));
                                }
                            }
                        }
                        else
                        {
                            if (currenttractor.Count() >= k)
                            {
                                for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                {
                                    maxtractor.Add(currenttractor.ElementAt(t));
                                }

                            }
                        }
                        currenttractor.Clear();

                    }



                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            maxtractor.Clear();
                            if (maxtractor.Count() > 0)
                            {
                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }
                                }
                            }
                            else
                            {
                                if (currenttractor.Count() >= k)
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }

                                }
                            }
                            currenttractor.Clear();


                        }

                    }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();


                            }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }

                }
            }
            return maxtractor;

        }

        /// <summary>
        /// 找出最长的拖拉机
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        List<int> longtractor(CardList temp, int thisColor, int mainColor, int mainNumber)
        {
            List<int> maxtractor = new List<int>();
            List<int> currenttractor = new List<int>();
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (temp.data[i * 13 + mainNumber] > 1)
                        {
                            currenttractor.Add(i);
                        }
                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            if (currenttractor.Count() > maxtractor.Count())
                            {
                                currenttractor.ForEach(a => maxtractor.Add(a));
                                currenttractor.Clear();
                            }

                        }

                    }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }

                            }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }

                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }

                            }
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && temp.data[i * 13 + mainNumber] > 0)
                        {
                            int j = i * 13 + mainNumber;
                            if (temp.data[j] > 1)
                            {
                                currenttractor.Add(j);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }
                            }
                        }
                    // 主级牌
                    if (temp.data[mainColor * 13 + mainNumber] > 1)
                    {
                        currenttractor.Add(mainColor * 13 + mainNumber);

                    }
                    else
                    {
                        if (currenttractor.Count() > maxtractor.Count())
                        {
                            currenttractor.ForEach(a => maxtractor.Add(a));
                            currenttractor.Clear();
                        }
                    }



                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            if (currenttractor.Count() > maxtractor.Count())
                            {
                                currenttractor.ForEach(a => maxtractor.Add(a));
                                currenttractor.Clear();
                            }
                        }

                    }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }

                            }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }

                }
            }
            return maxtractor;
        }
        /// <summary>
        /// 判断两张牌的大小
        /// </summary>
        /// <param name="a">牌1，用54个数字表示</param>
        /// <param name="b">牌2，用54个数字表示</param>
        /// <param name="mainColor"></param>
        /// <param name="mainNumber"></param>
        /// <returns>a大于b返回true，否则返回false,！！！！注意，返回false不一定代表a小于b，可能是a,b不在一个区间上</returns>
        bool biggerThan(int a, int b, int mainColor, int mainNumber)
        {
            if (a == -1)
                return false;
            if (b == -1)
                return true;
            if (a == b)
                return false;
            if (mainColor == 4)
            {
                //主牌
                if (a == 53 || a == 52 || a % 13 == mainNumber)
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber)
                    {
                        if (a == 53 || (a == 52 && b != 53))
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else//副牌
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber)
                        return false;
                    else
                    {
                        //同一花色
                        if ((int)a / 13 == (int)b / 13 && a % 13 > b % 13)
                            return true;
                        else
                            return false;
                    }
                }
            }
            else // 有主
            {
                // 主级牌
                if (a == 53 || a == 52 || a % 13 == mainNumber || (int)a / 13 == mainColor)
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber || (int)b / 13 == mainColor)
                    {
                        if (a == 53 || (a == 52 && b != 53) || (a % 13 == mainNumber && (int)a / 13 == mainColor && b < 52)
                            || (a % 13 == mainNumber && (int)a / 13 != mainColor && b % 13 != mainNumber && (int)b / 13 == mainColor)
                            || (a % 13 != mainNumber && (int)a / 13 == mainColor && b % 13 != mainNumber && (int)b / 13 == mainColor && a > b))
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber || (int)b / 13 == mainColor)
                        return false;
                    else
                    {
                        if ((int)a / 13 == (int)b / 13 && a % 13 > b % 13)
                            return true;
                        else
                            return false;
                    }
                }
            }
        }

    }

    public class RulePlayer
    {
        public CardList cardInHand;
        public RulePlayer()
        {
            cardInHand = new CardList();
        }
    }

    public class CardList
    {
        public
          int[] data; // 编号按照0~11->2~K, 12->A
        public
          CardList()
        {
            data = new int[54];
        }
        public
            CardList(CardList res)
        {
            data = new int[54];
            for (int i = 0; i < 54; i++)
                data[i] = res.data[i];
        }
        public int Count
        {
            get
            {
                int tmp = 0;
                for (int i = 0; i < 54; i++)
                    tmp += data[i];
                return tmp;
            }
        }

    };

    public class Dealer
    {
        #region BASICS
        public int mainNumber, mainColor;

        public int GetMainLevel()
        {
            return (mainNumber + 1) % 13;
        }

        public Card.Suit GetMainSuit()
        {
            // 花色编号01234对应方块 黑桃 梅花 红桃 无主
            switch (mainColor)
            {
                case 0:
                    return Card.Suit.Diamond;
                case 1:
                    return Card.Suit.Spade;
                case 2:
                    return Card.Suit.Club;
                case 3:
                    return Card.Suit.Heart;
                case 4:
                    return Card.Suit.Joker0;
                default:
                    throw new Exception("主花色不符合规范");
            }
        }

        // 全牌: 全部 4 副牌
        private Card[] m_totalCard;
        // 8 张底牌
        private Card[] m_bottom;
        // 分发给玩家的手牌
        private Card[] m_playerCard;
        // 玩家当前的手牌
        public List<Card>[] playersHandCard { get; set; }

        // 玩家的当前级数
        public int[] playerLevels { get; set; }
        // 玩家的升级数
        public int[] playerAddLevels = new int[playerNumber];
        // 抢底次数
        public int[] BidTimes;
        // 炒底次数
        public int[] fryTimes;
        // 埋底次数
        public int[] buryTimes;
        // 埋底分数
        public int[] buryScore;
        // 选择单打次数
        public int[] singleTimes;
        // 选择找朋友次数
        public int[] findFriendTimes;
        public int bottomSuccessID;//抄底玩家
        public int bottomSuccessScore;//抄底分数

        // 底牌接口
        public Card[] bottom
        {
            get { return m_bottom; }
            set { m_bottom = value; }
        }
        // 4 个玩家的手牌暂存地（发牌阶段用）
        public Card[] playerCard
        {
            get { return m_playerCard; }
        }

        // 玩家总数
        public const int playerNumber = 4;

        // 当前玩家ID
        private int m_currentPlayerId;
        public int currentPlayerId
        {
            get { return m_currentPlayerId; }
            set { m_currentPlayerId = value; }
        }

        // 当前台上方玩家ID；不是台上方的话就不在这个数组里面
        private int[] m_upperPlayersId = new int[0];
        public int[] upperPlayersId { get { return m_upperPlayersId; } }

        // 一共多少副牌
        public const int packNumber = 4;
        // 底牌的张数
        public const int bottomCardNumber = 8;
        // 初始时玩家拥有的手牌数
        public const int cardInHandInitialNumber = (packNumber * Card.cardNumberOfOnePack - bottomCardNumber) / 4;
        // 每个玩家手牌的最大数目
        public const int cardInHandMaxNumber = cardInHandInitialNumber + bottomCardNumber;

        // 庄家的 id
        public List<int> bankerPlayerId;

        /// <summary>
        /// 构造函数, 初始化荷官
        /// 获取 4 副牌
        /// </summary>
        /// 
        public Dealer()
        {
            score = new int[4];
            for (int i = 0; i < 4; i++)
                score[i] = 0;

            BidTimes = new int[4];
            for (int i = 0; i < 4; i++)
                BidTimes[i] = 0;

            fryTimes = new int[4];
            for (int i = 0; i < 4; i++)
                fryTimes[i] = 0;

            buryTimes = new int[4];
            for (int i = 0; i < 4; i++)
                buryTimes[i] = 0;

            buryScore = new int[4];
            for (int i = 0; i < 4; i++)
                buryScore[i] = 0;

            singleTimes = new int[4];
            for (int i = 0; i < 4; i++)
                singleTimes[i] = 0;


            findFriendTimes = new int[4];
            for (int i = 0; i < 4; i++)
                findFriendTimes[i] = 0;

            bottomSuccessID = -1;
            bottomSuccessScore = 0;

            currentHasBidder = false;

            m_totalCard = new Card[Card.cardNumberOfOnePack * packNumber];
            for (int i = 0; i < packNumber; i++)
            {
                Card.GetCardSet().CopyTo(m_totalCard, i * Card.cardNumberOfOnePack);
            }
            m_playerCard = new Card[cardInHandInitialNumber * 4];
            m_bottom = new Card[bottomCardNumber];

            playerLevels = new int[playerNumber];

            playersHandCard = new List<Card>[playerNumber];

            currentBidCards = new List<Card>[playerNumber];
            //currentLegalBidColors = new List<Card.Suit>[playerNumber];
            //Shuffle();
            //Cut();
            // 为炒底阶段玩家亮牌分配内存
            showCards = new List<Card>[playerNumber];
            showCardsHistory = new List<Card>[playerNumber];
            // 为玩家对战阶段出牌分配内存
            handOutCards = new List<Card>[playerNumber];

            for (int i = 0; i < playerNumber; i++)
            {
                playersHandCard[i] = new List<Card>();
                showCards[i] = new List<Card>();
                showCardsHistory[i] = new List<Card>();
                handOutCards[i] = new List<Card>();
                currentBidCards[i] = new List<Card>();
            }
            bankerPlayerId = new List<int>();
        }

        // 从指定玩家 ID 开始，逆序找上一个台上方玩家
        int GetLastUpperPlayerId(int id)
        {
            for (int i = 1; i < playerNumber; i++)
            {
                int thisId = (playerNumber + id - i) % playerNumber;
                // 如果找到了上一个台上方玩家
                if (Array.IndexOf(m_upperPlayersId, thisId) >= 0)
                {
                    return thisId;
                }
            }
            throw new Exception("找不到上一个台上方玩家");
        }

        /// <summary>
        /// 从指定玩家 ID 开始，逆序找上一个指定级数的台上方玩家，
        /// </summary>
        /// <param name="id">开始 ID </param>
        /// <param name="level">指定级数</param>
        /// <returns></returns>
        int GetLastUpperPlayerId(int id, int level)
        {
            for (int i = 1; i < playerNumber; i++)
            {
                int thisId = (playerNumber + id - i) % playerNumber;
                // 如果找到了上一个指定级数的台上方玩家
                if (IsUpperPlayer(thisId) && (playerLevels[thisId] - 1) % 13 == (level - 1) % 13)
                {
                    return thisId;
                }
            }
            throw new Exception("找不到上一个指定级数的台上方玩家");
        }

        /// <summary>
        /// 检查某玩家是否为台上方玩家
        /// </summary>
        /// <param name="id">要检查的玩家 ID</param>
        /// <returns></returns>
        public bool IsUpperPlayer(int id)
        {
            return Array.IndexOf(m_upperPlayersId, id) >= 0;
        }

        /// <summary>
        /// 检查某个玩家是否为庄家
        /// </summary>
        /// <param name="id">要检查的玩家 ID</param>
        /// <returns></returns>
        public bool IsBanker(int id)
        {
            return bankerPlayerId.IndexOf(id) >= 0;
        }
        /// <summary>
        /// 保证新增庄家的时候，不会重复添加
        /// </summary>
        /// <param name="id"></param>
        public void AddBanker(int id)
        {
            if (!IsBanker(id))
            {
                bankerPlayerId.Add(id);
            }
        }

        // 将底牌放入指定玩家手牌
        public void AddBottom(int playerId)
        {
            playersHandCard[playerId].AddRange(bottom);
            // 清空底牌
            bottom = new Card[0];
        }

        // 埋底
        public void BuryCards(int playerId, Card[] cards)
        {
            // 将要埋的牌放到底牌
            m_bottom = new Card[bottomCardNumber];
            Array.Copy(cards, m_bottom, bottomCardNumber);
            // 从玩家的手牌去除埋牌
            for (int i = 0; i < cards.Length; i++)
            {
                playersHandCard[playerId].Remove(cards[i]);
            }
        }

        #endregion

        #region DEAL
        // 洗牌
        public void Shuffle()
        {
            Card.Shuffle(m_totalCard);
        }
        // 多次洗牌
        public void Shuffle(int times)
        {
            for (int i = 0; i < times; i++)
            {
                Shuffle();
            }
        }

        // 分牌, 发牌的前一步
        // 把底牌抽出来再把剩下的牌分发给玩家
        public void Cut()
        {
            Array.Copy(m_totalCard, bottomCardNumber, m_playerCard, 0, cardInHandInitialNumber * 4);
            Array.Copy(m_totalCard, m_bottom, bottomCardNumber);
        }

        /// <summary>
        /// 发牌结束，更新主牌信息
        /// 没有任何的亮牌环节，不可能更新主花色
        /// </summary>
        public void UpdateMainDeal()
        {
#if (DEAL)
            // 如果是首盘
            if (round == 1)
            {
                mainNumber = 12;
                return;
            }
            // 如果台上方只有一个，更新主级数
            if (m_upperPlayersId.Length == 1)
            {
                // 用他的级数更新（必须保证玩家级数数组已经更新）
                mainNumber = (playerLevels[m_upperPlayersId[0]] + 11) % 13;
            }
            // 否则不更新，留到后面解决
            else
            {

            }
#else

#endif
        }

        #endregion

        // 抢底部分代码
        #region BID
        // 抢底阶段，当前所有玩家的亮牌；在抢底开始之前清空
        public List<Card>[] currentBidCards;
        // 每个玩家当前的合法亮牌花色
        //public List<Card.Suit>[] currentLegalBidColors;

        // 抢到底牌的玩家 ID
        public int gotBottomPlayerId { get; set; }
        // 抢到底牌的玩家的亮牌花色
        public Card.Suit gotBottomSuit { get; set; }
        // 抢到底牌的玩家的亮牌
        public List<Card> gotBottomShowCards;
        // 记录是否有玩家抢底的flag
        public bool hasBidder { get; set; }
        //记录当前玩家是否抢了底
        public bool currentHasBidder = false;
        // 记录 4 个玩家率先亮牌的顺序；bidOrder[0]=1 表示第一个亮牌的是玩家 id = 1
        public int[] bidOrder = new int[playerNumber];
        // 测试：抢底亮牌上限
        int m_bidCardsLimit = 5;


        // 抢底阶段：给出此玩家当前可以亮的花色，已知当前摸牌的数目
        public bool[] GetLegalBidColors(int playerId/*, int touchCardNumber*/)
        {
#if (BID)
            bool[] legalBidColors = new bool[4];
            // 构造当前该玩家已经摸到的牌
            //Card[] currentTouchCards = new Card[touchCardNumber];
            //playersHandCard[playerId].CopyTo(0, currentTouchCards, 0, touchCardNumber);
            // 检查该玩家是否为台上方玩家
            bool isUpperPlayer = Array.IndexOf(m_upperPlayersId, playerId) >= 0;
            // 只有台上方玩家才能抢底
            if (isUpperPlayer)
            {
                List<Card> levelCards;
                // 如果该玩家还没有亮牌
                if (currentBidCards[playerId].Count == 0)
                {
                    // 获取目前该玩家的摸牌当中，所有的级牌
                    levelCards = playersHandCard[playerId].FindAll(card => card.points == (playerLevels[playerId] - 1) % 13);

                    for (int i = 0; i < levelCards.Count; i++)
                    {
                        // 姑且认为可以亮出此花色的牌
                        int k = (int)levelCards[i].suit;
                        if (!legalBidColors[k])
                        {
                            legalBidColors[k] = true;
                        }
                    }
                }
                else// 如果该玩家已经亮了牌
                {
                    // 获取该玩家目前所有的级牌，而且花色要和他已经亮过的牌一致
                    levelCards = playersHandCard[playerId].FindAll(card => card.points == (playerLevels[playerId] - 1) % 13 && card.suit == currentBidCards[playerId][0].suit);

                    // 姑且认为可以亮出此花色的牌
                    if (levelCards.Count > 0)
                    {
                        int k = (int)levelCards[0].suit;
                        if (!legalBidColors[k])
                        {
                            legalBidColors[k] = true;
                        }
                    }
                }

                // 再根据其他玩家已经亮的牌决定此玩家可以亮的花色
                for (int i = 0; i < legalBidColors.Length; i++)
                {
                    // 如果先前认定这花色可以出
                    if (legalBidColors[i])
                    {
                        // 抢底要求亮牌总数
                        int bidNeedNumber;
                        // 看看现在最大亮牌数是多少
                        int maxBidNumber = currentBidCards.Max(cards => cards.Count);

                        // 如果之前已经有人亮牌
                        if (maxBidNumber > 0)
                        {
                            // 获得所有亮牌最多的玩家的 ID
                            int[] allMaxBidPlayersId = Array.FindAll(new int[4] { 0, 1, 2, 3 }, id => currentBidCards[id].Count == maxBidNumber);
                            // 获取这些亮牌最多的玩家的亮牌顺序
                            int[] allMaxBidPlayersOrder = Array.ConvertAll(allMaxBidPlayersId, id => Array.IndexOf(bidOrder, id));
                            // 找到该玩家的亮牌顺序
                            int thisPlayerBidOrder = Array.IndexOf(bidOrder, playerId);
                            // 如果这玩家还没有亮牌
                            if (thisPlayerBidOrder < 0)
                            {
                                // 后亮牌抢底者也只能在原来亮出的抢底主牌花色上增加主牌数来增加抢底机会
                                bidNeedNumber = maxBidNumber + 1 - currentBidCards[playerId].Count;
                            }
                            // 如果他已经亮牌了
                            else
                            {
                                // 如果所有亮牌最多的玩家都是在该玩家之后亮牌的
                                if (allMaxBidPlayersOrder.Min() > thisPlayerBidOrder)
                                {
                                    // 先亮主牌抢底者可以在同花色情况下逐张增加主牌张数保抢底权
                                    bidNeedNumber = maxBidNumber - currentBidCards[playerId].Count;

                                }
                                // 否则，如果有一个亮牌最多的玩家是在该玩家之前亮牌的
                                else
                                {
                                    // 后亮牌抢底者也只能在原来亮出的抢底主牌花色上增加主牌数来增加抢底机会
                                    bidNeedNumber = maxBidNumber + 1 - currentBidCards[playerId].Count;
                                }
                            }

                        }
                        // 否则如果之前没人亮牌
                        else
                        {
                            continue;
                        }

                        // 计算这花色的摸牌总共多少张
                        // 只有当总数至少为要求数目，才可以出这个花色的牌
                        int totalNum = levelCards.Count(card => card.suit == (Card.Suit)i);
                        legalBidColors[i] = totalNum >= bidNeedNumber;
                    }
                }
            }
            else
            {
            }
            return legalBidColors;
#else
            // 测试：只要比最大亮牌数多就行
            int bidNeedNumber;

            // 看看现在最大亮牌数是多少
            int maxBidNumber = currentBidCards.Max(cards => cards.Count);

            // 先亮主牌抢底者可以在同花色情况下逐张增加主牌张数保抢底权
            bidNeedNumber = maxBidNumber + 1 - currentBidCards[playerId].Count;

            // 测试：都可以亮
            bool[] legalBidColors = new bool[4];

            // 如果要亮牌数已经达到亮牌上限；或者玩家根本不够手牌
            if (maxBidNumber + 1 > m_bidCardsLimit || playersHandCard[playerId].Count < bidNeedNumber)
            {
                // 不可以亮了
                return legalBidColors;
            }
            else
            {
                // 都可以亮
                for (int i = 0; i < legalBidColors.Length; i++)
                {
                    legalBidColors[i] = true;
                }
                return legalBidColors;
            }

#endif
        }
        // 亮牌需要增加的数目
        //public int BidNeedNumber(int playerId)
        //{
        //    return currentBidCards.Max(cards => cards.Count) + 1 - currentBidCards[playerId].Count;
        //}
        // 抢底阶段：亮牌帮助函数
        public void BidHelper(int playerId/*, int currentTouchNumber*/, Card.Suit suit)
        {
#if (BID)
            int bidNeedNumber;
            // 检查当前亮牌玩家是否已经亮过牌
            if (currentBidCards[playerId].Count > 0)
            {

            }
            // 如果还没有亮牌
            else
            {
                // 记录他的亮牌顺序
                int idx = Array.IndexOf(bidOrder, -1);
                bidOrder[idx] = playerId;
            }

            // 看看现在最大亮牌数是多少
            int maxBidNumber = currentBidCards.Max(cards => cards.Count);

            // 如果现在最大亮牌数是 0 ，说明还没有人亮牌
            if (maxBidNumber == 0)
            {
                bidNeedNumber = 1;

            }
            // 否则，就说明之前已经有人亮牌
            else
            {
                // 获得所有亮牌最多的玩家的 ID
                int[] allMaxBidPlayersId = Array.FindAll(new int[4] { 0, 1, 2, 3 }, id => currentBidCards[id].Count == maxBidNumber);
                // 获取这些亮牌最多的玩家的亮牌顺序
                int[] allMaxBidPlayersOrder = Array.ConvertAll(allMaxBidPlayersId, id => Array.IndexOf(bidOrder, id));
                // 找到该玩家的亮牌顺序
                int thisPlayerBidOrder = Array.IndexOf(bidOrder, playerId);
                // 如果所有亮牌最多的玩家都是在该玩家之后亮牌的
                if (allMaxBidPlayersOrder.Min() > thisPlayerBidOrder)
                {
                    // 先亮主牌抢底者可以在同花色情况下逐张增加主牌张数保抢底权
                    bidNeedNumber = maxBidNumber - currentBidCards[playerId].Count;
                }
                // 否则，如果有一个亮牌最多的玩家是在该玩家之前亮牌的
                else
                {
                    // 后亮牌抢底者也只能在原来亮出的抢底主牌花色上增加主牌数来增加抢底机会
                    bidNeedNumber = maxBidNumber + 1 - currentBidCards[playerId].Count;
                }
            }

            // 构造一张级牌
            Card levelCard = new Card(suit, (playerLevels[playerId] - 1) % 13);
            for (int i = 0; i < bidNeedNumber; i++)
            {
                // 找到级牌所在位置
                //int idx = playersHandCard[playerId].FindIndex(card => card.suit == suit && card.points + 1 == playerLevels[playerId]);
                // 去除一张级牌
                if (!playersHandCard[playerId].Remove(levelCard))
                {
                    throw new Exception("抢底：级牌数不足够");
                }
                // 加入亮牌当中
                currentBidCards[playerId].Add(levelCard);
            }
            // 更新当前抢到底牌的玩家 ID
            gotBottomPlayerId = playerId;
            // 有人抢底了
            hasBidder = true;

            // 统计抢底次数
            BidTimes[playerId]++;
#else
            // 测试：只要比最大亮牌数多就行
            int bidNeedNumber;

            // 看看现在最大亮牌数是多少
            int maxBidNumber = currentBidCards.Max(cards => cards.Count);

            // 先亮主牌抢底者可以在同花色情况下逐张增加主牌张数保抢底权
            bidNeedNumber = maxBidNumber + 1 - currentBidCards[playerId].Count;

            // 从手牌中拿出要求数量的牌
            List<Card> temp = new List<Card>();
            for (int i = 0; i < bidNeedNumber; i++)
            {
                temp.Add(playersHandCard[playerId][0]);
                playersHandCard[playerId].RemoveAt(0);
            }
            currentBidCards[playerId].AddRange(temp);

            // 更新当前抢到底牌的玩家 ID
            gotBottomPlayerId = playerId;
            // 有人抢底了
            hasBidder = true;
#endif
        }


        // 判断是否不可能有更高出价者(抢底阶段)
        public bool NoHigherBid()
        {
            bool hasHigerBid = false;
            for (int i = 0; i < playerNumber; i++)
            {
                bool[] legalBidColors = GetLegalBidColors(i);
                for (int j = 0; j < legalBidColors.Length; j++)
                {
                    hasHigerBid |= legalBidColors[j];
                }
            }
            // 当没有更高筹码，或者只有一个台上方玩家时，认为不需要再进行抢底
            return !hasHigerBid || upperPlayersId.Length == 1;
        }

        /// <summary>
        /// 抢底阶段结束后，更新庄家
        /// </summary>
        public void UpdateBankerBid()
        {
            // 清空庄家列表
            bankerPlayerId.Clear();
            // 抢到底牌的为庄家
            bankerPlayerId.Add(gotBottomPlayerId);
        }

        /// <summary>
        /// 抢底阶段结束，更新主牌信息
        /// 暂且认为抢到底牌的玩家是庄家，这时候已经更新庄家了
        /// 假定庄家只有 1 个
        /// 花色编号01234对应方块 黑桃 梅花 红桃 无主 mainNumber：A 2 3 4 5 6 7 8 9 10 J Q K对应 12 0 1 2 3 4 5 6 7 8 9 10 11
        /// 庄家的亮牌可以确定主级数和主花色
        /// 根据抢底阶段，庄家亮出的牌（花色和点数），确定主花色和主级数
        /// 抢底不可能出现大小鬼亮牌
        /// 主花色就是庄家亮出的牌的花色；主级数就是庄家亮牌的点数（也一定是庄家的级数，这是因为庄家只允许亮点数是他的级数的牌）
        /// 此函数根据的是庄家级数来更新主级数
        /// input: playerLevels bankerPlayerId gotBottomSuit
        /// ouput: mainNumber mainColor
        /// </summary>
        public void UpdateMainBid()
        {
#if (BID)
            // 暂且认为抢到底牌的玩家是庄家，这时候已经更新庄家了
            // 庄家的亮牌可以确定主级数和主花色
            mainNumber = (playerLevels[bankerPlayerId[0]] + 11) % 13;
            switch (gotBottomSuit)
            {
                case Card.Suit.Club:
                    mainColor = 2;
                    break;
                case Card.Suit.Diamond:
                    mainColor = 0;
                    break;
                case Card.Suit.Heart:
                    mainColor = 3;
                    break;
                case Card.Suit.Spade:
                    mainColor = 1;
                    break;
                // 抢底不可能有大小鬼亮牌
                //case Card.Suit.Joker0:
                //    break;
                //case Card.Suit.Joker1:
                //    break;
                default:
                    break;
            }
#else

#endif
        }

        #endregion

        #region FRY
        // 当前炒底玩家
        private int m_currentFryPlayerId;
        public int currentFryPlayerId
        {
            get
            {
                return m_currentFryPlayerId;
            }
            set
            {
                m_currentFryPlayerId = value % playerNumber;
            }
        }
        // 炒底亮牌数下界，下一玩家必须亮牌后总数必须比这个数多
        public int fryCardLowerBound { get; set; }
        // 超出 5 个亮牌数就算最大
        private int m_fryCardLimit = 5;
        // 累计已经有多少玩家跳过炒底；一旦有 3 个玩家选择不跟，则炒底结束；一旦有 1 个玩家跟，重置此计数器
        public int skipFryCount { get; set; }
        // 目前各玩家亮出的筹码牌
        public List<Card>[] showCards;
        // 各玩家上次亮出的筹码牌：4家都亮了或者不跟后复制showCards以更新一次
        public List<Card>[] showCardsHistory;
        // 已经炒底亮牌的人数
        public int fryMoves;

        // 最后（炒底）亮牌玩家的亮牌（用第一张牌来代表，因为亮牌都是同花色同点数的）
        public Card lastFryShowCard;
        // 最后（炒底）亮牌玩家ID；-1 表示还没有设置，或者没有玩家亮牌
        public int lastFryShowPlayerId;
        // 按时间顺序记录各玩家亮出来的点数（从抢底最后一次亮牌记录到炒底结束；抢底之前清空）；用来台下方亮大小鬼的时候确定主级数的
        public List<int> showPointsHistory = new List<int>();
        // 用数牌确定的庄家 ID 记录（按时间顺序插入）；在最后亮大小鬼炒底的是台下方的时候，用来确定庄家（即中间即使有台上方亮大小鬼也可能没得做庄）
        // 从抢底结束（此时插入第一个记录），记录到炒底结束；在抢底开始前清空
        public List<int> bankerIdHistory = new List<int>();

        // 清空炒底阶段亮牌记录
        public void ClearShowCards()
        {
            for (int i = 0; i < playerNumber; i++)
            {
                showCards[i].Clear();
                showCardsHistory[i].Clear();
            }
        }

        /// <summary>
        /// 判断炒底出牌是否合法
        /// </summary>
        /// <param name="addFryCards">炒底出牌</param>
        /// <param name="playerID">玩家编号</param>
        /// <returns></returns>
        public bool fryCompare(Card[] addFryCards, int playerID)
        {
            //判断整齐性
            Card temp = new Card(addFryCards[0].suit, addFryCards[0].points);
            for (int i = 0; i < addFryCards.Length; i++)
            {
                if (addFryCards[i].suit != temp.suit || addFryCards[i].points != temp.points)
                    return false;
            }

            //判断是否为台上方级数牌或大小王
            if (!isUpperNumber(temp.points + 1) && temp.suit != Card.Suit.Joker0 && temp.suit != Card.Suit.Joker1)
                return false;

            //和最大牌比大小，先比数量，再比牌面
            List<Card> formerCard = new List<Card>();
            if (lastFryShowPlayerId == -1)
            {
                gotBottomShowCards.ForEach(a => formerCard.Add(a));
            }
            else
            {
                showCards[lastFryShowPlayerId].ForEach(a => formerCard.Add(a));
            }

            if (addFryCards.Count() > formerCard.Count())
            {
                return true;
            }
            else if (addFryCards.Count() == formerCard.Count())
            {
                if (temp.suit == Card.Suit.Joker1 && formerCard.Last().suit != Card.Suit.Joker1 ||
                    (temp.suit == Card.Suit.Joker0 && formerCard.Last().points != 13))
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        // 判断炒底时增加的筹码牌是否合法
        public Judgement IsLegalShow(Card[] addFryCards, int playerId)
        {
#if (FRY)
            // 测试：总亮牌数比前一个炒底玩家的多就行
            bool isValid;
            isValid = fryCompare(addFryCards, playerId);
            Console.WriteLine("判断完合法性");
            string message;
            if (isValid)
            {
                message = "合法亮牌";
            }
            else
            {
                message = "筹码不够大";
            }
            return new Judgement(message, isValid);
#else
            // 测试：总亮牌数比前一个炒底玩家的多就行
            bool isValid;
            int total = addFryCards.Length + showCards[playerId].Count;
            isValid = total > fryCardLowerBound && total <= m_fryCardLimit;
            string message;
            if (isValid)
            {
                message = "合法亮牌";
            }
            else
            {
                message = "筹码不够大";
            }
            return new Judgement(message, isValid);
#endif
        }

        // 判断埋底的合法性（炒底阶段）
        // 测试：只要埋下的牌数等于 8 就算合法
        public Judgement IsLegalBury(Card[] cards, int playerId)
        {
#if (FRY)
            string message;
            bool isValid;
            //牌数不为8张则不合法
            if (cards.Length != bottomCardNumber)
            {
                message = "必须埋 8 张底牌";
                isValid = false;
                return new Judgement(message, isValid);
            }

            List<Card> handcard = new List<Card>();
            playersHandCard[playerId].ForEach(a => handcard.Add(a));
            //检查这8张牌是否出自手牌
            for (int k = 0; k < bottomCardNumber; k++)
            {
                if (handcard.Contains(cards[k]))
                    handcard.Remove(cards[k]);
                else
                {
                    message = "亮牌和手牌不能对应，亮牌有误";
                    isValid = false;
                    return new Judgement(message, isValid);
                }

            }


            for (int i = 0; i < cards.Length; i++)
            {
                //存在大王、小王或台上主级牌且该台上牌为非分牌则不合法
                if (cards[i].suit == Card.Suit.Joker0 || cards[i].suit == Card.Suit.Joker1 ||
                    (isUpperNumber(cards[i].points + 1) && cards[i].points != 4 && cards[i].points != 9 && cards[i].points != 12))
                {
                    message = "存在大王、小王或台上主级牌且该台上牌为非分牌";
                    isValid = false;
                    return new Judgement(message, isValid);
                }
                //若存在台上方级数牌且为分牌，则必须所有非台上方级数牌分牌出完才能埋该牌
                else if (isUpperNumber(cards[i].points + 1) && (cards[i].points == 4 || cards[i].points == 9 || cards[i].points == 12))
                {
                    handcard.Clear();
                    playersHandCard[playerId].ForEach(a => handcard.Add(a));

                    for (int k = 0; k < bottomCardNumber; k++)
                    {
                        handcard.Remove(cards[k]);
                    }

                    int[] scorecard = { 4, 9, 12 };
                    for (int k = 0; k < 3; k++)
                        if (!isUpperNumber(scorecard[k] + 1))
                            foreach (Card a in handcard)
                                if (a.points == scorecard[k])
                                {
                                    message = "不存在台上方级数牌且为分牌，或者并非所有非台上方级数牌分牌出完";
                                    isValid = false;
                                    return new Judgement(message, isValid);
                                }
                }
            }
            message = "合法埋底";
            isValid = true;
            return new Judgement(message, isValid);
#else
            bool isValid = cards.Length == bottomCardNumber;
            string message;
            if (isValid)
            {
                message = "合法埋底";
            }
            else
            {
                message = "必须埋 8 张底牌";
            }
            return new Judgement(message, isValid);
#endif
        }

        // 炒底阶段：判断是否不可能有更高筹码者
        //public bool NoHigerFry()
        //{
        //    // 测试：当要求亮牌不比最大亮牌数小时，判定已经没有更高筹码了
        //    return fryCardLowerBound >= m_fryCardLimit;
        //}

        //// 判断是否不可能有更高出价者(炒底阶段)
        //public bool NoHigherFry()
        //{
        //    return fryCardLowerBound >= m_fryCardLimit;
        //}
        //// 是否所有玩家跳过炒底阶段
        //public bool AllSkipFry()
        //{
        //    return true;
        //}
        // 判断是否结束炒底
        public bool FryEnd()
        {
#if (FRY)
            bool allSkipFry = skipFryCount >= playerNumber - 1;

            if (allSkipFry)
            {
                Console.WriteLine("所有玩家跳过炒牌, 炒底结束");
            }
            return allSkipFry;
#else
            // 测试：当要求亮牌不比最大亮牌数小时，判定已经没有更高筹码了
            //bool noHigherFry = fryCardLowerBound >= m_fryCardLimit;
            bool allSkipFry = skipFryCount >= playerNumber - 1;
            //if (noHigherFry)
            //{
            //    Console.WriteLine("不可能有更高出价者, 炒底结束");
            //}
            if (allSkipFry)
            {
                Console.WriteLine("所有玩家跳过炒牌, 炒底结束");
            }
            return /*noHigherFry || */allSkipFry;
#endif
        }

        /// <summary>
        /// 更新炒底亮牌历史记录
        /// </summary>
        public void UpdateFryShowHistory()
        {
#if (FRY)
            fryMoves++;
            // 如果炒了一圈
            if (fryMoves % playerNumber == 0)
            {
                //showCards.CopyTo(showCardsHistory, 0);
                // 深复制
                for (int i = 0; i < showCards.Length; i++)
                {
                    showCardsHistory[i] = new List<Card>(showCards[i]);
                }
            }
#else

#endif
        }

        /// <summary>
        /// 判断是否为台上方的级数牌
        /// </summary>
        /// <param name="level">某数字</param>
        /// <returns></returns>
        public bool isUpperNumber(int level)
        {
            bool flag = false;
            for (int i = 0; i < m_upperPlayersId.Count(); i++)
            {
                if (level == playerLevels[m_upperPlayersId[i]])
                {
                    flag = true;
                    break;
                }
            }
            return flag;

        }


        /// <summary>
        /// 炒底阶段，帮指定玩家代理亮牌
        /// </summary>
        /// <param name="playerId">当前玩家</param>
        /// <returns>有得炒则返回Card[],否则返回空的Card[]！！！！！</returns>
        public Card[] AutoAddShowCard(int playerId)
        {
#if (FRY)
            CardList playershandcard = new CardList();
            playershandcard = ListCardToCardList(playersHandCard[playerId]);
            Card[] addShowCards = new Card[2];

            List<Card> formerCard = new List<Card>();
            if (lastFryShowPlayerId == -1)
            {
                gotBottomShowCards.ForEach(a => formerCard.Add(a));
            }
            else
            {
                showCards[lastFryShowPlayerId].ForEach(a => formerCard.Add(a));
            }


            for (int i = 0; i < m_upperPlayersId.Count(); i++)
            {
                //台上方级数牌
                //构造四种花色的台上方级数牌，方便得到这四种花色对应的下标
                Card[] temp = new Card[4];
                for (int k = 0; k < 4; k++)
                {
                    temp[k] = new Card((Card.Suit)k, playerLevels[m_upperPlayersId[i]] - 1);
                }
                //确定这四种花色的台上方级数牌是否数量足够
                for (int j = 0; j < 4; j++)
                    if (playershandcard.data[temp[j].CardToIndex()] > formerCard.Count())
                    {
                        int n = playershandcard.data[temp[j].CardToIndex()];
                        addShowCards = new Card[n];
                        for (int k = 0; k < n; k++)
                        {
                            addShowCards[k] = new Card((Card.Suit)j, temp[j].points);
                        }
                        return addShowCards;
                    }
            }
            //小王
            if ((playershandcard.data[52] == formerCard.Count() && formerCard.Last().points != 13)
                || playershandcard.data[52] > formerCard.Count())
            {
                int n = playershandcard.data[52];
                addShowCards = new Card[n];
                for (int k = 0; k < n; k++)
                {
                    addShowCards[k] = new Card((Card.Suit)4, 13);
                }
                return addShowCards;

            }
            if ((playershandcard.data[53] == formerCard.Count() && formerCard.Last().suit != Card.Suit.Joker1)
                || playershandcard.data[53] > formerCard.Count())
            {
                int n = playershandcard.data[53];
                addShowCards = new Card[n];
                for (int k = 0; k < n; k++)
                {
                    addShowCards[k] = new Card((Card.Suit)5, 13);
                }
                return addShowCards;
            }

            Console.WriteLine("没有合适的牌可以炒");
            addShowCards = new Card[0];
            return addShowCards;

#else
            // 随便找到合法的亮牌给玩家
            // 如果已经没有合法的亮牌了
            if (fryCardLowerBound >= m_fryCardLimit)
            {
                return new Card[0];
            }
            // 否则，如果还有的亮
            else
            {
                // 随便从玩家的手牌中抽出比亮牌下界多一张牌
                Card[] showCardTips = new Card[fryCardLowerBound + 1];
                playersHandCard[playerId].CopyTo(0, showCardTips, 0, showCardTips.Length);
                return showCardTips;
            }
#endif
        }
        // 炒底阶段，帮指定玩家代理埋底
        public Card[] AutoBuryCard(int playerId)
        {
#if (FRY)
            Card[] buryCards = new Card[bottomCardNumber];
            // 从手牌中选取

            List<Card> temp = new List<Card>();
            foreach (Card a in playersHandCard[playerId])
            {
                //去掉手牌中的大小王，台上且非分牌
                if (a.suit != Card.Suit.Joker0 && a.suit != Card.Suit.Joker1 &&
                    (!isUpperNumber(a.points + 1) || a.points == 4 || a.points == 9 || a.points == 12))
                    temp.Add(a);
            }

            //若剩下的牌中非台上分牌足够8张，去掉剩下的台上分牌
            int count = 0;
            foreach (Card a in temp)
            {
                //计算非台上分牌数目
                if (!isUpperNumber(a.points + 1) && (a.points == 4 || a.points == 9 || a.points == 12))
                    count++;
            }
            if (count >= bottomCardNumber)
            {
                for (int j = temp.Count() - 1; j >= 0; j--)  //从尾到头遍历，防止下标变化打乱
                {
                    Card a = new Card(temp[j].suit, temp[j].points);
                    if (isUpperNumber(a.points + 1) && (a.points == 4 || a.points == 9 || a.points == 12))
                        temp.Remove(a);
                }
            }

            //则只有台下分牌不够8张（本身概率很小），且抽到台上非分牌（概率比原先小很多）而分牌没有全出时才会 !islegalbury 
            Card[] tempCards = temp.ToArray();
            while (true)
            {
                //Random rnd = new Random();
                //buryCards = temp.OrderBy(x => rnd.Next()).Take(bottomCardNumber).ToArray();

                Card.Shuffle(tempCards);
                buryCards = tempCards.Take(bottomCardNumber).ToArray();

                if (IsLegalBury(buryCards, playerId).isValid)
                    return buryCards;
            }
#else
            // 随便从玩家的手中选 8 张牌作为提示
            Card[] buryCardTips = new Card[bottomCardNumber];
            playersHandCard[playerId].CopyTo(0, buryCardTips, 0, bottomCardNumber);
            return buryCardTips;
#endif
        }
        /// <summary>
        /// 根据指定玩家的亮牌点数（不能是大小鬼），确定新的庄家 ID
        /// </summary>
        /// <param name="playerId">亮牌的玩家 ID</param>
        /// <param name="points">亮牌的点数；一定要是0~12</param>
        /// <returns>由此确定的庄家 ID</returns>
        int GetBankerId(int playerId, int points)
        {
            // 如果他是台上方
            if (IsUpperPlayer(playerId))
            {
                // 如果亮出大王、小王或自己台上级主牌
                if (points + 1 == playerLevels[playerId])
                {
                    // 自己做庄
                    return playerId;
                }
                else
                {
                    // 找到上一个是亮牌点数的台上方
                    return GetLastUpperPlayerId(playerId, points + 1);
                }
            }
            // 如果他是台下方
            else
            {
                // 找到上一个是亮牌点数的台上方
                return GetLastUpperPlayerId(playerId, points + 1);
            }
        }

        /// <summary>
        /// 专门处理最后亮牌的是台下方，亮的还是大小鬼的情形
        /// 尽可能确定台上方高级玩家为庄家
        /// </summary>
        /// <param name="id">亮牌玩家的 ID</param>
        /// <returns></returns>
        void UpdateBankerFry(int id)
        {
#if (FRY)
            // 如果台上方玩家只有 1 个
            if (m_upperPlayersId.Length == 1)
            {
                // 那庄家肯定非他莫属了
                // 如果他还不是庄家
                //AddBanker(m_upperPlayersId[0]);
                bankerPlayerId[0] = m_upperPlayersId[0];
            }
            // 否则，如果有多个台上方玩家
            else
            {
                // 找到上一家能够用来确定庄家的亮牌
                // 先从有当前亮牌的玩家里面逆序找，如果找到台上方，或者是没出大小鬼的台下方，则返回他的亮牌
                // 如果上面找不到，再从有历史亮牌的里面找，如果找到台上方，或者是没出大小鬼的台下方，则返回他的亮牌
                // 如果上面都找不到，那我只能定抢到底牌的玩家为庄家了；也就是不更新庄家了（注意没人抢底的话，直接重新发牌）
                for (int i = 1; i < playerNumber; i++)
                {
                    int thisId = (playerNumber + id - i) % playerNumber;
                    // 如果当前亮牌里面有
                    if (showCards[thisId].Count > 0)
                    {
                        // 获得他的亮牌
                        Card thisCard = showCards[thisId][0];
                        // 如果他是台上方
                        if (IsUpperPlayer(thisId))
                        {
                            // 如果他亮的是大小鬼，或者是他自己的级数
                            if (thisCard.points == 13 || thisCard.points + 1 == playerLevels[thisId])
                            {
                                bankerPlayerId[0] = thisId;
                            }
                            // 亮的是其他级数
                            else
                            {
                                // 找到上一个是亮牌点数的台上方
                                int lastUpperPlayerId = GetLastUpperPlayerId(thisId, thisCard.points + 1);
                                // 他做庄
                                bankerPlayerId[0] = lastUpperPlayerId;
                            }
                        }
                        // 如果他是台下方
                        else
                        {
                            // 如果这人亮了大小鬼
                            if (thisCard.points == 13)
                            {
                                // 跳过
                            }
                            // 否则
                            else
                            {
                                // 找到上一个是亮牌点数的台上方
                                int lastUpperPlayerId = GetLastUpperPlayerId(thisId, thisCard.points + 1);
                                // 他做庄
                                bankerPlayerId[0] = lastUpperPlayerId;
                            }
                        }
                    }
                }
                for (int i = 1; i < playerNumber; i++)
                {
                    int thisId = (playerNumber + id - i) % playerNumber;
                    // 如果历史亮牌里面有
                    if (showCardsHistory[thisId].Count > 0)
                    {
                        // 获得他的亮牌
                        Card thisCard = showCardsHistory[thisId][0];
                        // 如果他是台上方
                        if (IsUpperPlayer(thisId))
                        {
                            // 如果他亮的是大小鬼，或者是他自己的级数
                            if (thisCard.points == 13 || thisCard.points + 1 == playerLevels[thisId])
                            {
                                bankerPlayerId[0] = thisId;
                            }
                            // 亮的是其他级数
                            else
                            {
                                // 找到上一个是亮牌点数的台上方
                                int lastUpperPlayerId = GetLastUpperPlayerId(thisId, thisCard.points + 1);
                                // 他做庄
                                bankerPlayerId[0] = lastUpperPlayerId;
                            }
                        }
                        // 如果他是台下方
                        else
                        {
                            // 如果这人亮了大小鬼
                            if (thisCard.points == 13)
                            {
                                // 跳过
                            }
                            // 否则
                            else
                            {
                                // 找到上一个是亮牌点数的台上方
                                int lastUpperPlayerId = GetLastUpperPlayerId(thisId, thisCard.points + 1);
                                // 他做庄
                                bankerPlayerId[0] = lastUpperPlayerId;
                            }
                        }
                    }
                }
            }
#else

#endif
        }

        /// <summary>
        /// 炒底阶段结束后，更新庄家
        /// </summary>
        public void UpdateBankerFry()
        {
#if (FRY)
            // 如果这是首盘
            if (round == 1)
            {
                // 抢到底牌的是庄家，这有UpdateBankerBid来handle
            }
            // 如果这不是首盘
            else
            {
                // 如果没有人炒底
                if (lastFryShowPlayerId < 0)
                {
                    // 跳过
                }
                // 如果有人炒底
                else
                {
                    // 检查最后炒底亮牌的玩家是不是台上方
                    bool isUpperPlayer = IsUpperPlayer(lastFryShowPlayerId);
                    // 如果是台上方
                    if (isUpperPlayer)
                    {
                        // 如果亮出大王、小王或自己台上级主牌
                        if (lastFryShowCard.points == 13 || lastFryShowCard.points == (playerLevels[lastFryShowPlayerId] - 1) % 13)
                        {
                            // 自己做庄
                            bankerPlayerId[0] = lastFryShowPlayerId;
                        }
                        else
                        {
                            // 找到上一个是亮牌点数的台上方
                            int lastUpperPlayerId = GetLastUpperPlayerId(lastFryShowPlayerId, lastFryShowCard.points + 1);
                            // 他做庄
                            bankerPlayerId[0] = lastUpperPlayerId;
                        }
                    }
                    // 如果不是台上方
                    else
                    {
                        // 如果这人亮了大小鬼
                        if (lastFryShowCard.points == 13)
                        {
                            // 用专门的函数处理
                            //UpdateBankerFry(lastFryShowPlayerId);
                            // 则确定上一次用数牌更新的庄家为庄家
                            bankerPlayerId[0] = bankerIdHistory.Last();
                        }
                        // 否则
                        else
                        {
                            // 找到上一个是亮牌点数的台上方
                            int lastUpperPlayerId = GetLastUpperPlayerId(lastFryShowPlayerId, lastFryShowCard.points + 1);
                            // 他做庄
                            bankerPlayerId[0] = lastUpperPlayerId;
                        }
                    }
                }
            }

#else

#endif
        }

        public void StoreBankerIdHistory(int points)
        {
#if (FRY)
            // 如果不是大小鬼
            if (points >= 0 && points < 13)
            {
                // 存储更新庄家信息
                bankerIdHistory.Add(GetBankerId(currentFryPlayerId, points));
            }
#endif
        }

        // 炒底阶段结束，更新主牌信息
        public void UpdateMainFry()
        {
#if (FRY)
            // 如果有人炒底
            if (lastFryShowPlayerId >= 0)
            {
                // 如果这是首盘
                if (round == 1)
                {
                    // 大家肯定是 1 级
                    mainNumber = 12;
                    switch (lastFryShowCard.suit)
                    {
                        case Card.Suit.Club:
                            mainColor = 2;
                            break;
                        case Card.Suit.Diamond:
                            mainColor = 0;
                            break;
                        case Card.Suit.Heart:
                            mainColor = 3;
                            break;
                        case Card.Suit.Spade:
                            mainColor = 1;
                            break;
                        // 无将
                        case Card.Suit.Joker0:
                        case Card.Suit.Joker1:
                            mainColor = 4;
                            break;
                        default:
                            break;
                    }
                }
                // 如果这不是首盘
                else
                {
                    // 如果最后亮牌的是台上方
                    if (Array.IndexOf(m_upperPlayersId, lastFryShowPlayerId) >= 0)
                    {
                        mainNumber = (lastFryShowCard.points + 1 + 11) % 13;

                        // 最终亮主牌最大一家是台上方，这门主级牌和相应的花色牌就是主牌
                        switch (lastFryShowCard.suit)
                        {
                            case Card.Suit.Club:
                                mainColor = 2;
                                break;
                            case Card.Suit.Diamond:
                                mainColor = 0;
                                break;
                            case Card.Suit.Heart:
                                mainColor = 3;
                                break;
                            case Card.Suit.Spade:
                                mainColor = 1;
                                break;
                            // 无将
                            // 最终亮主牌最大一家是台上方亮出大王或小王，则是台上方主级牌的无将是主牌
                            case Card.Suit.Joker0:
                            case Card.Suit.Joker1:
                                mainNumber = (playerLevels[lastFryShowPlayerId] + 11) % 13;
                                mainColor = 4;
                                break;
                            default:
                                break;
                        }
                    }
                    // 如果最后亮牌的不是台上方
                    else
                    {
                        // 最终亮主牌最大一家是台下方，这门主级牌和相应的花色牌就是主牌
                        mainNumber = (lastFryShowCard.points + 1 + 11) % 13;
                        int lastUpperPlayerId;
                        switch (lastFryShowCard.suit)
                        {
                            case Card.Suit.Club:
                                mainColor = 2;
                                break;
                            case Card.Suit.Diamond:
                                mainColor = 0;
                                break;
                            case Card.Suit.Heart:
                                mainColor = 3;
                                break;
                            case Card.Suit.Spade:
                                mainColor = 1;
                                break;
                            // 无将
                            case Card.Suit.Joker0:
                            case Card.Suit.Joker1:
                                // TODO：一定要确定主级数；找最后一次出现级牌就行
                                // 一定可以找到，至少抢底的时候不可能亮大小鬼，而只可能亮级牌

                                //// 逆序找到最近的台上方玩家
                                //lastUpperPlayerId = GetLastUpperPlayerId(lastFryShowPlayerId);
                                //Card lastUpperPlayerShowCard = showCards[lastUpperPlayerId][0];
                                //// 如果这台上方也出的大小鬼
                                //if (lastUpperPlayerShowCard.suit == Card.Suit.Joker0 || lastUpperPlayerShowCard.suit == Card.Suit.Joker0)
                                //{
                                //    // 这台上方的级数为主级数
                                //    mainNumber = (playerLevels[lastUpperPlayerId] + 11) % 13;
                                //}
                                //// 如果这台上方出的不是大小鬼
                                //else
                                //{
                                //    // 他出的牌的级数是主级数
                                //    mainNumber = (lastUpperPlayerShowCard.points + 1 + 11) % 13;
                                //}

                                // 找到先前最后一次亮的不是大小鬼的牌的点数，以此为主级数
                                mainNumber = (showPointsHistory.FindLast(points => points < 13) + 1 + 11) % 13;
                                mainColor = 4;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
#else

#endif
        }

        // 将指定玩家的亮牌放回去他的手牌
        public void ReturnShowCards(int playerId)
        {
            playersHandCard[playerId].AddRange(showCards[playerId]);
            // 清空亮牌
            showCards[playerId].Clear();
        }

        // 将指定玩家的手牌放到亮牌里去
        public void ShowCards(int playerId, Card[] cards)
        {
            showCards[playerId] = new List<Card>(cards);

            // 从手牌中去掉亮牌
            for (int i = 0; i < cards.Length; i++)
            {
                playersHandCard[playerId].Remove(cards[i]);
            }
        }

        #endregion

        #region FINDFRIEND

        // 生成信号牌
        // 随机抽一张非硬主的牌
        public void GenerateSignCard()
        {
#if (FINDFRIEND)
            // 获取一副牌
            List<Card> cardSet = new List<Card>(Card.GetCardSet());
            // 首先不能抽大小王
            cardSet.Remove(new Card(Card.Suit.Joker0, 13));
            cardSet.Remove(new Card(Card.Suit.Joker1, 13));
            // 其次，不能抽点数是台上方玩家级数的牌
            for (int i = 0; i < m_upperPlayersId.Length; i++)
            {
                cardSet.RemoveAll(card => card.points == (playerLevels[m_upperPlayersId[i]] - 1) % 13);
            }
            // 然后随机抽
            Random rdn = new Random();
            int idx = rdn.Next() % cardSet.Count;
            signCard = cardSet[idx];
#else
            // 随便抽一张牌
            // 获取一副牌
            List<Card> cardSet = new List<Card>(Card.GetCardSet());
            // 然后随机抽
            Random rdn = new Random();
            int idx = rdn.Next() % cardSet.Count;
            signCard = cardSet[idx];
#endif
        }
        #endregion

        #region FIGHT
        // 庄家是否单打
        public bool bankerIsFightAlone;
        // 信号牌
        public Card signCard;

        // 这一轮已经出牌的玩家个数
        private int m_handOutPlayerCount;
        public int handOutPlayerCount
        {
            get
            {
                return m_handOutPlayerCount;
            }
            set
            {
                m_handOutPlayerCount = value % playerNumber;
            }
        }
        // 当前轮数，4 个玩家都出 1 次牌为一轮
        private int m_circle = 1;
        public int circle
        {
            get { return m_circle; }
            set { m_circle = value; }
        }
        // 4 个玩家本轮的出牌
        public List<Card>[] handOutCards;
        // 存储上轮的出牌；用来升级
        public List<Card>[] handOutCardsHistory;
        // 当前出牌最大玩家
        public int biggestPlayerId;
        // 当前每个玩家的分数
        public int[] score;

        // 当前盘数，出完手牌为 1 盘；在积分到发牌的过渡阶段进行更新；-1表示没有设置
        private int m_round = -1;
        public int round
        {
            get { return m_round; }
            set { m_round = value; }
        }

        // 出牌要求牌数
        public int dealRequiredLength { get; set; }
        // 当前首家 ID，即第一个出牌玩家 ID
        public int firstHomePlayerId { get; set; }

        // Player到RulePlayer的转换
        RulePlayer[] PlayerToRulePlayer(/*List<Card>[] res*/)
        {
            RulePlayer[] tmp = new RulePlayer[4];
            for (int i = 0; i < 4; i++)
            {
                tmp[i] = new RulePlayer();
            }

            for (int i = 0; i < 4; i++)
                foreach (Card j in playersHandCard[i])
                    if (j != null)
                        tmp[i].cardInHand.data[j.CardToIndex()]++;
            return tmp;
        }

        // CardArray到CardList的转换
        public CardList CardArrayToCardList(Card[] res)
        {
            CardList tmp = new CardList();
            foreach (Card j in res)
                if (j != null)
                    tmp.data[j.CardToIndex()]++;
            return tmp;
        }

        // CardArray到CardList的转换
        CardList ListCardToCardList(List<Card> res)
        {
            CardList tmp = new CardList();
            foreach (Card j in res)
                if (j != null)
                    tmp.data[j.CardToIndex()]++;
            return tmp;
        }

        Card IndexToCard(int index)
        {
            if (index == 52)
            {
                Card tmp = new Card(4, 13);
                return tmp;
            }
            if (index == 53)
            {
                Card tmp = new Card(5, 13);
                return tmp;
            }
            //if (index % 13 == 12)
            //{
            //    Card tmp = new Card(index / 13, 0);
            //    return tmp;
            //}
            else
            {
                Card tmp = new Card(index / 13, (index + 1) % 13);
                return tmp;
            }
        }


        // CardArray到CardList的转换
        Card[] CardListToCardArray(CardList res)
        {
            int count = 0;
            for (int i = 0; i < 54; i++)
                count += res.data[i];
            //Console.WriteLine(count);

            // 如果 count = -1，则输出各玩家的手牌，人可读的，和机器可读的
            if (count < 0)
            {
                Console.WriteLine(count);

                Console.WriteLine(string.Format("当前主花色 {0}，主级数 {1}", GetMainSuit(), mainNumber));

                Console.WriteLine(string.Format("当前出牌玩家 {0}，首家 {1}", m_currentPlayerId, firstHomePlayerId));

                for (int i = 0; i < handOutCards.Length; i++)
                {
                    Console.WriteLine(string.Format("玩家 {0} 的出牌", i));
                    Card.PrintDeck(handOutCards[i]);
                    int[] cards = Card.ToInt(handOutCards[i]);
                    for (int j = 0; j < cards.Length; j++)
                    {
                        Console.Write(string.Format("{0} ", cards[j]));
                    }
                    Console.WriteLine();
                }

                for (int i = 0; i < playersHandCard.Length; i++)
                {
                    Console.WriteLine(string.Format("玩家 {0} 的手牌", i));
                    Card.PrintDeck(playersHandCard[i]);
                    int[] cards = Card.ToInt(playersHandCard[i]);
                    for (int j = 0; j < cards.Length; j++)
                    {
                        Console.Write(string.Format("{0} ", cards[j]));
                    }
                    Console.WriteLine();
                }

            }


            Card[] tmp = new Card[count];
            int top = -1;
            for (int i = 0; i < 54; i++)
                if (res.data[i] > 0)
                    for (int j = 0; j < res.data[i]; j++)
                    {
                        top++;
                        tmp[top] = IndexToCard(i);
                    }
            return tmp;
        }


        /// <summary>
        /// 判断两张牌的大小
        /// </summary>
        /// <param name="a">牌1，用54个数字表示</param>
        /// <param name="b">牌2，用54个数字表示</param>
        /// <param name="mainColor"></param>
        /// <param name="mainNumber"></param>
        /// <returns>a大于b返回true，否则返回false,！！！！注意，返回false不一定代表a小于b，可能是a,b不在一个区间上</returns>
        bool biggerThan(int a, int b, int mainColor, int mainNumber)
        {
            if (a == -1)
                return false;
            if (b == -1)
                return true;
            if (a == b)
                return false;
            if (mainColor == 4)
            {
                //主牌
                if (a == 53 || a == 52 || a % 13 == mainNumber)
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber)
                    {
                        if (a == 53 || (a == 52 && b != 53))
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else//副牌
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber)
                        return false;
                    else
                    {
                        //同一花色
                        if ((int)a / 13 == (int)b / 13 && a % 13 > b % 13)
                            return true;
                        else
                            return false;
                    }
                }
            }
            else // 有主
            {
                // 主级牌
                if (a == 53 || a == 52 || a % 13 == mainNumber || (int)a / 13 == mainColor)
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber || (int)b / 13 == mainColor)
                    {
                        if (a == 53 || (a == 52 && b != 53) || (a % 13 == mainNumber && (int)a / 13 == mainColor && b < 52)
                            || (a % 13 == mainNumber && (int)a / 13 != mainColor && b % 13 != mainNumber && (int)b / 13 == mainColor)
                            || (a % 13 != mainNumber && (int)a / 13 == mainColor && b % 13 != mainNumber && (int)b / 13 == mainColor && a > b))
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                {
                    if (b == 53 || b == 52 || b % 13 == mainNumber || (int)b / 13 == mainColor)
                        return false;
                    else
                    {
                        if ((int)a / 13 == (int)b / 13 && a % 13 > b % 13)
                            return true;
                        else
                            return false;
                    }
                }
            }
        }


        /// <summary>
        /// 找出长度为k的最大的拖拉机
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        List<int> findtractor(CardList temp, int k, int thisColor, int mainColor, int mainNumber)
        {
            List<int> maxtractor = new List<int>();
            List<int> currenttractor = new List<int>();
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (temp.data[i * 13 + mainNumber] > 1)
                        {
                            currenttractor.Add(i);
                        }
                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            //如果拖拉机长度大于等于k且最大牌较大,则将最大的k张牌赋值到maxtractor中
                            maxtractor.Clear();
                            if (maxtractor.Count() > 0)
                            {
                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }
                                }
                            }
                            else
                            {
                                if (currenttractor.Count() >= k)
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }

                                }
                            }
                            currenttractor.Clear();
                        }

                    }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);
                            }
                            else
                            {
                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();
                            }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }

                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();

                            }
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && temp.data[i * 13 + mainNumber] > 0)
                        {
                            int j = i * 13 + mainNumber;
                            if (temp.data[j] > 1)
                            {
                                currenttractor.Add(j);

                            }
                            else
                            {

                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();

                            }
                        }
                    // 主级牌
                    if (temp.data[mainColor * 13 + mainNumber] > 1)
                    {
                        currenttractor.Add(mainColor * 13 + mainNumber);

                    }
                    else
                    {
                        maxtractor.Clear();
                        if (maxtractor.Count() > 0)
                        {
                            if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                            {
                                for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                {
                                    maxtractor.Add(currenttractor.ElementAt(t));
                                }
                            }
                        }
                        else
                        {
                            if (currenttractor.Count() >= k)
                            {
                                for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                {
                                    maxtractor.Add(currenttractor.ElementAt(t));
                                }

                            }
                        }
                        currenttractor.Clear();

                    }



                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            maxtractor.Clear();
                            if (maxtractor.Count() > 0)
                            {
                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }
                                }
                            }
                            else
                            {
                                if (currenttractor.Count() >= k)
                                {
                                    for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                    {
                                        maxtractor.Add(currenttractor.ElementAt(t));
                                    }

                                }
                            }
                            currenttractor.Clear();


                        }

                    }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                maxtractor.Clear();
                                if (maxtractor.Count() > 0)
                                {
                                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }
                                    }
                                }
                                else
                                {
                                    if (currenttractor.Count() >= k)
                                    {
                                        for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                                        {
                                            maxtractor.Add(currenttractor.ElementAt(t));
                                        }

                                    }
                                }
                                currenttractor.Clear();


                            }
                    if (maxtractor.Count() > 0)
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }
                    else
                    {
                        if (currenttractor.Count() >= k)
                        {
                            for (int t = currenttractor.Count() - k; t < maxtractor.Count(); t++)
                            {
                                maxtractor.Add(currenttractor.ElementAt(t));
                            }
                        }
                    }

                }
            }
            return maxtractor;

        }

        /// <summary>
        /// 找出最长的拖拉机
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        List<int> longtractor(CardList temp, int thisColor, int mainColor, int mainNumber)
        {
            List<int> maxtractor = new List<int>();
            List<int> currenttractor = new List<int>();
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (temp.data[i * 13 + mainNumber] > 1)
                        {
                            currenttractor.Add(i);
                        }
                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            if (currenttractor.Count() > maxtractor.Count())
                            {
                                currenttractor.ForEach(a => maxtractor.Add(a));
                                currenttractor.Clear();
                            }

                        }

                    }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }

                            }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }

                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }

                            }
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && temp.data[i * 13 + mainNumber] > 0)
                        {
                            int j = i * 13 + mainNumber;
                            if (temp.data[j] > 1)
                            {
                                currenttractor.Add(j);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }
                            }
                        }
                    // 主级牌
                    if (temp.data[mainColor * 13 + mainNumber] > 1)
                    {
                        currenttractor.Add(mainColor * 13 + mainNumber);

                    }
                    else
                    {
                        if (currenttractor.Count() > maxtractor.Count())
                        {
                            currenttractor.ForEach(a => maxtractor.Add(a));
                            currenttractor.Clear();
                        }
                    }



                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            if (currenttractor.Count() > maxtractor.Count())
                            {
                                currenttractor.ForEach(a => maxtractor.Add(a));
                                currenttractor.Clear();
                            }
                        }

                    }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() > maxtractor.Count())
                                {
                                    currenttractor.ForEach(a => maxtractor.Add(a));
                                    currenttractor.Clear();
                                }

                            }
                    if (currenttractor.Count() > maxtractor.Count())
                    {
                        currenttractor.ForEach(a => maxtractor.Add(a));
                        currenttractor.Clear();
                    }

                }
            }
            return maxtractor;
        }

        /// <summary>
        /// 判断首位甩牌的合法性
        /// </summary>
        /// <param name="firstCard">首位出牌</param>
        /// <param name="player">四位玩家</param>
        /// <param name="thisplayer">当前玩家</param>
        /// <returns>false不合法，true合法</returns>
        bool canThrow(CardList firstCard, RulePlayer[] player, int thisplayer)
        {
            cardComb fc = new cardComb(firstCard, mainNumber, mainColor);
            if (!fc.valid)
                return false;

            int minf = -1;//当前玩家最小牌minf

            for (int i = 0; i < 54; i++)
            {
                if (firstCard.data[i] > 0)
                    if (minf == -1) minf = i;
                    else
                        minf = biggerThan(minf, i, mainColor, mainNumber) ? i : minf; //此时fc.valid=true，不用担心biggerthan返回false时表示i和minf不在一个区间                                                                                    
            }
            for (int p = 0; p < 4; p++)
            {
                if (p != thisplayer)
                {
                    for (int j = 0; j < 54; j++)
                    {
                        if (player[p].cardInHand.data[j] > 0 && (biggerThan(j, minf, mainColor, mainNumber) || j == minf))
                        {
                            return false;
                        }
                    }
                }

            }

            return true;
        }

        /// <summary>
        /// 判断出牌是否有效
        /// </summary>
        /// <param name="currentPlayer">当前玩家</param>
        /// <param name="winner">当前圈出牌最大玩家</param>
        /// <param name="player">四位玩家</param>
        /// <param name="firstCard">当前圈首位出牌牌面</param>
        /// <param name="maxCard">当前圈最大牌面</param>
        /// <param name="playCard">出的牌组</param>
        /// <returns></returns>
        public
          Judgement canPlay(CardList firstCard, RulePlayer[] player, int thisplayer, bool lastRound)
        {
            cardComb fc = new cardComb(firstCard, mainNumber, mainColor);

            if (!fc.valid)
                return new Judgement("invalid", false);
            if (!fc.thrown)
                return new Judgement("valid", true);
            else //甩牌咯
            {
                if (lastRound) return new Judgement("invalid", false);
                if (canThrow(firstCard, player, thisplayer))
                    return new Judgement("throw", true);
                else
                    return new Judgement("throwFail", false);
            }
        }

        /// <summary>
        /// 牌大小比较
        /// </summary>
        /// <param name="firstCard">首家出牌</param>
        /// <param name="playCard">当前玩家出牌</param>
        /// <param name="handCard">当前玩家手牌</param>
        /// <returns>0 代表不可出，1 代表可出不压制，2 代表可出压制</returns>
        public
          int orderCompare(CardList fC, CardList pC, CardList hC)
        {
            CardList firstCard = new CardList(fC);
            CardList playCard = new CardList(pC);
            CardList handCard = new CardList(hC);

            cardComb fc = new cardComb(firstCard, mainNumber, mainColor),
                pc = new cardComb(playCard, mainNumber, mainColor);

            if (fc.Count != pc.Count) // 总牌数不同肯定拒绝
                return 0;
            int state = 2;
            int firstColor = fc.thisColor;
            int firstSame = fc.thisSame;
            int firstType = fc.thisType;

            //牌数不够（不是没有）该花色牌全部得出，且必输
            //牌数够时，只能出该区间的牌
            //int hcount = 0;
            //int pcount = 0;
            //if (mainColor == 4)
            //{
            //    if (firstColor == 4)
            //    {
            //        int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
            //        for (int i = 0; i < 6; i++)
            //        {
            //            hcount += handCard.data[l[i]];
            //            pcount += playCard.data[l[i]];
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < 13; i++)
            //            if (i != mainNumber)
            //            {
            //                hcount += handCard.data[i + firstColor * 13];
            //                pcount += playCard.data[i + firstColor * 13];
            //            }

            //    }
            //}
            //else
            //{
            //    if (firstColor == mainColor)
            //    {
            //        int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
            //        for (int i = 0; i < 6; i++)
            //        {
            //            hcount += handCard.data[l[i]];
            //            pcount += playCard.data[l[i]];
            //        }
            //        for (int i = 0; i < 13; i++)
            //        {
            //            if (i != mainNumber)
            //            {
            //                hcount += handCard.data[i + mainColor * 13];
            //                pcount += playCard.data[i + mainColor * 13];
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < 13; i++)
            //            if (i != mainNumber)
            //            {
            //                hcount += handCard.data[i + firstColor * 13];
            //                pcount += playCard.data[i + firstColor * 13];
            //            }
            //    }
            //}
            //if (hcount < fc.Count && hcount != 0)
            //{
            //    if (hcount != pcount)
            //        return 0;
            //    else
            //        return 1;
            //}
            //else
            //{
            //    if (pc.thisColor != fc.thisColor && pc.thisColor != mainColor)
            //         return 0;
            //}


            if (fc.thrown)
            {
                //找出所有最长拖拉机
                List<int> maxtractor = new List<int>(); //firstCard中的拖拉机
                List<int> htractor = new List<int>();   //handCard中的对应拖拉机
                List<int> ptractor = new List<int>();   //playCard中的对应拖拉机
                int maxlength;  //handCard中最大但不大于firstCard的拖拉机长度
                int maxorder;   //maxorder同

                //找出firstCard所有拖拉机
                while (true)
                {
                    maxtractor.Clear();
                    htractor.Clear();
                    ptractor.Clear();
                    maxlength = -1;

                    //找出firstCard中最长拖拉机
                    maxtractor = longtractor(firstCard, fc.thisColor, mainColor, mainNumber);

                    int length = maxtractor.Count();
                    //若为拖拉机，则找handCard和playCard中的对应拖拉机
                    if (length > 1)
                    {
                        //首位出牌是主花色或当前出牌不是主花色时才需要考虑强制出牌
                        if (pc.thisColor != mainColor || fc.thisColor == mainColor)
                        {
                            //找出handCard中的playCard花色的最长拖拉机
                            maxlength = length;
                            maxorder = 4;
                            while (true)
                            {
                                htractor = findtractor(handCard, maxlength, pc.thisColor, mainColor, mainNumber);

                                //htractor为空，继续找拖拉机
                                if (!htractor.Any() && maxlength > 1)
                                    maxlength--;
                                //htractor为空，找maxorder同
                                else if (!htractor.Any() && maxlength == 1 && maxorder > 0)
                                {
                                    bool maxflag = false; //记录是否有maxorder同

                                    if (mainColor == 4)  //无主
                                    {
                                        if (pc.thisColor == mainColor)
                                        {
                                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                            for (int i = 0; i < 6; i++)
                                                if (handCard.data[l[i]] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[l[i]] -= maxorder;
                                                    break;
                                                }
                                        }
                                        else
                                        {
                                            for (int i = 0; i < 13; i++)
                                                if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                    break;
                                                }
                                        }
                                    }
                                    else//有主
                                    {
                                        if (pc.thisColor == mainColor)
                                        {
                                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                            for (int i = 0; i < 6; i++)
                                                if (handCard.data[l[i]] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[l[i]] -= maxorder;
                                                    break;
                                                }
                                            for (int i = 0; i < 13; i++)
                                                if (i != mainNumber && handCard.data[i + mainColor * 13] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[i + mainColor * 13] -= maxorder;
                                                    break;
                                                }
                                        }
                                        else
                                        {
                                            for (int i = 0; i < 13; i++)
                                                if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                    break;
                                                }
                                        }
                                    }
                                    //handCard中有maxorder同则跳出
                                    if (maxflag == true)
                                        break;
                                    maxorder--;
                                }

                                else
                                    break;
                            }
                            //handCard中存在拖拉机
                            if (maxlength > 1)
                            {
                                ptractor = findtractor(playCard, maxlength, pc.thisColor, mainColor, mainNumber);
                                //没有做到强制出牌，不合法
                                if (!ptractor.Any())
                                    return 0;
                            }
                            //handCard中没有拖拉机，找maxorder同
                            else
                            {
                                bool pflag = false; //用于记录playCard中是否有maxorder同
                                                    //playCard中找该花色的最大同
                                for (int i = 0; i < 54; i++)
                                {
                                    if (playCard.data[i] >= maxorder)
                                    {
                                        pflag = true;
                                        playCard.data[i] -= maxorder;
                                        break;
                                    }
                                }
                                //不满足强制性
                                if (!pflag)
                                    return 0;
                            }

                        }
                        //主牌压制，不需要考虑强制性，一旦整齐则压制
                        else
                        {
                            ptractor = findtractor(playCard, length, pc.thisColor, mainColor, mainNumber);
                            //不能匹配到同样长度的拖拉机，不整齐
                            if (!ptractor.Any())
                                return 1;
                        }
                    }
                    //没有拖拉机则结束循环
                    else
                    {
                        break;

                    }
                    //合法则去掉最长拖拉机后继续循环
                    foreach (int j in maxtractor)
                        firstCard.data[j] -= 2;
                    foreach (int j in ptractor)
                        playCard.data[j] -= 2;
                    foreach (int j in htractor)
                        handCard.data[j] -= 2;

                }



                //四同 三同 对子 单个 

                maxorder = -1;//handCard中playCard花色的最大同
                for (int k = 4; k > 0; k--)
                {
                    bool flag;
                    while (true)
                    {
                        flag = false;
                        //找firstCard中有没有该牌
                        for (int i = 0; i < 54; i++)
                            if (firstCard.data[i] == k)
                            {
                                flag = true;
                                firstCard.data[i] -= k;
                                break;
                            }
                        if (flag == false)
                            break;

                        //考虑强制性
                        if (pc.thisColor != mainColor || fc.thisColor == mainColor)
                        {
                            //handCard中playCard花色的最大同
                            maxorder = k;
                            for (maxorder = k; maxorder > 0; maxorder--)
                            {
                                bool maxflag = false; //记录是否有maxorder同
                                                      //无主
                                if (mainColor == 4)
                                {
                                    if (pc.thisColor == mainColor)
                                    {
                                        int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                        for (int i = 0; i < 6; i++)
                                            if (handCard.data[l[i]] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[l[i]] -= maxorder;
                                                break;
                                            }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < 13; i++)
                                            if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                break;
                                            }
                                    }
                                }
                                else//有主
                                {
                                    if (pc.thisColor == mainColor)
                                    {
                                        int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                        for (int i = 0; i < 6; i++)
                                            if (handCard.data[l[i]] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[l[i]] -= maxorder;
                                                break;
                                            }
                                        for (int i = 0; i < 13; i++)
                                            if (i != mainNumber && handCard.data[i + mainColor * 13] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[i + mainColor * 13] -= maxorder;
                                                break;
                                            }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < 13; i++)
                                            if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                break;
                                            }
                                    }
                                }
                                //handCard中有maxorder同则跳出
                                if (maxflag == true)
                                    break;

                            }

                            bool pflag = false; //用于记录playCard中是否有maxorder同
                                                //playCard中找该花色的最大同
                            for (int i = 0; i < 54; i++)
                            {
                                if (playCard.data[i] >= maxorder)
                                {
                                    pflag = true;
                                    playCard.data[i] -= maxorder;
                                    break;
                                }
                            }
                            //不满足强制性
                            if (!pflag)
                                return 0;

                        }
                        else//不需要考虑强制性，不整齐就不压制
                        {
                            bool pflag = false;
                            for (int i = 0; i < 54; i++)
                            {
                                if (playCard.data[i] == k)
                                {
                                    pflag = true;
                                    playCard.data[i] -= k;
                                    break;
                                }
                            }
                            if (pflag == false)
                                return 1;
                        }
                    }
                    //说明firstCard中没有k同
                    if (flag == false)
                        continue;
                    //考虑强制性，能走到这一步说明合理，但不压制

                }
                //考虑强制性，能走到这一步说明合法，但不压制
                if (pc.thisColor != mainColor || fc.thisColor == mainColor)
                    return 1;
                else//不考虑强制性，能走到这一步说明整齐，压制
                    return 2;
            }

            else // 首家非甩牌
            {
                // 四同 三同 对子 单张
                if (firstType <= 4 && firstType >= 1)
                {
                    int maxFirst = -1, maxPlay = -1;

                    for (int i = 0; i < 54; i++)
                    {
                        if (playCard.data[i] == firstSame && biggerThan(i, maxPlay, mainColor, mainNumber))
                            maxPlay = i;
                        if (firstCard.data[i] == firstSame && biggerThan(i, maxFirst, mainColor, mainNumber))
                            maxFirst = i;
                    }

                    //四同、三同先看是否存在连续的对应
                    if (firstType == 4 || firstType == 3)
                    {
                        int s = firstSame;
                        int firstLength = fc.data.Count;
                        int maxh = 0, maxhmax = -1; // Hand 手中最长连续长度maxh与起始点maxhmax
                        int maxp = 0, maxpmax = -1; // Play 打出的最长连续长度maxp与起始点maxpmax

                        // 在手牌中寻找
                        if (mainColor == 4)//无主
                        {
                            if (firstColor == mainColor)//主牌
                            {
                                // 在handCard中寻找最大匹配长度
                                int ctn = 0;                                                                                        // ctn连续计数
                                int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 }; // 要考虑的牌
                                for (int i = 0; i < 4; i++)
                                {
                                    if (handCard.data[l[i]] >= s)
                                        ctn++;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = l[i];
                                    }
                                }
                                for (int i = 4; i < l.Length; i++)
                                {
                                    if (handCard.data[l[i]] >= s)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = l[i];
                                    }
                                }

                                // 若现有连续长度超过首家出牌长度
                                if (maxh > fc.Count / s)
                                    maxh = fc.Count / s;

                                // 在playCard寻找相应匹配
                                ctn = 0; // ctn连续计数
                                for (int i = 0; i < 4; i++)
                                {
                                    if (playCard.data[l[i]] >= s)
                                        ctn++;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = l[i];
                                    }
                                }
                                for (int i = 4; i < l.Length; i++)
                                {
                                    if (playCard.data[l[i]] >= s)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = l[i];
                                    }
                                }

                                //强制出牌
                                if (maxp == maxh) // 连续配足了长度
                                {
                                    // 不需垫牌
                                    if (maxp == fc.Count / s)
                                    {
                                        if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    else // 不能压制
                                        state = 1;
                                }
                                else
                                    return 0; //  连续没配足长度
                            }
                            else // 首家出副牌
                            {
                                // 检查手牌的副牌段
                                int ctn = 0;
                                bool viceOccur = false;
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (handCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        if (handCard.data[i + firstColor * 13] >= s)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxh)
                                        {
                                            maxh = ctn;
                                            maxhmax = i + firstColor * 13;
                                        }
                                    }

                                if (viceOccur) // 手牌有副牌
                                {
                                    if (maxh >= fc.Count / s)
                                        maxh = fc.Count / s;

                                    // 检查出牌的副牌段
                                    for (int i = 0; i < 13; i++)
                                        if (i != mainNumber)
                                        {
                                            if (playCard.data[i + firstColor * 13] > 0)
                                                viceOccur = true;
                                            if (playCard.data[i + firstColor * 13] >= s)
                                                ctn++;
                                            else
                                                ctn = 0;
                                            if (ctn > maxp)
                                            {
                                                maxp = ctn;
                                                maxpmax = i + firstColor * 13;
                                            }
                                        }

                                    if (maxp == maxh) // 是否配足连续
                                    {
                                        if (maxp == fc.Count / s) // 不用垫牌
                                        {
                                            if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                                return 2;
                                            else
                                                return 1;
                                        }
                                        else
                                            state = 1;
                                    }
                                    else
                                        return 0;
                                }
                                else // 手牌没有副牌，看是否用主牌压制，否则返回1
                                {
                                    ctn = 0; // ctn连续计数
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (playCard.data[i * 13 + mainNumber] >= s)
                                            ctn++;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i * 13 + mainNumber;
                                        }
                                    }
                                    for (int i = 52; i < 53; i++)
                                    {
                                        if (playCard.data[i] >= s)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i;
                                        }
                                    }

                                    if (maxp == fc.Count / s)
                                        return 2;
                                    else
                                        return 1;
                                }
                            }
                        }
                        else // 打有主
                        {
                            if (firstColor == mainColor)
                            {
                                // 在handCard中寻找最大匹配长度
                                int i;
                                int ctn = 0; // ctn连续计数
                                for (i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (handCard.data[i + mainColor * 13] >= s)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxh)
                                        {
                                            maxh = ctn;
                                            maxhmax = i + mainColor * 13;
                                        }
                                    }
                                bool flag = true;
                                for (i = 0; i < 4; i++)
                                    if (handCard.data[i * 13 + mainNumber] >= s)
                                    {
                                        ctn++;
                                        flag = false;
                                        break;
                                    }
                                if (flag)
                                    ctn = 0;
                                if (ctn > maxh)
                                {
                                    maxh = ctn;
                                    maxhmax = i * 13 + mainNumber;
                                }
                                for (i = 52; i < 54; i++)
                                {
                                    if (handCard.data[i] >= s)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = i;
                                    }
                                }

                                // 在playCard寻找相应匹配
                                ctn = 0; // ctn连续计数
                                for (i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (playCard.data[i + mainColor * 13] >= s)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i + mainColor * 13;
                                        }
                                    }
                                flag = true;
                                for (i = 0; i < 4; i++)
                                    if (playCard.data[i * 13 + mainNumber] >= s)
                                    {
                                        ctn++;
                                        flag = false;
                                        break;
                                    }
                                if (flag)
                                    ctn = 0;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = i * 13 + mainNumber;
                                }
                                for (i = 52; i < 54; i++)
                                {
                                    if (playCard.data[i] >= s)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i;
                                    }
                                }

                                // 若现有连续长度超过首家出牌长度
                                if (maxh > fc.Count / s)
                                    maxh = fc.Count / s;

                                if (maxp == maxh) // 连续配足了长度
                                {
                                    // 不需垫牌
                                    if (maxp == fc.Count / s)
                                    {
                                        if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    else
                                        state = 1;
                                }
                                else
                                    return 0; // 连续没配足长度
                            }
                            else // 首家出副牌
                            {
                                // 检查手牌的副牌段
                                int ctn = 0;
                                bool viceOccur = false;
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (handCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        if (handCard.data[i + firstColor * 13] >= s)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxh)
                                        {
                                            maxh = ctn;
                                            maxhmax = i + firstColor * 13;
                                        }
                                    }

                                if (viceOccur) // 手牌有副牌
                                {
                                    if (maxh >= fc.Count / s)
                                        maxh = fc.Count / s;

                                    // 检查出牌的副牌段
                                    for (int i = 0; i < 13; i++)
                                        if (i != mainNumber)
                                        {
                                            if (playCard.data[i + firstColor * 13] > 0)
                                                viceOccur = true;
                                            if (playCard.data[i + firstColor * 13] >= s)
                                                ctn++;
                                            else
                                                ctn = 0;
                                            if (ctn > maxp)
                                            {
                                                maxp = ctn;
                                                maxpmax = i + firstColor * 13;
                                            }
                                        }

                                    if (maxp == maxh) // 是否配连续
                                    {
                                        if (maxp == fc.Count / s) // 不用垫牌
                                        {
                                            if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                                return 2;
                                            else
                                                return 1;
                                        }
                                        else
                                            state = 1;
                                    }
                                    else
                                        return 0;
                                }
                                else // 手牌没有副牌，看是否用主牌压制，否则返回1
                                {
                                    ctn = 0; // ctn连续计数
                                    int i;
                                    for (i = 0; i < 13; i++)
                                        if (i != mainNumber)
                                        {
                                            if (playCard.data[i + mainColor * 13] >= s)
                                                ctn++;
                                            else
                                                ctn = 0;
                                            if (ctn > maxp)
                                            {
                                                maxp = ctn;
                                                maxpmax = i;
                                            }
                                        }
                                    bool flag = true;
                                    for (i = 0; i < 4; i++)
                                        if (playCard.data[i * 13 + mainNumber] >= s)
                                        {
                                            ctn++;
                                            flag = false;
                                            break;
                                        }
                                    if (flag)
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i * 13 + mainNumber;
                                    }
                                    for (i = 52; i < 54; i++)
                                    {
                                        if (playCard.data[i] >= s)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i;
                                        }
                                    }

                                    if (maxp == fc.Count / s)
                                        return 2;
                                    else
                                        return 1;
                                }
                            }
                        }
                    }


                    //强制出牌但未出者return0；出主牌压制者return 2；手牌中没有与firstcard完全整齐牌state=1；更新firstcard中的最大牌
                    int ac = 0; // 已匹配的牌数
                    for (int i = 0; i < 54; i++)
                        if (firstCard.data[i] == firstSame)
                        {
                            // 在playCard与handCard中寻找匹配
                            if (mainColor == 4)
                            {
                                if (firstColor == 4)
                                {
                                    int sameLevel = -1;//手牌中能匹配的最大同
                                                       // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                    {
                                        if (sameLevel == -1)
                                            for (int k = 0; k < 4; k++)
                                                if (handCard.data[k * 13 + mainNumber] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k * 13 + mainNumber] -= j;
                                                    break;
                                                }
                                        if (sameLevel == -1)
                                            for (int k = 52; k <= 53; k++)
                                                if (handCard.data[k] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                    }
                                    // 若handCard中没有相应
                                    if (sameLevel == -1)
                                    {
                                        state = 1;
                                        continue;
                                    }

                                    // 在playCard中寻找同样匹配级别的，有最大同存在必须出，否则非法
                                    bool flag = false;
                                    for (int k = 0; k < 4; k++)
                                        if (playCard.data[k * 13 + mainNumber] >= sameLevel)
                                        {
                                            playCard.data[k * 13 + mainNumber] -= sameLevel;
                                            ac += sameLevel;
                                            flag = true;
                                            break;
                                        }
                                    if (!flag)
                                        for (int k = 52; k <= 53; k++)
                                            if (playCard.data[k] >= sameLevel)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                ac += sameLevel;
                                                flag = true;
                                                break;
                                            }
                                    if (flag)
                                    {
                                        if (sameLevel != firstSame)
                                            state = 1;
                                    }
                                    else
                                        return 0;
                                }
                                else // 首家出副牌
                                {
                                    int sameLevel = -1;
                                    // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                        if (sameLevel == -1)
                                            for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                                if (handCard.data[k] >= j && (k % 13) != mainNumber)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                    // 若handCard中没有相应 寻求主牌压制
                                    if (sameLevel == -1)
                                    {
                                        if (!pc.thrown && pc.thisColor == mainColor && pc.thisType == fc.thisType)
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    else // 在playCard中寻找同样匹配级别的
                                    {
                                        bool flag = false;
                                        for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                            if (playCard.data[k] >= sameLevel && (k & 13) != mainNumber)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                ac += sameLevel;
                                                flag = true;
                                                break;
                                            }
                                        if (flag)
                                        {
                                            if (sameLevel != firstSame)
                                                state = 1;
                                        }
                                        else
                                            return 0;
                                    }
                                }
                            }
                            else // 打有主
                            {
                                if (firstColor == mainColor)
                                {
                                    int sameLevel = -1;
                                    // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                    {
                                        if (sameLevel == -1)
                                            for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                                if (handCard.data[k] >= j && (k % 13) != mainNumber)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                        if (sameLevel == -1)
                                            for (int k = 0; k < 4; k++)
                                                if (handCard.data[k * 13 + mainNumber] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k * 13 + mainNumber] -= j;
                                                    break;
                                                }
                                        if (sameLevel == -1)
                                            for (int k = 52; k <= 53; k++)
                                                if (handCard.data[k] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                    }

                                    // 若handCard中没有相应
                                    if (sameLevel == -1)
                                    {
                                        state = 1;
                                        continue;
                                    }

                                    // 在playCard中寻找同样匹配级别的
                                    bool flag = false;
                                    for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                        if (playCard.data[k] >= sameLevel && (k % 13) != mainNumber)
                                        {
                                            playCard.data[k] -= sameLevel;
                                            ac += sameLevel;
                                            flag = true;
                                            break;
                                        }
                                    if (!flag)
                                        for (int k = 0; k < 4; k++)
                                            if (playCard.data[k * 13 + mainNumber] >= sameLevel)
                                            {
                                                playCard.data[k * 13 + mainNumber] -= sameLevel;
                                                ac += sameLevel;
                                                flag = true;
                                                break;
                                            }
                                    if (!flag)
                                        for (int k = 52; k <= 53; k++)
                                            if (playCard.data[k] >= sameLevel)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                ac += sameLevel;
                                                flag = true;
                                                break;
                                            }
                                    if (flag)
                                    {
                                        if (sameLevel != firstSame)
                                            state = 1;
                                    }
                                    else
                                        return 0;
                                }
                                else // 首家出副牌
                                {
                                    int sameLevel = -1;
                                    // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                        if (sameLevel == -1)
                                            for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                                if (handCard.data[k] >= j && (k % 13) != mainNumber)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }

                                    // 若handCard中没有相应 寻求主牌压制   
                                    if (sameLevel == -1)
                                    {
                                        if (!pc.thrown && pc.thisColor == mainColor && pc.thisType == fc.thisType)
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    else // 在playCard中寻找同样匹配级别的
                                    {
                                        bool flag = false;
                                        for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                            if (playCard.data[k] >= sameLevel && (k % 13) != mainNumber)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                ac += sameLevel;
                                                flag = true;
                                                break;
                                            }
                                        if (flag)
                                        {
                                            if (sameLevel != firstSame)
                                                state = 1;
                                        }
                                        else
                                            return 0;
                                    }
                                }
                            }
                        }

                    //playcard最大牌比firstcard最大牌小，state=1
                    if (!biggerThan(maxPlay, maxFirst, mainColor, mainNumber))
                    {
                        state = 1;
                    }

                    // 剩下是否补足
                    if (ac < fc.Count)
                    {
                        int tmp1 = 0, tmp2 = 0;
                        if (mainColor == 4)
                        {
                            if (firstColor == 4)
                            {
                                for (int k = 0; k < 4; k++)
                                {
                                    tmp1 += handCard.data[k * 13 + mainNumber];
                                    tmp2 += playCard.data[k * 13 + mainNumber];
                                }
                                for (int k = 52; k <= 53; k++)
                                {
                                    tmp1 += handCard.data[k];
                                    tmp2 += playCard.data[k];
                                }
                            }
                            else // 首家出副牌
                            {
                                for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                    if ((k % 13) != mainNumber)
                                    {
                                        tmp1 += handCard.data[k];
                                        tmp2 += playCard.data[k];
                                    }
                            }
                        }

                        else // 打有主
                        {
                            if (firstColor == mainColor)
                            {

                                for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                    if ((k % 13) != mainNumber)
                                    {
                                        tmp1 += handCard.data[k];
                                        tmp2 += playCard.data[k];
                                    }
                                for (int k = 0; k < 4; k++)
                                {
                                    tmp1 += handCard.data[k * 13 + mainNumber];
                                    tmp2 += playCard.data[k * 13 + mainNumber];
                                }
                                for (int k = 52; k <= 53; k++)
                                {
                                    tmp1 += handCard.data[k];
                                    tmp2 += playCard.data[k];
                                }
                            }

                            else // 首家出副牌
                            {
                                for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                    if ((k % 13) != mainNumber)
                                    {
                                        tmp1 += handCard.data[k];
                                        tmp2 += playCard.data[k];
                                    }
                            }
                        }

                        if (tmp1 > fc.Count - ac)
                            tmp1 = fc.Count - ac;
                        if (tmp2 == tmp1)
                            return 1;
                        else
                            return 0;
                    }

                }

                // 拖拉机
                if (firstType == 0)
                {
                    int firstLength = fc.data.Count;
                    int maxh = 0, maxhmax = -1; // Hand 手中最长拖拉机长度maxh与起始点maxhmax
                    int maxp = 0, maxpmax = -1; // Play 打出的最长拖拉机长度maxp与起始点maxpmax
                    int maxFirst = -1;

                    for (int i = 0; i < 54; i++)
                        if (firstCard.data[i] >= 2 && biggerThan(i, maxFirst, mainColor, mainNumber))
                            maxFirst = i;

                    // 在手牌中寻找
                    if (mainColor == 4)//无主
                    {
                        if (firstColor == mainColor)//主牌
                        {
                            // 在handCard中寻找最大匹配长度
                            int ctn = 0;                                                                                        // ctn连续计数
                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 }; // 要考虑的牌
                            for (int i = 0; i < 4; i++)
                            {
                                if (handCard.data[l[i]] >= 2)
                                    ctn++;
                                if (ctn > maxh)
                                {
                                    maxh = ctn;
                                    maxhmax = l[i];
                                }
                            }
                            for (int i = 4; i < l.Length; i++)
                            {
                                if (handCard.data[l[i]] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxh)
                                {
                                    maxh = ctn;
                                    maxhmax = l[i];
                                }
                            }

                            // 若现有拖拉机长度超过首家出牌长度
                            if (maxh > fc.Count / 2)
                                maxh = fc.Count / 2;

                            // 在playCard寻找相应匹配
                            ctn = 0; // ctn连续计数
                            for (int i = 0; i < 4; i++)
                            {
                                if (playCard.data[l[i]] >= 2)
                                    ctn++;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = l[i];
                                }
                            }
                            for (int i = 4; i < l.Length; i++)
                            {
                                if (playCard.data[l[i]] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = l[i];
                                }
                            }

                            //强制出牌
                            if (maxp == maxh) // 拖拉机配足了长度
                            {
                                // 不需垫牌
                                if (maxp == fc.Count / 2)
                                {
                                    if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                        return 2;
                                    else
                                        return 1;
                                }
                                // 需垫牌，判断垫牌是否合法
                                int tmp1 = 0, tmp2 = 0;
                                for (int i = 0; i < l.Length; i++)
                                {
                                    tmp1 += handCard.data[l[i]];
                                    tmp2 += playCard.data[l[i]];
                                }
                                if (tmp1 > fc.Count)
                                    tmp1 = fc.Count;
                                if (tmp1 == tmp2)
                                    return 1;
                                else
                                    return 0;
                            }
                            else
                                return 0; // 拖拉机没配足长度
                        }
                        else // 首家出副牌
                        {
                            // 检查手牌的副牌段
                            int ctn = 0;
                            bool viceOccur = false;
                            for (int i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (handCard.data[i + firstColor * 13] > 0)
                                        viceOccur = true;
                                    if (handCard.data[i + firstColor * 13] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = i + firstColor * 13;
                                    }
                                }

                            if (viceOccur) // 手牌有副牌
                            {
                                if (maxh >= fc.Count / 2)
                                    maxh = fc.Count / 2;

                                // 检查出牌的副牌段
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (playCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        if (playCard.data[i + firstColor * 13] >= 2)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i + firstColor * 13;
                                        }
                                    }

                                if (maxp == maxh) // 是否配足拖拉机
                                {
                                    if (maxp == fc.Count / 2) // 不用垫牌
                                    {
                                        if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    // 垫牌是否垫足
                                    int tmp1 = 0, tmp2 = 0;
                                    for (int i = 0; i < 13; i++)
                                        if (i != mainNumber)
                                        {
                                            tmp1 += handCard.data[i];
                                            tmp2 += playCard.data[i];
                                        }
                                    if (tmp1 >= fc.Count)
                                        tmp1 = fc.Count;
                                    if (tmp1 == tmp2)
                                        return 1;
                                    else
                                        return 0;
                                }
                                else
                                    return 0;
                            }
                            else // 手牌没有副牌，看是否用主牌压制，否则返回1
                            {
                                ctn = 0; // ctn连续计数
                                for (int i = 0; i < 4; i++)
                                {
                                    if (playCard.data[i * 13 + mainNumber] >= 2)
                                        ctn++;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i * 13 + mainNumber;
                                    }
                                }
                                for (int i = 52; i < 53; i++)
                                {
                                    if (playCard.data[i] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i;
                                    }
                                }

                                if (maxp == fc.Count / 2)
                                    return 2;
                                else
                                    return 1;
                            }
                        }
                    }
                    else // 打有主
                    {
                        if (firstColor == mainColor)
                        {
                            // 在handCard中寻找最大匹配长度
                            int i;
                            int ctn = 0; // ctn连续计数
                            for (i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (handCard.data[i + mainColor * 13] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = i + mainColor * 13;
                                    }
                                }
                            bool flag = true;
                            for (i = 0; i < 4; i++)
                                if (handCard.data[i * 13 + mainNumber] >= 2)
                                {
                                    ctn++;
                                    flag = false;
                                    break;
                                }
                            if (flag)
                                ctn = 0;
                            if (ctn > maxh)
                            {
                                maxh = ctn;
                                maxhmax = i * 13 + mainNumber;
                            }
                            for (i = 52; i < 54; i++)
                            {
                                if (handCard.data[i] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxh)
                                {
                                    maxh = ctn;
                                    maxhmax = i;
                                }
                            }

                            // 在playCard寻找相应匹配
                            ctn = 0; // ctn连续计数
                            for (i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (playCard.data[i + mainColor * 13] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i + mainColor * 13;
                                    }
                                }
                            flag = true;
                            for (i = 0; i < 4; i++)
                                if (playCard.data[i * 13 + mainNumber] >= 2)
                                {
                                    ctn++;
                                    flag = false;
                                    break;
                                }
                            if (flag)
                                ctn = 0;
                            if (ctn > maxp)
                            {
                                maxp = ctn;
                                maxpmax = i * 13 + mainNumber;
                            }
                            for (i = 52; i < 54; i++)
                            {
                                if (playCard.data[i] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = i;
                                }
                            }

                            // 若现有拖拉机长度超过首家出牌长度
                            if (maxh > fc.Count / 2)
                                maxh = fc.Count / 2;

                            if (maxp == maxh) // 拖拉机配足了长度
                            {
                                // 不需垫牌
                                if (maxp == fc.Count / 2)
                                {
                                    if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                        return 2;
                                    else
                                        return 1;
                                }
                                // 判断垫牌是否合法
                                int tmp1 = 0, tmp2 = 0;
                                for (i = 0; i < 13; i++)
                                {
                                    tmp1 += handCard.data[i + mainColor * 13];
                                    tmp2 += playCard.data[i + mainColor * 13];
                                }
                                for (i = 0; i < 4; i++)
                                {
                                    tmp1 += handCard.data[i * mainColor + mainNumber];
                                    tmp2 += playCard.data[i * mainColor + mainNumber];
                                }
                                for (i = 52; i < 54; i++)
                                {
                                    tmp1 += handCard.data[i];
                                    tmp2 += playCard.data[i];
                                }
                                if (tmp1 >= fc.Count)
                                    tmp1 = fc.Count;
                                if (tmp1 == tmp2)
                                    return 1;
                                else
                                    return 0;
                            }
                            else
                                return 0; // 拖拉机没配足长度
                        }
                        else // 首家出副牌
                        {
                            // 检查手牌的副牌段
                            int ctn = 0;
                            bool viceOccur = false;
                            for (int i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (handCard.data[i + firstColor * 13] > 0)
                                        viceOccur = true;
                                    if (handCard.data[i + firstColor * 13] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = i + firstColor * 13;
                                    }
                                }

                            if (viceOccur) // 手牌有副牌
                            {
                                if (maxh >= fc.Count / 2)
                                    maxh = fc.Count / 2;

                                // 检查出牌的副牌段
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (playCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        if (playCard.data[i + firstColor * 13] >= 2)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i + firstColor * 13;
                                        }
                                    }

                                if (maxp == maxh) // 是否配足拖拉机
                                {
                                    if (maxp == fc.Count / 2) // 不用垫牌
                                    {
                                        if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    // 垫牌是否垫足
                                    int tmp1 = 0, tmp2 = 0;
                                    for (int i = 0; i < 13; i++)
                                        if (i != mainNumber)
                                        {
                                            tmp1 += handCard.data[i + firstColor * 13];
                                            tmp2 += playCard.data[i + firstColor * 13];
                                        }
                                    if (tmp1 >= fc.Count)
                                        tmp1 = fc.Count;
                                    if (tmp1 == tmp2)
                                        return 1;
                                    else
                                        return 0;
                                }
                                else
                                    return 0;
                            }
                            else // 手牌没有副牌，看是否用主牌压制，否则返回1
                            {
                                ctn = 0; // ctn连续计数
                                int i;
                                for (i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (playCard.data[i + mainColor * 13] >= 2)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i;
                                        }
                                    }
                                bool flag = true;
                                for (i = 0; i < 4; i++)
                                    if (playCard.data[i * 13 + mainNumber] >= 2)
                                    {
                                        ctn++;
                                        flag = false;
                                        break;
                                    }
                                if (flag)
                                    ctn = 0;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = i * 13 + mainNumber;
                                }
                                for (i = 52; i < 54; i++)
                                {
                                    if (playCard.data[i] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i;
                                    }
                                }

                                if (maxp == fc.Count / 2)
                                    return 2;
                                else
                                    return 1;
                            }
                        }
                    }
                }
                // 只有当跟牌长度相等 且最大牌比首家大 state维持为2 否则state为1
                return state;
            }
        }

        /// <summary>
        /// 判断跟牌是否有效
        /// </summary>
        /// <param name="currentPlayer">当前玩家</param>
        /// <param name="winner">当前圈出牌最大玩家</param>
        /// <param name="player">四位玩家</param>
        /// <param name="firstCard">当前圈首位出牌牌面</param>
        /// <param name="maxCard">当前圈最大牌面</param>
        /// <param name="playCard">出的牌组</param>
        /// <returns></returns>
        public
           Judgement canPlay(CardList firstCard, CardList playCard, CardList handCard)
        {
            switch (orderCompare(firstCard, playCard, handCard))
            {
                case 0:
                    return new Judgement("invalid", false);
                case 1:
                    return new Judgement("notShot", true);
                case 2:
                    if (orderCompare(ListCardToCardList(handOutCards[biggestPlayerId]), playCard, handCard) <= 1)
                        return new Judgement("notShot", true);
                    else
                    {
                        biggestPlayerId = currentPlayerId;
                        return new Judgement("shot", true);
                    }
            }
            return new Judgement("placeholder", true);
        }

        // 判断出牌合法性(对战阶段)
        public Judgement IsLegalDeal(/*PlayerInfo[] m_player,*/ Card[] cards)
        {
#if (FIGHT)
            RulePlayer[] tmp = PlayerToRulePlayer(/*m_player*/);
            CardList playCard = CardArrayToCardList(cards);
            CardList firstCard = ListCardToCardList(handOutCards[firstHomePlayerId]);
            if (currentPlayerId == firstHomePlayerId)
            {
                // 最大一家初始化
                biggestPlayerId = firstHomePlayerId;
                if (playCard.Count == playersHandCard[currentPlayerId].Count)
                    return canPlay(playCard, tmp, currentPlayerId, true);
                else
                    return canPlay(playCard, tmp, currentPlayerId, false);
            }
            else
            {
                return canPlay(firstCard, playCard, tmp[currentPlayerId].cardInHand);
            }
#else
            // 暂且无规则
            if (dealRequiredLength <= 0)
            {
                return new Judgement("", cards.Length > 0);
            }
            else
            {
                return new Judgement("", cards.Length == dealRequiredLength);
            }
#endif
        }

        /// <summary>
        /// 用于首家甩牌失败时，选取他的出牌中最小的一张牌
        /// </summary>
        /// <param name="cards">首家的出牌</param>
        /// <returns>最小的一张牌</returns>
        public Card GetMinCard(Card[] cards)
        {
            return cards[0];
        }


        // 判断是否所有玩家的手牌为空
        public bool AllPlayersHandEmpty()
        {
            // 对于每一个玩家
            for (int i = 0; i < playersHandCard.Length; i++)
            {
                // 如果他还有手牌
                if (playersHandCard[i].Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        // 更新台上方玩家
        public void UpdateUpperPlayers()
        {
#if (FIGHT)

#else
            if (m_round == 1)    // 如果是首盘
            {
                // 所有玩家都是台上方
                m_upperPlayersId = new int[4] { 0, 1, 2, 3 };
            }
            else// 如果不是首盘
            {

            }
#endif
        }

        // 更新首家
        public void UpdateFirstHome()
        {
#if (FIGHT)
            //firstHomePlayerId = 0;
            firstHomePlayerId = biggestPlayerId;
#else

#endif
        }
        // 更新首家
        public void UpdateFirstHome(int res)
        {
#if (FIGHT)
            firstHomePlayerId = res;

#else

#endif
        }


        CardList combination(CardList fC, int fccount, CardList hC, int[] c, int[] l, int done, int now)
        {
            if (done == fccount)
            {
                try
                {
                    CardList pC = new CardList();
                    for (int i = 0; i < fccount; i++)
                        pC.data[c[i]]++;
                    Judgement tmp = canPlay(fC, pC, hC);
                    if (tmp.isValid)
                        return pC;
                    else
                    {
                        pC.data[0] = -1;
                        return pC;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("首家出牌：");
                    Card.PrintDeck(CardListToCardArray(fC));
                    Console.WriteLine("手牌：");
                    Card.PrintDeck(CardListToCardArray(hC));
                    Console.WriteLine("列表：");
                    Console.WriteLine(l);
                    Console.WriteLine("选择：");
                    Console.WriteLine(c);
                }

            }
            if (now + fccount - done >= l.Length)
            {
                CardList pC = new CardList();
                pC.data[0] = -1;
                return pC;
            }
            for (int i = now + 1; i < 1 + l.Length - fccount + done; i++)
            {
                c[done] = l[i];
                CardList tmp = combination(fC, fccount, hC, c, l, done + 1, i);
                if (tmp.data[0] != -1)
                    return tmp;
            }
            CardList pc = new CardList();
            pc.data[0] = -1;
            return pc;
        }

        // 对战阶段，帮指定玩家代理出牌
        public Card[] AutoHandOut()
        {
#if (FIGHT)
            RulePlayer[] rtmp = PlayerToRulePlayer(/*m_player*/);
            CardList firstCard = ListCardToCardList(handOutCards[firstHomePlayerId]);
            CardList handCard = rtmp[currentPlayerId].cardInHand;
            CardList ans = new CardList();
            if (playersHandCard[currentPlayerId].Count > 0)
            {
                if (currentPlayerId == firstHomePlayerId)
                {
                    // 直接找第一个不为零的出
                    for (int i = 0; i < 54; i++)
                        if (handCard.data[i] > 0)
                        {
                            ans.data[i] = 1;
                            break;
                        }
                }
                else
                {
                    cardComb fc = new cardComb(firstCard, mainNumber, mainColor);
                    int firstColor = fc.thisColor;
                    int hcount = 0;
                    if (mainColor == 4)
                    {
                        if (firstColor == 4)
                        {
                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                            for (int i = 0; i < 6; i++)
                                hcount += handCard.data[l[i]];
                        }
                        else
                        {
                            for (int i = 0; i < 13; i++)
                                if (i != mainNumber)
                                    hcount += handCard.data[i + firstColor * 13];

                        }
                    }
                    else
                    {
                        if (firstColor == mainColor)
                        {
                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                            for (int i = 0; i < 6; i++)
                                hcount += handCard.data[l[i]];
                            for (int i = 0; i < 13; i++)
                                if (i != mainNumber)
                                    hcount += handCard.data[i + mainColor * 13];
                        }
                        else
                        {
                            for (int i = 0; i < 13; i++)
                                if (i != mainNumber)
                                    hcount += handCard.data[i + firstColor * 13];
                        }
                    }
                    if (hcount < fc.Count) // 若手牌对应数少于首家出牌数
                    {
                        // 全部都要上 再随便加剩下的牌
                        if (mainColor == 4)
                        {
                            if (firstColor == 4)
                            {
                                int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                for (int i = 0; i < 6; i++)
                                {
                                    ans.data[i] = handCard.data[l[i]];
                                    handCard.data[l[i]] = 0;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        ans.data[i + firstColor * 13] = handCard.data[i + firstColor * 13];
                                        handCard.data[i + firstColor * 13] = 0;

                                    }
                            }
                        }
                        else
                        {
                            if (firstColor == mainColor)
                            {
                                int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                for (int i = 0; i < 6; i++)
                                {
                                    ans.data[i] = handCard.data[l[i]];
                                    handCard.data[l[i]] = 0;
                                }
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        ans.data[i + mainColor * 13] = handCard.data[i + mainColor * 13];
                                        handCard.data[i + mainColor * 13] = 0;
                                    }
                            }
                            else
                            {
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        ans.data[i + firstColor * 13] = handCard.data[i + firstColor * 13];
                                        handCard.data[i + firstColor * 13] = 0;
                                    }
                            }
                        }
                        int tmp = fc.Count - hcount;
                        for (int i = 0; i < 54; i++)
                            if (tmp > 0 && handCard.data[i] > 0)
                            {
                                if (tmp <= handCard.data[i])
                                {
                                    ans.data[i] += tmp;
                                    tmp = 0;
                                }
                                else
                                {
                                    ans.data[i] += handCard.data[i];
                                    tmp -= handCard.data[i];
                                }
                            }
                    }
                    else // 穷举组合
                    {
                        int[] l = new int[hcount];
                        if (mainColor == 4)
                        {
                            if (firstColor == 4)
                            {
                                int[] tmp = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                for (int i = 0; i < 6; i++)
                                    for (int j = 0; j < handCard.data[tmp[i]]; j++)
                                    {
                                        hcount--;
                                        l[hcount] = tmp[i];
                                    }

                            }
                            else
                            {
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                        for (int j = 0; j < handCard.data[i + firstColor * 13]; j++)
                                        {
                                            hcount--;
                                            l[hcount] = i + firstColor * 13;
                                        }
                            }
                        }
                        else
                        {
                            if (firstColor == mainColor)
                            {
                                int[] tmp = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                for (int i = 0; i < 6; i++)
                                    for (int j = 0; j < handCard.data[tmp[i]]; j++)
                                    {
                                        hcount--;
                                        l[hcount] = tmp[i];
                                    }
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                        for (int j = 0; j < handCard.data[i + mainColor * 13]; j++)
                                        {
                                            hcount--;
                                            l[hcount] = i + mainColor * 13;
                                        }
                            }
                            else
                            {
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                        for (int j = 0; j < handCard.data[i + firstColor * 13]; j++)
                                        {
                                            hcount--;
                                            l[hcount] = i + firstColor * 13;
                                        }
                            }
                        }
                        int[] c = new int[fc.Count];
                        ans = combination(firstCard, fc.Count, handCard, c, l, 0, -1);
                    }
                }
                return CardListToCardArray(ans);
            }
            else
            {
                return new Card[0];
            }
#else
            Card[] handOutCards;
            if (playersHandCard[currentPlayerId].Count > 0)
            {
                // 测试：选择指定长度的牌
                if (dealRequiredLength <= 0)
                {
                    handOutCards = new Card[1];
                    Array.Copy(playersHandCard[currentPlayerId].ToArray(), handOutCards, 1);
                }
                else
                {
                    handOutCards = new Card[dealRequiredLength];
                    Array.Copy(playersHandCard[currentPlayerId].ToArray(), handOutCards, dealRequiredLength);
                }
                return handOutCards;
            }
            else
            {
                return new Card[0];
            }
#endif
        }
        public void SetCurrentPlayerId(int id)
        {
            m_currentPlayerId = id;
        }

        public void ClearHandOutCards()
        {
            handOutCardsHistory = new List<Card>[playerNumber];

            for (int i = 0; i < playerNumber; i++)
            {
                handOutCardsHistory[i] = new List<Card>(handOutCards[i]);
                handOutCards[i].Clear();
            }
        }

        // TODO：
        //public void IncrementCurrentPlayerId()
        //{
        //    m_currentPlayerId = (m_currentPlayerId + 1) % playerNumber;
        //}
        public void UpdateNextPlayer()
        {
            // 如果所有玩家都出过牌
            if (handOutPlayerCount == 0)
            {
                // 设置下一出牌玩家为首家
                m_currentPlayerId = firstHomePlayerId;
            }
            else// 如果还有玩家没有出牌
            {
                // 下一玩家出牌
                m_currentPlayerId = (m_currentPlayerId + 1) % playerNumber;
            }
        }

        // 对战阶段更新庄家；出了信号牌的人为庄家
        public void UpdateBankerFight(Card[] dealCards)
        {
            // 如果庄家不选择单打，并且现在只有 1 个庄家
            if (!bankerIsFightAlone && bankerPlayerId.Count < 2)
            {
                int idx = Array.FindIndex(dealCards, card => card == signCard);
                // 如果找得到信号牌
                if (idx >= 0)
                {
                    // 确保不重复设置庄家
                    if (m_currentPlayerId != bankerPlayerId[0])
                    {
                        // 把当前出牌玩家记为庄家
                        //bankerPlayerId.Add(m_currentPlayerId);
                        AddBanker(m_currentPlayerId);
                    }
                    // 如果庄家自己出了信号牌
                    else
                    {
                        // 那就是单打了
                        bankerIsFightAlone = true;
                    }
                    // 将信号牌清除，因为信号牌用一次就没有了
                    signCard = null;
                }
            }
        }

        // 将选牌从玩家手牌去除加到出牌里
        public void HandOut(/*int playerId, */Card[] cards)
        {
            // 更新出牌
            handOutCards[m_currentPlayerId].Clear();
            handOutCards[m_currentPlayerId].AddRange(cards);
            // 更新手牌
            for (int i = 0; i < cards.Length; i++)
            {
                playersHandCard[m_currentPlayerId].Remove(cards[i]);
            }
        }

        public bool IsFisrtPlayerPlaying()
        {
            return m_currentPlayerId == firstHomePlayerId;
        }

        #endregion

        #region SCORE
        /// <summary>
        /// 计算每圈后的得分
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="tablescore"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public void addScore()
        {
#if (SCORE)
            for (int i = 0; i < 4; i++)
                score[biggestPlayerId] += countScore(ListCardToCardList(handOutCards[i]));
#else

#endif
        }

        /// <summary>
        /// 计算一个CardList中含有的分数
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public int countScore(CardList res)
        {
            int score = 5 * (res.data[3] + res.data[16] + res.data[29] + res.data[42])
                 + 10 * (res.data[8] + res.data[11] + res.data[21] + res.data[24] + res.data[34] + res.data[37] + res.data[47] + res.data[50]);
            return score;
        }

        /// <summary>
        /// 升级；以及更新台上方
        /// </summary>
        /// <param name="biggestPlayerId">当前圈出牌最大的玩家</param>
        /// <param name="score">四个玩家的分数</param>
        /// <param name="maxCard">当前圈出牌最大的牌面</param>
        /// <param name="mainNumber">主级数</param>
        /// <param name="mainColor">主花色</param>
        public void addLevel()
        {
#if (LEVEL)
            playerAddLevels = new int[playerNumber];
            CardList maxCard = ListCardToCardList(handOutCardsHistory[biggestPlayerId]);
            //判断底牌分是否有效，并确定分数翻倍倍数
            int bottomscore = countScore(CardArrayToCardList(bottom));
            int totalscore = 0;   //闲家分数总和
            for (int i = 0; i < 4; i++)
            {
                if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                {
                    totalscore += score[i];
                }
            }

            // 记录抄底分数
            bottomSuccessScore = totalscore;

            // 设置抄底玩家
            bottomSuccessID = biggestPlayerId;

            cardComb maxcomb = new cardComb(maxCard, mainNumber, mainColor);
            List<int> tempupper = new List<int>();//暂时存储新台上方

            //判断是否为主牌,是主牌则有底牌分
            if (maxcomb.thisColor == mainColor)
            {
                //闲家拿底牌则加分
                if (bankerPlayerId.Exists((int x) => x == biggestPlayerId ? true : false) == false)
                    totalscore += maxcomb.Count * 4 * bottomscore;
                //庄家拿底牌则减分
                else
                    totalscore -= maxcomb.Count * 4 * bottomscore;
                int rank = 0;
                totalscore -= 160;
                rank = Math.Abs(totalscore) / 80;
                if (totalscore > 0)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerAddLevels[i] += rank;
                            playerLevels[i] += rank;
                            tempupper.Add(i);
                        }

                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerAddLevels[i] += rank;
                            playerLevels[i] += rank;
                            tempupper.Add(i);
                        }
                }

            }
            //无底牌分时升级方法
            else
            {
                totalscore -= 160;
                int rank = System.Math.Abs(totalscore) / 80 + 1;
                if (totalscore == 0)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerAddLevels[i] += 4;
                            playerLevels[i] += 4;
                            tempupper.Add(i);
                        }
                }
                else if (totalscore < 80)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerAddLevels[i] += 2;
                            playerLevels[i] += 2;
                            tempupper.Add(i);
                        }
                }
                else if (totalscore < 160)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerAddLevels[i] += 1;
                            playerLevels[i] += 1;
                            tempupper.Add(i);
                        }
                }
                else if (totalscore < 240)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            tempupper.Add(i);
                        }
                }
                else if (totalscore < 320)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerAddLevels[i] += 1;
                            playerLevels[i] += 1;
                            tempupper.Add(i);
                        }
                }
                else if (totalscore < 400)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerAddLevels[i] += 2;
                            playerLevels[i] += 2;
                            tempupper.Add(i);
                        }
                }
                else if (totalscore == 400)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerAddLevels[i] += 4;
                            playerLevels[i] += 4;
                            tempupper.Add(i);
                        }
                }

            }
            // 更新成功抄底分数
            bottomSuccessScore = Math.Abs(bottomSuccessScore - totalscore);

            //更新台上方
            m_upperPlayersId = new int[tempupper.Count()];
            m_upperPlayersId = tempupper.ToArray();
#else
            // 测试：暂且不升级
#endif
        }
        #endregion


        #region TRANSITION
        /// <summary>
        /// 准备就绪到发牌的过渡阶段
        /// </summary>
        public void Ready2Deal()
        {
            // 清空上一次对战的历史记录
            score = new int[4];
            for (int i = 0; i < 4; i++)
                score[i] = 0;

            m_totalCard = new Card[Card.cardNumberOfOnePack * packNumber];
            for (int i = 0; i < packNumber; i++)
            {
                Card.GetCardSet().CopyTo(m_totalCard, i * Card.cardNumberOfOnePack);
            }
            m_playerCard = new Card[cardInHandInitialNumber * 4];
            m_bottom = new Card[bottomCardNumber];


            playersHandCard = new List<Card>[playerNumber];

            currentBidCards = new List<Card>[playerNumber];
            //currentLegalBidColors = new List<Card.Suit>[playerNumber];
            //Shuffle();
            //Cut();
            // 为炒底阶段玩家亮牌分配内存
            showCards = new List<Card>[playerNumber];
            showCardsHistory = new List<Card>[playerNumber];
            // 为玩家对战阶段出牌分配内存
            handOutCards = new List<Card>[playerNumber];

            for (int i = 0; i < playerNumber; i++)
            {
                playersHandCard[i] = new List<Card>();
                showCards[i] = new List<Card>();
                showCardsHistory[i] = new List<Card>();
                handOutCards[i] = new List<Card>();
                currentBidCards[i] = new List<Card>();
            }
            bankerPlayerId = new List<int>();

            // 如果还没开始
            if (m_round < 0)
            {
                // 设置盘数为 1 
                m_round = 1;
                // 所有玩家都是台上方
                m_upperPlayersId = new int[playerNumber] { 0, 1, 2, 3 };
                // 所有玩家的级数都是 1
                playerLevels = new int[playerNumber] { 1, 1, 1, 1 };
            }
            else
            {

            }



            //playerLevels = new int[playerNumber] {27,27,27,27 };

        }

        // 过渡阶段处理函数

        // 发牌到抢底的过渡函数
        public void Deal2Bid()
        {
            // 清空亮牌
            for (int i = 0; i < currentBidCards.Length; i++)
            {
                currentBidCards[i].Clear();
            }
            for (int i = 0; i < bidOrder.Length; i++)
            {
                bidOrder[i] = -1;
            }

            // 用负数指示没有玩家抢底
            gotBottomPlayerId = -1;
            // 暂时还没有人抢底
            hasBidder = false;
            // 清空数牌记录
            showPointsHistory.Clear();
            // 清空亮数牌更新庄家记录
            bankerIdHistory.Clear();

#if (DEBUG_BID_1)
            m_upperPlayersId = new int[2] { 1, 3 };
#endif

        }

        /// <summary>
        /// 摸牌到最后抢底的过渡阶段
        /// </summary>
        public void Touch2LastBid()
        {

        }

        // 处理最后抢底到埋底的过渡阶段
        public void LastBid2BidBury()
        {
            // 检查一下，是否有玩家抢底
            // 如果没有玩家抢底
            if (gotBottomPlayerId < 0)
            {
                //// 从台上方玩家中随机选择一个作为庄家
                //Random rdn = new Random();
                //int idx = rdn.Next() % upperPlayersId.Length;
                //gotBottomPlayerId = upperPlayersId[idx];
                //// 随机确定这局的花色？gotBottomSuit
            }
            else
            {
                // 将抢底成功者的亮牌花色记录下来
                gotBottomSuit = currentBidCards[gotBottomPlayerId][0].suit;
                gotBottomShowCards = new List<Card>(currentBidCards[gotBottomPlayerId]);
            }
        }

        // 炒底之前的初始化函数
        public void Bid2Fry()
        {
            // 重置不跟的玩家个数
            skipFryCount = 0;
            //更新荷官的最后炒底亮牌
            lastFryShowCard = null;
            // 更新最后亮牌玩家 ID
            lastFryShowPlayerId = -1;

            // 设置当前庄家的下一个玩家为第一个炒底的玩家
            // 因为庄家已经买过底了
            currentFryPlayerId = (gotBottomPlayerId + 1) % playerNumber;
            // 清空荷官存储的炒底阶段亮牌
            ClearShowCards();
            // 重置圈数
            fryMoves = 0;
            // 把庄家亮牌的点数放到点数记录里面
            showPointsHistory.Add(currentBidCards[gotBottomPlayerId][0].points);
            // 把庄家的 ID 存到庄家更新记录里
            bankerIdHistory.Add(gotBottomPlayerId);
        }

        // 抢底之后重新发牌的初始化函数
        public void Bid2Deal()
        {
            // 清空庄家 ID
            bankerPlayerId.Clear();
        }

        // 处理炒底阶段亮牌到埋底过渡阶段
        public void FryShow2Bury()
        {

        }

        // 处理炒底阶段，从埋底重新回到亮牌的过渡流程
        public void FryBury2Show()
        {
            // 设置下一玩家亮牌
            currentFryPlayerId++;
        }
        /// <summary>
        /// 炒底到找朋友过渡阶段
        /// </summary>
        public void Fry2FindFriend()
        {

        }

        /// <summary>
        /// 找完朋友到停留过渡阶段
        /// </summary>
        public void FindFriend2Linger()
        {
            // 设置首家出牌ID
            firstHomePlayerId = bankerPlayerId[0];
        }

        /// <summary>
        /// 找朋友到对战的过渡阶段
        /// </summary>
        public void FindFriend2Fight()
        {
            // 首次出牌对牌数没有限制
            dealRequiredLength = -1;
            // 重置轮数为 1 
            circle = 1;

            // 设置首家亮牌为庄家
            firstHomePlayerId = bankerPlayerId[0];
            // 设置当前出牌玩家为首家
            currentPlayerId = firstHomePlayerId;
        }

        /// <summary>
        /// 对战到积分阶段
        /// </summary>
        public void Fight2Score()
        {

        }

        /// <summary>
        /// 积分到重新发牌的过渡阶段
        /// </summary>
        public void Score2Deal()
        {
            // 计分结束，盘数增加
            round++;
            // 清空玩家分数
            score = new int[playerNumber];
            // 清空庄家 ID
            bankerPlayerId.Clear();
            // 将轮数归零
            m_circle = 0;
        }


        /// <summary>
        /// 亮底牌到重新发牌
        /// </summary>
        public void ShowBottom2Deal()
        {
            // 计分结束，盘数增加
            round++;
            // 清空玩家分数
            score = new int[playerNumber];
            // 清空庄家 ID
            bankerPlayerId.Clear();
            // 将轮数归零
            m_circle = 0;

            // 重置计数器
            score = new int[4];
            for (int i = 0; i < 4; i++)
                score[i] = 0;

            BidTimes = new int[4];
            for (int i = 0; i < 4; i++)
                BidTimes[i] = 0;

            fryTimes = new int[4];
            for (int i = 0; i < 4; i++)
                fryTimes[i] = 0;

            buryTimes = new int[4];
            for (int i = 0; i < 4; i++)
                buryTimes[i] = 0;

            buryScore = new int[4];
            for (int i = 0; i < 4; i++)
                buryScore[i] = 0;

            singleTimes = new int[4];
            for (int i = 0; i < 4; i++)
                singleTimes[i] = 0;


            findFriendTimes = new int[4];
            for (int i = 0; i < 4; i++)
                findFriendTimes[i] = 0;

            bottomSuccessID = -1;
            bottomSuccessScore = 0;

            currentHasBidder = false;

        }
        #endregion

        /// <summary>
        /// 荷官更新函数
        /// </summary>
        public void Update()
        {
            // 如果轮到首家出牌
            if (currentPlayerId == firstHomePlayerId)
            {
                // 重新设置出牌要求长度, 即没有要求
                dealRequiredLength = -1;
            }
        }


        string[] titles = new string[7] { "平民", "士兵", "中尉", "都督", "少将", "卫将军", "大将军" };

        // 将不同等级，现在支持 1~7 级，转换成对应的头衔
        public string GetTitle(int level)
        {
            if (level < 1 || level >= titles.Length)
            {
                return titles[0];
            }
            else
            {
                return titles[level - 1];
            }
        }
    }
}