﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Models
{
    public class UserGoals
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int SkillId { get; set; }

        public string SkillName { get; set; }

        public int SkillGoal { get; set; }

    }
}
