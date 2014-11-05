namespace ElasticsearchTests.Models
{
    public class Employee
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public int? Age { get; set; }
        public string About { get; set; }
        public string[] Interests { get; set; }
        public string Suggest { get; set; }
    }
}
