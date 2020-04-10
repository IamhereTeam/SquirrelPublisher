using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelPublisher
{
    public class Publisher
    { 
        public static bool PublishProject(string path)
        {
            System.Threading.Thread.Sleep(20000);

            return true;
        }
    }
}