﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZServerManager.Server.Classes.SerializationClasses.ManagerClasses.ManagerConfigClasses
{
    public class Mod
    {
        public long id { get; set; }
        public string name { get; set; }
        public long workshopID { get; set; }
    }
}
