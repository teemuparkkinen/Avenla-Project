﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Login_System.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Login_System.Controllers
{
    [Authorize(Roles = "User, Admin, Superadmin")]
    public class SkillGoalsController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public SkillGoalsController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        // GET: SkillGoals
#nullable enable
        public IActionResult Index(int? id, string name, string date)
        {

            if (id == null)
            {
                Console.WriteLine("No group selected. This is most likely an error.");
                return View();
            }
            var group =_context.Group.Where(x => x.id == id).First();
            TempData["GroupID"] = group.id;
            TempData["GroupName"] = group.name;

            if (id != null)
            {
                //We can't pass a date if we are accessing the view from the groups index
                //So we have to get the latest date from the DB
                date = GetLatestDate(id).ToString("dd.MM.yyyy");
                TempData["LatestDate"] = GetLatestDate(id).ToString("dd.MM.yyyy");
            }

            //Getting skillgoals for the correct group and correct date if the date has been selected
            if (id!=null)
            {
                var model = new SkillGoalIndexVM();
                var tempModel = new List<SkillGoals>();
                var modelCheck = new List<string>();
                var listOfSkills = new List<Skills>();

                foreach (var skillGoal in _context.SkillGoals.Where(x => (x.GroupId == id)).OrderByDescending(x=>x.Date))
                {
                    //modelCheck is to prevent duplicates
                    if (!modelCheck.Contains(skillGoal.SkillName) /*&& skillGoal.Date.ToString("dd.MM.yyyy") == date*/)
                    {
                        skillGoal.LatestGoal = skillGoal.SkillGoal;
                        tempModel.Add(skillGoal);
                        modelCheck.Add(skillGoal.SkillName);
                    }
                }

                model.Goals = tempModel;
                model.SkillDates = GetDates(_context.SkillGoals, name);
                //model.DateToDelete = date;

                if (model != null)
                {
                    return View(model);
                }
                Console.WriteLine("No entries have been found!");

                return View(_context.SkillGoals.ToList());
            }

            return View();
        }
#nullable disable

        public IActionResult RefreshIndex([Bind("GroupName, SelectedDate")]SkillGoalIndexVM goal)
        {
            TempData["LatestDate"] = goal.SelectedDate;

            return RedirectToAction(nameof(Index), "SkillGoals", new { name = goal.GroupName, date = goal.SelectedDate });
        }
        public IActionResult ListByDate(string name)
        {
            if (name == null)
            {
                name = TempData["GroupName"].ToString();
            }

            var model = new List<SkillGoals>();

            foreach (var item in _context.SkillGoals.Where(x => x.SkillName == name))
            {
                model.Add(item);
            }

            TempData.Keep();

            return View(model);
        }

        // GET: SkillGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var skillGoals = await _context.SkillGoals
                .FirstOrDefaultAsync(m => m.Id == id);

            if (skillGoals == null)
            {
                return NotFound();
            }
            return View(skillGoals);
        }

        // GET: SkillGoals/Create
        public  IActionResult Create(int id, string name)
        {
            var model = new CreateSkillGoalsVM();
            var skillsList = new List<Skills>();
            var listModel = new List<SkillGoals>();
            DateTime date = DateTime.Now;
            int groupId = _context.Group.FirstOrDefault(x => x.id == id).id;
            TempData["id"] = id;

                foreach(var item in _context.SkillGoals.Where(x => (x.GroupId == id)).OrderByDescending(x => x.Date))
                {
                    var tempModel = new SkillGoals
                    {
                        SkillId = item.SkillId,
                        SkillName = item.SkillName,
                        GroupName = name,
                        GroupId = groupId,
                        SkillGoal = (item.SkillGoal >= 0) ? item.SkillGoal : 0
                    };
                    if (!listModel.Any(p => p.SkillName == tempModel.SkillName))
                    {
                        listModel.Add(tempModel);
                    }
            }
            
            model.GroupName = name;
            model.Groupid = id;
            model.SkillGoals = listModel;
           
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string source, [Bind("SkillGoal")] CreateSkillGoalsVM goals,  string[] Skill, string GroupName, int Groupid, List<int> SkillId, int[] SkillGoal)
        {
            if (ModelState.IsValid)
            {

            
            int i = 0;
            var groupId = Groupid;
            var group = await _context.Group.FirstOrDefaultAsync(x => x.id == groupId );
            foreach (var skill in SkillId)
            {
                var skillFromTable= await _context.Skills.FirstOrDefaultAsync(x => x.Id  == skill);
                var skillGoal = new SkillGoals
                {
                    //SkillName = skill.Skill,
                    SkillGoal = SkillGoal[i],
                    Date = DateTime.Now,
                    SkillName = skillFromTable.Skill,
                    SkillId = skillFromTable.Id,
                    GroupName = group.name,
                    GroupId = Groupid
                };
                _context.Add(skillGoal);
                i++;
            }
             await _context.SaveChangesAsync();


            /**/

            if (source == "create")
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GoalSetAddUser;
                TempData["ActionPhase"] = "[3/3]";

                
                return RedirectToAction(nameof(Create), "GroupMembers", new { source = "create", id =  Groupid, group = GroupName});
            }
          }
            return RedirectToAction(nameof(Index), new { id = Groupid});
            
        }

        // GET: SkillGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            string groupName = TempData["GroupName"].ToString();
            TempData.Keep();

            if (id == null)
            {
                return NotFound();
            }

            var skillGoals = await _context.SkillGoals.FindAsync(id);

            if (skillGoals == null)
            {
                return NotFound();
            }
            ViewBag.GroupName = groupName;

            return View(skillGoals);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, SkillName, GroupName, SkillGoal, Date, Skillid")] SkillGoals skillGoals)
        {
            if (id != skillGoals.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skillGoals);
                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillGoalsExists(skillGoals.Id))
                    {
                        return NotFound();
                    }

                    else
                    {
                        throw;
                    }
                }
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GoalEdited;

                return RedirectToAction(nameof(Index), new { name = TempData.Peek("GroupName") });
            }
            return View(skillGoals);
        }

       

        // GET: SkillGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skillGoals = await _context.SkillGoals
                .FirstOrDefaultAsync(m => m.Id == id);

            if (skillGoals == null)
            {
                return NotFound();
            }

            return View(skillGoals);
        }

        // POST: SkillGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skillGoals = await _context.SkillGoals.FindAsync(id);
            _context.SkillGoals.Remove(skillGoals);
            await _context.SaveChangesAsync();

            TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GoalDeleted;

            return RedirectToAction(nameof(Index), new { name = TempData.Peek("GroupName") });
        }

        //This deletes the entire form, not just individual skills
        public async Task<IActionResult> DeleteForm(string date, string group)
        {
            if (date == null || group == null)
            {
                return NotFound();
            }

            else
            {
                try
                {
                    foreach (var entry in _context.SkillGoals.Where(x => x.GroupName == group))
                    {
                        if (entry.Date.ToString("dd.MM.yyyy") == date)
                        {
                            _context.Remove(entry);
                        }
                    }
                    await _context.SaveChangesAsync();
                    TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GoalDeleted;
                }

                catch
                {
                    TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GoalDeletedFail;
                }
            }
            return RedirectToAction(nameof(Index), new { name = group });
        }

        private bool SkillGoalsExists(int id)
        {
            return _context.SkillGoals.Any(e => e.Id == id);
        }

        public static List<SelectListItem> GetItems(DbSet<Skills> skillList)
        {
            List<SelectListItem> ls = new List<SelectListItem>();
            foreach (var item in skillList)
            {
                ls.Add(new SelectListItem() { Text = item.Skill, Value = item.Skill });
            }
            return ls;
        }

        //This takes the list of skills and groupname to put all the dates where skillgoal entries have been made (for that specific group) into a list
        public static List<SelectListItem> GetDates(DbSet<SkillGoals> skillList, string groupName)
        {
            List<SelectListItem> ls = new List<SelectListItem>();
            var dateList = new List<string>();

            foreach (var item in skillList.Where(x => x.GroupName == groupName))
            {
                if (!dateList.Contains(item.Date.ToString("dd.MM.yyyy")))
                {
                    dateList.Add(item.Date.ToString("dd.MM.yyyy"));
                }               
            }

            foreach (var date in dateList)
            {
                ls.Add(new SelectListItem() { Text = date, Value = date });
            }
            return ls;
        }

        public DateTime GetLatestDate()
        {
            var goalList = _context.SkillGoals.ToList();
            var dateList = new List<DateTime>();
            DateTime maxDate;

            foreach (var goal in goalList)
            {
                if (!dateList.Contains(goal.Date))
                {
                    dateList.Add(goal.Date);
                }
            }

            if (dateList.Count() != 0)
            {
                maxDate = dateList.Max();
            }

            else
            {
                maxDate = DateTime.Now;
            }
            return maxDate;
        }

        public DateTime GetLatestDate(int? group)
        {
            var dateList = new List<DateTime>();
            DateTime maxDate;
            foreach (var goal in _context.SkillGoals.Where(x => (x.GroupId == group)))
            {
                if (!dateList.Contains(goal.Date))
                {
                    dateList.Add(goal.Date);
                }
            }

            if (dateList.Count() != 0)
            {
                maxDate = dateList.Max();
            }

            else
            {
                maxDate = DateTime.Now;
            }
            return maxDate;
        }

        //Get latest goal for a specific skill and group
        public int GetLatest(SkillGoals goal)
        {
            int currentGoal = 0;
            var dateList = new List<DateTime>();

            foreach (var skillGoal in _context.SkillGoals.Where(x => (x.GroupName == goal.GroupName) && (x.SkillName == goal.SkillName)))
            {
                dateList.Add(skillGoal.Date);
            }

            var dateResult = dateList.Max();

            foreach (var skillGoal in _context.SkillGoals.Where(x => (x.GroupName == goal.GroupName) && (x.SkillName == goal.SkillName) && (x.Date == dateResult)))
            {
                currentGoal = skillGoal.SkillGoal;
            }
            return currentGoal;
        }
    }
}
