using MyMVVM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.MainWindow
{
    public class IsPeopleDB
    {
        public static void UpdateIsPeopleConfig1(string u)
        {
            DB.ExecuteNonQuery($"update config set ispeople = 1,transfernum={u} where id = 1");
        }

        public static void UpdateIsPeopleConfig2()
        {
            DB.ExecuteNonQuery("update config set ispeople = 2 where id = 1");
        }
    }
}
