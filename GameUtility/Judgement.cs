using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace GameUtility
{
    [Serializable]
    public class Judgement
    {
        // 判决信息
        public string message { get; set; }
        // 操作是否合法
        public bool isValid { get; set; }
        // 构造函数
        public Judgement(string message,bool isValid)
        {
            this.message = message;
            this.isValid = isValid;
        }
    }
}
