﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class Group
    {
        public int id { get; set; }

        public string name { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
        
    }
}
