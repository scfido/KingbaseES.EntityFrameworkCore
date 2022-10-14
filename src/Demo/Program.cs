using kingbase_demo;

public class Program
{
    /* Kdbndp_efcode测试用例 */
    static void TestKdbndp_efcode()
    {
        using (var db = new Modules())
        {
            /* 添加数据 */
            db.Kdb_Blog_Tests.Add(new Blog_Test
            {
                Id = Guid.NewGuid(),
                Ids = Guid.NewGuid(),
                Name = "刘备",
                Sex = true,
                Sexy = true,
                Age = 45,
                Ager = 45,
                Birth = DateTime.Now,
                Birthy = DateTime.Now,
                Money = 1.5f,
                Moneies = 1.5f,
                Pi = 36.25,
                Pis = 36.25,
                State = State.无用,
                States = State.有用
            });
            var count = db.SaveChanges();
            Console.WriteLine("{0} records saved to database", count);
            Console.WriteLine("All blogs in database:");

            /* 搜索数据 */
            foreach (var blog in db.Kdb_Blog_Tests)
            {
                Console.WriteLine("Id: {0}", blog.Id);
                Console.WriteLine("Ids: {0}", blog.Ids);
                Console.WriteLine("Name: {0}", blog.Name);
                Console.WriteLine("Sex: {0}", blog.Sex);
                Console.WriteLine("Sexy: {0}", blog.Sexy);
                Console.WriteLine("Age: {0}", blog.Age);
                Console.WriteLine("Ager: {0}", blog.Ager);
                Console.WriteLine("Birth: {0}", blog.Birth);
                Console.WriteLine("Birthy: {0}", blog.Birthy);
                Console.WriteLine("Money: {0}", blog.Money);
                Console.WriteLine("Birthy: {0}", blog.Birthy);
                Console.WriteLine("Money: {0}", blog.Money);
                Console.WriteLine("Moneies: {0}", blog.Moneies);
                Console.WriteLine("Pi: {0}", blog.Pi);
                Console.WriteLine("Pis: {0}", blog.Pis);
                Console.WriteLine("State: {0}", blog.State);
                Console.WriteLine("States: {0}", blog.States);
            }
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        TestKdbndp_efcode();
        Console.ReadKey();
    }
}
