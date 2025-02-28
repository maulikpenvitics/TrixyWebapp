namespace TrixyWebapp.ViewModels
{
    public class CreateUserViewModel
    {
        public string? Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public IFormFile? ProfileImage { get; set; } // File upload field
        public string? ProfileImageUrl { get; set; } // Store the image path
    }
}
