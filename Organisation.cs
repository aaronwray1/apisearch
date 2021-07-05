using System;
using System.Collections.Generic;

namespace searchapi
{
    public class Organisation
    {
        public string ID { get; set; }
        public string name { get; set; }

        public List<Secret> Secrets { get; set; }


    }

}
