﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Login_System.Controllers
{
    public class LessonsController : Controller
    {
        private readonly GeneralDataContext _context;
	    private readonly UserManager<AppUser> UserMgr;

        public LessonsController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
	        UserMgr = userManager;
        }

        // GET: Lessons
        public IActionResult Index(int id)
        {
            var lessons = _context.Lessons.Where(x => x.CourseID == id).ToList();
            Lesson Lmodel = new Lesson();
            Lmodel.LessonList = lessons;
            Lmodel.CourseID = id;
            ViewBag.CourseName = _context.Courses.FirstOrDefault(x => x.id == id).CourseName;

            return View(Lmodel);
        }

	public async Task<IActionResult> Attend(int id)
	{
	        AppUser tempUser = await UserMgr.FindByNameAsync(User.Identity.Name);
            Lesson tempLesson = await _context.Lessons.FindAsync(id);
            int index = 0;
            int memberIndex = 0;

            foreach (var courseMember in _context.SkillCourseMembers.Where(x => (x.CourseID == tempLesson.CourseID) && (x.UserID == tempUser.Id)))
            {
                memberIndex++;
            }	
            
            foreach (var member in _context.LessonUsers.Where(x => (x.LessonID == id) && (x.MemberName == User.Identity.Name)))
            {
                index++;
            }

            if (index == 0 && memberIndex != 0)
            {
                LessonUser model = new LessonUser
                {
                    MemberName = User.Identity.Name,
                    MemberID = tempUser.Id,
                    LessonID = id,
                    Attending = true
                };

                try
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                }

                catch
                {
                    
                }
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_AttendSuccess + " " + tempLesson.LessonName + "!";
                return RedirectToAction(nameof(Index), "SkillCourses");
            }
            TempData["ActionResult"] = Resources.ActionMessages.ActionResult_AttendFail;
            return RedirectToAction(nameof(Index), "SkillCourses");
	}
	
        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // GET: Lessons/Create
        public IActionResult Create(int id)
        {
            var model = new CreateLessonVM
            {
                CourseID = id
            };
            ViewBag.CourseName = _context.Courses.FirstOrDefault(x => x.id == id).CourseName;
            return View(model);
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
	    [Authorize(Roles = "Admin , Superadmin")]
        public async Task<IActionResult> Create(CreateLessonVM lesson)
        {
            if (ModelState.IsValid)
            {
                Lesson tempLesson = new Lesson
                {
                    CourseID = lesson.CourseID,
                    LessonName = lesson.LessonName,
                    Location = lesson.Location
                };

                //We need to do some stuff with the string to get it to work as datetime. Thanks to the american date format
                string tempDate = DateTime.ParseExact(lesson.DateString, "dd.MM.yyyy", CultureInfo.CurrentCulture).ToShortDateString();
                tempDate += ' ' + lesson.HourString + ':' + lesson.MinuteString;
                tempLesson.Date = DateTime.Parse(tempDate, CultureInfo.CurrentCulture);

                var tempCourse = _context.Courses.FirstOrDefault(x => x.id == lesson.CourseID);
                tempLesson.CourseName = tempCourse.CourseName;
                _context.Add(tempLesson);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), "Lessons", new { id= lesson.CourseID});
            }
            return View(lesson);
        }

        // GET: Lessons/Edit/5
        [Authorize(Roles = "Admin , Superadmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Edit(int id, Lesson lesson)
        {
            if (id != lesson.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(lesson.Id))
                    {
                        return NotFound();
                    }

                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id=lesson.CourseID });
            }
            return View(lesson);
        }

        // GET: Lessons/Delete/5
	[Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
	    [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new {id = lesson.CourseID});
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }
}
