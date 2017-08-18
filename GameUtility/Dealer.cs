using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                            int temp = data[data.Count() - 1] % 13;
                            if (flag && ((temp != 11 && mainNumber == 12) || (temp != 12 && mainNumber != 12)))
                            {
                                thrown = true;
                                goto isThrown;
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
                }
            }

            //确定类型 拖拉机 单张 对子 三同 四同
            if (thisSame == 2 && data.Count > 1)
                thisType = 0;
            else
                thisType = thisSame;
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
        public int mainNumber, mainColor;

        // 全牌: 全部 4 副牌
        private Card[] m_totalCard;
        // 8 张底牌
        private Card[] m_bottom;
        // 分发给玩家的手牌
        private Card[] m_playerCard;
        // 玩家当前的手牌
        public List<Card>[] playersHandCard { get; set; }

        // 玩家的级数，由服务器主程序同步进来
        public int[] playerLevels { get; set; }

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
        // 出牌要求牌数
        public int dealRequiredLength { get; set; }
        // 当前首家 ID，即第一个出牌玩家 ID
        public int firstHomePlayerId { get; set; }

        // 玩家总数
        public const int playerNumber = 4;

        // 当前玩家ID
        private int m_currentPlayerId;
        public int currentPlayerId
        {
            get { return m_currentPlayerId; }
            set { m_currentPlayerId = value; }
        }

        // 当前台上方玩家ID
        private int[] m_upperPlayersId;
        public int[] upperPlayersId { get { return m_upperPlayersId; } }

        // 一共多少副牌
        public const int packNumber = 4;
        // 底牌的张数
        public const int bottomCardNumber = 8;
        // 初始时玩家拥有的手牌数
        public const int cardInHandInitialNumber = (packNumber * Card.cardNumberOfOnePack - bottomCardNumber) / 4;
        // 每个玩家手牌的最大数目
        public const int cardInHandMaxNumber = cardInHandInitialNumber + bottomCardNumber;

        // 抢底阶段，当前所有玩家的亮牌
        public List<Card>[] currentBidCards;
        // 每个玩家当前的合法亮牌花色
        //public List<Card.Suit>[] currentLegalBidColors;

        // 抢到底牌的玩家 ID
        public int gotBottomPlayerId { get; set; }
        // 抢到底牌的玩家的亮牌花色
        public Card.Suit gotBottomSuit { get; set; }
        // 抢到底牌的玩家的亮牌
        public List<Card> gotBottomShowCards;

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
        // 庄家的 id
        public List<int> bankerPlayerId;
        // 最后（炒底）亮牌玩家的亮牌（用第一张牌来代表，因为亮牌都是同花色同点数的）
        public Card lastFryShowCard;
        // 最后（炒底）亮牌玩家ID
        public int lastFryShowPlayerId;

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
        // 当前出牌最大玩家
        public int biggestPlayerId;
        // 当前每个玩家的分数
        public int[] score;

        // 当前盘数，出完手牌为 1 盘
        private int m_round = 1;
        public int round
        {
            get { return m_round; }
            set { m_round = value; }
        }

        /// <summary>
        /// 构造函数, 初始化荷官
        /// 获取 4 副牌
        /// </summary>
        public Dealer()
        {
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
                handOutCards[i] = new List<Card>();
                currentBidCards[i] = new List<Card>();
            }
            bankerPlayerId = new List<int>();
        }
        // 洗牌
        public void Shuffle()
        {
            Random rnd = new Random();
            int rand_idx;
            Card temp;
            for (int i = 0; i < m_totalCard.Length; i++)
            {
                rand_idx = rnd.Next(m_totalCard.Length);
                temp = m_totalCard[i];
                m_totalCard[i] = m_totalCard[rand_idx];
                m_totalCard[rand_idx] = temp;
            }
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

        // 抢底阶段：给出此玩家当前可以亮的花色，已知当前摸牌的数目
        public bool[] GetLegalBidColors(int playerId/*, int touchCardNumber*/)
        {
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
                    levelCards = playersHandCard[playerId].FindAll(card => card.points + 1 == playerLevels[playerId]);

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
                    levelCards = playersHandCard[playerId].FindAll(card => card.points + 1 == playerLevels[playerId] && card.suit == currentBidCards[playerId][0].suit);

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
                // 根据其他玩家已经亮的牌决定此玩家可以亮的花色
                for (int i = 0; i < legalBidColors.Length; i++)
                {
                    // 如果先前认定这花色可以出
                    if (legalBidColors[i])
                    {
                        // 计算这花色的摸牌，加上已经亮的牌，总共多少张
                        // 只有当总数大于当前亮牌数最大的玩家，才可以出这个花色的牌
                        int totalNum = levelCards.Count(card => card.suit == (Card.Suit)i) + currentBidCards[playerId].Count;
                        legalBidColors[i] = totalNum > currentBidCards.Max(cards => cards.Count);
                    }
                }
            }
            else
            {
            }
            return legalBidColors;
        }
        // 亮牌需要增加的数目
        //public int BidNeedNumber(int playerId)
        //{
        //    return currentBidCards.Max(cards => cards.Count) + 1 - currentBidCards[playerId].Count;
        //}
        // 抢底阶段：亮牌帮助函数
        public void BidHelper(int playerId, int currentTouchNumber, Card.Suit suit)
        {
            int bidNeedNumber = currentBidCards.Max(cards => cards.Count) + 1 - currentBidCards[playerId].Count;
            // 构造一张级牌
            Card levelCard = new Card(suit, playerLevels[playerId] - 1);
            for (int i = 0; i < bidNeedNumber; i++)
            {
                // 找到级牌所在位置
                //int idx = playersHandCard[playerId].FindIndex(card => card.suit == suit && card.points + 1 == playerLevels[playerId]);
                // 去除一张级牌
                if (!playersHandCard[playerId].Remove(levelCard))
                {
                    Console.WriteLine("抢底：判断级牌数是否足够，出错");
                }
                // 加入亮牌当中
                currentBidCards[playerId].Add(levelCard);
            }
            // 更新当前抢到底牌的玩家 ID
            gotBottomPlayerId = playerId;
        }

        // 清空炒底阶段亮牌记录
        public void ClearShowCards()
        {
            for (int i = 0; i < playerNumber; i++)
            {
                showCards[i].Clear();
                showCardsHistory[i].Clear();
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
        }

        // 判断炒底时增加的筹码牌是否合法
        public Judgement IsLegalShow(Card[] addFryCards, int playerId)
        {
            // 测试：总亮牌数比前一个炒底玩家的多就行
            bool isValid;
            isValid = addFryCards.Length + showCards[playerId].Count > fryCardLowerBound;
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
        }

        // 判断埋底的合法性（炒底阶段）
        // 测试：只要埋下的牌数等于 8 就算合法
        public Judgement IsLegalBury(Card[] cards)
        {
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
        }

        // Player到RulePlayer的转换
        RulePlayer[] PlayerToRulePlayer(PlayerInfo[] res)
        {
            RulePlayer[] tmp = new RulePlayer[4];
            for (int i = 0; i < 4; i++)
            {
                tmp[i] = new RulePlayer();
            }
            for (int i = 0; i < 4; i++)
                foreach (Card j in res[i].cardInHand)
                    if (j != null)
                        tmp[i].cardInHand.data[j.CardToIndex()]++;
            return tmp;
        }

        // CardArray到CardList的转换
        CardList CardArrayToCardList(Card[] res)
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

        /// <summary>
        /// 判断两张牌的大小
        /// </summary>
        /// <param name="a">牌1，用54个数字表示</param>
        /// <param name="b">牌2，用54个数字表示</param>
        /// <param name="mainColor"></param>
        /// <param name="mainNumber"></param>
        /// <returns>a>b返回true，否则返回false</returns>
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
                // 大小王
                if (a == 53 || (a == 52 && b != 53))
                    return true;
                if (b == 53 || (b == 52 && a != 53))
                    return false;
                // 主级牌
                if (a % 13 == mainNumber && b % 13 == mainNumber)
                    return false;
                if (a % 13 == mainNumber && b % 13 != mainNumber)
                    return true;
                if (a % 13 != mainNumber && b % 13 == mainNumber)
                    return false;
                // 副牌
                if (a % 13 > b % 13)
                    return true;
                else
                    return false;
            }
            else // 有主
            {
                // 大小王
                if (a == 53 || (a == 52 && b != 53))
                    return true;
                if (b == 53 || (b == 52 && a != 53))
                    return false;
                // 主级牌
                if (b % 13 == mainNumber && b / 13 == mainColor)
                    return false;
                if (a % 13 == mainNumber && a / 13 == mainColor)
                    return true;
                // 副主级牌
                if (b % 13 == mainNumber && b / 13 != mainColor)
                    return false;
                if (a % 13 == mainNumber && a / 13 != mainColor)
                    return true;
                //主花色牌
                if (a / 13 == mainColor)
                    if (b / 13 == mainColor)
                    {
                        if (a % 13 > b % 13)
                            return true;
                        else
                            return false;
                    }
                    else
                        return true;
                // 副牌
                if (a % 13 > b % 13)
                    return true;
                else
                    return false;
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
                        if (player[p].cardInHand.data[j] > 0 && biggerThan(j, minf, mainColor, mainNumber))
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
            {
                return new Judgement("invalid", false);
            }
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
          int orderCompare(CardList firstCard, CardList playCard, CardList handCard)
        {
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
            int hcount = 0;
            int pcount = 0;
            if (mainColor == 4)
            {
                if (firstColor == 4)
                {
                    int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                    for (int i = 0; i < 6; i++)
                    {
                        hcount += handCard.data[l[i]];
                        pcount += playCard.data[l[i]];
                    }
                }
                else
                {
                    for (int i = 0; i < 13; i++)
                        if (i != mainNumber)
                        {
                            hcount += handCard.data[i + firstColor * 13];
                            pcount += playCard.data[i + firstColor * 13];
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
                        hcount += handCard.data[l[i]];
                        pcount += playCard.data[l[i]];
                    }
                    for (int i = 0; i < 13; i++)
                    {
                        if (i != mainNumber)
                        {
                            hcount += handCard.data[i + mainColor * 13];
                            pcount += playCard.data[i + mainColor * 13];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 13; i++)
                        if (i != mainNumber)
                        {
                            hcount += handCard.data[i + firstColor * 13];
                            pcount += playCard.data[i + firstColor * 13];
                        }
                }
            }
            if (hcount < fc.Count)
            {
                if (hcount != pcount)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (pc.thisColor != fc.thisColor)
                    return 0;
            }


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
                        if (playCard.data[i] == firstSame && biggerThan(i, maxPlay, mainColor, mainNumber))
                            maxPlay = i;

                    //强制出牌但未出者return0；出主牌压制者retur 2；手牌中没有与firstcard完全整齐牌state=1；更新firstcard中的最大牌
                    for (int i = 0; i < 54; i++)
                    {
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
                                            flag = true;
                                            break;
                                        }
                                    if (!flag)
                                        for (int k = 52; k <= 53; k++)
                                            if (playCard.data[k] >= sameLevel)
                                            {
                                                playCard.data[k] -= sameLevel;
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
                                                if (handCard.data[k] >= j && k != mainNumber)
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
                                            if (playCard.data[k] >= sameLevel && k != mainNumber)
                                            {
                                                playCard.data[k] -= sameLevel;
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
                                                if (handCard.data[k] >= j && k != mainNumber)
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
                                        if (playCard.data[k] >= sameLevel && k != mainNumber)
                                        {
                                            playCard.data[k] -= sameLevel;
                                            flag = true;
                                            break;
                                        }
                                    if (!flag)
                                        for (int k = 0; k < 4; k++)
                                            if (playCard.data[k * 13 + mainNumber] >= sameLevel)
                                            {
                                                playCard.data[k * 13 + mainNumber] -= sameLevel;
                                                flag = true;
                                                break;
                                            }
                                    if (!flag)
                                        for (int k = 52; k <= 53; k++)
                                            if (playCard.data[k] >= sameLevel)
                                            {
                                                playCard.data[k] -= sameLevel;
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
                                                if (handCard.data[k] >= j && k != mainNumber)
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
                                            if (playCard.data[k] >= sameLevel && k != mainNumber)
                                            {
                                                playCard.data[k] -= sameLevel;
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

                            // 更新最大
                            if (biggerThan(i, maxFirst, mainColor, mainNumber))
                                maxFirst = i;
                        }
                    }

                    //playcard最大牌比firstcard最大牌小，state=1
                    if (biggerThan(maxFirst, maxPlay, mainColor, mainNumber))
                    {
                        state = 1;
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
                                    if (handCard.data[i + firstColor * 13] >= 2)
                                    {
                                        if (handCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        ctn++;
                                    }
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
                                        if (playCard.data[i + firstColor * 13] >= 2)
                                        {
                                            if (playCard.data[i + firstColor * 13] > 0)
                                                viceOccur = true;
                                            ctn++;
                                        }
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
                                if (handCard.data[i * mainColor + mainNumber] >= 2)
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
                                maxhmax = i * mainColor + mainNumber;
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
                                if (playCard.data[i * mainColor + mainNumber] >= 2)
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
                                maxpmax = i * mainColor + mainNumber;
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
                                    if (handCard.data[i + firstColor * 13] >= 2)
                                    {
                                        if (handCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        ctn++;
                                    }
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
                                        if (playCard.data[i + firstColor * 13] >= 2)
                                        {
                                            if (playCard.data[i + firstColor * 13] > 0)
                                                viceOccur = true;
                                            ctn++;
                                        }
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
                                    if (playCard.data[i * mainColor + mainNumber] >= 2)
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
                                    maxpmax = i;
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
                        return new Judgement("shot", true);
            }
            return new Judgement("placeholder", true);
        }

        // 判断出牌合法性(对战阶段)
        public Judgement IsLegalDeal(PlayerInfo[] m_player, Card[] cards)
        {
            RulePlayer[] tmp = PlayerToRulePlayer(m_player);
            CardList playCard = CardArrayToCardList(cards);
            CardList firstCard = ListCardToCardList(handOutCards[firstHomePlayerId]);
            if (currentPlayerId == firstHomePlayerId)
            {
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

            //// 暂且无规则
            //if (dealRequiredLength <= 0)
            //{
            //    return new Judgement("", cards.Length > 0);
            //}
            //else
            //{
            //    return new Judgement("", cards.Length == dealRequiredLength);
            //}
        }

        /// <summary>
        /// 计算每圈后的得分
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="tablescore"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public void addScore()
        {
            for (int i = 0; i < 4; i++)
                score[biggestPlayerId] += countScore(ListCardToCardList(handOutCards[i]));
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
        /// 升级
        /// </summary>
        /// <param name="biggestPlayerId">当前圈出牌最大的玩家</param>
        /// <param name="score">四个玩家的分数</param>
        /// <param name="maxCard">当前圈出牌最大的牌面</param>
        /// <param name="mainNumber">主级数</param>
        /// <param name="mainColor">主花色</param>
        public void addLevel()
        {
            CardList maxCard = ListCardToCardList(handOutCards[biggestPlayerId]);
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

            cardComb maxcomb = new cardComb(maxCard, mainNumber, mainColor);

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
                rank = System.Math.Abs(totalscore) / 80;
                m_upperPlayersId = null;
                m_upperPlayersId = new int[4];
                if (totalscore > 0)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerLevels[i] += rank;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }

                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerLevels[i] += rank;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }

            }
            //无底牌分时升级方法
            else
            {
                totalscore -= 160;
                int rank = System.Math.Abs(totalscore) / 80 + 1;
                m_upperPlayersId = null;
                m_upperPlayersId = new int[4];
                if (totalscore == 0)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerLevels[i] += 4;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }
                else if (totalscore < 80)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerLevels[i] += 2;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }
                else if (totalscore < 160)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == true)
                        {
                            playerLevels[i] += 1;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }
                else if (totalscore < 240)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }
                else if (totalscore < 320)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerLevels[i] += 1;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }
                else if (totalscore < 400)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerLevels[i] += 2;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }
                else if (totalscore == 400)
                {
                    for (int i = 0; i < 4; i++)
                        if (bankerPlayerId.Exists((int x) => x == i ? true : false) == false)
                        {
                            playerLevels[i] += 4;
                            m_upperPlayersId[m_upperPlayersId.Length] = i;
                        }
                }

            }


        }

        public void SetCurrentPlayerId(int id)
        {
            m_currentPlayerId = id;
        }



        public void ClearHandOutCards()
        {
            for (int i = 0; i < playerNumber; i++)
            {
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
            // 测试：当要求亮牌不比最大亮牌数小时，判定已经没有更高筹码了
            bool noHigherFry = fryCardLowerBound >= m_fryCardLimit;
            bool allSkipFry = skipFryCount >= playerNumber;
            if (noHigherFry)
            {
                Console.WriteLine("不可能有更高出价者, 炒底结束");
            }
            if (allSkipFry)
            {
                Console.WriteLine("所有玩家跳过炒牌, 炒底结束");
            }
            return noHigherFry || allSkipFry;
        }

        /// <summary>
        /// 更新炒底亮牌历史记录
        /// </summary>
        public void UpdateFryShowHistory()
        {
            fryMoves++;
            // 如果炒了一圈
            if (fryMoves % playerNumber == 0)
            {
                //showCards.CopyTo(showCardsHistory, 0);
                // 深复制
                for(int i = 0; i < showCards.Length; i++)
                {
                    showCardsHistory[i] = new List<Card>(showCards[i]);
                }
            }
        }

        // 从指定玩家 ID 开始，逆序找上一个台上方玩家
        int GetLastUpperPlayerId(int id)
        {
            for (int i = 1; i < 4; i++)
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
            for (int i = 1; i < 4; i++)
            {
                int thisId = (playerNumber + id - i) % playerNumber;
                // 如果找到了上一个指定级数的台上方玩家
                if (IsUpperPlayer(thisId) && playerLevels[thisId] == level)
                {
                    return thisId;
                }
            }
            throw new Exception("找不到上一个指定级数的台上方玩家");
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

        /// <summary>
        /// 专门处理最后亮牌的是台下方，亮的还是大小鬼的情形
        /// 尽可能确定台上方高级玩家为庄家
        /// </summary>
        /// <param name="id">亮牌玩家的 ID</param>
        /// <returns></returns>
        void UpdateBankerFry(int id)
        {
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
                for(int i = 1; i < playerNumber; i++)
                {
                    int thisId = (playerNumber + id - i) % playerNumber;
                    // 如果当前亮牌里面有
                    if (showCards[thisId].Count>0)
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
        }

        /// <summary>
        /// 炒底阶段结束后，更新庄家
        /// </summary>
        public void UpdateBankerFry()
        {
            // 如果这是首盘
            if (round == 1)
            {
                // 抢到底牌的是庄家，这有UpdateBankerBid来handle
            }
            // 如果这不是首盘
            else
            {
                // 检查最后炒底亮牌的玩家是不是台上方
                bool isUpperPlayer = IsUpperPlayer(lastFryShowPlayerId);
                // 如果是台上方
                if (isUpperPlayer)
                {
                    // 如果亮出大王、小王或自己台上级主牌
                    if (lastFryShowCard.points == 13 || lastFryShowCard.points + 1 == playerLevels[lastFryShowPlayerId])
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
                        UpdateBankerFry(lastFryShowPlayerId);
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

        // 更新台上方玩家
        public void UpdateUpperPlayers()
        {
            if (m_round == 1)    // 如果是首盘
            {
                // 所有玩家都是台上方
                m_upperPlayersId = new int[4] { 0, 1, 2, 3 };
            }
            else// 如果不是首盘
            {

            }
        }

        // 更新首家
        public void UpdateFirstHome()
        {
            //firstHomePlayerId = 0;
            firstHomePlayerId = biggestPlayerId;
        }
        // 更新首家
        public void UpdateFirstHome(int res)
        {
            firstHomePlayerId = res;
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
                    // 把当前出牌玩家记为庄家
                    //bankerPlayerId.Add(m_currentPlayerId);
                    AddBanker(m_currentPlayerId);
                    // 将信号牌清除，因为信号牌用一次就没有了
                    signCard = null;
                }
            }
        }


        // 发牌结束，更新主牌信息
        public void UpdateMainDeal()
        {
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
        }

        // 抢底阶段结束，更新主牌信息
        public void UpdateMainBid()
        {
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
        }


        // 炒底阶段结束，更新主牌信息
        public void UpdateMainFry()
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
                    // 最终亮主牌最大一家是台上方，这门主级牌和相应的花色牌就是主牌
                    mainNumber = (lastFryShowCard.points + 1 + 11) % 13;
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
                            // 逆序找到最近的台上方玩家
                            lastUpperPlayerId = GetLastUpperPlayerId(lastFryShowPlayerId);
                            Card lastUpperPlayerShowCard = showCards[lastUpperPlayerId][0];
                            // 如果这台上方也出的大小鬼
                            if (lastUpperPlayerShowCard.suit == Card.Suit.Joker0 || lastUpperPlayerShowCard.suit == Card.Suit.Joker0)
                            {
                                // 这台上方的级数为主级数
                                mainNumber = (playerLevels[lastUpperPlayerId] + 11) % 13;
                            }
                            // 如果这台上方出的不是大小鬼
                            else
                            {
                                // 他出的牌的级数是主级数
                                mainNumber = (lastUpperPlayerShowCard.points + 1 + 11) % 13;
                            }
                            mainColor = 4;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // 生成信号牌
        // 随机抽一张非硬主的牌
        public void GenerateSignCard()
        {
            // 获取一副牌
            List<Card> cardSet = new List<Card>(Card.GetCardSet());
            // 首先不能抽大小王
            cardSet.Remove(new Card(Card.Suit.Joker0, 13));
            cardSet.Remove(new Card(Card.Suit.Joker1, 13));
            // 其次，不能抽点数是台上方玩家级数的牌
            for (int i = 0; i < m_upperPlayersId.Length; i++)
            {
                cardSet.RemoveAll(card => card.points == playerLevels[m_upperPlayersId[i]]);
            }
            // 然后随机抽
            Random rdn = new Random();
            int idx = rdn.Next() % cardSet.Count;
            signCard = cardSet[idx];
        }

        // 炒底阶段，帮指定玩家代理亮牌
        public Card[] AutoAddShowCard(int playerId)
        {
            // 随意选择合法的亮牌
            // 测试：牌数比最低筹码大即为合法亮牌
            int n = fryCardLowerBound + 1 - showCards[playerId].Count;
            Card[] addShowCards = new Card[n];
            // 从手牌中选取
            Array.Copy(playersHandCard[playerId].ToArray(), addShowCards, n);
            return addShowCards;
        }
        // 炒底阶段，帮指定玩家代理埋底
        // 测试：埋 8 张牌就是合法的
        public Card[] AutoBuryCard(int playerId)
        {
            // 随意选择合法的埋牌
            Card[] buryCards = new Card[bottomCardNumber];
            // 从手牌中选取
            Array.Copy(playersHandCard[playerId].ToArray(), buryCards, bottomCardNumber);
            return buryCards;
        }

        // 对战阶段，帮指定玩家代理出牌
        public Card[] AutoHandOut(int playerId)
        {
            Card[] handOutCards;
            if (playersHandCard[playerId].Count > 0)
            {
                // 测试：选择指定长度的牌
                if (dealRequiredLength <= 0)
                {
                    handOutCards = new Card[1];
                    Array.Copy(playersHandCard[playerId].ToArray(), handOutCards, 1);
                }
                else
                {
                    handOutCards = new Card[dealRequiredLength];
                    Array.Copy(playersHandCard[playerId].ToArray(), handOutCards, dealRequiredLength);
                }
                return handOutCards;
            }
            else
            {
                return new Card[0];
            }
        }

    }
}