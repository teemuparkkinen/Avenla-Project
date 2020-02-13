﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Login_System.Controllers
{
    public class UserSkillsController : Controller
    {
        private readonly UserSkillsDataContext _context;
        private readonly SkillDataContext skillContext;
        private UserManager<AppUser> UserMgr { get; }

        //These will be set in the index, and be used by other controller methods.
        public int userId;
        public string userName;

        public UserSkillsController(UserSkillsDataContext context, SkillDataContext sContext, UserManager<AppUser> userManager)
        {
            _context = context;
            skillContext = sContext;
            UserMgr = userManager;
        }

        public async Task<IActionResult> Index (int? id)
        {
            if (id == null)
            {
                id = Convert.ToInt32(TempData["UserId"]);
            }

            var model = new List<Skills>();

            foreach (var skill in skillContext.Skills)
            {
                /*
                skill.EntryCount = CountEntries(skill.Skill);
                try
                {
                    DateTime latestDateEntry = GetLatest(skill.Skill);
                    skill.LatestEntry = latestDateEntry.ToString("MM/dd/yyyy H:mm");
                    skill.LatestEval = GetLatestEval(skill.Skill, latestDateEntry);
                }
                catch
                {
                    skill.LatestEntry = "Not available!";
                    skill.LatestEval = 0;
                }
                */
                model.Add(skill);
            }
            //This is useful information we'll need in other controller actions
            //userId = id;
            var tempUser = await UserMgr.FindByIdAsync(id.ToString());
            //userName = tempUser.UserName;

            TempData["UserId"] = id;
            //TempData["UserName"] = tempUser.UserName;

            return View(model);
        }

        public async Task <IActionResult> ListByDate(int? id)
        {
            if (id == null)
            {
                id = Convert.ToInt32(TempData["UserId"]);
            }

            TempData["UserId"] = id;
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserName"] = tempUser.UserName;

            var model = new List<string>(); 
            
            foreach (var item in _context.UserSkills)
            {
                if (item.UserID == id && !model.Contains(item.Date.ToString()))
                {
                    model.Add(item.Date.ToString());
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SkillList(string date)
        {
            var model = new List<UserSkillsVM>();

            int userId = Convert.ToInt32(TempData["UserId"]);
            //string userName = TempData["UserName"].ToString();
            
            AppUser tempUser = await UserMgr.FindByIdAsync(userId.ToString());
            TempData["UserName"] = tempUser.UserName;

            foreach (var skill in _context.UserSkills)
            {
                if (skill.Date.ToString() == date && skill.UserID == userId)
                {
                    var usrSkill = new UserSkillsVM();

                    usrSkill.Id = Convert.ToInt32(skill.Id);
                    usrSkill.UserID = skill.UserID;
                    usrSkill.UserName = userName;
                    usrSkill.SkillName = skill.SkillName;
                    usrSkill.SkillLevel = skill.SkillLevel;
                    usrSkill.Date = skill.Date.ToString("MM/dd/yyyy H:mm");
                    usrSkill.AdminEval = skill.AdminEval;

                    model.Add(usrSkill);
                }
            }

            TempData.Keep();
            return View(model);
        }

        /* OLD INDEX. REMOVE THIS AT SOME POINT
        public async Task<IActionResult> OLD_Index(int? id)
        {
            var model = new List<UserSkillsVM>();

            ViewBag.UserId = id;

            var userName = UserMgr.GetUserName(User);
           
            foreach (var skill in _context.UserSkills)
            {               
                
                if (!User.IsInRole("Admin"))
                {
                    //Depending on where the Index is accessed from, the ID may be null. If that's the case it is automatically set to be the same as the ID of the current user
                    if (id == null)
                    {
                        id = Convert.ToInt32(UserMgr.GetUserId(User));
                    }
                    //If the UserID of the skill is the same as the id that is passed from AppUser Index (that is the index of the current user), the skill is added to the list.
                    if (skill.UserID == id || Convert.ToInt32(UserMgr.GetUserId(User)) == skill.UserID)
                    {
                        var usrSkill = new UserSkillsVM();

                        usrSkill.Id = skill.Id;
                        usrSkill.UserID = skill.UserID;
                        usrSkill.UserName = userName;
                        usrSkill.SkillName = skill.SkillName;
                        usrSkill.SkillLevel = skill.SkillLevel;
                        usrSkill.Date = skill.Date.ToString("MM/dd/yyyy");

                        model.Add(usrSkill);
                        ViewBag.UserName = usrSkill.UserName;
                    }
                }
                //The index has different behavior for admins
                else if (User.IsInRole("Admin"))
                {
                    if (id == null)
                    {
                        id = Convert.ToInt32(UserMgr.GetUserId(User));

                        if (skill.UserID == id)
                        {
                            var usrSkill = new UserSkillsVM();

                            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());

                            usrSkill.Id = skill.Id;
                            usrSkill.UserID = skill.UserID;
                            usrSkill.UserName = tempUser.UserName;
                            usrSkill.SkillName = skill.SkillName;
                            usrSkill.SkillLevel = skill.SkillLevel;
                            usrSkill.Date = skill.Date.ToString("MM/dd/yyyy");

                            model.Add(usrSkill);
                            ViewBag.UserName = usrSkill.UserName;
                        }
                        //var usrSkill = new UserSkillsVM();
                        //ViewBag.UserName = "All users";
                        //This lists ALL entries to the userskills db. It's very messy and not really useful.                        
                    }
                    else if (skill.UserID == id)
                    {
                        var usrSkill = new UserSkillsVM();
                        //This gets the info of the selected user and sets the username value to it.
                        AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
                        
                        usrSkill.Id = skill.Id;
                        usrSkill.UserID = skill.UserID;
                        usrSkill.UserName = tempUser.UserName;
                        usrSkill.SkillName = skill.SkillName;
                        usrSkill.SkillLevel = skill.SkillLevel;
                        usrSkill.Date = skill.Date.ToString("MM/dd/yyyy");

                        model.Add(usrSkill);
                        ViewBag.UserName = usrSkill.UserName;
                    }
                }                
            }            
            return View(model);
        }
        */

        // GET: UserSkills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSkills = await _context.UserSkills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSkills == null)
            {
                return NotFound();
            }

            return View(userSkills);
        }

        // GET: UserSkills/Create
        public IActionResult Create(int? id)
        {
            //var model = new List<UserSkillsWithSkillVM>();
            var tempModel = new UserSkillsWithSkillVM();
            var tempList = new Dictionary<int, string>();
            var Skills = skillContext.Skills.ToList();
            int dictKey = 0;
            if (id == null)
            {
                id = TempData["UserId"] as int?;
                TempData.Keep();
            }
            foreach (var skill in Skills)
            {
                tempList.Add(dictKey, skill.Skill);
                dictKey++;
            }
            tempModel.SkillList = tempList;
            //model.Add(tempModel);
            return View(tempModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkillList, SkillLevel, SkillCount")] UserSkillsWithSkillVM userSkills)
        {
            var model = new List<UserSkills>();

            //Date is declared here so that it's guaranteed to be the same for all skills.
            DateTime date = DateTime.Now;

            //Looping through the entries, adding them to a UserSkills object and adding that to a list.
            for (int i = 0; i < userSkills.SkillList.Count(); i++)
            {
                var tempModel = new UserSkills
                {
                    SkillLevel = userSkills.SkillLevel[i],
                    SkillName = userSkills.SkillList[i],
                    UserID = Convert.ToInt32(TempData["UserId"]),
                    Id = null,
                    Date = date               
                };

                if (User.IsInRole("Admin"))
                {
                    tempModel.AdminEval = "Admin Evaluation";
                }
                else
                {
                    tempModel.AdminEval = "Self Assessment";
                }

                model.Add(tempModel);
            }

            foreach (var entry in model)
            {
                _context.Add(entry);                
            }

            await _context.SaveChangesAsync();

            TempData.Keep();
            return RedirectToAction(nameof(ListByDate), "UserSkills", new { id = TempData.Peek("UserId") });

            /*
             _context.Add(userSkills);
             await _context.SaveChangesAsync();
             return RedirectToAction(nameof(Index), "UserSkills", new { id = model.UserID });
            */

            //return View(model);
        }

        // GET: UserSkills/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                id = TempData["UserId"] as int?;
            }

            var userSkills = await _context.UserSkills.FindAsync(id);
            if (userSkills == null)
            {
                return NotFound();
            }

            TempData.Keep();
            return View(userSkills);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserID,SkillName,SkillLevel,Date")] UserSkills userSkills)
        {
            //int redirectId = userSkills.UserID;

            if (id != userSkills.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userSkills);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserSkillsExists(Convert.ToInt32(userSkills.Id)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "UserSkills", new { id = userSkills.UserID });
            }
            return View(userSkills);
        }

        // GET: UserSkills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSkills = await _context.UserSkills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSkills == null)
            {
                return NotFound();
            }

            return View(userSkills);
        }

        // POST: UserSkills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userSkills = await _context.UserSkills.FindAsync(id);
            _context.UserSkills.Remove(userSkills);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserSkillsExists(int id)
        {
            return _context.UserSkills.Any(e => e.Id == id);
        }

        /*
        [HttpGet]
        public ActionResult ListSkills()
        {
            var skills = skillContext.Skills.ToList();
            var viewModel = new ListSkillsVM { Skills = skills };
            return View(viewModel);
        }
        */

        public int CountEntries(string skillName)
        {
            int entryCount = 0;

            int userId = Convert.ToInt32(TempData["UserId"]);
            foreach (var skill in _context.UserSkills)
            {
                if (skill.SkillName == skillName && skill.UserID == userId)
                {
                    entryCount++;
                }
            }

            TempData.Keep();
            return entryCount;
        }

        public DateTime GetLatest (string skillName)
        {
            DateTime latestDate;
            List<DateTime> allDates = new List<DateTime>();
            int userId = Convert.ToInt32(TempData["UserId"]);

            foreach (var skill in _context.UserSkills)
            {
                if (skill.SkillName == skillName && skill.UserID == userId)
                {
                    allDates.Add(skill.Date);
                }
            }
            latestDate = allDates.Max();
            TempData.Keep();
            return latestDate;
        }

        public int GetLatestEval (string skillName, DateTime latestDate)
        {
            int latestEval = 0;
            int userId = Convert.ToInt32(TempData["UserId"]);

            foreach (var skill in _context.UserSkills)
            {
                if (skill.SkillName == skillName && skill.UserID == userId && skill.Date == latestDate)
                {
                    latestEval = skill.SkillLevel;
                }
            }

            TempData.Keep();
            return latestEval;
        }
    }
}
