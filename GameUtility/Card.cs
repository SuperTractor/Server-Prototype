using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtility
{
    [Serializable]
    public class Card : IEquatable<Card>
    {
        // Joker0 小王, Joker1 大王
        public enum Suit { Diamond, Spade, Club, Heart, Joker0, Joker1 };
        // 花色
        public Suit suit { get; set; }
        // 点数：0~12 表示 A,2到10,J,Q,K；13表示大小鬼
        public int points { get; set; }
        // 一副牌的牌数
        public const int cardNumberOfOnePack = 54;
        // Constructor
        public Card(Suit _suit, int _points)
        {
            suit = _suit;
            points = _points;
        }

        // 获取 1 副牌
        public static Card[] GetCardSet()
        {
            Card[] card_set = new Card[54];
            // 4 花色
            for (int i = 0; i < 4; i++)
            {
                // 13 点
                for (int j = 0; j < 13; j++)
                {
                    card_set[i * 13 + j] = new Card((Suit)i, j);
                }
            }
            // 大小王
            card_set[52] = new Card(Suit.Joker0, 13);
            card_set[53] = new Card(Suit.Joker1, 13);
            return card_set;
        }
        
        // Rule:求单张卡对应下标
        public int CardToIndex()
        {
            if (suit != Suit.Joker0 && suit != Suit.Joker1)
            {
                if (points == 0)
                    return (int)suit * 13 + 12;
                else
                    return (int)suit * 13 + points-1;
            }
            else if (suit == Suit.Joker0)
                return 52;
            else
                return 53;
        }

        // Card 数组转 4 副牌 int 数组
        public static int[] ToInt(Card[] cards)
        {
            int[] ints = new int[cardNumberOfOnePack];
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    if (cards[i].suit != Suit.Joker0 && cards[i].suit != Suit.Joker1)
                    {
                        ints[(int)cards[i].suit * 13 + cards[i].points]++;
                    }
                    else if (cards[i].suit == Suit.Joker0)
                    {
                        ints[52]++;
                    }
                    else
                    {
                        ints[53]++;
                    }
                }
            }
            return ints;
        }

        // Card 数组转 4 副牌 int 数组
        public static int[] ToInt(List<Card> cards)
        {
            int[] ints = new int[cardNumberOfOnePack];
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null)
                {
                    if (cards[i].suit != Suit.Joker0 && cards[i].suit != Suit.Joker1)
                    {
                        ints[(int)cards[i].suit * 13 + cards[i].points]++;
                    }
                    else if (cards[i].suit == Suit.Joker0)
                    {
                        ints[52]++;
                    }
                    else
                    {
                        ints[53]++;
                    }
                }
            }
            return ints;
        }

        // 4 副牌 int 数组转 Card 数组
        public static Card[] ToCard(int[] ints)
        {
            Card[] cards = new Card[ints.Sum()];
            int count = 0;
            for (int i = 0; i < ints.Length; i++)
            {
                for (int j = 0; j < ints[i]; j++)
                {
                    if (i < cardNumberOfOnePack-2)
                    {
                        cards[count] = new Card((Suit)(i / 13), i % 13);
                    }
                    else if (i == 52)
                    {
                        cards[count] = new Card(Suit.Joker0, 13);
                    }
                    else
                    {
                        cards[count] = new Card(Suit.Joker1, 13);
                    }
                    count++;
                }
            }
            return cards;
        }

        override public string ToString()
        {
            string suit_string = "", points_string = "";

            switch (suit)
            {
                case Suit.Club:
                    suit_string = "\u2663";
                    break;
                case Suit.Diamond:
                    suit_string = "\u2666";
                    break;
                case Suit.Spade:
                    suit_string = "\u2660";
                    break;
                case Suit.Heart:
                    suit_string = "\u2665";
                    break;
                case Suit.Joker0:
                    suit_string = "\u263a";
                    break;
                case Suit.Joker1:
                    suit_string = "\u263b";
                    break;
                default:
                    break;
            }
            if (suit != Suit.Joker0 && suit != Suit.Joker1)
            {
                if (points < 10)
                {
                    points_string = (points + 1).ToString();
                }
                else if (points == 10)
                {
                    points_string = "J";
                }
                else if (points == 11)
                {
                    points_string = "Q";
                }
                else if (points == 12)
                {
                    points_string = "K";
                }
                else { }
            }
            else
            {
                points_string = "";
            }
            return "(" + suit_string + points_string + ")";
        }

        static public void PrintDeck(Card[] deck)
        {
            for (int j = 0; j < deck.Length; j++)
            {
                if (j % 13 == 0 && j > 0)
                {
                    Console.WriteLine(" ");
                    //card_str += "\n";
                }
                if (deck[j] != null)
                {
                    Console.Write("{0}{1}\t",j, deck[j]);
                }
            }
            Console.WriteLine();
        }

        static public void PrintDeck(List<Card> deck)
        {
            for (int j = 0; j < deck.Count; j++)
            {
                if (j % 13 == 0 && j > 0)
                {
                    Console.WriteLine();
                    //card_str += "\n";
                }
                if (deck[j] != null)
                {
                    Console.Write("{0}{1}\t", j, deck[j]);
                }

            }
            Console.WriteLine();
        }
        // 将牌组转化为字符串
        static public string DeckToString(List<Card> deck)
        {
            string display_str = "";
            for(int i = 0; i < deck.Count; i++)
            {
                display_str += i.ToString() + deck[i]+"\t";
            }
            return display_str;
        }

        public bool Equals(Card other)
        {
            if (other == null) return false;
            if (suit == Suit.Joker0 || suit == Suit.Joker1)
            {
                return suit == other.suit;
            }
            else
            {
                return points == other.points && suit == other.suit;
            }
        }

        // 延迟输出牌组, 用于抢底时, 显示手牌, 模拟摸牌效果
        static public void DelayPrint(List<Card> deck,int milliseconds)
        {

        }
    }
}
