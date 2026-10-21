using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Api.Dtos
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot be longer then 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Description cannot be longere then 200 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [AllowedValues("To Do", "In Progress", "Done", ErrorMessage = "Status must be 'To Do', 'In Progress', or 'Done'.")]
        public string Status { get; set; } = "To Do";
    }
}
