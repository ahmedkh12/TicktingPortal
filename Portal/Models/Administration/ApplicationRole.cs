namespace Portal.Models.Administration
{
    public class ApplicationRole : IdentityRole
    {
        // Add custom properties here for Application Role
        public string? Description { get; set; }
    }
}
