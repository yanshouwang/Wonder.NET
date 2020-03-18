using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Logger
{
    public enum LogMode
    {
        /// <summary>
        /// 记录所有日志
        /// </summary>
        All = 0,
        /// <summary>
        /// 只记录最新一条日志
        /// </summary>
        Newer = 1
    }
}
