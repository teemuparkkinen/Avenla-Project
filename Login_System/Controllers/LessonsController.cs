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
        private readonly LessonDataContext _context;
	    private readonly LessonUserDataContext _ucontext;
	    private readonly UserManager<AppUser> UserMgr;
        private readonly SkillCourseDataContext courseContext;

        public LessonsController(LessonDataContext context, LessonUserDataContext ucontext, UserManager<AppUser> userManager, SkillCourseDataContext courseCon)
        {
            _context = context;
	        _ucontext = ucontext;
	        UserMgr = userManager;
            courseContext = courseCon;
        }

        // GET: Lessons
        public async Task<IActionResult> Index()
        {
            return View(await _context.Lessons.ToListAsync());
        }

	public async Task<IActionResult> Attend(int id)
	{
	    AppUser tempUser = await UserMgr.FindByNameAsync(User.Identity.Name);
            Lesson tempLesson = await _context.Lessons.FindAsync(id);
            int index = 0;
	    
            foreach (var member in _ucontext.LessonUsers.Where(x => (x.LessonID == id) && (x.MemberName == User.Identity.Name)))
            {
                index++;
            }
            if (index == 0)
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
                    _ucontext.Add(model);
                    await _ucontext.SaveChangesAsync();
                }
                catch
                {
                    Console.WriteLine("SOME SHIT HAPPENED PANIC");
                }
                TempData["ActionResult"] = "Successfully attended the lesson \"" + tempLesson.LessonName + "\" !";
                return RedirectToAction(nameof(Index), "SkillCourses");
            }
            TempData["ActionResult"] = "Could not attend the lesson!";
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
            return View(model);
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
	[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("CourseID,LessonName,DateString,HourString,MinuteString,Location")] CreateLessonVM lesson)
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
                //
                tempDate += ' ' + lesson.HourString + ':' + lesson.MinuteString + ':' + "00";
                tempLesson.Date = DateTime.Parse(tempDate, CultureInfo.CurrentCulture);
                var tempCourse = courseContext.Courses.FirstOrDefault(x => x.id == lesson.CourseID);
                tempLesson.CourseName = tempCourse.CourseName;
                _context.Add(tempLesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "SkillCourses");
            }
            return View(lesson);
        }

        // GET: Lessons/Edit/5
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseID,CourseName,LessonName,Date,Location")] Lesson lesson)
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
                return RedirectToAction(nameof(Index));
            }
            return View(lesson);
        }

        // GET: Lessons/Delete/5
	[Authorize(Roles = "Admin")]
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
	[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }
}