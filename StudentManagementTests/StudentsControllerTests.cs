using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using StudentManagement.Data;
using StudentManagement.Models;
using StudentManagement.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace StudentManagementTests
{
    public class StudentsControllerTests
    {
    [Fact]
        public async Task GetStudents_ReturnsAllStudents()
        {
        //1. Luodaan dbContextOptions in-memory -tietokantaa varten
        var options = new DbContextOptionsBuilder<StudentContext>()
        .UseInMemoryDatabase(databaseName: "StudentDbTest")
        .Options;

        //2. Lisätään "testidataan" muutama opiskelija
        using (var context = new StudentContext(options))
        {
            context.Students.Add(new Student { Id = 1, FirstName = "Maija", LastName = "Meikäläinen", Age =20 });
            context.Students.Add(new Student { Id = 2, FirstName = "Matti", LastName = "Meikäläinen", Age =22 });
            await context.SaveChangesAsync();
        }

        //3. Suoritetaan varsinainen testi uudella Context-instanssilla
        using (var context = new StudentContext(options))
        {
            var controller = new StudentsController(context);

            var result = await controller.GetStudents();

            //4.Tarkistetaan, että tuloksena on kaikki oppilaat (2 kpl)
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Student>>>(result);
            var studentsList = Assert.IsAssignableFrom<IEnumerable<Student>>(actionResult.Value);

            Assert.Equal(2, studentsList.Count());
        }
    }

    [Fact]
    public async Task GetStudent_ReturnsNotFound_WhenStudentDoesNotExists()
    {
        //InMemory -kanta luodaan
        var options = new DbContextOptionsBuilder<StudentContext>()
            .UseInMemoryDatabase(databaseName: "StudentDbTest_NotFound")
            .Options;

        //Ei lisätä dataa, jotta opiskelijaa ei löydy
        using (var context = new StudentContext(options))
        {
            var controller = new StudentsController(context);
            var result = await controller.GetStudent(99); //99 puuttuu

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
    [Fact]
    public async Task PostStudent_AddsNewStudent()
    {
        var options = new DbContextOptionsBuilder<StudentContext>()
        .UseInMemoryDatabase(databaseName: "StudentDbTest_Post")
        .Options;

        using (var context = new StudentContext(options))
        {
            var controller = new StudentsController(context);
            var student = new Student {Id = 3, FirstName = "Meeri", LastName = "Meikäläinen", Age = 20 };
            var result = await controller.PostStudent(student);

            var actionResult = Assert.IsType<ActionResult<Student>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdStudent = Assert.IsType<Student>(createdResult.Value);

            Assert.Equal("Meeri", createdStudent.FirstName);
            Assert.Equal("Meikäläinen", createdStudent.LastName);
            Assert.Equal(20, createdStudent.Age);
            }

        }
    [Fact]
    public async Task PutStudent_UpdatesStudent()
    {
        var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_Put")
                .Options;
        
        using (var context = new StudentContext(options))
        {
            context.Students.Add(new Student { Id = 1, FirstName = "Mirja", LastName = "Meikäläinen", Age = 27 });
            await context.SaveChangesAsync();
        }
        using (var context = new StudentContext(options))
        {
            var controller = new StudentsController(context);
            var updatedStudent = new Student { Id = 1, FirstName = "Mikko", LastName = "Virtanen", Age = 37 };

            var result = await controller.PutStudent(1, updatedStudent);

            // Ei sisältöä palautettavana:
            Assert.IsType<NoContentResult>(result);
        }
        // 3. Varmistetaan, että tiedot on päivitetty
        using (var context = new StudentContext(options))
        {
            var student = await context.Students.FindAsync(1);

            Assert.NotNull(student);
            Assert.Equal("Mikko", student.FirstName);
            Assert.Equal("Virtanen", student.LastName);
            Assert.Equal(37, student.Age);
        }
        
    }
    [Fact]
    public async Task DeleteStudent_DeletesStudent()
        {
            var options = new DbContextOptionsBuilder<StudentContext>()
            .UseInMemoryDatabase(databaseName: "StudentDbTest_Delete")
            .Options;

            // Lisää opiskelija tietokantaan
            using (var context = new StudentContext(options))
            {
                context.Students.Add(new Student { Id = 1, FirstName = "Mummo", LastName = "Mahtavainen", Age = 25 });
                await context.SaveChangesAsync();
            }

            // Poista opiskelija ja tarkista, että se on poistettu
            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var result = await controller.DeleteStudent(1);

            // Ei sisältöä palautettavana:
            Assert.IsType<NoContentResult>(result);

            // Varmistetaan, että opiskelija on poistettu tietokannasta
            var student = await context.Students.FindAsync(1);
            Assert.Null(student);
        }
    }
    }    
}