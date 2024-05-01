public class RandomUserGenerator
{
    private static readonly Random _random = new Random();

    public static int GenerateRandomAge()
    {
        return _random.Next(0, 100); 
    }

    public static string GenerateRandomName()
    {
  
        string[] firstNames = { "John", "Emma", "Michael", "Sophia", "James", "Olivia", "William", "Isabella", "David", "Emily" };
        string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

        string firstName = firstNames[_random.Next(firstNames.Length)];
        string lastName = lastNames[_random.Next(lastNames.Length)];

        return $"{firstName} {lastName}";
    }

    public static string GenerateRandomSex()
    {
        return _random.Next(2) == 0 ? "MALE" : "FEMALE";
    }

    public static string GenerateRandomZipCode()
    {
        Random random = new Random();
        int zipCode = random.Next(10000, 99999);
        return zipCode.ToString();
    }

}
