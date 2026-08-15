using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Grade
{
    public class GradeUpdateDto
    {
        [Required]
        public double Score { get; set; }
    }
}
