using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using Mobile_Application_Development.Models;
using Microsoft.Maui.Storage;

namespace Mobile_Application_Development.Data
{
    public class TermDatabase
    {
        public Task<Term> SaveAsync(Term term) => SaveTermAsync(term);

        private readonly string _dbPath;
        private SQLiteAsyncConnection? _conn;

        public TermDatabase()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "mad_terms.db3");
            InitializeAsync(); 
        }

        private async Task InitializeAsync()
        {
            await GetConnAsync();
            await SeedIfEmptyAsync();
        }

        private async Task<SQLiteAsyncConnection> GetConnAsync()
        {
            if (_conn == null)
            {
                _conn = new SQLiteAsyncConnection(
                    _dbPath,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache
                );

                await _conn.CreateTableAsync<Term>();
                await _conn.CreateTableAsync<Course>();
                await _conn.CreateTableAsync<Assessment>();
            }
            return _conn;
        }

        
        private async Task SeedIfEmptyAsync()
        {
            var terms = await GetTermsAsync();
            if (terms.Count > 0) return;   

            var term = new Term
            {
                Title = "Term 1",
                StartDate = System.DateTime.Today,
                EndDate = System.DateTime.Today.AddMonths(3)
            };
            await SaveTermAsync(term);

            var course = new Course
            {
                TermId = term.Id,
                Title = "Sample Course",
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                Status = CourseStatus.InProgress,
                InstructorName = "Anika Patel",
                InstructorPhone = "555-123-4567",
                InstructorEmail = "anika.patel@strimeuniversity.edu"
            };
            await SaveCourseAsync(course);

            var perf = new Assessment
            {
                CourseId = course.Id,
                Type = AssessmentType.Performance,
                Title = "Performance Assessment",
                DueDate = System.DateTime.Today.AddDays(7)
            };
            await SaveAssessmentAsync(perf);

            var obj = new Assessment
            {
                CourseId = course.Id,
                Type = AssessmentType.Objective,
                Title = "Objective Assessment",
                DueDate = System.DateTime.Today.AddDays(14)
            };
            await SaveAssessmentAsync(obj);
        }
        

        public async Task<List<Term>> GetTermsAsync()
        {
            var db = await GetConnAsync();
            return await db.Table<Term>().OrderBy(t => t.StartDate).ToListAsync();
        }

        public async Task<Term> SaveTermAsync(Term term)
        {
            var db = await GetConnAsync();
            if (term.Id == 0) await db.InsertAsync(term);
            else await db.UpdateAsync(term);
            return term;
        }

        public async Task DeleteTermAsync(Term term)
        {
            var db = await GetConnAsync();
            var courses = await db.Table<Course>().Where(c => c.TermId == term.Id).ToListAsync();
            foreach (var c in courses)
                await DeleteCourseAsync(c);
            await db.DeleteAsync(term);
        }

        public async Task<List<Course>> GetCoursesForTermAsync(int termId)
        {
            var db = await GetConnAsync();
            return await db.Table<Course>().Where(c => c.TermId == termId).OrderBy(c => c.StartDate).ToListAsync();
        }

        public async Task<int> GetCourseCountForTermAsync(int termId)
        {
            var db = await GetConnAsync();
            return await db.Table<Course>().Where(c => c.TermId == termId).CountAsync();
        }

        public async Task<Course> SaveCourseAsync(Course course)
        {
            var db = await GetConnAsync();
            if (course.Id == 0) await db.InsertAsync(course);
            else await db.UpdateAsync(course);
            return course;
        }

        public async Task DeleteCourseAsync(Course course)
        {
            var db = await GetConnAsync();
            var assessments = await db.Table<Assessment>().Where(a => a.CourseId == course.Id).ToListAsync();
            foreach (var a in assessments)
                await db.DeleteAsync(a);
            await db.DeleteAsync(course);
        }

        public async Task<List<Assessment>> GetAssessmentsForCourseAsync(int courseId)
        {
            var db = await GetConnAsync();
            return await db.Table<Assessment>().Where(a => a.CourseId == courseId).OrderBy(a => a.Type).ToListAsync();
        }

        public async Task<Assessment> SaveAssessmentAsync(Assessment assessment)
        {
            var db = await GetConnAsync();
            if (assessment.Id == 0) await db.InsertAsync(assessment);
            else await db.UpdateAsync(assessment);
            return assessment;
        }

        public async Task DeleteAssessmentAsync(Assessment assessment)
        {
            var db = await GetConnAsync();
            await db.DeleteAsync(assessment);
        }

        
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            var db = await GetConnAsync();
            return await db.Table<Course>().OrderBy(c => c.Title).ToListAsync();
        }

        
        public async Task<List<Assessment>> GetAllAssessmentsAsync()
        {
            var db = await GetConnAsync();
            return await db.Table<Assessment>().OrderBy(a => a.DueDate).ToListAsync();
        }


    }

}