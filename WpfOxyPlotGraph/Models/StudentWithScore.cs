using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;

namespace WpfOxyPlotGraph.Models
{
    public class StudentWithScore
    {
        public Student Student { get; set; } = default!;
        public TestScore Score { get; set; } = default!;

        private static IEnumerable<StudentWithScore>? _joinedData;
        public static IEnumerable<StudentWithScore> GetSeedDatas()
        {
            if (_joinedData != null) return _joinedData;

            List<Student> students = new()
      {
        new Student { Id = 1, Name = "김철수" },
        new Student { Id = 2, Name = "이영희" },
        new Student { Id = 3, Name = "박민수" },
        new Student { Id = 4, Name = "정지우" },
        new Student { Id = 5, Name = "홍길동" }
      };

            Random random = new Random();
            List<TestScore> testScores = new List<TestScore>();
            foreach (Student student in students)
            {
                for (int i = 1; i <= 5; i++)
                {
                    DateTime date = new DateTime(2023, i, 1);
                    testScores.Add(new TestScore
                    {
                        Id = i,
                        Date = date,
                        KorScore = random.Next(0, 101),
                        MathScore = random.Next(0, 101),
                        EngScore = random.Next(0, 101),
                        StudentId = student.Id,
                    });
                }
            }
            _joinedData = from student in students
                          join score in testScores on student.Id equals score.StudentId
                          select new StudentWithScore { Student = student, Score = score };

            return _joinedData;
        }
        //임의 데이터를 생성하는 것이 아닌 mysql하고 연동하고 학생 데이터를 조회해서 가져오도록 구현
        public static IEnumerable<StudentWithScore> GetDatasFromDb()
        {
            using (var connection = new MySqlConnection("Server=localhost;Database=mydatabase;Uid=root;Pwd=password;"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM students";
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var student = new Student
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };
                }
                return _joinedData;
            }
            return _joinedData;
            }
        }
    }


