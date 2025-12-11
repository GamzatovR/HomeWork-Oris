using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyORMLibraryTest
{
    internal class AuxiliaryEntities
    {
    }
    class User
    {
        public int id { get; set; }
        public string UserName { get; set; } = "Rustam";
        public int Age { get; set; } = 19;
        public string Password { get; set; } = "Qwerty123";
    }
}
