using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Mobile_Application_Development.Models;
using Mobile_Application_Development.Services;

namespace Mobile_Application_Development.Tests
{
    [TestClass]
    public class ReportFormatterTests
    {
        [TestMethod]
        public void BuildCsv_IncludesTitleTimestampRangeHeaderAndRows()
        {
            var from = new DateTime(2025, 01, 01);
            var to = new DateTime(2025, 12, 31);

            var assessments = new List<Assessment>
            {
                new Assessment { Id=1, Title="Perf A", Type=AssessmentType.Performance, StartDate=new DateTime(2025,02,01), DueDate=new DateTime(2025,02,10) },
                new Assessment { Id=2, Title="Obj B",  Type=AssessmentType.Objective,   StartDate=new DateTime(2025,03,05), DueDate=new DateTime(2025,03,20) },
            };

            var csv = ReportFormatter.BuildCsv(assessments, from, to, "Upcoming Assessments Report");

            StringAssert.Contains(csv, "Upcoming Assessments Report");
            StringAssert.Contains(csv, "Generated:");
            StringAssert.Contains(csv, "Range: 2025-01-01 to 2025-12-31");
            StringAssert.Contains(csv, "AssessmentId,Type,Title,StartDate,DueDate");
            StringAssert.Contains(csv, "1,Performance,\"Perf A\",2025-02-01,2025-02-10");
            StringAssert.Contains(csv, "2,Objective,\"Obj B\",2025-03-05,2025-03-20");
        }
    }
}
 