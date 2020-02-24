﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;

namespace Login_System.Models
{
    public class SkillCourseMemberDataContext: DbContext
    {
        public SkillCourseMemberDataContext(DbContextOptions<SkillCourseMemberDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }       
        public DbSet<SkillCourseMember> SkillCourseMembers { get; set; }
    }
}
